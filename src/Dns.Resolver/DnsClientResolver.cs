// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Nerve.Dns.Client;

namespace Nerve.Dns.Resolver;

public sealed class DnsClientResolver : ResolverBase
{
    private readonly IDnsClient dnsClient;

    public DnsClientResolver(IDnsClient dnsClient, IResolver? next = null)
        : base(null, next)
    {
        this.dnsClient = dnsClient;
    }

    public override async Task<(Message? message, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
        => (await this.dnsClient.ResolveAsync(question, cancellationToken), false, false);
}
