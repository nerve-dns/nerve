// SPDX-FileCopyrightText: 2024 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using FastEndpoints;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using Nerve.Service.Domain;
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

            builder.Services.AddFastEndpoints();

            WebApplication webApplication = builder.Build();

            webApplication.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = ["index.html"],
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "management"))
            });
            webApplication.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "management"))
            });

            webApplication.UseFastEndpoints();

            using (IServiceScope serviceScope = webApplication.Services.CreateScope())
            {
                var webHostEnvironment = serviceScope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

                var db = serviceScope.ServiceProvider.GetRequiredService<NerveDbContext>();

                await db.Database.EnsureCreatedAsync(cancellationTokenSource.Token);
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
