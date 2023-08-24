// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver.Blocklist;

public sealed class CompiledBlocklist
{
    private readonly Dictionary<string, string> blocklist;

    public Dictionary<string, string> Blocklist => blocklist;

    public CompiledBlocklist(Dictionary<string, string> blocklist)
        => this.blocklist = blocklist;

    public bool IsBlocked(string domain, out string? ip)
        => this.blocklist.TryGetValue(domain, out ip);

    public void Add(string domain, string ip)
        => this.blocklist.Add(domain, ip);

    public void Add(Dictionary<string, string> domainsAndIps)
    {
        foreach (var domainAndIp in domainsAndIps)
        {
            this.blocklist[domainAndIp.Key] = domainAndIp.Value;
        }
    }

    public bool Remove(string domain)
        => this.blocklist.Remove(domain);
}
