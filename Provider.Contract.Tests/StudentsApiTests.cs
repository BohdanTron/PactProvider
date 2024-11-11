using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

namespace Provider.Contract.Tests
{
    public class StudentsApiTests : IClassFixture<StudentsApiFixture>
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        private readonly StudentsApiFixture _fixture;
        private readonly ITestOutputHelper _output;

        public StudentsApiTests(StudentsApiFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public void EnsureStudentsApiHonorsPactWithConsumer()
        {
            // Arrange
            var pactConfig = new PactVerifierConfig
            {
                Outputters = new List<IOutput>
                {
                    new XunitOutput(_output)
                },
                LogLevel = PactLogLevel.Debug
            };

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<StudentsApiTests>()
                .Build();

            var pactBrokerUrl = configuration["PactBroker:Url"] ?? Environment.GetEnvironmentVariable("PACT_BROKER_BASE_URL");
            var pactBrokerToken = configuration["PactBroker:Token"] ?? Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN");

            var shouldPublishResults = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PACT_BROKER_PUBLISH_VERIFICATIONS_RESULT"));
            var version = Environment.GetEnvironmentVariable("GIT_COMMIT");
            var branch = Environment.GetEnvironmentVariable("BRANCH_NAME");
            var buildUri = Environment.GetEnvironmentVariable("BUILD_URL");

            // Act / Assert
            var pactVerifier = new PactVerifier("StudentApi", pactConfig);

            pactVerifier
                .WithHttpEndpoint(_fixture.ServerUri)
                .WithMessages(scenarios =>
                {
                    scenarios.Add("an event indicating that a student has been created", () => new StudentCreatedEvent(10));
                }, Options)
                .WithPactBrokerSource(new Uri(pactBrokerUrl), options =>
                {
                    options.TokenAuthentication(pactBrokerToken);
                    options.PublishResults(shouldPublishResults, version, publishOptions =>
                    {
                        publishOptions.ProviderBranch(branch);
                        publishOptions.BuildUri(new Uri(buildUri));
                    });
                })
                .WithProviderStateUrl(new Uri(_fixture.ServerUri, "/provider-states"))
                .Verify();
        }
    }
}