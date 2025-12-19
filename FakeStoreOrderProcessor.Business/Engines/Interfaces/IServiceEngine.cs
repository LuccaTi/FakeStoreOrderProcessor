using FakeStoreOrderProcessor.Library.DTO.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Engines.Interfaces
{
    public interface IServiceEngine
    {
        public Task ProcessAsync(CancellationToken cancellationToken);
        public Task ProcessSingleProductFile(string file, bool moveToProcessing, CancellationToken cancellationToken);
        public Task ProcessSingleOrderCreatedFile(string file, bool moveToProcessing, CancellationToken cancellationToken);
        public Task ProcessSingleOrderPaymentFile(string file, bool moveToProcessing, CancellationToken cancellationToken);
        public Task ProcessSingleOrderShippedFile(string file, bool moveToProcessing, CancellationToken cancellationToken);
        public Task ProcessSingleOrderDeliveredFile(string file, bool moveToProcessing, CancellationToken cancellationToken);
        public Task ProcessAddress(OrderCreatedDto? orderToRegister, CancellationToken cancellationToken);
        public Task ProcessCustomer(OrderCreatedDto? orderToRegister, string street, int number, CancellationToken cancellationToken);
        public Task CancelOrders(CancellationToken cancellationToken);
    }
}
