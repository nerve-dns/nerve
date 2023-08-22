// SPDX-FileCopyrightText: 2023 nerve-dns
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
        if (index == ipEndPoints.Length)
        {
            index = 0;
        }

        return ipEndPoints[index++];
    }
}
