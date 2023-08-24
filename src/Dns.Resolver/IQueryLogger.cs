// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Resolver;

public interface IQueryLogger
{
    Task LogAsync(long timestamp, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken);
}
