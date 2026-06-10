using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== Full CRUD + Bidirectional Streaming Demo - Day 18 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    using var bidirectional = client.UserEvents();
    Console.WriteLine("Starting Bidirectional Streaming...");

    // 1. Reading responses is delegated to a background Task
    // so the main thread does not get blocked
    var readTask = Task.Run(async () =>
    {
        try
        {
            await foreach (var response in bidirectional.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"Event Response → {response.EventType} | User: {response.User?.Name}");
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            Console.WriteLine("Response stream cancelled.");
        }
    });

    // 2. Main thread continues sending messages
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

    // Small delay to allow first response to appear in console
    await Task.Delay(500);

    // 3. Other CRUD operations can run normally because main flow is free
    var createReply = await client.CreateUserAsync(new CreateUserRequest
    {
        Name = "Omid Hosseini",
        Email = "omid@example.com"
    });

    Console.WriteLine($"\nCreated User ID: {createReply.User.Id}");

    // If GetAllUsers is implemented on the server:
    // var allUsers = await client.GetAllUsersAsync(new Empty());
    // Console.WriteLine($"Total users in system: {allUsers.Users.Count}");

    // 4. Finally, signal that we are done sending messages
    await bidirectional.RequestStream.CompleteAsync();

    // Wait for the reading task to finish
    await readTask;
}
catch (RpcException ex)
{
    Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
}

Console.WriteLine("\nFull CRUD + Streaming scenario completed.");
Console.ReadKey();