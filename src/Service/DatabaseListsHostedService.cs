// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service;

public sealed partial class DatabaseListsHostedService : IHostedService
{
    private readonly IListService listService;

    public DatabaseListsHostedService(IListService listService)
    {
        this.listService = listService;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.listService.LoadListsAsync(cancellationToken);

        _ = this.RefreshAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Refresh one hour later so that the file cache is expired
            await Task.Delay(TimeSpan.FromHours(25), cancellationToken);
            
            await this.listService.LoadListsAsync(cancellationToken);
        }
    }
}
