// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Buffers;
using System.Net.Http.Headers;

namespace Nerve.Dns.Client;

public sealed class HttpsDnsClient : IDnsClient
{
    private const string DnsMessageContentType = "application/dns-message";
    private const int MaxUdpDatagramSize = 512;

    private readonly IUriProvider uriProvider;
    private readonly HttpClient httpClient;
    private readonly Random random;

    public HttpsDnsClient(IUriProvider uriProvider)
    {
        this.uriProvider = uriProvider;
        this.httpClient = new HttpClient();
        this.httpClient.DefaultRequestHeaders.Add("Accept", DnsMessageContentType);
        this.random = new Random();
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

            HttpResponseMessage httpResponseMessage = await this.httpClient.PostAsync(this.uriProvider.Get(), byteArrayContent, cancellationToken);
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
}
