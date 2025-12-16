using FakeStoreOrderProcessor.Business;
using FakeStoreOrderProcessor.Business.Engines;
using FakeStoreOrderProcessor.Business.Engines.Interfaces;
using FakeStoreOrderProcessor.Business.Orchestrators;
using FakeStoreOrderProcessor.Business.Orchestrators.Interfaces;
using FakeStoreOrderProcessor.Business.Repositories;
using FakeStoreOrderProcessor.Business.Repositories.Interfaces;
using FakeStoreOrderProcessor.Business.Services;
using FakeStoreOrderProcessor.Business.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Options;
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

                        services.AddHttpClient("ApiClient", (serviceProvider, client) =>
                        {
                            var settings = serviceProvider.GetRequiredService<IOptions<ServiceSettings>>().Value;

                            string? baseAddress = settings.ApiUrl;
                            if (string.IsNullOrEmpty(baseAddress))
                                throw new Exception("API URL was not provided!");

                            int timeout = settings.Timeout;
                            if (timeout == 0)
                                timeout = 30;

                            client.BaseAddress = new Uri(baseAddress);
                            client.Timeout = TimeSpan.FromSeconds(timeout);
                        });
                        services.AddScoped<IFileService, FileService>();
                        services.AddScoped<IApiService, ApiService>();
                        services.AddScoped<IProductRepository, ProductRepository>();
                        services.AddScoped<IProcessedFileLogRepository, ProcessedFileLogRepository>();
                        services.AddScoped<IAddressRepository, AddressRepository>();
                        services.AddScoped<ICustomerRepository, CustomerRepository>();

                        services.AddAutoMapper(typeof(BusinessAssemblyMarker).Assembly);
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
