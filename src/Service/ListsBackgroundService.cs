// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Service;

public sealed partial class ListsBackgroundService : BackgroundService
{
    // TODO: Support multiple hosts?
    [GeneratedRegex("(?<ip>[0-9.]+)\\s+(?<host>[\\w.-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex HostsRegex();
    private static Regex HostsPattern = HostsRegex();

    private static TimeSpan HttpClientTimeout = TimeSpan.FromSeconds(5);

    private const char HostsCommentChar = '#';

    private readonly ILogger<ListsBackgroundService> logger;
    private readonly IOptionsMonitor<NerveOptions> nerveOptions;
    private readonly IDomainAllowlistService domainAllowlistService;
    private readonly IDomainBlocklistService domainBlocklistService;

    private readonly SemaphoreSlim loadListsSemaphore = new(1, 1);

    public ListsBackgroundService(
        IOptionsMonitor<NerveOptions> nerveOptions,
        ILogger<ListsBackgroundService> logger,
        IDomainAllowlistService domainAllowlistService,
        IDomainBlocklistService domainBlocklistService)
    {
        this.logger = logger;
        this.domainAllowlistService = domainAllowlistService;
        this.domainBlocklistService = domainBlocklistService;
        this.nerveOptions = nerveOptions;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: Some kind of caching or change tracking would be nicer?
        this.domainAllowlistService.Clear();
        this.domainBlocklistService.Clear();

        this.nerveOptions.OnChange(nerveOptions => _ = this.LoadListsAsync(nerveOptions, cancellationToken));

        while (!cancellationToken.IsCancellationRequested)
        {
            // TODO: This should probably only refresh the URL lists and not all..
            await this.LoadListsAsync(this.nerveOptions.CurrentValue, cancellationToken);

            await Task.Delay(TimeSpan.FromDays(1), cancellationToken);
        }
    }

    private async Task LoadListsAsync(NerveOptions nerveOptions, CancellationToken cancellationToken)
    {
        await this.loadListsSemaphore.WaitAsync(cancellationToken);

        try
        {
            this.logger.LogInformation("Loading {AllowlistsCount} allowlists and {BlocklistsCount} blocklists", nerveOptions.Allowlists.Count, nerveOptions.Blocklists.Count);

            foreach (Blocklist blocklist in nerveOptions.Blocklists)
            {
                foreach (string path in blocklist.Lists.Where(list => !list.StartsWith("http")))
                {
                    await this.LoadFileBlocklistAsync(blocklist, path, allowlist: false, cancellationToken);
                }

                foreach (string url in blocklist.Lists.Where(list => list.StartsWith("http")))
                {
                    await this.LoadUrlBlocklistAsync(blocklist, url, allowlist: false, cancellationToken);
                }
            }

            foreach (Blocklist blocklist in nerveOptions.Allowlists)
            {
                foreach (string path in blocklist.Lists.Where(list => !list.StartsWith("http")))
                {
                    await this.LoadFileBlocklistAsync(blocklist, path, allowlist: true, cancellationToken);
                }

                foreach (string url in blocklist.Lists.Where(list => list.StartsWith("http")))
                {
                    await this.LoadUrlBlocklistAsync(blocklist, url, allowlist: true, cancellationToken);
                }
            }

            this.logger.LogInformation("Total allowlist size is {TotalAllowlistSize} domains and total blocklist size is {TotalBlocklistSize} domains", this.domainAllowlistService.Size, this.domainBlocklistService.Size);
        }
        finally
        {
            this.loadListsSemaphore.Release();
        }
    }

    private async Task LoadFileBlocklistAsync(Blocklist blocklist, string path, bool allowlist, CancellationToken cancellationToken)
    {
        var hostsAndIps = new Dictionary<string, string>();

        using var streamReader = new StreamReader(path);

        string? line;
        while ((line = await streamReader.ReadLineAsync(cancellationToken)) != null)
        {
            if (line.StartsWith(HostsCommentChar))
            {
                continue;
            }

            (string ip, string host) = ParseLine(line);
            hostsAndIps[host] = ip;
        }

        this.logger.LogInformation("Loaded {Count} {AllowedOrBlocked} domains from {Path}", hostsAndIps.Count, allowlist ? "allowed" : "blocked", path);
        
        if (allowlist)
        {
            this.domainAllowlistService.Add(IPAddress.Parse(blocklist.Ip), hostsAndIps.Keys);
        }
        else
        {
            this.domainBlocklistService.Add(IPAddress.Parse(blocklist.Ip), hostsAndIps);
        }
    }

    private async Task LoadUrlBlocklistAsync(Blocklist blocklist, string url, bool allowlist, CancellationToken cancellationToken)
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
            foreach ((string ip, string hostname) in ipAndHostnames)
            {
                hostsAndIps[hostname] = ip;
            }

            if (allowlist)
            {
                this.domainAllowlistService.Add(IPAddress.Parse(blocklist.Ip), hostsAndIps.Keys);
            }
            else
            {
                this.domainBlocklistService.Add(IPAddress.Parse(blocklist.Ip), hostsAndIps);
            }

            this.logger.LogInformation("Loaded {Count} {AllowedOrBlocked} domains from {Url}", hostsAndIps.Count, allowlist ? "allowed" : "blocked", url);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            this.logger.LogError(exception, "Error while loading list from {Url}", url);
        }
    }

    private static (string ip, string host) ParseLine(string line)
    {
        Match match = HostsPattern.Match(line);
        return match.Success
            ? (match.Groups["ip"].Value, match.Groups["host"].Value)
            : ("0.0.0.0", line);
    }
}
