// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Domain.Lists;
using Nerve.Service.Endpoints.Lists.Models;

using ListTypeModel = Nerve.Service.Endpoints.Lists.Models.ListType;
using ListTypeDomain = Nerve.Service.Domain.Lists.ListType;

namespace Nerve.Service.Endpoints.Lists;

public static class ListEndpoints
{
    public static void AddListEndpoints(this WebApplication app)
    {
        app.MapGet("/api/lists", GetListsAsync);
        app.MapPost("/api/lists/refresh", RefreshListsAsync);
        app.MapDelete("/api/lists/{id}", DeleteListAsync);
        app.MapPost("/api/lists/{ip}", AddListAsync);
        app.MapPost("/api/lists/{ip}/{listId}/refresh", RefreshListAsync);
    }

    public static async Task<Results<Ok<ListResponse[]>, BadRequest>> GetListsAsync(
        [FromServices] NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        List<List> lists = await dbContext.Lists
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var listDomainCount = await dbContext.Domains
            .AsNoTracking()
            .Where(d => d.ListId != null)
            .GroupBy(x => x.ListId)
            .ToDictionaryAsync(x => x.Key!.Value, x => x.Count(), cancellationToken);

        return TypedResults.Ok<ListResponse[]>([.. lists.Select(list =>
        {
            var domainCount = listDomainCount.TryGetValue(list.Id, out int value)
                ? value
                : 0;

            return new ListResponse(list.Id, list.Ip, list.Location, (ListTypeModel)list.Type, list.LastRefreshed, domainCount);
        })]);
    }

    public static async Task<Results<Ok, BadRequest>> RefreshListsAsync(
        [FromServices] IListService listService,
        CancellationToken cancellationToken)
    {
        await listService.LoadListsAsync(cancellationToken);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, BadRequest>> AddListAsync(
        string ip,
        AddListRequest addBlocklistListRequest,
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (addBlocklistListRequest.Ip != ip)
        {
            return TypedResults.BadRequest();
        }

        dbContext.Lists.Add(new List
        {
            Id = 0,
            Type = (ListTypeDomain)addBlocklistListRequest.Type,
            Ip = addBlocklistListRequest.Ip,
            Location = addBlocklistListRequest.Location,
            LastRefreshed = null
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, NotFound, BadRequest>> DeleteListAsync(
        int id,
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var list = await dbContext.Lists.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

        if (list is null)
        {
            return TypedResults.NotFound();
        }

        _ = await dbContext.Domains
            .Where(d => d.ListId == id)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Lists.Remove(list);

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, BadRequest>> RefreshListAsync(
        [FromRoute] string ip,
        [FromRoute] int listId,
        [FromServices] IListService listService,
        CancellationToken cancellationToken)
    {
        await listService.RefreshAsync(listId, cancellationToken);

        return TypedResults.Ok();
    }
}
