// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Endpoints.Resolvers.Models;

public sealed record AddResolverRequest(string Endpoint, ProtocolDto Protocol);
