// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Dns.Server;

namespace Nerve.Service;

public class NerveBackgroundService : BackgroundService
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
        await this.dnsServer.StartAsync(cancellationToken);
    }
}
