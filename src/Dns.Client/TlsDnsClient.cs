// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Buffers.Binary;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace Nerve.Dns.Client;

public class TlsDnsClient : IDnsClient
{
    private const int MaxUdpDatagramSize = 512;
    private const int MessageLengthSize = 2;
    private const int DnsOverTlsPort = 853;

    private static readonly Dictionary<string, IPAddress> knownTlsDnsServers = new()
    {
        { "one.one.one.one", IPAddress.Parse("1.1.1.1") },
        { "dns.google", IPAddress.Parse("8.8.8.8") },
        { "anycast.uncensoreddns.org", IPAddress.Parse("91.239.100.100") },
        { "dot.xfinity.com", IPAddress.Parse("96.113.151.145") }
    };

    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);
    private readonly Dictionary<string, TlsSocket> sockets;
    private readonly string host;

    public TlsDnsClient(string host)
    {
        this.host = host;
        this.sockets = new Dictionary<string, TlsSocket>();
    }

    /// <inheritdoc />
    public async Task<Message> ResolveAsync(Question question, CancellationToken cancellationToken = default)
    {
        // TODO: Review thread safety (locking needs to be only on a per host base)
        await this.semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            if (!this.sockets.TryGetValue(this.host, out TlsSocket? tlsSocket))
            {
                IPAddress? ip = knownTlsDnsServers[this.host] ?? throw new NotImplementedException("Only known DoT DNS server are currently supported");
                tlsSocket = new TlsSocket
                {
                    Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                };
                await tlsSocket.Socket.ConnectAsync(ip, DnsOverTlsPort);

                tlsSocket.SslStream = new SslStream(new NetworkStream(tlsSocket.Socket, ownsSocket: true));

                var sslClientAuthenticationOptions = new SslClientAuthenticationOptions()
                {
                    TargetHost = this.host
                };
                await tlsSocket.SslStream.AuthenticateAsClientAsync(sslClientAuthenticationOptions, cancellationToken);

                this.sockets[this.host] = tlsSocket;
            }

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

            byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxUdpDatagramSize + MessageLengthSize);

            try
            {
                // Skip two bytes for message length later
                ushort offset = MessageLengthSize;

                var domainNameOffsetCache = new Dictionary<string, ushort>();
                requestMessage.Serialize(bytes, ref offset, domainNameOffsetCache);

                // Prepend actual message length
                BinaryPrimitives.WriteUInt16BigEndian(bytes.AsSpan(0, MessageLengthSize), (ushort)(offset - 2));

                await tlsSocket.SslStream.WriteAsync(bytes.AsMemory(0, offset), cancellationToken);

                await tlsSocket.SslStream.ReadAsync(bytes, cancellationToken);

                ushort responseLength = BinaryPrimitives.ReadUInt16BigEndian(bytes);

                offset = 0;
                var responseMessage = new Message();
                responseMessage.Deserialize(bytes.AsSpan(MessageLengthSize, responseLength), ref offset);

                return responseMessage;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(bytes);
            }
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}
