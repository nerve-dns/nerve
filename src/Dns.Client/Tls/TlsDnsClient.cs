// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace Nerve.Dns.Client.Tls;

public class TlsDnsClient : IDnsClient
{
    private const int MaxUdpDatagramSize = 512;
    private const int MessageLengthSize = 2;
    private const int DnsOverTlsPort = 853;

    private const byte MaxRetries = 3;

    private static readonly FrozenDictionary<string, IPAddress> knownTlsDnsServers = new Dictionary<string, IPAddress>()
    {
        { "one.one.one.one", IPAddress.Parse("1.1.1.1") },
        { "dns.google", IPAddress.Parse("8.8.8.8") },
        { "anycast.uncensoreddns.org", IPAddress.Parse("91.239.100.100") },
        { "dot.xfinity.com", IPAddress.Parse("96.113.151.145") },
        { "dns.quad9.net", IPAddress.Parse("9.9.9.9") },
        { "unfiltered.joindns4.eu", IPAddress.Parse("86.54.11.100") },
        { "dns.adguard-dns.com", IPAddress.Parse("94.140.15.15") },
        { "unfiltered.adguard-dns.com", IPAddress.Parse("94.140.14.141") },
    }.ToFrozenDictionary();

    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly Dictionary<string, TlsSocket> sockets;
    private readonly string host;

    public TlsDnsClient(string host)
    {
        this.host = host;
        this.sockets = new Dictionary<string, TlsSocket>(capacity: 10);
    }

    private async Task<TlsSocket> GetTlsSocketAsync(CancellationToken cancellationToken)
    {
        if (!this.sockets.TryGetValue(this.host, out TlsSocket? tlsSocket))
        {
            IPAddress? ip = knownTlsDnsServers[this.host]
                ?? throw new NotImplementedException("Only known DoT DNS server are currently supported");

            tlsSocket = new TlsSocket
            {
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    NoDelay = true
                }
            };

            await tlsSocket.Socket.ConnectAsync(ip, DnsOverTlsPort);

            tlsSocket.SslStream = new SslStream(new NetworkStream(tlsSocket.Socket, ownsSocket: true));

            var sslClientAuthenticationOptions = new SslClientAuthenticationOptions()
            {
                TargetHost = this.host
            };
            await tlsSocket.SslStream.AuthenticateAsClientAsync(sslClientAuthenticationOptions, cancellationToken);

            this.sockets[this.host] = tlsSocket;
            return tlsSocket;
        }

        return tlsSocket;
    }

    private void CloseTlsSocket()
    {
        this.sockets[this.host].SslStream.Dispose();
        this.sockets.Remove(this.host);
    }

    /// <inheritdoc />
    public async Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default)
    {
        // TODO: Review thread safety (locking needs to be only on a per host base)
        await this.semaphoreSlim.WaitAsync(cancellationToken);

        byte[] requestMessageBytes = ArrayPool<byte>.Shared.Rent(MaxUdpDatagramSize + MessageLengthSize);

        try
        {
            var requestMessage = new Message
            {
                Header = new Header
                {
                    Id = (ushort)Random.Shared.Next(ushort.MaxValue),
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

            do
            {
                try
                {
                    TlsSocket tlsSocket = await this.GetTlsSocketAsync(cancellationToken);

                    ushort offset = MessageLengthSize;

                    var domainNameOffsetCache = new Dictionary<string, ushort>();
                    requestMessage.Serialize(requestMessageBytes, ref offset, domainNameOffsetCache);

                    BinaryPrimitives.WriteUInt16BigEndian(requestMessageBytes.AsSpan(0, MessageLengthSize), (ushort)(offset - 2));

                    await tlsSocket.SslStream.WriteAsync(requestMessageBytes.AsMemory(0, offset), cancellationToken);

                    int read = await tlsSocket.SslStream.ReadAsync(requestMessageBytes, cancellationToken);
                    if (read == 0)
                    {
                        throw new ConnectionClosedException();
                    }

                    ushort responseLength = BinaryPrimitives.ReadUInt16BigEndian(requestMessageBytes);

                    offset = 0;
                    var responseMessage = new Message();
                    responseMessage.Deserialize(requestMessageBytes.AsSpan(MessageLengthSize, responseLength), ref offset);

                    return responseMessage;
                }
                catch (Exception exception) when (exception is IOException || exception is ObjectDisposedException)
                {
                    this.CloseTlsSocket();

                    if (currentRetry == MaxRetries)
                    {
                        break;
                    }

                    currentRetry++;
                }
            }
            while (true);

            throw new DnsClientException($"Failed to resolve question '{question}' after '{MaxRetries}' retries");
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(requestMessageBytes);
            this.semaphoreSlim.Release();
        }
    }
}
