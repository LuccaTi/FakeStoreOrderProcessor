using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class CustomerRepository : ApiRepository<CustomerDto, CreateCustomerDto, UpdateCustomerDto>, ICustomerRepository
    {
        private const string _endpoint = "api/v1/Customer";

        public CustomerRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory, _endpoint)
        {

        }

        public async Task<CustomerDto?> GetByLoginRequestAsync(LoginRequestDto loginRequest, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/login", loginRequest, cancellationToken);
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
            return await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken);
        }
    }
}
