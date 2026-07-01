// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nerve.Service.Domain.Domains;

public sealed class DomainEntityTypeConfiguration : IEntityTypeConfiguration<Domain>
{
    public void Configure(EntityTypeBuilder<Domain> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.Action);
        builder.HasIndex(d => d.Source);

        builder.HasIndex(d => d.Value)
            .IsUnique();

        builder.HasOne(d => d.List);
    }
}
