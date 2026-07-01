// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Configuration;
using System.Net;

using Microsoft.EntityFrameworkCore;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1873:Potenziell kostspielige Protokollierung vermeiden", Justification = "It's just once during startup")]
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

        @this.AddMemoryCache();

        @this.AddSingleton<IDomainAllowlistService, DomainAllowlistService>();
        @this.AddSingleton<IDomainBlocklistService, DomainBlocklistService>();
        @this.AddSingleton<IQueryLogger, BulkDatabaseQueryLogger>();
        @this.AddSingleton<IDnsServer>(
            serviceProvider =>
            {
                var nerveOptions = serviceProvider.GetRequiredService<IOptions<NerveOptions>>();
                var logger = serviceProvider.GetRequiredService<ILogger<IDnsServer>>();
                var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
                
                using var scope = serviceScopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<NerveDbContext>();

                var resolvers = dbContext.Resolvers
                    .AsNoTracking()
                    .ToList();

                if (resolvers.Count == 0)
                {
                    throw new ConfigurationErrorsException("At least one resolver is required");
                }
                
                var firstForwarderProtocol = resolvers[0].Protocol;

                if (resolvers.Any(r => r.Protocol != firstForwarderProtocol))
                {
                    throw new ConfigurationErrorsException("Currently all resolvers needs to have the same protocol");
                }

                IDnsClient dnsClient;

                if (firstForwarderProtocol == Domain.Resolvers.Protocol.Udp)
                {
                    // TODO: Support other ports?
                    IPEndPoint[] forwarders = [.. resolvers.Select(r => new IPEndPoint(IPAddress.Parse(r.Endpoint), 53))];
                    IIpEndPointProvider ipEndPointProvider = forwarders.Length == 1
                        ? new SingleIpEndPointProvider(forwarders[0])
                        : new RoundRobinIpEndPointProvider(forwarders);
                    dnsClient = new UdpDnsClient(ipEndPointProvider);

                    logger.LogInformation("Using UDP for DNS forwarder (DNS over UDP) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<IPEndPoint>)forwarders));
                }
                else if (firstForwarderProtocol == Domain.Resolvers.Protocol.Https)
                {
                    Uri[] forwarders = [.. resolvers.Select(r => new Uri(r.Endpoint))];

                    IUriProvider uriProvider = forwarders.Length == 1
                        ? new SingleUriProvider(forwarders[0])
                        : new RoundRobinUriProvider(forwarders);
                    dnsClient = new HttpsDnsClient(uriProvider);

                    logger.LogInformation("Using HTTPS for DNS forwarder (DNS over HTTPS) with forwarders '{Forwarders}'", string.Join(", ", (IEnumerable<Uri>)forwarders));
                }
                else
                {
                    dnsClient = new TlsDnsClient(resolvers[0].Endpoint);

                    logger.LogInformation("Using TLS for DNS forwarder (DNS over TLS) with forwarders '{Forwarders}'", string.Join(", ", resolvers.Select(r => r.Endpoint)));
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

        @this.AddSingleton<IListService, ListService>();

        @this.AddHostedService<DatabaseListsHostedService>();
        @this.AddHostedService<NerveBackgroundService>();

        return @this;
    }
}
