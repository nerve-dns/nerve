// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public sealed class RoundRobinUriProvider : IUriProvider
{
    private readonly Uri[] uris;
    private int index;

    public RoundRobinUriProvider(Uri[] uris)
    {
        this.uris = uris;
        this.index = 0;
    }

    public Uri Get()
    {
        if (index == uris.Length)
        {
            index = 0;
        }

        return uris[index++];
    }
}
