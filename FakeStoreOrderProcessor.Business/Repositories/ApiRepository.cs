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
    public abstract class ApiRepository<T> : IApiRepository<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiRepository<T>> _logger;
        protected readonly string _endpointPath;

        public ApiRepository(HttpClient httpClient, ILogger<ApiRepository<T>> logger, string endpointPath)
        {
            _httpClient = httpClient;
            _logger = logger;
            _endpointPath = endpointPath;
        }
        
        public async Task<List<T>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<T>>(_endpointPath);
        }

        public async Task<T?> GetByIdAsync(long id)
        {
            return await _httpClient.GetFromJsonAsync<T>($"{_endpointPath}/{id}");
        }

        public async Task<T?> PostAsync(T entity)
        {
            var response = await _httpClient.PostAsJsonAsync(_endpointPath, entity);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task PatchAsync(long id, T entity)
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
