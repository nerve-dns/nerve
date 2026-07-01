// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nerve.Service.Domain.Lists;

public sealed class ListEntityTypeConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.HasKey(query => query.Id);

        builder.HasIndex(query => query.Ip)
            .IsUnique(false);

        builder.HasIndex(query => query.Location)
            .IsUnique(false);
    }
}
