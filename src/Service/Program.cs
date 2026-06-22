// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Endpoints.Stats;
using Nerve.Service.Endpoints.Queries;
using Nerve.Service.Extensions;

namespace Nerve.Service;

public static class Program
{
    private static CancellationTokenSource cancellationTokenSource = new();
    private static bool keepRestarting;

    private static async Task Main(string[] args)
    {
        await StartAsync(args);

        while (keepRestarting)
        {
            Console.WriteLine("Restarting Nerve");

            keepRestarting = false;
            await StartAsync(args);
        }
    }

    public static void Restart()
    {
        keepRestarting = true;
        cancellationTokenSource.Cancel();
    }

    private static async Task StartAsync(string[] args)
    {
        try
        {
            cancellationTokenSource = new CancellationTokenSource();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("nerve.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddConfiguration(configuration);

            builder.Services.AddNerve(configuration);

            builder.WebHost.UseUrls("http://*:8080");

            WebApplication webApplication = builder.Build();

            webApplication.AddStatEndpoints();
            webApplication.AddQueryEndpoints();

            using (IServiceScope serviceScope = webApplication.Services.CreateScope())
            {
                var webHostEnvironment = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var db = serviceScope.ServiceProvider.GetRequiredService<NerveDbContext>();

#if DEBUG
                await db.Database.EnsureDeletedAsync(cancellationTokenSource.Token);
#endif

                await db.Database.MigrateAsync(cancellationTokenSource.Token);
            }

            await webApplication.RunAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
