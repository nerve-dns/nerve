// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Client;

public sealed class RoundRobinIpEndPointProvider : IIpEndPointProvider
{
    private readonly IPEndPoint[] ipEndPoints;
    private int index;

    public RoundRobinIpEndPointProvider(IPEndPoint[] ipEndPoints)
    {
        this.ipEndPoints = ipEndPoints;
        this.index = 0;
    }

    public IPEndPoint Get()
    {
        if (this.index == this.ipEndPoints.Length)
        {
            this.index = 0;
        }

        return this.ipEndPoints[this.index++];
    }
}
