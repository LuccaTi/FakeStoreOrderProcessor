using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Product;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class ProductRepository : ApiRepository<ProductDto, CreateProductDto, UpdateProductDto>, IProductRepository
    {
        private const string _endpoint = "api/v1/Product";

        public ProductRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory, _endpoint)
        {

        }

        public async Task<ProductDto?> GetByTitleDescription(TitleDescriptionDto titleDescriptionDto, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/title-description", titleDescriptionDto, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                if(ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        }

        public async Task<bool> ProductExistsAsync(long id, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, $"{_endpoint}/{id}/product-exists");
            var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<ProductDto?> PostWithFileNameAsync(CreateProductDto productDto, string fileName, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/create-with-log/{fileName}");
            request.Content = JsonContent.Create(productDto);
            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken);
        }
    }
}
