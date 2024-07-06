// SPDX-FileCopyrightText: 2023 nerve-dns
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Nerve.Dns.Client;
using Nerve.Dns.Client.Tls;
using Nerve.Dns.Resolver;
using Nerve.Dns.Resolver.Allowlist;
using Nerve.Dns.Resolver.Blocklist;
using Nerve.Dns.Server;
using Nerve.Metrics;
using Nerve.Service.Domain;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Nerve.Service.Extensions;

public static class NerveServiceCollectionExtensions
{
    public static IServiceCollection AddNerve(this IServiceCollection @this, IConfiguration configuration)
    {
        @this.AddWindowsService(options =>
        {
            options.ServiceName = "NerveService";
        });

        // TODO: Review
        IConfigurationSection influxDbMetricsExporterSection = configuration.GetSection("Nerve:InfluxDBMetricsExporter");
        if (influxDbMetricsExporterSection.Exists())
        {
            // TODO: Save and load from eg. ".instanceid" file? Needs to be unique between restarts..
            const string instanceId = "60b74ea3-a39a-4195-9ded-d25bcecedac3";
            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService("Nerve", "nerve", "0.0.1", autoGenerateServiceInstanceId: false, instanceId);

            @this.AddOpenTelemetry()
                .WithMetrics(builder => builder.AddInfluxDBMetricsExporter(options =>
                {
                    options.Endpoint = new Uri(influxDbMetricsExporterSection.GetValue<string?>("Endpoint") ?? throw new ArgumentException("Endpoint is required"));
                    options.Bucket = influxDbMetricsExporterSection.GetValue<string?>("Bucket") ?? throw new ArgumentException("Bucket is required");
                    options.FlushInterval = influxDbMetricsExporterSection.GetValue<int?>("FlushInterval") ?? throw new ArgumentException("FlushInterval is required"); ;
                    options.Org = influxDbMetricsExporterSection.GetValue<string?>("Organization") ?? throw new ArgumentException("Organization is required");
                    options.Token = influxDbMetricsExporterSection.GetValue<string?>("Token") ?? throw new ArgumentException("Token is required");
                })
                .SetResourceBuilder(resourceBuilder)
                .AddMeter("Nerve"));
        }

        @this.AddSingleton<NerveMetrics>();

        @this.AddDbContext<NerveDbContext>();

        @this.AddOptions<NerveOptions>()
            .Bind(configuration.GetSection(NerveOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        @this.PostConfigure<NerveOptions>(nerveOptions =>
        {
            // Set default forwarders
            if (nerveOptions.Forwarders.Length == 0)
            {
                if (nerveOptions.ForwarderMode == ForwarderMode.Udp)
                {
                    nerveOptions.Forwarders = ["1.1.1.1"];
                }
                else if (nerveOptions.ForwarderMode == ForwarderMode.Https)
                {
                    nerveOptions.Forwarders = ["https://cloudflare-dns.com/dns-query"];
                }
                else
                {
                    nerveOptions.Forwarders = ["one.one.one.one"];
                }
            }
        });

        @this.AddMemoryCache();

        @this.AddSingleton<IDomainAllowlistService, DomainAllowlistService>();
        @this.AddSingleton<IDomainBlocklistService, DomainBlocklistService>();
        @this.AddSingleton<IQueryLogger, BulkDatabaseQueryLogger>();
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
                else if (nerveOptions.Value.ForwarderMode == ForwarderMode.Https)
                {
                    Uri[] forwarders = nerveOptions.Value.Forwarders.Select(ip => new Uri(ip)).ToArray();

                    IUriProvider uriProvider = forwarders.Length == 1
                        ? new SingleUriProvider(forwarders[0])
                        : new RoundRobinUriProvider(forwarders);
                    dnsClient = new HttpsDnsClient(uriProvider);

                    logger.LogInformation("Using HTTPS for DNS forwarder (DNS over HTTPS) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<Uri>)forwarders));
                }
                else
                {
                    dnsClient = new TlsDnsClient(nerveOptions.Value.Forwarders.First());

                    logger.LogInformation("Using TLS for DNS forwarder (DNS over TLS) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<string>)nerveOptions.Value.Forwarders));
                }

                var nerveMetrics = serviceProvider.GetRequiredService<NerveMetrics>();

                var dnsClientResolver = new DnsClientResolver(dnsClient);
                var domainListsResolver = new DomainListsResolver(serviceProvider.GetRequiredService<IDomainAllowlistService>(), serviceProvider.GetRequiredService<IDomainBlocklistService>(), nerveMetrics, dnsClientResolver);
                var cacheResolver = new CacheResolver(serviceProvider.GetRequiredService<IMemoryCache>(), nerveMetrics, domainListsResolver);
                var querryLoggingResolver = new QueryLoggingResolver(serviceProvider.GetRequiredService<IQueryLogger>(), cacheResolver);
                return new UdpDnsServer(
                    serviceProvider.GetRequiredService<ILogger<UdpDnsServer>>(),
                    new IPEndPoint(IPAddress.Parse(nerveOptions.Value.Ip), nerveOptions.Value.Port),
                    querryLoggingResolver,
                    nerveMetrics);
            });

        @this.AddHostedService<NerveBackgroundService>();
        @this.AddHostedService<ListsBackgroundService>();

        return @this;
    }
}
