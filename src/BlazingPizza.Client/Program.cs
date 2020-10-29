using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Authentication.WebAssembly.AppService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = GetBaseAddress(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddHttpClient<OrdersClient>(client => client.BaseAddress = GetBaseAddress(builder.HostEnvironment.BaseAddress));
            builder.Services.AddScoped<OrderState>();

            builder.Services.AddStaticWebAppsAuthentication<PizzaAuthenticationState, RemoteUserAccount, EasyAuthOptions>();

            await builder.Build().RunAsync();
        }

        private static Uri GetBaseAddress(string baseAddress)
        {
            if (baseAddress.Contains("localhost"))
            {
                return new Uri("http://localhost:7071");
            }

            if (baseAddress.Contains("codespaces"))
            {
                return new Uri(baseAddress.Replace("5000", "7071"));
            }

            return new Uri(baseAddress);
        }
    }
}
