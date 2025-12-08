using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeStoreOrderProcessor.Business.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FakeStoreOrderProcessor.Business.Engines
{
    public class ServiceEngine : IServiceEngine
    {
        private const string _className = "ServiceEngine";
        private readonly ILogger<ServiceEngine> _logger;
        public ServiceEngine(ILogger<ServiceEngine> logger)
        {
            _logger = logger;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"{_className} - Service testing...");
            /*while (!cancellationToken.IsCancellationRequested)
            {

            }*/
        }
    }
}
