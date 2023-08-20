// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Microsoft.Extensions.Options;

using Nerve.Dns.Client;
using Nerve.Dns.Resolver;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Dns.Server;

namespace Nerve.Service.Extensions;

public static class NerveServiceCollectionExtensions
{
    public static IServiceCollection AddNerve(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.Configure<NerveOptions>(configuration.GetSection("Nerve"));

        @this.AddSingleton<IDomainBlocklistService, DomainBlocklistService>();
        @this.AddSingleton<IDnsServer>(
            serviceProvider =>
            {
                var nerveOptions = serviceProvider.GetRequiredService<IOptions<NerveOptions>>();

                IIpEndPointProvider ipEndPointProvider = nerveOptions.Value.Forwarders.Length == 1
                    ? new SingleIpEndPointProvider(new IPEndPoint(IPAddress.Parse(nerveOptions.Value.Forwarders[0]), 53))
                    : new RoundRobinIpEndPointProvider(nerveOptions.Value.Forwarders.Select(ip => new IPEndPoint(IPAddress.Parse(ip), 53)).ToArray());
                var dnsClientResolver = new DnsClientResolver(new UdpDnsClient(ipEndPointProvider));
                var resolver = new DomainBlocklistResolver(serviceProvider.GetRequiredService<IDomainBlocklistService>(), dnsClientResolver);
                return new UdpDnsServer(serviceProvider.GetRequiredService<ILogger<UdpDnsServer>>(), new IPEndPoint(IPAddress.Parse(nerveOptions.Value.Ip), nerveOptions.Value.Port), resolver);
            });

        @this.AddHostedService<NerveBackgroundService>();

        return @this;
    }
}
