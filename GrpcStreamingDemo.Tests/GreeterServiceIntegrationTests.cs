using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;           // Important: this namespace
using GrpcStreamingDemo.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GrpcStreamingDemo.Tests.Integration;

public class GreeterServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly Greeter.GreeterClient _client;   // Correct: Greeter.GreeterClient

    public GreeterServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Override logger for the test environment (optional)
                services.AddSingleton<ILogger<GreeterService>, Logger<GreeterService>>();
            });
        });

        var httpClient = _factory.CreateDefaultClient();

        var channel = GrpcChannel.ForAddress(httpClient.BaseAddress!, new GrpcChannelOptions
        {
            HttpClient = httpClient
        });

        _client = new Greeter.GreeterClient(channel);
    }

    [Fact]
    public async Task SayHello_Integration_ShouldReturnGreeting()
    {
        // Arrange: Create the authentication metadata required by AuthInterceptor
        var headers = new Metadata { { "x-api-key", "my-secret-api-key-12345" } };
        var request = new HelloRequest { Name = "Ali Jenabi" };

        // Act: Pass the headers into the gRPC client call
        var reply = await _client.SayHelloAsync(request, headers: headers);

        // Assert
        Assert.NotNull(reply);
        Assert.Contains("Hello Ali Jenabi", reply.Message);
        Assert.True(reply.Success);
    }

    [Fact]
    public async Task SayHelloWithValidation_InvalidName_ShouldReturnError()
    {
        var request = new HelloRequest { Name = "Al" };

        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _client.SayHelloWithValidationAsync(request));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_Endpoint_ShouldReturnHealthy()
    {
        var response = await _factory.CreateClient().GetAsync("/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Healthy", content);
    }

    [Fact]
    public async Task AuthInterceptor_ValidApiKey_ShouldSucceed()
    {
        var headers = new Metadata { { "x-api-key", "my-secret-api-key-12345" } };
        var request = new HelloRequest { Name = "Secure User" };

        var reply = await _client.SayHelloAsync(request, headers: headers);

        Assert.True(reply.Success);
    }
}