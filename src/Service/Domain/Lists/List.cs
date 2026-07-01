// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Domain.Lists;

public sealed class List
{
    public required int Id { get; init; }

    public required ListType Type { get; init; }

    public required string Ip { get; init; }

    public required string Location { get; init; }

    public DateTime? LastRefreshed { get; set; }
}
