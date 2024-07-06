// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns;
using Nerve.Dns.Resolver;
using Type = Nerve.Dns.Type;

namespace Nerve.Service.Responses.Queries;

public sealed record GetQueriesResponse(QueryDto[] Queries);
public sealed record QueryDto(int Id, long Timestamp, string Client, Type Type, string Domain, ResponseCode ResponseCode, float Duration, Status Status);
