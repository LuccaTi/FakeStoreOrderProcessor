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
        protected readonly ILogger<ApiRepository<TEntity, TCreateEntity, TUpdateEntity>> _logger;
        protected readonly string _endpointPath;
        protected readonly string _className;

        public ApiRepository(IHttpClientFactory httpClientFactory, ILogger<ApiRepository<TEntity, TCreateEntity, TUpdateEntity>> logger, string endpointPath, string className)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
            _logger = logger;
            _endpointPath = endpointPath;
            _className = className;
        }
        
        public async Task<List<TEntity>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<TEntity>>(_endpointPath);
        }

        public async Task<TEntity?> GetByIdAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<TEntity>($"{_endpointPath}/{id}");
        }

        public async Task<TEntity?> PostAsync(TCreateEntity entity)
        {
            var response = await _httpClient.PostAsJsonAsync(_endpointPath, entity);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TEntity>();
        }

        public async Task PatchAsync(long id, TUpdateEntity entity)
        {
            var response = await _httpClient.PatchAsJsonAsync($"{_endpointPath}/{id}", entity);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(long id)
        {
            var response = await _httpClient.DeleteAsync($"{_endpointPath}/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
