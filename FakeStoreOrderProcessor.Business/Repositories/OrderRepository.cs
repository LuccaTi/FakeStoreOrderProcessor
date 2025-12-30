using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Library.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories
{
    public class OrderRepository : ApiRepository<OrderDto, CreateOrderDto, UpdateOrderDto>, IOrderRepository
    {
        private const string _endpoint = "api/v1/Order";

        public OrderRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory, _endpoint)
        {

        }

        public async Task<IEnumerable<OrderDto>> GetAllDayBeforeAsync(CancellationToken cancellationToken)
        {
            var orders = await _httpClient.GetFromJsonAsync<IEnumerable<OrderDto>>($"{_endpoint}/day-before", cancellationToken);

            if (orders == null)
                return new List<OrderDto>();

            return orders;
        }

        public async Task<IEnumerable<OrderDto>> GetAllActiveOrNotAsync(CancellationToken cancellationToken)
        {
            var orders = await _httpClient.GetFromJsonAsync<IEnumerable<OrderDto>>($"{_endpoint}/active-or-not", cancellationToken);
            if (orders == null)
                return new List<OrderDto>();

            return orders;
        }

        public async Task<OrderDto?> GetByGuidAsync(string orderGuid, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _httpClient.GetFromJsonAsync<OrderDto>($"{_endpoint}/{orderGuid}", cancellationToken);
                return order;
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
        }

        public async Task<OrderWithOrderItemsDto?> GetByGuidWithOrderItemsAsync(string orderGuid, CancellationToken cancellationToken)
        {
            var order = await _httpClient.GetFromJsonAsync<OrderWithOrderItemsDto>($"{_endpoint}/{orderGuid}/with-order-items", cancellationToken);
            return order;
        }

        public async Task PatchOrderAsync(string orderGuid, UpdateOrderDto orderDto, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync($"{_endpointPath}/{orderGuid}", orderDto, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteWithGuidAsync(string orderGuid, CancellationToken cancellationToken)
        {
            var response = await _httpClient.DeleteAsync($"{_endpointPath}/{orderGuid}");
            response.EnsureSuccessStatusCode();
        }
    }
}
