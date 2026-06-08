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

    // Previous methods (Unary and Server Streaming) remain unchanged

    // New method: Client Streaming
    public override async Task<HelloReply> SayHelloClientStream(
        IAsyncStreamReader<HelloRequest> requestStream,
        ServerCallContext context)
    {
        int receivedCount = 0;
        string lastName = string.Empty;

        _logger.LogInformation("Starting Client Streaming...");

        await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
        {
            receivedCount++;
            lastName = request.Name;

            _logger.LogInformation(
                "Received message #{Count} from {Name}: {Message}",
                receivedCount,
                request.Name,
                request.Message);

            // Simulate per-message processing
            await Task.Delay(300);
        }

        _logger.LogInformation(
            "Client Streaming completed. Total received: {Count}",
            receivedCount);

        return new HelloReply
        {
            Message = $"✅ {receivedCount} messages successfully received from {lastName}.",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            MessageCount = receivedCount,
            Success = true,
            TotalReceived = receivedCount
        };
    }
}