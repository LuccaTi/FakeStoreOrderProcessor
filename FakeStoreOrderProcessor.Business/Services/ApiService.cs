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

        public ApiService(IProductRepository productRepository)
        {
            Products = productRepository;
        }
    }
}
