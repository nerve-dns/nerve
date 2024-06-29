// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Web.Controller.Stats.Dtos;

public sealed record StatsDto(long TotalQueries, long TotalQueriesBlocked, float PercentageBlocked, long TotalBlocklistSize, long TotalAllowlistSize);
