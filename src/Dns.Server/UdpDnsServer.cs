// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Net;
using System.Net.Sockets;

using Microsoft.Extensions.Logging;

using Nerve.Dns.Resolver;

namespace Nerve.Dns.Server;

public class UdpDnsServer : IDnsServer
{
    private const int MaxDnsUdpDatagramSize = 512;

    private readonly ILogger<UdpDnsServer> logger;
    private readonly Socket socket;
    private readonly IPEndPoint ipEndPoint;
    private readonly IResolver resolver;

    public UdpDnsServer(
        ILogger<UdpDnsServer> logger,
        IPEndPoint ipEndPoint,
        IResolver resolver)
    {
        this.logger = logger;
        this.ipEndPoint = ipEndPoint;
        this.resolver = resolver;
        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        this.socket.Bind(this.ipEndPoint);

        var anyIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (!cancellationToken.IsCancellationRequested)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(MaxDnsUdpDatagramSize);

            try
            {
                SocketReceiveMessageFromResult result =
                    await socket.ReceiveMessageFromAsync(buffer, SocketFlags.None, anyIpEndPoint, cancellationToken);

                await ProcessDatagram((IPEndPoint)result.RemoteEndPoint, buffer, result.ReceivedBytes, cancellationToken);
            }
            catch (Exception exception) when (exception is not ObjectDisposedException && exception is not TaskCanceledException && exception is not SocketException)
            {
                this.logger.LogError(exception, "Error in receive loop");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        this.socket.Close();
        return Task.CompletedTask;
    }

    private async ValueTask ProcessDatagram(IPEndPoint remoteEndPoint, byte[] buffer, int received, CancellationToken cancellationToken)
    {
        ushort offset = 0;

        var message = new Message();
        message.Deserialize(buffer.AsSpan(0, received), ref offset);

        try
        {
            (Message? resolverResponse, bool blocked, bool cached) = await this.resolver.ResolveAsync(remoteEndPoint, message.Questions.Single(), cancellationToken);

            if (resolverResponse is not null)
            {
                message.Header.ResponseCode = resolverResponse.Header.ResponseCode;
                message.Answers = resolverResponse.Answers;
                message.Authorities = resolverResponse.Authorities;
                message.Additionals = resolverResponse.Additionals;
                message.Header.AnswerCount = (ushort)message.Answers.Count;
                message.Header.AuthorityCount = (ushort)message.Authorities.Count;
                message.Header.AdditionalCount = (ushort)message.Additionals.Count;
            }
            else
            {
                message.Header.ResponseCode = ResponseCode.ServFail;
            }
        }
        catch (Exception exception)
        {
            message.Header.ResponseCode = ResponseCode.ServFail;

            this.logger.LogError(exception, "Error while resolving questions {Questions}", string.Join(", ", message.Questions.Select(q => q.ToString())));
        }

        message.Header.QueryResponse = true;

        byte[] bytes = ArrayPool<byte>.Shared.Rent(MaxDnsUdpDatagramSize);

        try
        {
            offset = 0;
            var domainNameOffsetCache = new Dictionary<string, ushort>();
            message.Serialize(bytes.AsSpan(), ref offset, domainNameOffsetCache);

            await socket.SendToAsync(bytes.AsMemory(0, offset), SocketFlags.None, remoteEndPoint, cancellationToken);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}
