// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Text.Json.Serialization;

namespace Nerve.Service.Web;

public class NerveWebStartup
{
    private readonly IConfiguration configuration;

    public NerveWebStartup(IConfiguration configuration)
        => this.configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                builder =>
                {
                    builder.AllowAnyOrigin() 
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
        });
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseCors("AllowAll");

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
