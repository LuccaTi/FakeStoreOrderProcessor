using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using FakeStoreOrderProcessor.Business;
using FakeStoreOrderProcessor.Business.Configuration;
using FakeStoreOrderProcessor.Business.Interfaces;
using FakeStoreOrderProcessor.Business.Logging;
using System;
using System.IO;
using Topshelf;

namespace FakeStoreOrderProcessor.Host
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Config.LoadConfig();

                // DI Container
                var services = new ServiceCollection();
                ConfigureServices(services);
                var serviceProvider = services.BuildServiceProvider();

                // Start TopShelf
                var exitCode = HostFactory.Run(hostConfig =>
                {
                    hostConfig.Service<ServiceLifeCycleManager>(config =>
                    {
                        config.ConstructUsing(work => serviceProvider.GetRequiredService<ServiceLifeCycleManager>());
                        config.WhenStarted((work, _) =>
                        {
                            Logger.Info("Starting application...");
                            work.Start();
                            return true;
                        });

                        config.WhenStopped((work, _) =>
                        {
                            work.Stop();
                            Logger.Info("Application terminated!");
                            return true;
                        });

                    });

                    if (Config.UseSerilog)
                    {
                        hostConfig.UseSerilog();
                    }

                    hostConfig.RunAsLocalSystem();
                    hostConfig.SetServiceName("Template");
                    hostConfig.SetDisplayName("Template");
                    hostConfig.SetDescription("Service template used to create Windows services.");

                });

                // For generic cases: (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
                int exitCodeValue = (int)exitCode;
                Environment.ExitCode = exitCodeValue;
                Console.WriteLine($"ExitCode: {Environment.ExitCode}");
            }
            catch (Exception ex)
            {
                // Terminates the service when errors are not caught by Topshelf at startup
                HandleStartupError(ex);
                Console.WriteLine($"{DateTime.Now} - Error: {ex}");
                Environment.Exit(1);
            }

        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceProcessingOrchestrator, ServiceProcessingOrchestrator>();
            services.AddSingleton<IServiceProcessingEngine, ServiceProcessingEngine>();
            services.AddSingleton<ServiceLifeCycleManager>();
        }

        private static void HandleStartupError(Exception exception)
        {
            // Creates a file due to the chance that the Logger may not have been initialized
            string fatalErrorDirectory = Path.Combine(AppContext.BaseDirectory, "StartupErrors");
            if (!Directory.Exists(fatalErrorDirectory))
                Directory.CreateDirectory(fatalErrorDirectory);

            string timeStamp = DateTime.Now.Date.ToString("yyyyMMdd");
            string file = Path.Combine(fatalErrorDirectory, $"{timeStamp}_ERROR_.txt");
            string errorMsg = $"{DateTime.Now} - Error during application startup: {exception.ToString()}{Environment.NewLine}";
            File.AppendAllText(file, errorMsg);
        }
    }
}
