// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver;

public interface IQueryLogger
{
    Task LogAsync(DateTime timestampUtc, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken);
}
