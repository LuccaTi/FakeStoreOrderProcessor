using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using System.Timers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FakeStoreOrderProcessor.Business.Orchestrators.Interfaces;


namespace FakeStoreOrderProcessor.Business
{
    public class ServiceLifeCycleManager : BackgroundService
    {
        private const string _className = "ServiceLifeCycleManager";
        private List<Task> _tasks = new();
        private readonly ILogger<ServiceLifeCycleManager> _logger;
        private readonly IOptions<ServiceSettings> _settings;
        private readonly IEnumerable<IServiceOrchestrator> _orchestrators;

        public ServiceLifeCycleManager(ILogger<ServiceLifeCycleManager> logger, IOptions<ServiceSettings> settings, IEnumerable<IServiceOrchestrator> orchestrators)
        {
            _logger = logger;
            _settings = settings;
            _orchestrators = orchestrators;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_className} - ExecuteAsync - Application started successfully!");
            _logger.LogInformation($"{_className} - ExecuteAsync - Starting processing tasks...");

            foreach(var orchestrator in _orchestrators)
            {
                _tasks.Add(orchestrator.EventHandlerAsync(cancellationToken));
            }

            await Task.WhenAll(_tasks);

            _logger.LogInformation($"{_className} - ExecuteAsync - All tasks concluded, stopping service...");
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_className} - StopAsync - Stop signal received");
            await base.StopAsync(cancellationToken);
            _logger.LogInformation($"{_className} - StopAsync - Service finalized.");
        }
    }
}
