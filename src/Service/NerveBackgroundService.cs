// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns.Server;

namespace Nerve.Service;

public sealed class NerveBackgroundService : BackgroundService
{
    private readonly ILogger<NerveBackgroundService> logger;
    private readonly IDnsServer dnsServer;

    public NerveBackgroundService(ILogger<NerveBackgroundService> logger, IDnsServer dnsServer)
    {
        this.logger = logger;
        this.dnsServer = dnsServer;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        this.logger.LogInformation("Nerve background service started");

        try
        {
            await this.dnsServer.StartAsync(cancellationToken);
        }
        catch (TaskCanceledException)
        {
            // Ignore
        }
        catch (Exception exception)
        {
            this.logger.LogError(exception, "{Message}", exception.Message);

            // In order for the Windows Service Management system to leverage configured
            // recovery options, we need to terminate the process with a non-zero exit code
            Environment.Exit(1);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await this.dnsServer.StopAsync(cancellationToken);

        this.logger.LogInformation("Nerve background service stopped");
    }
}
