// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Nerve.Dns;
using Nerve.Dns.Resolver;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Counters;
using Nerve.Service.Domain.Queries;
using Type = Nerve.Dns.Type;

namespace Nerve.Service;

public sealed class DatabaseQueryLogger : IQueryLogger
{
    private readonly NerveDbContext nerveDbContext;
    private readonly IOptionsMonitor<NerveOptions> nerveOptionsMonitor;

    public DatabaseQueryLogger(NerveDbContext nerveDbContext, IOptionsMonitor<NerveOptions> nerveOptionsMonitor)
    {
        this.nerveDbContext = nerveDbContext;
        this.nerveOptionsMonitor = nerveOptionsMonitor;
    }

    public async Task LogAsync(long timestamp, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken)
    {
        if (status == Status.Forwarded)
        {
            await this.nerveDbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Counters SET Value = Value + 1 WHERE Id = {CounterType.Queries}", cancellationToken);
        }
        else
        {
            CounterType counterType = status == Status.Cached ? CounterType.Cached : CounterType.Blocked;
            await this.nerveDbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Counters SET Value = Value + 1 WHERE Id = {CounterType.Queries}; UPDATE Counters SET Value = Value + 1 WHERE Id = {counterType};", cancellationToken);
        }

        PrivacyMode privacyMode = this.nerveOptionsMonitor.CurrentValue.PrivacyMode;
        if (privacyMode == PrivacyMode.Anonymous)
        {
            return;
        }

        this.nerveDbContext.Queries.Add(new Query
        {
            Timestamp = timestamp,
            Client = privacyMode == PrivacyMode.Everything || privacyMode == PrivacyMode.HideDomains ? client : "0.0.0.0",
            Type = type,
            Domain = privacyMode == PrivacyMode.Everything || privacyMode == PrivacyMode.HideClients ? domain : "hidden",
            ResponseCode = responseCode,
            Duration = duration,
            Status = status
        });

        await this.nerveDbContext.SaveChangesAsync(cancellationToken);
    }
}
