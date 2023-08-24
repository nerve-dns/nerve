// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Diagnostics;
using System.Net;

using Microsoft.Extensions.Logging;

namespace Nerve.Dns.Resolver;

public sealed class QueryLoggingResolver : ResolverBase
{
    private readonly ILogger<QueryLoggingResolver> logger;

    public QueryLoggingResolver(ILogger<QueryLoggingResolver> logger, IQueryLogger queryLogger, IResolver? next = null)
        : base(queryLogger ?? throw new ArgumentNullException(nameof(next), "Querry logging resolver requires a query logger"), 
                next ?? throw new ArgumentNullException(nameof(next), "Query logging resolver requires a next resolver"))
    {
        this.logger = logger;
    }

    public override async Task<(Message?, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        (Message? message, bool blocked, bool cached) = await base.next!.ResolveAsync(remoteEndPoint, question, cancellationToken);

        Status status = Status.Forwarded;
        if (cached)
        {
            status = Status.Cached;
        }
        else if (blocked)
        {
            status = Status.Blocked;
        }

        await this.queryLogger!.LogAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds(), remoteEndPoint.Address.ToString(), question.Type, question.Name, message!.Header.ResponseCode, (float)stopwatch.Elapsed.TotalSeconds, status, cancellationToken);

        return (message, blocked, cached);
    }
}
