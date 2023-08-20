// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver.Blocklist;

public sealed class DomainBlocklistService : IDomainBlocklistService
{
    private readonly Dictionary<IPAddress, CompiledBlocklist> blocklist = new();

    public void Add(IPAddress remoteIp, string domain, string ip)
    {
        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            compiledBlocklist.Add(domain, ip);
            return;
        }

        var newCompiledBlocklist = new CompiledBlocklist(new Dictionary<string, string>()
        {
            { domain, ip },
        });
        blocklist.Add(remoteIp, newCompiledBlocklist);
    }

    public void Add(IPAddress remoteIp, Dictionary<string, string> domainsAndIps)
    {
        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            compiledBlocklist.Add(domainsAndIps);
            return;
        }

        var newCompiledBlocklist = new CompiledBlocklist(domainsAndIps);
        this.blocklist.Add(remoteIp, newCompiledBlocklist);
    }

    public bool Remove(IPAddress remoteIp, string domain)
    {
        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            return compiledBlocklist.Remove(domain);
        }

        return false;
    }

    public bool Remove(IPAddress remoteIp)
        => this.blocklist.Remove(remoteIp);

    public bool TryGet(IPAddress remoteIp, out CompiledBlocklist? compiledBlocklist)
        => this.blocklist.TryGetValue(remoteIp, out compiledBlocklist);

    public bool IsBlocked(IPAddress remoteIp, string domain, out string? ip)
    {
        // Global blocklist
        if (this.blocklist.TryGetValue(IPAddress.Any, out var globalCompiledBlocklist))
        {
            return globalCompiledBlocklist.IsBlocked(domain, out ip);
        }

        // IP specific blocklist
        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            return compiledBlocklist.IsBlocked(domain, out ip);
        }

        ip = null;
        return false;
    }
}
