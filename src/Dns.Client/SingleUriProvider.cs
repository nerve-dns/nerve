// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public sealed class SingleUriProvider : IUriProvider
{
    private readonly Uri uri;

    public SingleUriProvider(Uri uri)
        => this.uri = uri;

    public Uri Get()
        => this.uri;
}
