// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using Nerve.Service.Extensions;
using Nerve.Service.Web;

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
        services.AddNerve();
    })
    .ConfigureWebHost(webConfig =>
    {
        webConfig.UseKestrel();
        webConfig.UseStartup<NerveWebStartup>();
        webConfig.UseUrls("http://localhost:50001/");
        webConfig.ConfigureLogging(logging =>
        {
            logging
                .AddConfiguration(configuration.GetSection("Logging"))
                .AddConsole();
        });
    })
    .Build();

await host.RunAsync();
