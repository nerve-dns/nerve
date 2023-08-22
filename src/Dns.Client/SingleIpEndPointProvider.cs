// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Client;

public sealed class SingleIpEndPointProvider : IIpEndPointProvider
{
    private readonly IPEndPoint ipEndpoint;

    public SingleIpEndPointProvider(IPEndPoint endpointPoint)
        => this.ipEndpoint = endpointPoint;

    public IPEndPoint Get()
        => this.ipEndpoint;
}
