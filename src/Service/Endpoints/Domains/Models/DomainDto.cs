// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Domains.Models;

public sealed record DomainsDto(int TotalDomains, DomainDto[] Domains);

public sealed record DomainDto(int Id, DomainActionDto Action, DomainSourceDto Source, string Value);

public enum DomainActionDto : byte
{
    Allow = 0,
    Block = 1
}

public enum DomainSourceDto : byte
{
    List = 0,
    Manual = 1
}
