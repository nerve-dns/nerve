// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

namespace Nerve.Dns.Resolver;

public abstract class ResolverBase : IResolver
{
    protected IResolver? next;

    protected ResolverBase(IResolver? next = null)
        => this.next = next;

    public abstract Task<Message?> ResolveAsync(IPEndPoint remoteEndPoint, Question question, CancellationToken cancellationToken = default);

    public IResolver SetNext(IResolver next)
    {
        this.next = next;
        return next;
    }
}
