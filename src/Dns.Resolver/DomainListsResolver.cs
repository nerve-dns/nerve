// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Metrics;

namespace Nerve.Dns.Resolver;

public sealed class DomainListsResolver : ResolverBase
{
    private static readonly Message nxDomainMessage = new()
    {
        Header = new Header
        {
            ResponseCode = ResponseCode.NxDomain
        }
    };

    private readonly IDomainAllowlistService domainAllowlistService;
    private readonly IDomainBlocklistService domainBlocklistService;
    private readonly NerveMetrics? nerveMetrics;

    public DomainListsResolver(IDomainAllowlistService domainAllowlistService, IDomainBlocklistService domainBlocklistService, NerveMetrics? nerveMetrics = null, IResolver? next = null)
        : base(null, next ?? throw new ArgumentNullException(nameof(next), "Domain lists resolver requires a next resolver"))
    {
        this.domainAllowlistService = domainAllowlistService;
        this.domainBlocklistService = domainBlocklistService;
        this.nerveMetrics = nerveMetrics;
    }

    public override async Task<(Message? message, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
    {
        // Allowlist has precedence over blocklist
        if (this.domainAllowlistService.IsAllowed(remoteEndPoint.Address, question.Name))
        {
            return await base.next!.ResolveAsync(remoteEndPoint, question, cancellationToken);
        }

        if (this.domainBlocklistService.IsBlocked(remoteEndPoint.Address, question.Name, out string? ip))
        {
            this.nerveMetrics?.AddRequestBlocked();
            // TODO: Use IP to override the resolved address for eg., A records
            return (nxDomainMessage, blocked: true, cached: false);
        }

        return await base.next!.ResolveAsync(remoteEndPoint, question, cancellationToken);
    }
}
