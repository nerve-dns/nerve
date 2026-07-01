// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Service.Domain;

namespace Nerve.Service;

public sealed partial class ListService : IListService
{
    // TODO: Support multiple hosts?
    [GeneratedRegex("(?<ip>[0-9.]+)\\s+(?<host>[\\w.-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex HostsRegex();
    private static readonly Regex HostsPattern = HostsRegex();

    private static readonly TimeSpan HttpClientTimeout = TimeSpan.FromSeconds(15);

    private const char HostsCommentChar = '#';

    private const string HttpListPrefix = "http";

    private readonly ILogger<ListService> logger;
    private readonly IDomainAllowlistService domainAllowlistService;
    private readonly IDomainBlocklistService domainBlocklistService;
    private readonly IServiceScopeFactory serviceScopeFactory;

    private readonly SemaphoreSlim loadListsSemaphore = new(1, 1);

    public ListService(
        ILogger<ListService> logger,
        IDomainAllowlistService domainAllowlistService,
        IDomainBlocklistService domainBlocklistService,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.logger = logger;
        this.domainAllowlistService = domainAllowlistService;
        this.domainBlocklistService = domainBlocklistService;
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public async Task RefreshAsync(int listId, CancellationToken cancellationToken)
    {
        await using var scope = this.serviceScopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<NerveDbContext>();

        var list = await dbContext.Lists
            .FirstOrDefaultAsync(b => b.Id == listId, cancellationToken);

        if (list is null)
        {
            return;
        }

        if (!list.Location.StartsWith(HttpListPrefix))
        {
            await this.LoadFileBlocklistAsync(list.Ip, list.Location, allowlist: false, cancellationToken);
        }

        if (list.Location.StartsWith(HttpListPrefix))
        {
            await this.LoadUrlBlocklistAsync(dbContext, list.Id, list.Ip, list.Location, allowlist: false, cancellationToken);
        }

        list.LastRefreshed = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task LoadListsAsync(CancellationToken cancellationToken)
    {
        this.domainAllowlistService.Clear();
        this.domainBlocklistService.Clear();

        await using var scope = this.serviceScopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<NerveDbContext>();

        await this.loadListsSemaphore.WaitAsync(cancellationToken);

        _ = await dbContext.Domains
            .Where(d => d.Source == Domain.Domains.DomainSource.List)
            .ExecuteDeleteAsync(cancellationToken);

        try
        {
            var lists = await dbContext.Lists
                .ToListAsync(cancellationToken);
            
            this.logger.LogInformation("Loading {AllowlistsCount:n0} allowlists and {BlocklistsCount:n0} blocklists", lists.Count(l => l.Type == Domain.Lists.ListType.Allowlist), lists.Count(l => l.Type == Domain.Lists.ListType.Blocklist));

            foreach (var list in lists)
            {
                bool allowlist = list.Type == Domain.Lists.ListType.Allowlist;

                if (!list.Location.StartsWith(HttpListPrefix))
                {
                    await this.LoadFileBlocklistAsync(list.Ip, list.Location, allowlist, cancellationToken);
                }

                if (list.Location.StartsWith(HttpListPrefix))
                {
                    await this.LoadUrlBlocklistAsync(dbContext, list.Id, list.Ip, list.Location, allowlist, cancellationToken);
                }

                list.LastRefreshed = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var manualBlockedDomains = await dbContext.Domains
                .AsNoTracking()
                .Where(d => d.Source == Domain.Domains.DomainSource.Manual)
                .Where(d => d.Action == Domain.Domains.DomainAction.Block)
                .ToDictionaryAsync(d => d.Value, _ => string.Empty, cancellationToken);
                
            this.domainBlocklistService.Add(IPAddress.Any, manualBlockedDomains);

            var manualAllowedDomains = await dbContext.Domains
                .AsNoTracking()
                .Where(d => d.Source == Domain.Domains.DomainSource.Manual)
                .Where(d => d.Action == Domain.Domains.DomainAction.Allow)
                .Select(d => d.Value)
                .ToListAsync(cancellationToken);
                
            this.domainAllowlistService.Add(IPAddress.Any, manualAllowedDomains);

            this.logger.LogInformation("Total allowlist size is {TotalAllowlistSize:n0} domains and total blocklist size is {TotalBlocklistSize:n0} domains", this.domainAllowlistService.Size, this.domainBlocklistService.Size);
        }
        finally
        {
            this.loadListsSemaphore.Release();
        }
    }

    private async Task LoadFileBlocklistAsync(string ip, string path, bool allowlist, CancellationToken cancellationToken)
    {
        var hostsAndIps = new Dictionary<string, string>();

        if (hostsAndIps.Count == 0)
        {
            using var streamReader = new StreamReader(path, Encoding.UTF8);

            string? line;
            while ((line = await streamReader.ReadLineAsync(cancellationToken)) != null)
            {
                if (line.StartsWith(HostsCommentChar))
                {
                    continue;
                }

                (string ipParsed, string host) = ParseLine(line);
                hostsAndIps[host] = ipParsed;
            }

            this.logger.LogInformation("Loaded {Count:n0} {AllowedOrBlocked} domains from {Path}", hostsAndIps.Count, allowlist ? "allowed" : "blocked", path);
        }
        else
        {
            this.logger.LogInformation("Loaded {Count:n0} {AllowedOrBlocked} domains from cache for '{Path}'", hostsAndIps.Count, allowlist ? "allowed" : "blocked", path);
        }

        if (allowlist)
        {
            this.domainAllowlistService.Add(IPAddress.Parse(ip), hostsAndIps.Keys);
        }
        else
        {
            this.domainBlocklistService.Add(IPAddress.Parse(ip), hostsAndIps);
        }
    }

    private async Task LoadUrlBlocklistAsync(NerveDbContext dbContext, int listId, string ip, string url, bool allowlist, CancellationToken cancellationToken)
    {
        try
        {
            var hostsAndIps = new Dictionary<string, string>();

            using var httpClient = new HttpClient();
            httpClient.Timeout = HttpClientTimeout;
            // TODO: This should probably be somehow stream based but few hundred thousand lines are no problem
            string content = await httpClient.GetStringAsync(url, cancellationToken);
            string[] lines = content.Split('\n');
            IEnumerable<(string ip, string hostname)> ipAndHostnames = lines.Where(line => !line.StartsWith(HostsCommentChar)).Select(ParseLine);
            foreach ((string ipParsed, string hostname) in ipAndHostnames)
            {
                hostsAndIps[hostname] = ipParsed;
            }

            this.logger.LogInformation("Loaded {Count:n0} {AllowedOrBlocked} domains from '{Url}'", hostsAndIps.Count, allowlist ? "allowed" : "blocked", url);

            await this.AddDomains(dbContext, listId, ip, allowlist, hostsAndIps, cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            this.logger.LogError(exception, "Error while loading list from {Url}", url);
        }
    }

    private async Task AddDomains(NerveDbContext dbContext, int? listId, string ip, bool allowlist, Dictionary<string, string> domains, CancellationToken cancellationToken)
    {
        var ipAddress = IPAddress.Parse(ip);

        IEnumerable<string> newDomains;

        if (allowlist)
        {
            newDomains = this.domainAllowlistService.TryGet(ipAddress, out CompiledAllowlist? compiledAllowlist)
                ? domains.Keys.Except(compiledAllowlist.Allowlist).ToList()
                : domains.Keys;

            this.domainAllowlistService.Add(ipAddress, domains.Keys);
        }
        else
        {
            newDomains = this.domainBlocklistService.TryGet(ipAddress, out CompiledBlocklist? compiledBlocklist)
                ? domains.Keys.Except(compiledBlocklist.Blocklist.Keys).ToList()
                : domains.Keys;

            this.domainBlocklistService.Add(ipAddress, domains);
        }

        dbContext.Domains.AddRange(newDomains
            .Select(domain => new Domain.Domains.Domain
            {
                Id = 0,
                Action = allowlist ? Domain.Domains.DomainAction.Allow : Domain.Domains.DomainAction.Block,
                Source = listId is not null ? Domain.Domains.DomainSource.List : Domain.Domains.DomainSource.Manual,
                Value = domain,
                ListId = listId
            }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (string ip, string host) ParseLine(string line)
    {
        Match match = HostsPattern.Match(line);
        return match.Success
            ? (match.Groups["ip"].Value, match.Groups["host"].Value)
            : ("0.0.0.0", line);
    }
}
