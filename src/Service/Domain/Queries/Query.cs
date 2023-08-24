// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns;
using Nerve.Dns.Resolver;
using Type = Nerve.Dns.Type;

namespace Nerve.Service.Domain.Queries;

public sealed class Query
{
    public int Id { get; set; }
    public long Timestamp { get; set; }
    public string Client { get; set; } = null!;
    public Type Type { get; set; }
    public string Domain { get; set; } = null!;
    public ResponseCode ResponseCode { get; set; }
    public float Duration { get; set; }
    public Status Status { get; set; }
}
