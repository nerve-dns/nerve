// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public interface IDnsClient
{
    Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default);
}
