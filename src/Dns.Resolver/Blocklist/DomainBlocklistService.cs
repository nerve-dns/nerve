// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver.Blocklist;

public sealed class DomainBlocklistService : IDomainBlocklistService
{
    private readonly Dictionary<IPAddress, CompiledBlocklist> blocklist = new();
    private long size = 0;

    public long Size => this.size;

    public void Add(IPAddress remoteIp, string domain, string ip)
    {
        this.size += 1;

        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            compiledBlocklist.Add(domain, ip);
            return;
        }

        var newCompiledBlocklist = new CompiledBlocklist(new Dictionary<string, string>()
        {
            { domain, ip },
        });
        this.blocklist.Add(remoteIp, newCompiledBlocklist);
    }

    public void Add(IPAddress remoteIp, Dictionary<string, string> domainsAndIps)
    {
        this.size += domainsAndIps.Count;

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
        this.size -= 1;

        if (this.blocklist.TryGetValue(remoteIp, out var compiledBlocklist))
        {
            return compiledBlocklist.Remove(domain);
        }

        return false;
    }

    public bool Remove(IPAddress remoteIp)
    {
        if (this.blocklist.TryGetValue(remoteIp, out CompiledBlocklist? compiledBlocklist))
        {
            this.size -= compiledBlocklist.Blocklist.Count;
            return this.blocklist.Remove(remoteIp);
        }
        return false;
    }

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
