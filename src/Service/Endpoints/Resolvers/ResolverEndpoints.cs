// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Endpoints.Resolvers.Models;
using Nerve.Service.Domain.Resolvers;

namespace Nerve.Service.Endpoints.Resolvers;

public static class ResolverEndpoints
{
    public static void AddResolverEndpoints(this WebApplication app)
    {
        app.MapGet("/api/resolvers", GetResolversAsync);
        app.MapPost("/api/resolvers", AddResolverAsync);
        app.MapDelete("/api/resolvers/{id}", DeleteResolverAsync);
    }

    public static async Task<Results<Ok<GetResolversResponse>, BadRequest>> GetResolversAsync(
        [FromServices] NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var resolvers = await dbContext.Resolvers
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = resolvers.Select(r => new ResolverDto(r.Id, r.Endpoint, (ProtocolDto)r.Protocol)).ToArray();
        return TypedResults.Ok(new GetResolversResponse(dtos));
    }

    public static async Task<Ok> AddResolverAsync(
        AddResolverRequest addResolverRequest,
        [FromServices] NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        dbContext.Resolvers.Add(new Resolver
        {
            Id = 0,
            Endpoint = addResolverRequest.Endpoint,
            Protocol = (Protocol)addResolverRequest.Protocol
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, NotFound>> DeleteResolverAsync(
        [FromRoute] int id,
        [FromServices] NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        int deleted = await dbContext.Resolvers
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted == 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok();
    }
}
