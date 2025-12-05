using FakeStoreOrderProcessor.Business.Configuration;
using FakeStoreOrderProcessor.Business.Logging;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Reflection.Metadata.Ecma335;
using System.Timers;
using FakeStoreOrderProcessor.Business.Interfaces;


namespace FakeStoreOrderProcessor.Business
{
    public class ServiceLifeCycleManager
    {
        #region Attributes
        private const string _className = "ServiceLifeCycleManager";
        private List<Task> _tasks;
        #endregion

        #region Dependencies
        private readonly IServiceProcessingOrchestrator _orchestrator;
        #endregion

        public ServiceLifeCycleManager(IServiceProcessingOrchestrator orchestrator)
        {
            try
            {
                _tasks = new List<Task>();
                _orchestrator = orchestrator;
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "ServiceLifeCycleManager constructor", $"Error: {ex.ToString()}{Environment.NewLine}Application will be terminated by TopShelf!");
                throw;
            }
        }

        #region Methods
        public void Start()
        {
            try
            {
                Logger.Info("Application started successfully!");
                CreateWorkThreads();
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "Start", $"Error: {ex.Message}");
                throw;
            }

        }

        private void CreateWorkThreads()
        {
            try
            {
                _tasks.Add(Task.Run(_orchestrator.EventHandlerAsync));
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "CreateWorkThreads", $"Error: {ex.Message}");
                throw;
            }
        }
        public void Stop()
        {
            try
            {
                Logger.Info("Request to stop received, stopping application...");
                _orchestrator.SignalStop();
                Dispose();
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "Stop", $"Error: {ex.Message}");
                throw;
            }
        }
        public void Dispose()
        {
            try
            {
                Task.WaitAll(_tasks.ToArray(), TimeSpan.FromSeconds(30));

                foreach (var task in _tasks)
                {
                    task.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(_className, "Dispose", $"Error: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}
