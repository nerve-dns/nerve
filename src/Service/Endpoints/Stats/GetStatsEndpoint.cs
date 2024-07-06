// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Counters;
using Nerve.Service.Responses.Stats;

namespace Nerve.Service.Endpoints.Stats;

public class GetStatsEndpoint : EndpointWithoutRequest<GetStatsResponse>
{
    private readonly NerveDbContext nerveDbContext;
    private readonly IDomainBlocklistService domainBlocklistService;
    private readonly IDomainAllowlistService domainAllowlistService;

    public GetStatsEndpoint(
        NerveDbContext nerveDbContext,
        IDomainBlocklistService domainBlocklistService,
        IDomainAllowlistService domainAllowlistService)
    {
        this.nerveDbContext = nerveDbContext;
        this.domainBlocklistService = domainBlocklistService;
        this.domainAllowlistService = domainAllowlistService;
    }

    public override void Configure()
    {
        Get("/api/stats");
        // TODO: Add authentication
        AllowAnonymous();
    }

    public override async Task<GetStatsResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        Counter counterQueries = await this.nerveDbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Queries, cancellationToken);
        Counter counterBlocked = await this.nerveDbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Blocked, cancellationToken);

        return new GetStatsResponse(
            counterQueries.Value,
            counterBlocked.Value,
            counterQueries.Value > 0 ? (float)Math.Round(((float)counterBlocked.Value / counterQueries.Value) * 100.0f, 2) : 0.0f,
            this.domainBlocklistService.Size,
            this.domainAllowlistService.Size);
    }
}
