// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Stats.Models;

public sealed record DomainAndCountResponse(string Domain, long Count);

public sealed record ClientAndCountResponse(string Client, long Count);

public sealed record GetStatsTopResponse(DomainAndCountResponse[] TopAllowedDomains, DomainAndCountResponse[] TopBlockedDomains, ClientAndCountResponse[] TopClients);
