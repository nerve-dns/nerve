// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Net;
using System.Net.Http.Headers;

namespace Nerve.Dns.Client;

public sealed class HttpsDnsClient : IDnsClient
{
    private const string DnsMessageContentType = "application/dns-message";
    private const int MaxUdpDatagramSize = 512;

    private readonly UdpDnsClient udpDnsClient;
    private readonly IUriProvider uriProvider;
    private readonly HttpClient httpClient;
    private readonly Random random;
    // TODO: Clear this cache after X minutes to not have stale resolved IP addresses?
    private readonly Dictionary<Uri, Uri> resolvedUrisCache;

    public HttpsDnsClient(IUriProvider uriProvider)
    {
        // TODO: What to do with this hardcoded resolver IP?
        this.udpDnsClient = new UdpDnsClient(IPAddress.Parse("1.1.1.1"));
        this.uriProvider = uriProvider;
        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("Accept", DnsMessageContentType);
        this.random = new Random();
        this.resolvedUrisCache = new Dictionary<Uri, Uri>();
    }

    public HttpsDnsClient(Uri uri)
        : this(new SingleUriProvider(uri))
    {

    }

    public HttpsDnsClient(params Uri[] uris)
        : this(new RoundRobinUriProvider(uris))
    {

    }

    /// <inheritdoc />
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

        try
        {
            var domainNameOffsetCache = new Dictionary<string, ushort>();
            requestMessage.Serialize(bytes, ref offset, domainNameOffsetCache);

            var byteArrayContent = new ByteArrayContent(bytes, 0, offset);
            byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(DnsMessageContentType);

            Uri originalUri = this.uriProvider.Get();
            if (!this.resolvedUrisCache.TryGetValue(originalUri, out Uri? resolvedUri))
            {
                resolvedUri = await this.ResolveUriAsync(originalUri);
                this.resolvedUrisCache.Add(originalUri, resolvedUri);
            }

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = resolvedUri,
                Content = byteArrayContent,
                Headers = {
                    { "Host", originalUri.Host }
                },
            };
            HttpResponseMessage httpResponseMessage = await this.httpClient.SendAsync(httpRequestMessage, cancellationToken);
            httpResponseMessage.EnsureSuccessStatusCode();

            byte[] responseMessageBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync(cancellationToken);

            offset = 0;
            var responseMessage = new Message();
            responseMessage.Deserialize(responseMessageBytes, ref offset);
            return responseMessage;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }

    // TODO: Is this the right/best way? You have to somehow resolve the DoH server endpoint somehow..
    private async Task<Uri> ResolveUriAsync(Uri uri)
    {
        var uriBuilder = new UriBuilder(uri);
        // TODO: Only IPv4 (A) supported, what about IPv6 (AAAA)?
        Message message = await this.udpDnsClient.ResolveAsync(new Question(new DomainName(uri.Host), Type.A, Class.In));
        uriBuilder.Host = ((AResourceData)message.Answers.First().ResourceData!).Address.ToString();
        return uriBuilder.Uri;
    }
}
