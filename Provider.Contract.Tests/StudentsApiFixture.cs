using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Provider.Contract.Tests.Fakes;
using Provider.Contract.Tests.Middlewares;

namespace Provider.Contract.Tests
{
    public class StudentsApiFixture : IAsyncLifetime
    {
        private WebApplication _server;

        public Uri ServerUri { get; } = new("http://localhost:9001");

        private readonly IConfiguration Configuration
            = new ConfigurationBuilder()
                .AddJsonFile("appsettings-Tests.json")
                .Build();

        public async Task InitializeAsync()
        {
            //Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var builder = WebApplication.CreateBuilder();

            builder.Environment.EnvironmentName = "Development";

            builder.Configuration.AddConfiguration(Configuration);
                
            builder.ConfigureServices();

            builder.WebHost.UseUrls(ServerUri.ToString());

            builder.Services.AddSingleton<IEventPublisher, EventPublisherFake>();

            _server = builder.Build();

            _server.UseMiddleware<ProviderStateMiddleware>();
            _server.Configure();

            await _server.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _server.DisposeAsync();
        }
    }
}
