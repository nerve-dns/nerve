// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nerve.Service.Domain.Resolvers;

public sealed class ResolverEntityTypeConfiguration : IEntityTypeConfiguration<Resolver>
{
    public void Configure(EntityTypeBuilder<Resolver> builder)
    {
        builder.HasKey(query => query.Id);

        builder.HasIndex(query => query.Endpoint);
        builder.HasIndex(query => query.Protocol);
    }
}
