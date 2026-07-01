// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns.Resolver;

namespace Nerve.Service.Endpoints.Stats.Models;

public sealed record GetStatsHistoryResponse(Dictionary<DateTime, Dictionary<Status, int>> QueryHistory, Dictionary<DateTime, Dictionary<string, int>> ClientHistory);
