// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace Nerve.Dns.Client;

public class UdpDnsClient : IDnsClient
{
    private const int MaxUdpDatagramSize = 512;

    private readonly IIpEndPointProvider ipEndPointProvider;
    private readonly UdpClient udpClient;
    private readonly Random random;

    public UdpDnsClient(IIpEndPointProvider ipEndPointProvider)
    {
        this.ipEndPointProvider = ipEndPointProvider;
        this.udpClient = new UdpClient(port: 0);
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

    /// <summary>
    /// Resolves the given question and returns the response <see cref="Message"/>. 
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="cancellationToken">The optional cancellation token.</param>
    /// <returns></returns>
    public async Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default)
    {
        var requestMessage = new Message
        {
            Header = new Header
            {
                Id = (ushort)random.Next(ushort.MaxValue),
                Flags = new Flags
                {
                    QueryResponse = false,
                    OpCode = OpCode.Query,
                    AuthoritativeAnswer = false,
                    Truncation = false,
                    RecursionDesired = true,
                    RecursionAvailable = true,
                    Zero = false,
                    ResponseCode = ResponseCode.NoError
                },
                QuestionCount = 1,
                AnswerCount = 0,
                AuthorityCount = 0,
                AdditionalCount = 0
            }
        };
        requestMessage.Questions.Add(question);

        byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxUdpDatagramSize);
        ushort offset = 0;

        requestMessage.Serialize(bytes, ref offset);

        try
        {
            await this.udpClient.SendAsync(bytes.AsMemory(0, offset), this.ipEndPointProvider.Get(), cancellationToken);
        }
        finally
        { 
            ArrayPool<byte>.Shared.Return(bytes);
        }

        UdpReceiveResult response = await this.udpClient.ReceiveAsync(cancellationToken);

        offset = 0;
        var responseMessage = new Message();
        responseMessage.Deserialize(response.Buffer, ref offset);
        return responseMessage;
    }
}
