// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Microsoft.Extensions.Caching.Memory;

using Nerve.Metrics;

namespace Nerve.Dns.Resolver;

public sealed class CacheResolver : ResolverBase
{
    private static readonly Message ServerFailResolverResponse =  new()
    {
        Header = new Header
        {
            ResponseCode = ResponseCode.ServFail
        }
    };

    private readonly IMemoryCache memoryCache;
    private readonly NerveMetrics? nerveMetrics;

    public CacheResolver(IMemoryCache memoryCache, NerveMetrics? nerveMetrics = null, IResolver? next = null)
        : base(null, next ?? throw new ArgumentNullException(nameof(next), "Cache resolver requires a next resolver"))
    {
        this.memoryCache = memoryCache;
        this.nerveMetrics = nerveMetrics;
    }

    public override async Task<(Message? message, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default)
    {
        if (this.memoryCache.TryGetValue(question, out Message? cachedMessage))
        {
            this.nerveMetrics?.AddCacheHit();
            return (message: cachedMessage, blocked: false, cached: true);
        }

        (Message? resolvedMessage, bool blocked, bool cached) = await base.next!.ResolveAsync(remoteEndPoint, question, cancellationToken);
        if (resolvedMessage == null)
        {
            return (ServerFailResolverResponse, blocked, cached);
        }
        
        if (!blocked)
        {
            // TODO: How to handle TTL
            uint lowestTtl = resolvedMessage.Answers.Count > 0
                ? resolvedMessage.Answers.Min(record => record.Ttl)
                : 300;
            this.memoryCache.Set(question, resolvedMessage, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(lowestTtl),
                Size = 1
            });
        }

        this.nerveMetrics?.AddCacheMiss();

        return (message: resolvedMessage, blocked, cached: false);
    }
}
