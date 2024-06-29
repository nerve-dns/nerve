// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Counters;
using Nerve.Service.Web.Controller.Stats.Dtos;

namespace Nerve.Service.Web.Controller.Stats;

[ApiController]
[Route("api/stats")]
public sealed class StatsController : ControllerBase
{
    private const int TopItemCount = 10;

    private readonly NerveDbContext nerveDbContext;
    private readonly IDomainBlocklistService domainBlocklistService;
    private readonly IDomainAllowlistService domainAllowlistService;

    public StatsController(
        NerveDbContext nerveDbContext,
        IDomainBlocklistService domainBlocklistService,
        IDomainAllowlistService domainAllowlistService)
    {
        this.nerveDbContext = nerveDbContext;
        this.domainBlocklistService = domainBlocklistService;
        this.domainAllowlistService = domainAllowlistService;
    }

    [HttpGet]
    public async Task<StatsDto> GetStatsAsync(CancellationToken cancellationToken)
    {
        Counter counterQueries = await this.nerveDbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Queries, cancellationToken);
        Counter counterBlocked = await this.nerveDbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Blocked, cancellationToken);

        return new StatsDto(
            counterQueries.Value,
            counterBlocked.Value,
            counterQueries.Value > 0 ? (float)Math.Round(((float)counterBlocked.Value / counterQueries.Value) * 100.0f, 2) : 0.0f,
            this.domainBlocklistService.Size,
            this.domainAllowlistService.Size);
    }

    [HttpGet("top")]
    public async Task<StatsTopDto> GetStatsTopAsync(CancellationToken cancellationToken)
    {
        var topResolvedDomains = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Dns.Resolver.Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topBlockedDomains = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status == Dns.Resolver.Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topClients = await this.nerveDbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Dns.Resolver.Status.Blocked)
            .GroupBy(query => query.Client)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        return new StatsTopDto(
            topResolvedDomains.Select(domainAndCount => new DomainAndCountDto(domainAndCount.Domain, domainAndCount.Count)).ToArray(),
            topBlockedDomains.Select(domainAndCount => new DomainAndCountDto(domainAndCount.Domain, domainAndCount.Count)).ToArray(),
            topClients.Select(domainAndCount => new ClientAndCountDto(domainAndCount.Domain, domainAndCount.Count)).ToArray());
    }
}
