// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Microsoft.EntityFrameworkCore;

using Nerve.Service.Domain;
using Nerve.Service.Extensions;
using Nerve.Service.Web;

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

            using IHost host = new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddConfiguration(configuration);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddNerve(configuration);
                })
                .ConfigureWebHost(webConfig =>
                {
                    webConfig.UseKestrel();
                    webConfig.UseStartup<NerveWebStartup>();
                    webConfig.UseUrls("http://0.0.0.0:50000/");
                    webConfig.UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "management"));
                    webConfig.ConfigureLogging(logging =>
                    {
                        logging
                            .AddConfiguration(configuration.GetSection("Logging"))
                            .AddConsole();
                    });
                })
                .Build();

            using (IServiceScope serviceScope = host.Services.CreateScope())
            {
                var webHostEnvironment = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var db = serviceScope.ServiceProvider.GetRequiredService<NerveDbContext>();

                db.Database.Migrate();
            }

            await host.RunAsync(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }
}
