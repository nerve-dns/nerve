// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Domains;
using Nerve.Service.Endpoints.Domains.Models;

using DomainEntity = Nerve.Service.Domain.Domains.Domain;

namespace Nerve.Service.Endpoints.Domains;

public static class DomainEndpoints
{
    private const int MaxPerPage = 10;

    public static void AddDomainEndpoints(this WebApplication app)
    {
        app.MapGet("/api/domains", GetDomainsAsync);
        app.MapPost("/api/domains", AddDomainAsync);
        app.MapDelete("/api/domains/{id}", DeleteDomainAsync);
    }

    public static async Task<Results<Ok<DomainsDto>, BadRequest>> GetDomainsAsync(
        string? searchDomain,
        int? page,
        NerveDbContext dbContext,
        CancellationToken cancellationToken)
    {
        int cappedPage = Math.Max(1, page ?? 1);

        IQueryable<DomainEntity> query = dbContext
            .Domains
            .AsNoTracking();

        if (searchDomain is not null)
        {
            query = query.Where(query => query.Value.Contains(searchDomain));
        }

        List<DomainEntity> blocklists = await query
            .Skip((cappedPage - 1) * MaxPerPage)
            .Take(MaxPerPage)
            .ToListAsync(cancellationToken);

        var totalDomains = await query.CountAsync(cancellationToken);

        return TypedResults.Ok(new DomainsDto(totalDomains, [.. blocklists.Select(domain => new DomainDto(domain.Id, (DomainActionDto)domain.Action, (DomainSourceDto)domain.Source, domain.Value))]));
    }

    public static async Task<Results<Ok, BadRequest>> AddDomainAsync(
        DomainDto domainDto,
        [FromServices] NerveDbContext dbContext,
        [FromServices] IDomainBlocklistService domainBlocklistService,
        [FromServices] IDomainAllowlistService domainAllowlistService,
        CancellationToken cancellationToken)
    {
        dbContext.Domains.Add(new DomainEntity
        {
            Id = 0,
            Action = (DomainAction)domainDto.Action,
            Source = DomainSource.Manual,
            Value = domainDto.Value
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        if (domainDto.Action == DomainActionDto.Block)
        {
            domainBlocklistService.Add(IPAddress.Any, domainDto.Value, "0.0.0.0");
        }
        else
        {
            domainAllowlistService.Add(IPAddress.Any, domainDto.Value);
        }

        return TypedResults.Ok();
    }

    public static async Task<Results<Ok, NotFound>> DeleteDomainAsync(
        int id,
        [FromServices] NerveDbContext dbContext,
        [FromServices] IDomainBlocklistService domainBlocklistService,
        [FromServices] IDomainAllowlistService domainAllowlistService,
        CancellationToken cancellationToken)
    {
        var domain = await dbContext.Domains.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (domain is null)
        {
            return TypedResults.NotFound();
        }

        dbContext.Domains.Remove(domain);

        await dbContext.SaveChangesAsync(cancellationToken);

        if (domain.Action == DomainAction.Block)
        {
            domainBlocklistService.Remove(IPAddress.Any, domain.Value);
        }
        else
        {
            domainAllowlistService.Remove(IPAddress.Any, domain.Value);
        }

        return TypedResults.Ok();
    }
}
