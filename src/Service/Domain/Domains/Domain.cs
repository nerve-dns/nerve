// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using DomainList = Nerve.Service.Domain.Lists.List;

namespace Nerve.Service.Domain.Domains;

public sealed class Domain
{
    public required int Id { get; init; }

    public required DomainAction Action { get; init; }

    public required DomainSource Source  { get; init; }

    public required string Value { get; init; }

    public int? ListId { get; init; }

    public DomainList? List { get; init; }
}
