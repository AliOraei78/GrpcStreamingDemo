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

    // New method for error handling / validation demo
    public override Task<HelloReply> SayHelloWithValidation(HelloRequest request, ServerCallContext context)
    {
        // Robust validation logic
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "User name cannot be empty."),
                new Metadata { { "error-detail", "Name field is required" } }
            );
        }

        if (request.Name.Length < 3)
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Name must be at least 3 characters long.")
            );
        }

        if (!string.IsNullOrEmpty(request.Email) && !request.Email.Contains("@"))
        {
            throw new RpcException(
                new Status(StatusCode.InvalidArgument, "Invalid email format."),
                new Metadata { { "validation-error", "Invalid email format" } }
            );
        }

        _logger.LogInformation("Validated request successfully: {Name}", request.Name);

        return Task.FromResult(new HelloReply
        {
            Message = $"Hello {request.Name}! Validation completed successfully.",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Success = true
        });
    }

    public override async Task<HelloReply> SayHelloWithDelay(HelloRequest request, ServerCallContext context)
    {
        _logger.LogInformation(
            "Starting delayed operation for {Name} - requested delay: {Delay} seconds",
            request.Name,
            request.DelaySeconds);

        int delay = request.DelaySeconds > 0 ? request.DelaySeconds : 5;

        try
        {
            // Simulate long-running work
            for (int i = 1; i <= delay; i++)
            {
                // Check for cancellation
                if (context.CancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Operation cancelled by client.");
                    throw new RpcException(
                        new Status(StatusCode.Cancelled, "Operation was cancelled by the user."));
                }

                await Task.Delay(1000, context.CancellationToken);
                _logger.LogInformation("Second {i} of {delay} elapsed", i, delay);
            }

            return new HelloReply
            {
                Message = $"Operation with {delay}s delay completed successfully.",
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Success = true
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation cancelled via exception.");

            throw new RpcException(
                new Status(StatusCode.Cancelled, "Operation was cancelled."));
        }
    }
}