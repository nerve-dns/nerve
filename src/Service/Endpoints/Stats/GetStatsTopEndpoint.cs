// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver;
using Nerve.Service.Domain;
using Nerve.Service.Responses.Stats;

namespace Nerve.Service.Endpoints.Stats;

public class GetStatsTopEndpoint : EndpointWithoutRequest<GetStatsTopResponse>
{
    private const int TopItemCount = 10;

    private readonly NerveDbContext nerveDbContext;

    public GetStatsTopEndpoint(NerveDbContext nerveDbContext)
        => this.nerveDbContext = nerveDbContext;

    public override void Configure()
    {
        Get("/api/stats/top");
        // TODO: Add authentication
        AllowAnonymous();
    }

    public override async Task<GetStatsTopResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        var topResolvedDomains = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topBlockedDomains = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status == Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topClients = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Status.Blocked)
            .GroupBy(query => query.Client)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        return new GetStatsTopResponse(
            topResolvedDomains.Select(domainAndCount => new DomainAndCountResponse(domainAndCount.Domain, domainAndCount.Count)).ToArray(),
            topBlockedDomains.Select(domainAndCount => new DomainAndCountResponse(domainAndCount.Domain, domainAndCount.Count)).ToArray(),
            topClients.Select(domainAndCount => new ClientAndCountResponse(domainAndCount.Domain, domainAndCount.Count)).ToArray());
    }
}
