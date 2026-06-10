using Grpc.Core;
using GrpcStreamingDemo.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GrpcStreamingDemo.Tests.Services;

public class GreeterServiceTests
{
    private readonly GreeterService _service;
    private readonly Mock<ILogger<GreeterService>> _loggerMock;

    public GreeterServiceTests()
    {
        _loggerMock = new Mock<ILogger<GreeterService>>();
        _service = new GreeterService(_loggerMock.Object);
    }

    [Fact]
    public async Task SayHello_ShouldReturnGreetingMessage()
    {
        // Arrange
        var request = new HelloRequest { Name = "Ali Jenabi" };
        var context = new TestServerCallContext();

        // Act
        var reply = await _service.SayHello(request, context);

        // Assert
        Assert.NotNull(reply);
        Assert.Contains("Hello Ali Jenabi", reply.Message);
        Assert.True(reply.Success);
        Assert.Equal(1, reply.MessageCount);
    }

    [Fact]
    public async Task SayHelloWithValidation_InvalidName_ShouldThrowRpcException()
    {
        // Arrange
        var request = new HelloRequest { Name = "Al" }; // Less than 3 characters
        var context = new TestServerCallContext();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RpcException>(
            async () => await _service.SayHelloWithValidation(request, context));

        Assert.Equal(StatusCode.InvalidArgument, exception.StatusCode);
    }

    [Theory]
    [InlineData("Ali")]
    [InlineData("Reza Ahmadi")]
    public async Task SayHelloWithValidation_ValidName_ShouldSucceed(string name)
    {
        var request = new HelloRequest { Name = name };
        var context = new TestServerCallContext();

        var reply = await _service.SayHelloWithValidation(request, context);

        Assert.True(reply.Success);
        Assert.Contains(name, reply.Message);
    }
}

// Helper class for TestServerCallContext
public class TestServerCallContext : ServerCallContext
{
    protected override string MethodCore => "/greet.Greeter/SayHello";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "127.0.0.1";
    protected override DateTime DeadlineCore => DateTime.UtcNow.AddSeconds(30);
    protected override Metadata RequestHeadersCore => new Metadata();
    protected override CancellationToken CancellationTokenCore => CancellationToken.None;
    protected override Metadata ResponseTrailersCore => new Metadata();
    protected override Status StatusCore { get; set; } = new Status(StatusCode.OK, "");
    protected override WriteOptions? WriteOptionsCore { get; set; }

    // Fix CS0534: Implement missing abstract member for AuthContext
    protected override AuthContext AuthContextCore => null!;

    // Fix CS0534: Implement missing abstract member for PropagationToken
    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options) => null!;

    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }
}