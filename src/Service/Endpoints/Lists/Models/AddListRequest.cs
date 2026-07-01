// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Lists.Models;

public sealed record AddListRequest(string Ip, string Location, ListType Type);

public enum ListType
{
    Allowlist = 0,
    Blocklist = 1
}
