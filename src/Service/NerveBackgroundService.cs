// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service;

public class NerveBackgroundService : BackgroundService
{
    private readonly ILogger<NerveBackgroundService> logger;

    public NerveBackgroundService(ILogger<NerveBackgroundService> logger)
    {
        this.logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Nerve background service started");
        return Task.CompletedTask;
    }
}
