// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Dns.Client;

public sealed class RoundRobinHostProvider : IHostProvider
{
    private readonly string[] hosts;

    private int index;

    public RoundRobinHostProvider(string[] hosts)
    {
        this.hosts = hosts;
        this.index = 0;
    }

    public string Get()
    {
        int length = this.hosts.Length;

        int newIndex = Interlocked.Increment(ref this.index) - 1;
        
        int wrappedIndex = newIndex % length;
        
        if (wrappedIndex < 0)
        {
            wrappedIndex += length;
        }

        return this.hosts[wrappedIndex];
    }
}
