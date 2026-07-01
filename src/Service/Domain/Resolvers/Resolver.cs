// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Domain.Resolvers;

public sealed class Resolver
{
    public required int Id { get; init; }

    public required string Endpoint { get; init; }

    public required Protocol Protocol { get; init; } 
}
