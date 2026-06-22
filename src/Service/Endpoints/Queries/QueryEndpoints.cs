// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Domain.Queries;
using Nerve.Service.Endpoints.Queries.Models;

namespace Nerve.Service.Endpoints.Queries;

public static class QueryEndpoints
{
    private const int MaxPerPage = 10;

    public static void AddQueryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/queries", GetQueries);
    }

    public static async Task<Results<Ok<QueryDto[]>, BadRequest>> GetQueries(
        int? page,
        string? input,
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        int cappedPage = Math.Max(1, page ?? 1);

        IQueryable<Query> query = dbContext
            .Queries
            .AsNoTracking();

        if (input is not null)
        {
            query = query.Where(query => query.Domain.StartsWith(input) || query.Client.StartsWith(input));
        }

        List<Query> queries = await query
            .OrderByDescending(query => query.Timestamp)
            .Skip((cappedPage - 1) * MaxPerPage)
            .Take(MaxPerPage)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok<QueryDto[]>([.. queries.Select(query => new QueryDto(query.Id, query.Timestamp, query.Client, query.Type, query.Domain, query.ResponseCode, query.Duration, query.Status))]);
    }
}
