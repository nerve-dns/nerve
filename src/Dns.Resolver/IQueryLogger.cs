// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver;

public interface IQueryLogger
{
    Task LogAsync(long timestamp, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken);
}
