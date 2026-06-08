using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Server Streaming Demo - Day 4 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007"); // check service port
var client = new Greeter.GreeterClient(channel);

try
{
    var request = new HelloRequest
    {
        Name = "Ali Jenabi",
        Language = "C#",
        Count = 8 // number of requested messages
    };

    Console.WriteLine("Sending Server Streaming request...\n");

    using var call = client.SayHelloServerStream(request);

    await foreach (var reply in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"📨 Message received: {reply.Message}");
        Console.WriteLine($"   Timestamp: {reply.Timestamp} | Counter: {reply.MessageCount}");
        Console.WriteLine("   ──────────────────────────────");
    }

    Console.WriteLine("\n✅ Server Streaming completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();