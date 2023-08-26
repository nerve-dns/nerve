// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

namespace Nerve.Service.Web;

public class NerveWebStartup
{
    private readonly IConfiguration configuration;

    public NerveWebStartup(IConfiguration configuration)
        => this.configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
