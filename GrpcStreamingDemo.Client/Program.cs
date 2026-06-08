using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Client Streaming Demo - Day 5 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007"); // check service port
var client = new Greeter.GreeterClient(channel);

try
{
    using var call = client.SayHelloClientStream();

    Console.WriteLine("Sending multiple messages to server...\n");

    for (int i = 1; i <= 10; i++)
    {
        var request = new HelloRequest
        {
            Name = "Ali Jenabi",
            Message = $"Message #{i} from client",
            Language = "C#",
            Email = "ali@example.com"
        };

        await call.RequestStream.WriteAsync(request);
        Console.WriteLine($"📤 Message {i} sent");

        await Task.Delay(400); // simulate delay
    }

    await call.RequestStream.CompleteAsync(); // signal end of stream

    Console.WriteLine("\nWaiting for final server response...");

    var response = await call;

    Console.WriteLine("\n✅ Final server response:");
    Console.WriteLine($"Message: {response.Message}");
    Console.WriteLine($"Total Received: {response.TotalReceived}");
    Console.WriteLine($"Timestamp: {response.Timestamp}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();