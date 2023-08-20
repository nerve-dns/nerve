// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Nerve.Dns.Client;
using Nerve.Dns.Resolver;
using Nerve.Dns.Server;

namespace Nerve.Service.Extensions;

public static class NerveServiceCollectionExtensions
{
    public static IServiceCollection AddNerve(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.Configure<NerveOptions>(configuration.GetSection("Nerve"));
        @this.AddSingleton<IDnsServer>(
            serviceProvider =>
            {
                var resolver = new DnsClientResolver(new UdpDnsClient(IPAddress.Parse("1.1.1.1")));
                return new UdpDnsServer(serviceProvider.GetRequiredService<ILogger<UdpDnsServer>>(), new IPEndPoint(IPAddress.Parse("0.0.0.0"), 5333), resolver);
            });

        @this.AddHostedService<NerveBackgroundService>();

        return @this;
    }
}
