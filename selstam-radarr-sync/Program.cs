using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace selstam_radarr_sync
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logConfiguration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(logConfiguration)
                .CreateLogger();

            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            try
            {
                serviceProvider.GetService<RadarrSync>().Run();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error occured.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(LoadConfiguration());

            services.AddTransient<IRadarrService, RadarrService>();
            services.AddTransient<RadarrSync>();

            return services;
        }

        private static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false);

            return builder.Build();
        }
    }
}