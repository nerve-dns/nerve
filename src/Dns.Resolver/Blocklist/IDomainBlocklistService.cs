// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver.Blocklist;

public interface IDomainBlocklistService
{
    long Size { get; }

    void Add(IPAddress remoteIp, string domain, string ip);
    void Add(IPAddress remoteIp, Dictionary<string, string> domainsAndIps);
    bool Remove(IPAddress remoteIp, string domain);
    bool Remove(IPAddress remoteIp);
    bool TryGet(IPAddress remoteIp, out CompiledBlocklist? compiledBlocklist);
    bool IsBlocked(IPAddress remoteIp, string domain, out string? ip);
}
