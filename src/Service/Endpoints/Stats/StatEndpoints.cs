// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver;
using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Counters;
using Nerve.Service.Endpoints.Stats.Models;

namespace Nerve.Service.Endpoints.Stats;

public static class StatEndpoints
{
    private const int TopItemCount = 10;

    public static void AddStatEndpoints(this WebApplication app)
    {
        app.MapGet("/api/stats", GetStats);
        app.MapGet("/api/stats/top", GetStatsTop);
        app.MapGet("/api/stats/history", GetStatsHistory);
    }

    public static async Task<Results<Ok<GetStatsResponse>, BadRequest>> GetStats(
        NerveDbContext dbContext,
        IDomainBlocklistService domainBlocklistService,
        IDomainAllowlistService domainAllowlistService,
        CancellationToken cancellationToken)
    {
        Counter counterQueries = await dbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Queries, cancellationToken);
        Counter counterBlocked = await dbContext.Counters.AsNoTracking().FirstAsync(counter => counter.Id == (int)CounterType.Blocked, cancellationToken);

        var averageDurationTime = await dbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status == Dns.Resolver.Status.Forwarded)
            .OrderByDescending(query => query.Timestamp)
            .Take(1_000)
            .AverageAsync(query => (float?)query.Duration, cancellationToken);

        averageDurationTime ??= 0.0f;

        return TypedResults.Ok(new GetStatsResponse(
            counterQueries.Value,
            counterBlocked.Value,
            counterQueries.Value > 0 ? (float)Math.Round((float)counterBlocked.Value / counterQueries.Value * 100.0f, 2) : 0.0f,
            domainBlocklistService.Size,
            domainAllowlistService.Size,
            averageDurationTime.Value));
    }

    public static async Task<Results<Ok<GetStatsTopResponse>, BadRequest>> GetStatsTop(
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var topResolvedDomains = await dbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topBlockedDomains = await dbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status == Status.Blocked)
            .GroupBy(query => query.Domain)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        var topClients = await dbContext.Queries
            .AsNoTracking()
            .Where(query => query.Status != Status.Blocked)
            .GroupBy(query => query.Client)
            .Select(group => new { Domain = group.Key, Count = group.Count() })
            .OrderByDescending(domainAndCount => domainAndCount.Count)
            .Take(TopItemCount)
            .ToArrayAsync(cancellationToken);

        return TypedResults.Ok(new GetStatsTopResponse(
            [.. topResolvedDomains.Select(domainAndCount => new DomainAndCountResponse(domainAndCount.Domain, domainAndCount.Count))],
            [.. topBlockedDomains.Select(domainAndCount => new DomainAndCountResponse(domainAndCount.Domain, domainAndCount.Count))],
            [.. topClients.Select(domainAndCount => new ClientAndCountResponse(domainAndCount.Domain, domainAndCount.Count))]));
    }

    public static async Task<Results<Ok<GetStatsHistoryResponse>, BadRequest>> GetStatsHistory(
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var lastDay = DateTime.UtcNow.AddHours(-24);

        var lastDayQueries = await dbContext.Queries
            .AsNoTracking()
            .Where(x => x.Timestamp >= lastDay)
            .ToListAsync(cancellationToken);

        var lastDayQueryHistory = lastDayQueries.GroupBy(x => new DateTime(
                x.Timestamp.Year, x.Timestamp.Month, x.Timestamp.Day,
                x.Timestamp.Hour, 0, 0, DateTimeKind.Utc))
            .Select(g => new { Hour = g.Key, Group = g.CountBy(x => x.Status) })
            .OrderBy(x => x.Hour)
            .ToDictionary(x => x.Hour, x => x.Group.ToDictionary(x => x.Key, x => x.Value));

        var lastDayClientHistory = lastDayQueries.GroupBy(q => new DateTime(
                q.Timestamp.Year, q.Timestamp.Month, q.Timestamp.Day,
                q.Timestamp.Hour, 0, 0, DateTimeKind.Utc))
            .Select(g => new { Hour = g.Key, Group = g.CountBy(x => x.Client) })
            .OrderBy(x => x.Hour)
            .ToDictionary(x => x.Hour, x => x.Group.ToDictionary(x => x.Key, x => x.Value));

        return TypedResults.Ok(new GetStatsHistoryResponse(lastDayQueryHistory, lastDayClientHistory));
    }
}
