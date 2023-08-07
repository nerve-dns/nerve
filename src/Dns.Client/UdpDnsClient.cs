// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Client;

public class UdpDnsClient : IDnsClient
{
    private readonly IIpEndPointProvider ipEndPointProvider;
    private readonly Random random;

    public UdpDnsClient(IIpEndPointProvider ipEndPointProvider)
    {
        this.ipEndPointProvider = ipEndPointProvider;
        this.random = new Random();
    }

    public UdpDnsClient(IPAddress ipAddress)
        : this(new IPEndPoint(ipAddress, 53))
    {
        
    }

    public UdpDnsClient(IPEndPoint serverEndPoint)
        : this(new SingleIpEndPointProvider(serverEndPoint))
    {

    }

    public UdpDnsClient(params IPEndPoint[] ipEndPoints)
        : this(new RoundRobinIpEndPointProvider(ipEndPoints))
    {

    }

    public Task<DnsResponse> ResolveAsync(Question question, CancellationToken cancellationToken = default)
    {
        var header = new Header
        {
            Id = (ushort)random.Next(ushort.MaxValue),
            Flags = new Flags
            {
                QueryResponse = false,
                OpCode = OpCode.Query,
                AuthoritativeAnswer = false,
                Truncation = false,
                RecursionDesired = true,
                RecursionAvailable = false,
                Zero = false,
                ResponseCode = ResponseCode.NoError
            },
            QuestionCount = 1,
            AnswerCount = 0,
            AdditionalCount = 0,
            AuthorityCount = 0
        };

        IPEndPoint iPEndPoint = ipEndPointProvider.Get();

        return Task.FromResult(new DnsResponse
        {
            ResponseCode = ResponseCode.NoError
        });
    }
}
