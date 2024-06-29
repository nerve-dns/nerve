// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Web.Controller.Stats.Dtos;

public sealed record DomainAndCountDto(string Domain, long Count);
public sealed record ClientAndCountDto(string Client, long Count);
public sealed record StatsTopDto(DomainAndCountDto[] TopAllowedDomains, DomainAndCountDto[] TopBlockedDomains, ClientAndCountDto[] TopClients);
