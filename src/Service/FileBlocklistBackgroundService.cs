// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Options;

using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Service;

public sealed partial class FileBlocklistBackgroundService : BackgroundService
{
    // TODO: Support multiple hosts?
    [GeneratedRegex("(?<ip>[0-9.]+)\\s+(?<host>[\\w.-]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex HostsRegex();

    private static Regex HostsPattern = HostsRegex();

    private readonly ILogger<FileBlocklistBackgroundService> logger;
    private readonly NerveOptions nerveOptions;
    private readonly IDomainBlocklistService domainBlocklistService;

    public FileBlocklistBackgroundService(
        IOptions<NerveOptions> nerveOptions,
        ILogger<FileBlocklistBackgroundService> logger,
        IDomainBlocklistService domainBlocklistService)
    {
        this.logger = logger;
        this.domainBlocklistService = domainBlocklistService;
        this.nerveOptions = nerveOptions.Value;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (this.nerveOptions.Blocklists == null)
        {
            return Task.CompletedTask;
        }
        
        foreach (Blocklist blocklist in this.nerveOptions.Blocklists)
        {
            foreach (string path in blocklist.Lists.Where(list => !list.StartsWith("http")))
            {
                this.LoadFileBlocklist(blocklist, path);
            }
        }

        return Task.CompletedTask;
    }

    private void LoadFileBlocklist(Blocklist blocklist, string path)
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

        this.logger.LogDebug("Loaded {Count} domains from {Path}", hostsAndIps.Count, path);
        this.domainBlocklistService.Add(IPAddress.Parse(blocklist.Ip), hostsAndIps);
    }

    private static (string ip, string host) ParseLine(string line)
    {
        Match match = HostsPattern.Match(line);
        return match.Success
            ? (match.Groups["ip"].Value, match.Groups["host"].Value)
            : ("0.0.0.0", line);
    }
}
