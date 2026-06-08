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

    // Previous methods (Unary, Server Streaming, Client Streaming) unchanged...

    // New method: Bidirectional Streaming
    public override async Task SayHelloBidirectional(
        IAsyncStreamReader<HelloRequest> requestStream,
        IServerStreamWriter<HelloReply> responseStream,
        ServerCallContext context)
    {
        int receivedCount = 0;

        _logger.LogInformation("Starting Bidirectional Streaming...");

        // Task for reading requests and sending responses concurrently
        var readTask = Task.Run(async () =>
        {
            await foreach (var request in requestStream.ReadAllAsync(context.CancellationToken))
            {
                receivedCount++;
                _logger.LogInformation("Received message from client: {Message}", request.Message);

                // Send immediate response back to client
                var reply = new HelloReply
                {
                    Message = $"Server received: {request.Message}",
                    Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    MessageCount = receivedCount,
                    Success = true,
                    TotalReceived = receivedCount
                };

                await responseStream.WriteAsync(reply);

                // Simulated processing delay
                await Task.Delay(600);
            }
        });

        await readTask;

        _logger.LogInformation(
            "Bidirectional Streaming completed. Total received: {Count}",
            receivedCount);
    }
}