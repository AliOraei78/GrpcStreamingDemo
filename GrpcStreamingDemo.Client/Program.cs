using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== Full CRUD + Bidirectional Streaming Demo - Day 18 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    // Bidirectional Streaming
    using var bidirectional = client.UserEvents();

    Console.WriteLine("Starting Bidirectional Streaming...");

    // Send Create event
    await bidirectional.RequestStream.WriteAsync(new UserEventRequest
    {
        Action = "create",
        User = new User
        {
            Name = "Nazanin Karimi",
            Email = "nazanin@example.com",
            Role = "Editor"
        }
    });

    // Receive responses
    await foreach (var response in bidirectional.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Event Response → {response.EventType} | User: {response.User?.Name}");
    }

    // Additional CRUD tests
    var createReply = await client.CreateUserAsync(new CreateUserRequest
    {
        Name = "Omid Hosseini",
        Email = "omid@example.com"
    });

    Console.WriteLine($"\nCreated User ID: {createReply.User.Id}");

    var allUsers = await client.GetAllUsersAsync(new Empty()); // if implemented earlier
    Console.WriteLine($"Total users in system: {allUsers.Users.Count}");
}
catch (RpcException ex)
{
    Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nFull CRUD + Streaming scenario completed.");
Console.ReadKey();