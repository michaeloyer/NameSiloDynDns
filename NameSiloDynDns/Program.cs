using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NameSiloDynDns.HttpClientSetup;
using NameSiloDynDns.NameSilo;
using NameSiloDynDns.Services;
using Serilog;
using System.Threading.Tasks;

namespace NameSiloDynDns
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureHostConfiguration(config =>
                    config
                        .AddJsonFile("hostsettings.json", optional: true)
                        .AddEnvironmentVariables("ASPNETCORE_")
                        .AddCommandLine(args)
                )
                .ConfigureAppConfiguration((host, config) =>
                    config.AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{host.HostingEnvironment.EnvironmentName}.json", optional: true)
                )
                .UseSerilog((host, config) => config.ReadFrom.Configuration(host.Configuration))
                .ConfigureServices((host, services) =>
                {
                    services.AddHttpClient<NameSiloHttpClient>()
                        .ConfigureNameSiloHttpLogging();

                    services
                        .AddHostedService<UpdateService>()
                        .AddSingleton(host.Configuration.GetSection("NameSiloApi").Get<ApiConfiguration>(IncludePrivateProperties))
                        .AddSingleton(host.Configuration.GetSection("HostToUpdate").Get<HostToUpdate>(IncludePrivateProperties))
                        .AddScoped<NameSiloRepository>();
                })
                .RunConsoleAsync();
        }

        static void IncludePrivateProperties(BinderOptions options) => options.BindNonPublicProperties = true;
    }
}
