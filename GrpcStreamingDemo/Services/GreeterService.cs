using Grpc.Core;
using GrpcStreamingDemo;

namespace GrpcStreamingDemo.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private static readonly List<User> _users = new();
    private static int _nextId = 1;
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

    // Day 10: Metadata & Headers
    // Note: Changed to 'async Task<HelloReply>' to support awaiting WriteResponseHeadersAsync
    public override async Task<HelloReply> SayHelloWithMetadata(
        HelloRequest request,
        ServerCallContext context)
    {
        // Reading metadata from client request
        var metadata = context.RequestHeaders;

        string userAgent = metadata.GetValue("user-agent") ?? "Unknown";
        string authToken = metadata.GetValue("authorization") ?? "";
        string customHeader = metadata.GetValue("x-custom-header") ?? "Not provided";
        string acceptLanguage = metadata.GetValue("accept-language") ?? "fa";

        _logger.LogInformation(
            "Metadata received - User-Agent: {UserAgent}, Language: {Language}, Custom: {Custom}",
            userAgent, acceptLanguage, customHeader);

        // Simple token validation (demo purpose)
        if (!string.IsNullOrEmpty(authToken) && authToken.StartsWith("Bearer "))
        {
            _logger.LogInformation("Authentication token validated successfully.");
        }

        // CORRECT WAY: Create a Metadata collection for response headers
        var responseHeaders = new Grpc.Core.Metadata
    {
        { "x-server-version", "1.0.10" },
        { "x-processed-by", "GrpcStreamingDemo-Day10" },
        { "x-response-time", DateTime.UtcNow.ToString("o") }
    };

        // Send the response headers immediately before the main response message
        await context.WriteResponseHeadersAsync(responseHeaders);

        // Sending trailers (data sent after main response - this property exists!)
        context.ResponseTrailers.Add("x-total-requests-today", "42");
        context.ResponseTrailers.Add("x-session-id", Guid.NewGuid().ToString());

        // Removed Task.FromResult because the method is now async
        return new HelloReply
        {
            Message = $"Hello {request.Name}! Metadata processed successfully. Language: {acceptLanguage}",
            Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            Success = true
        };
    }

    // Day 16: CRUD - Create User
    public override Task<UserReply> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var user = new User
        {
            Id = _nextId++,
            Name = request.Name,
            Email = request.Email,
            Role = request.Role ?? "User",
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        _users.Add(user);

        _logger.LogInformation("User created: {Name} (ID: {Id})", user.Name, user.Id);

        return Task.FromResult(new UserReply
        {
            User = user,
            Success = true,
            Message = "User created successfully",
            Timestamp = DateTime.UtcNow.ToString("o")
        });
    }

    // Day 16: Get User
    public override Task<UserReply> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = _users.FirstOrDefault(u => u.Id == request.Id);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.Id} not found."));
        }

        return Task.FromResult(new UserReply
        {
            User = user,
            Success = true,
            Message = "User retrieved successfully",
            Timestamp = DateTime.UtcNow.ToString("o")
        });
    }

    // Day 17: Update User
    public override Task<UserReply> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var user = _users.FirstOrDefault(u => u.Id == request.Id);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.Id} not found."));
        }

        // Update fields
        if (!string.IsNullOrEmpty(request.Name)) user.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.Role)) user.Role = request.Role;

        _logger.LogInformation("User updated: {Name} (ID: {Id})", user.Name, user.Id);

        return Task.FromResult(new UserReply
        {
            User = user,
            Success = true,
            Message = "User updated successfully",
            Timestamp = DateTime.UtcNow.ToString("o")
        });
    }

    // Day 17: Delete User
    public override Task<DeleteReply> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var user = _users.FirstOrDefault(u => u.Id == request.Id);

        if (user == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"User with ID {request.Id} not found."));
        }

        _users.Remove(user);
        _logger.LogInformation("User deleted: ID {Id}", request.Id);

        return Task.FromResult(new DeleteReply
        {
            Success = true,
            Message = $"User with ID {request.Id} deleted successfully"
        });
    }

    // Day 17: Server Streaming - Stream All Users
    public override async Task StreamUsers(
        Empty request,
        IServerStreamWriter<UserReply> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation("Starting Server Streaming of all users...");

        foreach (var user in _users)
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            var reply = new UserReply
            {
                User = user,
                Success = true,
                Message = "User streamed",
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            await responseStream.WriteAsync(reply);
            _logger.LogInformation("Streamed user: {Name} (ID: {Id})", user.Name, user.Id);

            await Task.Delay(600);
        }

        _logger.LogInformation("Server Streaming completed.");
    }

    // Day 18: Bidirectional Streaming - User Events
    public override async Task UserEvents(
        IAsyncStreamReader<UserEventRequest> requestStream,
        IServerStreamWriter<UserEventReply> responseStream,
        ServerCallContext context)
    {
        _logger.LogInformation("Bidirectional streaming started - waiting for user events...");

        await foreach (var eventRequest in requestStream.ReadAllAsync(context.CancellationToken))
        {
            if (context.CancellationToken.IsCancellationRequested)
                break;

            _logger.LogInformation(
                "Received event: {Action} for user {Name}",
                eventRequest.Action,
                eventRequest.User?.Name);

            User? processedUser = null;

            // Process event
            if (eventRequest.Action == "create" && eventRequest.User != null)
            {
                var newUser = new User
                {
                    Id = _nextId++,
                    Name = eventRequest.User.Name,
                    Email = eventRequest.User.Email,
                    Role = eventRequest.User.Role,
                    CreatedAt = DateTime.UtcNow.ToString("o")
                };

                _users.Add(newUser);
                processedUser = newUser;
            }
            else if (eventRequest.Action == "update" && eventRequest.User != null)
            {
                var user = _users.FirstOrDefault(u => u.Id == eventRequest.User.Id);

                if (user != null)
                {
                    user.Name = eventRequest.User.Name ?? user.Name;
                    user.Email = eventRequest.User.Email ?? user.Email;
                    user.Role = eventRequest.User.Role ?? user.Role;

                    processedUser = user;
                }
            }

            // Send immediate response back to client
            var reply = new UserEventReply
            {
                EventType = eventRequest.Action,
                User = processedUser,
                Timestamp = DateTime.UtcNow.ToString("o")
            };

            await responseStream.WriteAsync(reply);

            _logger.LogInformation("Event processed and returned: {Action}", eventRequest.Action);

            await Task.Delay(400); // Simulated processing delay
        }

        _logger.LogInformation("Bidirectional streaming completed.");
    }
}