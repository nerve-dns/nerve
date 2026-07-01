// SPDX-FileCopyrightText: 2023 - 2026 varelen and nerve contributors
// 
// SPDX-License-Identifier: BSD-3-Clause

using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Nerve.Service.Endpoints.Domains.Models;
using Nerve.Service.Endpoints.Lists.Models;
using Nerve.Service.Endpoints.Queries.Models;
using Nerve.Service.Endpoints.Resolvers.Models;
using Nerve.Service.Endpoints.Stats.Models;

namespace Nerve.Service.Web;

public sealed class NerveApiClient
{
    public const string NerveApiHttpClientName = "NerveApi";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly IHttpClientFactory httpClientFactory;

    public NerveApiClient(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<GetStatsResponse?> GetStatsAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        return await httpClient.GetFromJsonAsync<GetStatsResponse>("stats", JsonSerializerOptions);
    }

    public async Task<GetStatsTopResponse?> GetStatsTopAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        return await httpClient.GetFromJsonAsync<GetStatsTopResponse>("stats/top", JsonSerializerOptions);
    }

    public async Task<GetStatsHistoryResponse?> GetStatsHistoryAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        return await httpClient.GetFromJsonAsync<GetStatsHistoryResponse>("stats/history", JsonSerializerOptions);
    }

    public async Task<QueryDto[]?> GetQueriesAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        return await httpClient.GetFromJsonAsync<QueryDto[]>("queries?page=0", JsonSerializerOptions);
    }

    public async Task<List<ListResponse>?> GetBlocklistsAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        return await httpClient.GetFromJsonAsync<List<ListResponse>>("lists", JsonSerializerOptions);
    }

    public async Task AddListAsync(string ip, string list, ListType type)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        var addBlocklistListRequest = new AddListRequest(ip, list, type);
        
        await httpClient.PostAsJsonAsync($"lists/{ip}", addBlocklistListRequest, JsonSerializerOptions);
    }

    public async Task RefreshListAsync(string ip, int listId)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.PostAsync($"lists/{ip}/{listId}/refresh", content: null);
    }

    public async Task DeleteListAsync(string ip, string location, ListType type)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.DeleteAsync($"lists/{ip}?location={WebUtility.UrlEncode(location)}&type={type}");
    }

    public async Task<DomainsDto?> GetDomainsAsync(string? searchDomain, int page = 1)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        var searchDomainQuery = "";

        if (searchDomain is not null)
        {
            searchDomainQuery = $"&searchDomain={searchDomain}";
        }

        return await httpClient.GetFromJsonAsync<DomainsDto>($"domains?page={page}{searchDomainQuery}", JsonSerializerOptions);
    }

    public async Task AddDomainAsync(string domain, DomainActionDto domainActionDto)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        var domainDto = new DomainDto(0, domainActionDto, DomainSourceDto.Manual, domain);
        
        await httpClient.PostAsJsonAsync($"domains", domainDto, JsonSerializerOptions);
    }

    public async Task DeleteDomainAsync(int id)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.DeleteAsync($"domains/{id}");
    }

    public async Task RefreshListsAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.PostAsync($"lists/refresh", content: null);
    }

    public async Task RestartAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.PostAsync($"system/restart", content: null);
    }

    public async Task<GetResolversResponse?> GetResolversAsync()
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        return await httpClient.GetFromJsonAsync<GetResolversResponse>($"resolvers", JsonSerializerOptions);
    }

    public async Task AddResolverAsync(string endpoint, ProtocolDto protocolDto)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);

        var request = new AddResolverRequest(endpoint, protocolDto);
        
        await httpClient.PostAsJsonAsync($"resolvers", request, JsonSerializerOptions);
    }

    public async Task DeleteResolverAsync(int id)
    {
        var httpClient = this.httpClientFactory.CreateClient(NerveApiHttpClientName);
        
        await httpClient.DeleteAsync($"resolvers/{id}");
    }
}
