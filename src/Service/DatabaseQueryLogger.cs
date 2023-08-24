// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;

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

    public DatabaseQueryLogger(NerveDbContext nerveDbContext)
        => this.nerveDbContext = nerveDbContext;

    public async Task LogAsync(long timestamp, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken)
    {
        this.nerveDbContext.Queries.Add(new Query
        {
            Id = 0,
            Timestamp = timestamp,
            Client = client,
            Type = type,
            Domain = domain,
            ResponseCode = responseCode,
            Duration = duration,
            Status = status
        });

        await this.nerveDbContext.SaveChangesAsync(cancellationToken);

        if (status == Status.Forwarded)
        {
            await this.nerveDbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Counters SET Value = Value + 1 WHERE Id = {CounterType.Queries}", cancellationToken);
        }
        else
        {
            CounterType counterType = status == Status.Cached ? CounterType.Cached : CounterType.Blocked;
            await this.nerveDbContext.Database.ExecuteSqlInterpolatedAsync($"UPDATE Counters SET Value = Value + 1 WHERE Id = {CounterType.Queries}; UPDATE Counters SET Value = Value + 1 WHERE Id = {counterType};", cancellationToken);
        }
    }
}
