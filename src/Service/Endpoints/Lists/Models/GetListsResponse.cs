// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Lists.Models;

public sealed record GetListsResponse(ListResponse[] Lists);

public sealed record ListResponse(int Id, string Ip, string Location, ListType Type, DateTime? LastRefreshed, int DomainCount);
