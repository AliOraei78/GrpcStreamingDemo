using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Bidirectional Streaming Demo - Day 6 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    using var call = client.SayHelloBidirectional();

    Console.WriteLine("Bidirectional stream started. Sending and receiving messages...\n");

    // Task for sending messages from client
    var sendTask = Task.Run(async () =>
    {
        for (int i = 1; i <= 8; i++)
        {
            var request = new HelloRequest
            {
                Name = "Ali Rezaei",
                Message = $"Bidirectional message #{i} from client",
                Language = "C#"
            };

            await call.RequestStream.WriteAsync(request);
            Console.WriteLine($"📤 Sent message {i}");
            await Task.Delay(700);
        }

        await call.RequestStream.CompleteAsync();
        Console.WriteLine("✅ Client finished sending messages.");
    });

    // Receiving responses from server
    await foreach (var reply in call.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"📨 Received from server: {reply.Message}");
        Console.WriteLine($"   Counter: {reply.MessageCount} | Timestamp: {reply.Timestamp}");
        Console.WriteLine("   ──────────────────────────────");
    }

    await sendTask;

    Console.WriteLine("\n✅ Bidirectional streaming completed successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();