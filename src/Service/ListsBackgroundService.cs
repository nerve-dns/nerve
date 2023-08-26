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

    private readonly ILogger<ListsBackgroundService> logger;
    private readonly IOptionsMonitor<NerveOptions> nerveOptions;
    private readonly IDomainAllowlistService domainAllowlistService;
    private readonly IDomainBlocklistService domainBlocklistService;

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

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        // TODO: Some kind of caching or change tracking would be nicer?
        this.domainAllowlistService.Clear();
        this.domainBlocklistService.Clear();

        this.nerveOptions.OnChange(this.LoadLists);

        this.LoadLists(this.nerveOptions.CurrentValue);

        return Task.CompletedTask;
    }

    private void LoadLists(NerveOptions nerveOptions)
    {
        this.logger.LogInformation("Loading {AllowlistsCount} allowlists and {BlocklistsCount} blocklists", nerveOptions.Allowlists.Count, nerveOptions.Blocklists.Count);

        foreach (Blocklist blocklist in nerveOptions.Blocklists)
        {
            foreach (string path in blocklist.Lists.Where(list => !list.StartsWith("http")))
            {
                this.LoadFileBlocklist(blocklist, path, allowlist: false);
            }
        }

        foreach (Blocklist blocklist in nerveOptions.Allowlists)
        {
            foreach (string path in blocklist.Lists.Where(list => !list.StartsWith("http")))
            {
                this.LoadFileBlocklist(blocklist, path, allowlist: true);
            }
        }
    }

    private void LoadFileBlocklist(Blocklist blocklist, string path, bool allowlist)
    {
        var hostsAndIps = new Dictionary<string, string>();

        using var streamReader = new StreamReader(path);

        string? line;
        while ((line = streamReader.ReadLine()) != null)
        {
            if (line.StartsWith("#"))
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

    private static (string ip, string host) ParseLine(string line)
    {
        Match match = HostsPattern.Match(line);
        return match.Success
            ? (match.Groups["ip"].Value, match.Groups["host"].Value)
            : ("0.0.0.0", line);
    }
}
