// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Responses.Stats;

public sealed record DomainAndCountResponse(string Domain, long Count);
public sealed record ClientAndCountResponse(string Client, long Count);
public sealed record GetStatsTopResponse(DomainAndCountResponse[] TopAllowedDomains, DomainAndCountResponse[] TopBlockedDomains, ClientAndCountResponse[] TopClients);
