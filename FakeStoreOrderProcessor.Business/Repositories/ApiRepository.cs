using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public abstract class ApiRepository<TEntity, TCreateEntity, TUpdateEntity> 
        : IApiRepository<TEntity, TCreateEntity, TUpdateEntity> 
        where TEntity : class
        where TCreateEntity : class
        where TUpdateEntity : class
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _endpointPath;

        public ApiRepository(IHttpClientFactory httpClientFactory, string endpointPath)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _endpointPath = endpointPath;
        }
        
        public async Task<List<TEntity>?> GetAllAsync(CancellationToken cancellationToken)
        {
            return await _httpClient.GetFromJsonAsync<List<TEntity>>(_endpointPath, cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
        {
            return await _httpClient.GetFromJsonAsync<TEntity>($"{_endpointPath}/{id}", cancellationToken);
        }

        public async Task<TEntity?> PostAsync(TCreateEntity entity, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync(_endpointPath, entity, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TEntity>(cancellationToken);
        }

        public async Task PatchAsync(long id, TUpdateEntity entity, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync($"{_endpointPath}/{id}", entity, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(long id, CancellationToken cancellationToken)
        {
            var response = await _httpClient.DeleteAsync($"{_endpointPath}/{id}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
    }
}
