// SPDX-FileCopyrightText: 2023 nerve-dns
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
