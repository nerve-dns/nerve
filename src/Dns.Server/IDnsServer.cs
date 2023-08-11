// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Server;

public interface IDnsServer
{
    Task StartAsync(CancellationToken cancellationToken = default);
}
