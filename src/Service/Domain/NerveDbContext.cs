// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain.Counters;
using Nerve.Service.Domain.Queries;

namespace Nerve.Service.Domain;

public class NerveDbContext : DbContext
{
    private readonly IConfiguration Configuration;

    public DbSet<Query> Queries { get; set; }
    public DbSet<Counter> Counters { get; set; }

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

        base.OnModelCreating(modelBuilder);
    }
}
