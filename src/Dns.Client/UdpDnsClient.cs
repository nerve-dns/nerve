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

    private const byte MaxRetries = 3;

    private readonly IIpEndPointProvider ipEndPointProvider;
    private readonly UdpClient udpClient;
    private readonly Random random;
    private readonly SemaphoreSlim semaphoreSlim = new(initialCount: 1, maxCount: 1);

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

    /// <inheritdoc />
    public async Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default)
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);

        try
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

            byte currentRetry = 0;
            while (true)
            {
                byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxUdpDatagramSize);

                try
                {
                    ushort offset = 0;

                    var domainNameOffsetCache = new Dictionary<string, ushort>();
                    requestMessage.Serialize(bytes, ref offset, domainNameOffsetCache);

                    await this.udpClient.SendAsync(bytes.AsMemory(0, offset), this.ipEndPointProvider.Get(), cancellationToken);
                    UdpReceiveResult response = await this.udpClient.ReceiveAsync(cancellationToken);

                    offset = 0;
                    var responseMessage = new Message();
                    responseMessage.Deserialize(response.Buffer, ref offset);
                    return responseMessage;
                }
                catch (Exception)
                {
                    if (currentRetry == MaxRetries)
                    {
                        break;
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bytes);
                }

                currentRetry++;
            }

            throw new DnsClientException($"Failed to resolve question '{question}' after '{MaxRetries}' retries");
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}
