using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Services.Interfaces
{
    public interface IApiService
    {
        IProductRepository Products { get; }
        IProcessedFileLogRepository ProcessedFileLogs { get; }
        IAddressRepository Addresses { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
    }
}
