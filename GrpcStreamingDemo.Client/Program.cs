using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== CRUD Demo - Day 16 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    // Create User
    var createReply = await client.CreateUserAsync(new CreateUserRequest
    {
        Name = "Ali Jenabi",
        Email = "Ali@example.com",
        Role = "Admin"
    });

    Console.WriteLine($"✅ User Created - ID: {createReply.User.Id}");

    // Get User
    var getReply = await client.GetUserAsync(new GetUserRequest { Id = createReply.User.Id });
    Console.WriteLine($"✅ User Retrieved: {getReply.User.Name} - {getReply.User.Email}");
}
catch (RpcException ex)
{
    Console.WriteLine($"❌ Error: {ex.Status.Detail}");
}

Console.ReadKey();