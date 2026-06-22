// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Stats.Models;

public sealed record GetStatsResponse(long TotalQueries, long TotalQueriesBlocked, float PercentageBlocked, long TotalBlocklistSize, long TotalAllowlistSize, float AverageForwardingDuration);
