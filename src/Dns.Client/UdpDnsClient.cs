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

    // TODO: Configurable?
    private static readonly TimeSpan SendTimeout = TimeSpan.FromMilliseconds(500);

    // TODO: Configurable?
    private static readonly TimeSpan ReceiveTimeout = TimeSpan.FromMilliseconds(500);

    private const byte MaxRetries = 3;

    private readonly IIpEndPointProvider ipEndPointProvider;
    private readonly Random random;
    private readonly SemaphoreSlim semaphoreSlim = new(initialCount: 1, maxCount: 1);
    private UdpClient? udpClient;

    public UdpDnsClient(IIpEndPointProvider ipEndPointProvider)
    {
        this.ipEndPointProvider = ipEndPointProvider;
        this.random = new Random();

        this.ReinitializeUdpClient();
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

    private void ReinitializeUdpClient()
    {
        this.udpClient?.Dispose();
        this.udpClient = new UdpClient(port: 0);
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

                    ValueTask<int> sendTask = this.udpClient!.SendAsync(bytes.AsMemory(0, offset), this.ipEndPointProvider.Get(), cancellationToken);
                    await sendTask.AsTask().WaitAsync(SendTimeout, cancellationToken);
                    
                    ValueTask<UdpReceiveResult> receiveTask = this.udpClient.ReceiveAsync(cancellationToken);
                    UdpReceiveResult response = await receiveTask.AsTask().WaitAsync(ReceiveTimeout, cancellationToken);

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

            // Try to recover if something else is broken
            this.ReinitializeUdpClient();

            throw new DnsClientException($"Failed to resolve question '{question}' after '{MaxRetries}' retries");
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}
