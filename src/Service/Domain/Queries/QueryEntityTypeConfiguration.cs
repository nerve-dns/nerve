// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nerve.Service.Domain.Queries;

public class QueryEntityTypeConfiguration : IEntityTypeConfiguration<Query>
{
    public void Configure(EntityTypeBuilder<Query> builder)
    {
        builder.HasKey(query => query.Id);

        builder.HasIndex(query => query.Timestamp);
        builder.HasIndex(query => query.Client);
        builder.HasIndex(query => query.Domain);
        builder.HasIndex(query => query.Type);
        builder.HasIndex(query => query.Status);
    }
}
