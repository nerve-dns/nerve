// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Diagnostics.Metrics;

namespace Nerve.Metrics;

public sealed class NerveMetrics
{
    private const string MeterName = "Nerve";

    // TODO: Research which metric to use
    private readonly Counter<long> requestsTotalCounter;
    private readonly Counter<long> requestsBlockedCounter;
    private readonly Counter<long> cacheHitsCounter;
    private readonly Counter<long> cacheMissesCounter;

    private readonly UpDownCounter<long> blocklistCounter;
    private readonly UpDownCounter<long> allowlistCounter;

    public NerveMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(MeterName);

        this.requestsTotalCounter = meter.CreateCounter<long>("nerve.requests.total");
        this.requestsBlockedCounter = meter.CreateCounter<long>("nerve.requests.blocked");
        this.cacheHitsCounter = meter.CreateCounter<long>("nerve.cache.hits");
        this.cacheMissesCounter = meter.CreateCounter<long>("nerve.cache.misses");
        this.blocklistCounter = meter.CreateUpDownCounter<long>("nerve.blocklist.count");
        this.allowlistCounter = meter.CreateUpDownCounter<long>("nerve.allowlist.count");
    }

    public void AddRequestTotal()
    {
        this.requestsTotalCounter.Add(1);
    }

    public void AddRequestBlocked()
    {
        this.requestsBlockedCounter.Add(1);
    }

    public void AddCacheHit()
    {
        this.cacheHitsCounter.Add(1);
    }

    public void AddCacheMiss()
    {
        this.cacheMissesCounter.Add(1);
    }

    public void AddBlocklist(long count)
    {
        this.blocklistCounter.Add(count);
    }

    public void AddAllowlist(long count)
    {
        this.allowlistCounter.Add(count);
    }
}
