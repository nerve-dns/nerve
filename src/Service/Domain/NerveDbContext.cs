// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain.Counters;
using Nerve.Service.Domain.Lists;
using Nerve.Service.Domain.Queries;
using Nerve.Service.Domain.Resolvers;

using DomainEntity = Nerve.Service.Domain.Domains.Domain;

namespace Nerve.Service.Domain;

public sealed class NerveDbContext : DbContext
{
    private readonly IConfiguration Configuration;

    public DbSet<Query> Queries { get; set; }

    public DbSet<Counter> Counters { get; set; }

    public DbSet<List> Lists { get; set; }

    public DbSet<DomainEntity> Domains { get; set; }

    public DbSet<Resolver> Resolvers { get; set; }

    public NerveDbContext(IConfiguration configuration)
        => this.Configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite(this.Configuration.GetConnectionString("NerveDatabase"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NerveDbContext).Assembly);

        modelBuilder.Entity<Counter>().HasData(
        [
            new() { Id = (int)CounterType.Queries, Value = 0 },
            new() { Id = (int)CounterType.Cached, Value = 0 },
            new() { Id = (int)CounterType.Blocked, Value = 0 }
        ]);

        modelBuilder.Entity<Resolver>().HasData(
        [
            new() { Id = 1, Endpoint = "https://unfiltered.joindns4.eu/dns-query", Protocol = Protocol.Https },
            new() { Id = 2, Endpoint = "https://dns.quad9.net/dns-query", Protocol = Protocol.Https }
        ]);

        base.OnModelCreating(modelBuilder);
    }
}
