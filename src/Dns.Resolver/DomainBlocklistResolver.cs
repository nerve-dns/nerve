// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Nerve.Dns.Resolver.Blocklist;

namespace Nerve.Dns.Resolver;

public sealed class DomainBlocklistResolver : ResolverBase
{
    private static readonly Message nxDomainMessage = new()
    {
        Header = new Header
        {
            ResponseCode = ResponseCode.NxDomain
        }
    };
    private readonly IDomainBlocklistService domainBlocklistService;

    public DomainBlocklistResolver(IDomainBlocklistService domainBlocklistService, IResolver? next = null)
        : base(null, next ?? throw new ArgumentNullException(nameof(next), "Blocklist resolver requires a next resolver"))
    {
        this.domainBlocklistService = domainBlocklistService;
    }

    public override async Task<(Message? message, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
    {
        if (this.domainBlocklistService.IsBlocked(remoteEndPoint.Address, question.Name, out string? ip))
        {
            // TODO: Use IP to override the resolved address for eg., A records
            return (nxDomainMessage, true, false);
        }

        return await base.next!.ResolveAsync(remoteEndPoint, question, cancellationToken);
    }
}
