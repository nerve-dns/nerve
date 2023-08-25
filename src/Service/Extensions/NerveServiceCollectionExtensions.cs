// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Nerve.Dns.Client;
using Nerve.Dns.Resolver;
using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Dns.Server;
using Nerve.Service.Domain;

namespace Nerve.Service.Extensions;

public static class NerveServiceCollectionExtensions
{
    public static IServiceCollection AddNerve(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.AddWindowsService(options =>
        {
            options.ServiceName = "NerveService";
        });

        @this.AddDbContext<NerveDbContext>();

        @this.Configure<NerveOptions>(configuration.GetSection("Nerve"));
        @this.PostConfigure<NerveOptions>(nerveOptions =>
        {
            // Set default forwarders
            if (nerveOptions.Forwarders.Length == 0)
            {
                if (nerveOptions.ForwarderMode == ForwarderMode.Udp)
                {
                    nerveOptions.Forwarders = new[] { "1.1.1.1" };
                }
                else
                {
                    nerveOptions.Forwarders = new[] { "https://cloudflare-dns.com/dns-query" };
                }
            }
        });

        @this.AddMemoryCache();

        @this.AddSingleton<IDomainAllowlistService, DomainAllowlistService>();
        @this.AddSingleton<IDomainBlocklistService, DomainBlocklistService>();
        @this.AddSingleton<IQueryLogger, DatabaseQueryLogger>();
        @this.AddSingleton<IDnsServer>(
            serviceProvider =>
            {
                var nerveOptions = serviceProvider.GetRequiredService<IOptions<NerveOptions>>();
                var logger = serviceProvider.GetRequiredService<ILogger<NerveOptions>>();

                IDnsClient dnsClient;
                if (nerveOptions.Value.ForwarderMode == ForwarderMode.Udp)
                {
                    // TODO: Support other ports?
                    IPEndPoint[] forwarders = nerveOptions.Value.Forwarders.Select(ip => new IPEndPoint(IPAddress.Parse(ip), 53)).ToArray();
                    IIpEndPointProvider ipEndPointProvider = forwarders.Length == 1
                        ? new SingleIpEndPointProvider(forwarders[0])
                        : new RoundRobinIpEndPointProvider(forwarders);
                    dnsClient = new UdpDnsClient(ipEndPointProvider);

                    logger.LogInformation("Using UDP for DNS forwarder (DNS over UDP) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<IPEndPoint>)forwarders));
                }
                else
                {
                    Uri[] forwarders = nerveOptions.Value.Forwarders.Select(ip => new Uri(ip)).ToArray();

                    IUriProvider uriProvider = forwarders.Length == 1
                        ? new SingleUriProvider(forwarders[0])
                        : new RoundRobinUriProvider(forwarders);
                    dnsClient = new HttpsDnsClient(uriProvider);

                    logger.LogInformation("Using HTTPS for DNS forwarder (DNS over HTTPS) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<Uri>)forwarders));
                }

                var dnsClientResolver = new DnsClientResolver(dnsClient);
                var domainListsResolver = new DomainListsResolver(serviceProvider.GetRequiredService<IDomainAllowlistService>(), serviceProvider.GetRequiredService<IDomainBlocklistService>(), dnsClientResolver);
                var cacheResolver = new CacheResolver(serviceProvider.GetRequiredService<IMemoryCache>(), domainListsResolver);
                var querryLoggingResolver = new QueryLoggingResolver(serviceProvider.GetRequiredService<ILogger<QueryLoggingResolver>>(), serviceProvider.GetRequiredService<IQueryLogger>(), cacheResolver);
                return new UdpDnsServer(serviceProvider.GetRequiredService<ILogger<UdpDnsServer>>(), new IPEndPoint(IPAddress.Parse(nerveOptions.Value.Ip), nerveOptions.Value.Port), querryLoggingResolver);
            });

        @this.AddHostedService<NerveBackgroundService>();
        @this.AddHostedService<FileBlocklistBackgroundService>();

        return @this;
    }
}
