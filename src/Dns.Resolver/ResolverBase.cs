// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver;

public abstract class ResolverBase : IResolver
{
    protected IQueryLogger? queryLogger;
    protected IResolver? next;

    protected ResolverBase(IQueryLogger? queryLogger = null, IResolver? next = null)
    {
        this.queryLogger = queryLogger;
        this.next = next;
    }

    public abstract Task<(Message? message, bool blocked, bool cached)> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default);

    public IResolver SetNext(IResolver next)
    {
        this.next = next;
        return next;
    }
}
