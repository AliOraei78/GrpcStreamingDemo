using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== Clean Architecture Demo - Day 20 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

var createReply = await client.CreateUserAsync(new CreateUserRequest
{
    Name = "Fateme Mohammadi",
    Email = "fateme@example.com",
    Role = "Moderator"
});

Console.WriteLine($"✅ User Created - ID: {createReply.User.Id} | Name: {createReply.User.Name}");

var getReply = await client.GetUserAsync(new GetUserRequest { Id = createReply.User.Id });
Console.WriteLine($"✅ User Retrieved: {getReply.User.Name}");

Console.WriteLine("\n🎉 Clean Architecture implemented successfully!");
Console.ReadKey();