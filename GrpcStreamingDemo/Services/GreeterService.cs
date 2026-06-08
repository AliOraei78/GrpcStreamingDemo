using Grpc.Core;
using GrpcStreamingDemo;

namespace GrpcStreamingDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;

    public GreeterService(ILogger<GreeterService> logger)
    {
        _logger = logger;
    }

    // Previous Unary method (unchanged)
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Received Unary request from: {Name}", request.Name);

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}!",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            MessageCount = 1,
            Success = true
        });
    }

    // New method: Server Streaming
    public override async Task SayHelloServerStream(
        HelloRequest request,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "Starting Server Streaming for user: {Name} - Count: {Count}",
            request.Name,
            request.Count);

        int total = request.Count > 0 ? request.Count : 5; // default 5 messages

        for (int i = 1; i <= total; i++)
        {
            // Check if client cancelled the request
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Request was cancelled by the client.");
                break;
            }

            var reply = new HelloReply
            {
                Message = $"Message {i} of {total} - Hello {request.Name}!",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                MessageCount = i,
                Success = true
            };

            await responseStream.WriteAsync(reply);
            _logger.LogInformation("Sent message #{i}", i);

            // Simulate real streaming delay
            await Task.Delay(800);
        }

        _logger.LogInformation("Server Streaming completed.");
    }
}