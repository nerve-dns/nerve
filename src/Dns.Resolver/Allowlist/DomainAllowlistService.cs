// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Metrics;

using System.Net;

namespace Nerve.Dns.Resolver.Allowlist;

public sealed class DomainAllowlistService : IDomainAllowlistService
{
    private readonly Dictionary<IPAddress, CompiledAllowlist> allowlist = new();
    private readonly ReaderWriterLockSlim readerWriterLockSlim = new();
    private readonly NerveMetrics? nerveMetrics;
    private long size = 0;

    public long Size => this.size;

    public DomainAllowlistService(NerveMetrics? nerveMetrics = null)
        => this.nerveMetrics = nerveMetrics;

    public void Add(IPAddress remoteIp, string domain)
    {
        this.readerWriterLockSlim.EnterWriteLock();

        try
        {
            this.size += 1;
            this.nerveMetrics?.AddAllowlist(1);

            if (this.allowlist.TryGetValue(remoteIp, out var compiledAllowlist))
            {
                compiledAllowlist.Add(domain);
                return;
            }

            var newCompiledBlocklist = new CompiledAllowlist(new HashSet<string>()
            {
                { domain },
            });
            this.allowlist.Add(remoteIp, newCompiledBlocklist);
        }
        finally
        {
            this.readerWriterLockSlim.ExitWriteLock();
        }
    }

    public void Add(IPAddress remoteIp, IEnumerable<string> domains)
    {
        this.readerWriterLockSlim.EnterWriteLock();

        try
        {
            int domainCount = domains.Count();
            this.size += domainCount;
            this.nerveMetrics?.AddAllowlist(domainCount);

            if (this.allowlist.TryGetValue(remoteIp, out var compiledBlocklist))
            {
                compiledBlocklist.Add(domains);
                return;
            }

            var newCompiledAllowlist = new CompiledAllowlist(domains);
            this.allowlist.Add(remoteIp, newCompiledAllowlist);
        }
        finally
        {
            this.readerWriterLockSlim.ExitWriteLock();
        }
    }

    public bool Remove(IPAddress remoteIp, string domain)
    {
        this.readerWriterLockSlim.EnterWriteLock();
        
        try
        {
            this.size -= 1;
            this.nerveMetrics?.AddAllowlist(-1);

            if (this.allowlist.TryGetValue(remoteIp, out var compiledBlocklist))
            {
                return compiledBlocklist.Remove(domain);
            }

            return false;
        }
        finally
        {
            this.readerWriterLockSlim.ExitWriteLock();
        }
    }

    public bool Remove(IPAddress remoteIp)
    {
        this.readerWriterLockSlim.EnterWriteLock();

        try
        {
            if (this.allowlist.TryGetValue(remoteIp, out CompiledAllowlist? compiledAllowlist))
            {
                this.size -= compiledAllowlist.Allowlist.Count;
                this.nerveMetrics?.AddAllowlist(compiledAllowlist.Allowlist.Count);
                return this.allowlist.Remove(remoteIp);
            }

            return false;
        }
        finally
        {
            this.readerWriterLockSlim.ExitWriteLock();
        }
    }

    public bool TryGet(IPAddress remoteIp, out CompiledAllowlist? compiledAllowlist)
    {
        this.readerWriterLockSlim.EnterReadLock();
        
        try
        {
            return this.allowlist.TryGetValue(remoteIp, out compiledAllowlist);
        }
        finally
        {
            this.readerWriterLockSlim.ExitReadLock();
        }
    }

    public bool IsAllowed(IPAddress remoteIp, string domain)
    {
        this.readerWriterLockSlim.EnterReadLock();

        try
        {
            // Global blocklist
            if (this.allowlist.TryGetValue(IPAddress.Any, out var globalCompiledBlocklist))
            {
                return globalCompiledBlocklist.IsAllowed(domain);
            }

            // IP specific blocklist
            if (this.allowlist.TryGetValue(remoteIp, out var compiledBlocklist))
            {
                return compiledBlocklist.IsAllowed(domain);
            }

            return false;
        }
        finally
        {
            this.readerWriterLockSlim.ExitReadLock();
        }
    }

    public void Clear()
    {
        this.readerWriterLockSlim.EnterWriteLock();

        try
        {
            this.allowlist.Clear();
            this.size = 0;
        }
        finally
        {
            this.readerWriterLockSlim.ExitWriteLock();
        }
    }
}
