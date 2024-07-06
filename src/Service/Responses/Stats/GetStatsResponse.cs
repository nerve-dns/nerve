// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Responses.Stats;

public sealed record GetStatsResponse(long TotalQueries, long TotalQueriesBlocked, float PercentageBlocked, long TotalBlocklistSize, long TotalAllowlistSize);
