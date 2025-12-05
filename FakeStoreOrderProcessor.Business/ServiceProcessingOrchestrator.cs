using FakeStoreOrderProcessor.Business.Configuration;
using FakeStoreOrderProcessor.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeStoreOrderProcessor.Business.Logging;

namespace FakeStoreOrderProcessor.Business
{
    public class ServiceProcessingOrchestrator : IServiceProcessingOrchestrator
    {
        #region Attributes
        private const string _className = "ServiceProcessingOrchestrator";
        private readonly int _timerSeconds;
        private readonly CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Dependencies
        private readonly IServiceProcessingEngine _serviceProcessingEngine;
        #endregion

        public ServiceProcessingOrchestrator(IServiceProcessingEngine serviceProcessingEngine)
        {
            try
            {
                _timerSeconds = Config.Interval;
                _cancellationTokenSource = new CancellationTokenSource();
                _serviceProcessingEngine = serviceProcessingEngine;
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "ServiceProcessingOrchestrator constructor", $"Error: {ex.Message}");
                throw;
            }
        }

        #region Methods
        public async Task EventHandlerAsync()
        {
            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    await _serviceProcessingEngine.ProcessAsync(_cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.Debug(_className, "EventHandlerAsync", "Application termination by signaling and cancellation token");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error(_className, "EventHandlerAsync", $"Error: {ex.Message}");
                }
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_timerSeconds), _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
        public void SignalStop()
        {
            try
            {
                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "SignalStop", $"Error: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}
