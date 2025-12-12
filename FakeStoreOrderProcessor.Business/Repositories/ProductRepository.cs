using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Product;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class ProductRepository : ApiRepository<ProductDto, CreateProductDto, UpdateProductDto>, IProductRepository
    {
        private const string _classNameProducts = "ProductRepository";
        private const string _endpoint = "api/v1/Product";

        public ProductRepository(IHttpClientFactory httpClientFactory, ILogger<ProductRepository> logger) : base(httpClientFactory, logger, _endpoint, _classNameProducts)
        {

        }

        public async Task<ProductDto?> GetByTitleDescription(TitleDescriptionDto titleDescriptionDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/title-description", titleDescriptionDto);
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
            return await response.Content.ReadFromJsonAsync<ProductDto>();
        }

        public async Task<bool> ProductExistsAsync(long id)
        {
            var request = new HttpRequestMessage(HttpMethod.Head, $"{_endpoint}/{id}/product-exists");
            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
