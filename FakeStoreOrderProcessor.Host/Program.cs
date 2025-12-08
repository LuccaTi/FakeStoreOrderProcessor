using FakeStoreOrderProcessor.Business;
using FakeStoreOrderProcessor.Business.Engines;
using FakeStoreOrderProcessor.Business.Interfaces;
using FakeStoreOrderProcessor.Business.Orchestrators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using System;
using System.IO;

namespace FakeStoreOrderProcessor.Host
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .Build())
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs/system_log_.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true);

            Log.Logger = loggerConfiguration.CreateBootstrapLogger();

            try
            {
                Log.Information("Starting service host configuration...");

                var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                    .UseWindowsService(options =>
                    {
                        options.ServiceName = "FakeStoreOrderProcessor";
                    })
                    .UseSerilog()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.Configure<ServiceSettings>(hostContext.Configuration.GetSection("ServiceSettings"));
                        services.AddHostedService<ServiceLifeCycleManager>();
                        services.AddSingleton<IServiceOrchestrator, ServiceOrchestrator>();
                        services.AddSingleton<IServiceEngine, ServiceEngine>();
                    })
                    .Build();

                Log.Information($"Host built successfully, starting service....");

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "A fatal error has occurred while starting service host!");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
