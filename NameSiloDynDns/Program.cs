using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NameSiloDynDns.HttpClientSetup;
using NameSiloDynDns.NameSilo;
using NameSiloDynDns.Services;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using System.Threading.Tasks;

namespace NameSiloDynDns
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .UseSerilog((host, config) => config.ReadFrom.Configuration(host.Configuration))
                .ConfigureServices((host, services) =>
                {
                    var hostToUpdateConfigruration = host.Configuration.GetSection("HostToUpdate").Get<HostToUpdate>(IncludePrivateProperties);

                    services.AddHttpClient<NameSiloHttpClient>()
                        .ConfigureNameSiloHttpLogging()
                        .AddPolicyHandler(HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .WaitAndRetryAsync(retryCount: hostToUpdateConfigruration.RetryAttempts,
                                retryAttempt => hostToUpdateConfigruration.RetryTimeSpan));

                    services
                        .AddSingleton(host.Configuration.GetSection("NameSiloApi").Get<ApiConfiguration>(IncludePrivateProperties))
                        .AddSingleton(hostToUpdateConfigruration)
                        .AddTransient<NameSiloRepository>()
                        .AddHostedService<UpdateService>()
                        ;
                })
                .RunConsoleAsync();
        }

        static void IncludePrivateProperties(BinderOptions options) => options.BindNonPublicProperties = true;
    }
}
