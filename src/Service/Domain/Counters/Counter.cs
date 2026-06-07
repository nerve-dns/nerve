// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Domain.Counters;

public sealed class Counter
{
    public int Id { get; set; }
    public long Value { get; set; }
}
