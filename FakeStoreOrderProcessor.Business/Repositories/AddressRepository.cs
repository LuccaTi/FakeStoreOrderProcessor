using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Address;
using FakeStoreOrderProcessor.Library.DTO.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class AddressRepository : ApiRepository<AddressDto, CreateAddressDto, UpdateAddressDto>, IAddressRepository
    {
        private const string _endpoint = "api/v1/Address";

        public AddressRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory, _endpoint)
        {

        }

        public async Task<AddressDto?> GetByStreetNumberAsync(StreetNumberDto streetNumberDto, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/street-number", streetNumberDto, cancellationToken);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
            return await response.Content.ReadFromJsonAsync<AddressDto>(cancellationToken);
        }
    }
}
