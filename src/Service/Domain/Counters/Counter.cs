// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Domain.Counters;

public sealed class Counter
{
    public int Id { get; set; }
    public long Value { get; set; }
}
