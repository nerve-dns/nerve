// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public sealed class SingleHostProvider : IHostProvider
{
    private readonly string host;

    public SingleHostProvider(string host)
        => this.host = host;

    public string Get()
        => this.host;
}
