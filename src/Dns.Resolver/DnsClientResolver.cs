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
        : base(next)
    {
        this.dnsClient = dnsClient;
    }

    public override async Task<Message?> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
        => await this.dnsClient.ResolveAsync(question, cancellationToken);
}
