// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver.Allowlist;

public sealed class CompiledAllowlist
{
    private readonly HashSet<string> allowlist = new();

    public HashSet<string> Allowlist => this.allowlist;

    public CompiledAllowlist(HashSet<string> allowlist)
        => this.allowlist = allowlist;

    public CompiledAllowlist(IEnumerable<string> allowlist)
        => this.Add(allowlist);

    public bool IsAllowed(string domain)
        => this.allowlist.Contains(domain);

    public void Add(string domain)
        => this.allowlist.Add(domain);

    public void Add(IEnumerable<string> domains)
    {
        foreach (var domain in domains)
        {
            this.allowlist.Add(domain);
        }
    }

    public bool Remove(string domain)
        => this.allowlist.Remove(domain);
}
