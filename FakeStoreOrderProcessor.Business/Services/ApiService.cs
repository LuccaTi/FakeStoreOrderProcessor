using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FakeStoreOrderProcessor.Business.Services
{
    public class ApiService : IApiService
    {
        private const string _className = "ApiService";

        public IProductRepository Products { get; }
        public IProcessedFileLogRepository ProcessedFileLogs { get; }
        public IAddressRepository Addresses { get; }
        public ICustomerRepository Customers { get; }
        public IOrderRepository Orders { get; }

        public ApiService(IProductRepository productRepository, 
            IProcessedFileLogRepository processedFileLogRepository,
            IAddressRepository addressRepository,
            ICustomerRepository customersRepository,
            IOrderRepository orderRepository)
        {
            Products = productRepository;
            ProcessedFileLogs = processedFileLogRepository;
            Addresses = addressRepository;
            Customers = customersRepository;
            Orders = orderRepository;
        }
    }
}
