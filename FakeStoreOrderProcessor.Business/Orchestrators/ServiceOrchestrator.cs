using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FakeStoreOrderProcessor.Business.Engines.Interfaces;
using FakeStoreOrderProcessor.Business.Orchestrators.Interfaces;

namespace FakeStoreOrderProcessor.Business.Orchestrators
{
    public class ServiceOrchestrator : IServiceOrchestrator
    {
        private const string _className = "ServiceOrchestrator";
        private readonly ILogger<ServiceOrchestrator> _logger;
        private readonly IServiceEngine _serviceProcessingEngine;
        private readonly IOptions<ServiceSettings> _settings;

        public ServiceOrchestrator(ILogger<ServiceOrchestrator> logger, IServiceEngine serviceProcessingEngine, IOptions<ServiceSettings> settings)
        {
            _logger = logger;
            _serviceProcessingEngine = serviceProcessingEngine;
            _settings = settings;
        }

        public async Task EventHandlerAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _serviceProcessingEngine.ProcessAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"{_className} - EventHandlerAsync - Application termination by signaling and cancellation token");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"{_className} - EventHandlerAsync - An unhandled exception has ocurred, {ex.Message}");
                }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_settings.Value.Interval), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
