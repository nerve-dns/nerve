// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
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
        int length = this.uris.Length;

        int newIndex = Interlocked.Increment(ref this.index) - 1;
        
        int wrappedIndex = newIndex % length;
        
        if (wrappedIndex < 0)
        {
            wrappedIndex += length;
        }

        return this.uris[wrappedIndex];
    }
}
