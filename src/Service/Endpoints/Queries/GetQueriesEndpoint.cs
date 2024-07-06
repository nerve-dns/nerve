// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Domain.Queries;
using Nerve.Service.Responses.Queries;

namespace Nerve.Service.Endpoints.Queries;

public class GetQueriesEndpoint : EndpointWithoutRequest<GetQueriesResponse>
{
    private const int MaxPerPage = 10;

    private readonly NerveDbContext nerveDbContext;

    public GetQueriesEndpoint(NerveDbContext nerveDbContext)
        => this.nerveDbContext = nerveDbContext;

    public override void Configure()
    {
        Get("/api/queries");
        // TODO: Add authentication
        AllowAnonymous();
    }

    public override async Task<GetQueriesResponse> ExecuteAsync(CancellationToken cancellationToken)
    {
        int page = Math.Max(1, base.Query<int>("page", isRequired: true));

        IQueryable<Query> query = nerveDbContext
            .Queries
            .AsNoTracking();

        string? input = base.Query<string?>("input", isRequired: false);

        if (input is not null)
        {
            query = query.Where(query => query.Domain.StartsWith(input) || query.Client.StartsWith(input));
        }

        List<Query> queries = await query
            .OrderByDescending(query => query.Timestamp)
            .Skip((page - 1) * MaxPerPage)
            .Take(MaxPerPage)
            .ToListAsync(cancellationToken);

        QueryDto[] queryDtos = queries.Select(query => new QueryDto(query.Id, query.Timestamp, query.Client, query.Type, query.Domain, query.ResponseCode, query.Duration, query.Status))
            .ToArray();

        return new GetQueriesResponse(queryDtos);
    }
}
