using FakeStoreOrderProcessor.Library.DTO.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Repositories.Interfaces
{
    public interface IOrderRepository : IApiRepository<OrderDto, CreateOrderDto, UpdateOrderDto>
    {
        public Task<IEnumerable<OrderDto>> GetAllDayBeforeAsync(CancellationToken cancellationToken);
        public Task<IEnumerable<OrderDto>> GetAllActiveOrNotAsync(CancellationToken cancellationToken);
        public Task<OrderDto?> GetByGuidAsync(string orderGuid, CancellationToken cancellationToken);
        public Task<OrderWithOrderItemsDto?> GetByGuidWithOrderItemsAsync(string orderGuid, CancellationToken cancellationToken);
        public Task PatchOrderAsync(string orderGuid, UpdateOrderDto orderDto, CancellationToken cancellationToken);
        public Task DeleteWithGuidAsync(string orderGuid, CancellationToken cancellationToken);
    }
}
