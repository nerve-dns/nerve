// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Text;

using Microsoft.EntityFrameworkCore;

using EFCore.BulkExtensions;

using Nerve.Dns;
using Nerve.Dns.Resolver;
using Nerve.Service.Domain;
using Nerve.Service.Domain.Counters;
using Nerve.Service.Domain.Queries;
using Type = Nerve.Dns.Type;

using Microsoft.Extensions.Options;

namespace Nerve.Service;

public sealed class BulkDatabaseQueryLogger : IQueryLogger
{
    // TODO: Configurable?
    private const string AnonymizedIp = "0.0.0.0";
    private const string AnonymizedDomain = "hidden";
    private const int BulkInsertDelayMilliseconds = 500;

    private readonly ILogger<BulkDatabaseQueryLogger> logger;
    private readonly IServiceScopeFactory serviceScopeFactory;
    private readonly IHostApplicationLifetime hostApplicationLifetime;
    private readonly IOptionsMonitor<NerveOptions> nerveOptionsMonitor;

    private readonly List<Query> bulkQueries = new(40_000);
    private readonly SemaphoreSlim semaphoreSlim = new(initialCount: 1, maxCount: 1);

    public BulkDatabaseQueryLogger(
        ILogger<BulkDatabaseQueryLogger> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime hostApplicationLifetime,
        IOptionsMonitor<NerveOptions> nerveOptionsMonitor)
    {
        this.logger = logger;
        this.serviceScopeFactory = serviceScopeFactory;
        this.hostApplicationLifetime = hostApplicationLifetime;
        this.nerveOptionsMonitor = nerveOptionsMonitor;

        _ = this.InsertQueriesBulkPeriodicallyAsync();
    }

    private async Task InsertQueriesBulkPeriodicallyAsync()
    {
        try
        {
            while (!this.hostApplicationLifetime.ApplicationStopping.IsCancellationRequested)
            {
                await this.semaphoreSlim.WaitAsync(this.hostApplicationLifetime.ApplicationStopping);

                try
                {
                    if (this.bulkQueries.Count > 0)
                    {
                        using (var scope = this.serviceScopeFactory.CreateScope())
                        {
                            var nerveDbContext = scope.ServiceProvider.GetRequiredService<NerveDbContext>();

                            var updateCountersQuery = new StringBuilder(200);
                            updateCountersQuery.Append($"UPDATE Counters SET Value = Value + {this.bulkQueries.Count} WHERE Id = {(byte)CounterType.Queries};");
                            updateCountersQuery.Append($"UPDATE Counters SET Value = Value + {this.bulkQueries.Count(query => query.Status == Status.Cached)} WHERE Id = {(byte)CounterType.Cached};");
                            updateCountersQuery.Append($"UPDATE Counters SET Value = Value + {this.bulkQueries.Count(query => query.Status == Status.Blocked)} WHERE Id = {(byte)CounterType.Blocked};");
                            await nerveDbContext.Database.ExecuteSqlRawAsync(updateCountersQuery.ToString());                            

                            await nerveDbContext.BulkInsertAsync(this.bulkQueries, cancellationToken: this.hostApplicationLifetime.ApplicationStopping);
                        }

                        this.bulkQueries.Clear();
                    }
                }
                finally
                {
                    this.semaphoreSlim.Release();
                }

                await Task.Delay(TimeSpan.FromMilliseconds(BulkInsertDelayMilliseconds));
            }
        }
        catch (Exception exception) when (exception is not TaskCanceledException)
        {
            this.logger.LogError(exception, "Error while inserting batched queries");
        }
    }

    public async Task LogAsync(long timestamp, string client, Type type, string domain, ResponseCode responseCode, float duration, Status status, CancellationToken cancellationToken)
    {
        await this.semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            PrivacyMode privacyMode = this.nerveOptionsMonitor.CurrentValue.PrivacyMode;
            if (privacyMode == PrivacyMode.Anonymous)
            {
                return;
            }

            this.bulkQueries.Add(new Query
            {
                Timestamp = timestamp,
                Client = privacyMode == PrivacyMode.Everything || privacyMode == PrivacyMode.HideDomains ? client : AnonymizedIp,
                Type = type,
                Domain = privacyMode == PrivacyMode.Everything || privacyMode == PrivacyMode.HideClients ? domain : AnonymizedDomain,
                ResponseCode = responseCode,
                Duration = duration,
                Status = status
            });
        }
        finally
        {
            this.semaphoreSlim.Release();
        }
    }
}
