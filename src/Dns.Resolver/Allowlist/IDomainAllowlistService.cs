// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver.Allowlist;

public interface IDomainAllowlistService
{
    long Size { get; }

    void Add(IPAddress remoteIp, string domain);
    void Add(IPAddress remoteIp, IEnumerable<string> domains);
    bool Remove(IPAddress remoteIp, string domain);
    bool Remove(IPAddress remoteIp);
    bool TryGet(IPAddress remoteIp, out CompiledAllowlist? compiledAllowlist);
    bool IsAllowed(IPAddress remoteIp, string domain);
    void Clear();
}
