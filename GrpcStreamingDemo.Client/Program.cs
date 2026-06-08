using Grpc.Net.Client;
using GrpcStreamingDemo;

using var channel = GrpcChannel.ForAddress("https://localhost:7007"); // Check the Service project's port
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(new HelloRequest
{
    Name = "Beginner User",
    Language = "C#"
});

Console.WriteLine("Server Response: " + reply.Message);
Console.WriteLine("Timestamp: " + reply.Timestamp);
Console.WriteLine("Message Count: " + reply.MessageCount);

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();