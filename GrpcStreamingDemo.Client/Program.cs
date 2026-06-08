using Grpc.Net.Client;
using Grpc.Core;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Deadlines and Cancellation Demo - Day 8 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007"); // ensure correct server port
var client = new Greeter.GreeterClient(channel);

try
{
    var request = new HelloRequest
    {
        Name = "Ali Jenabi",
        DelaySeconds = 8 // 8 seconds delay
    };

    // Set deadline (e.g., 4 seconds)
    var deadline = DateTime.UtcNow.AddSeconds(4);

    Console.WriteLine("Sending request with 4-second deadline...");

    var reply = await client.SayHelloWithDelayAsync(
        request,
        deadline: deadline,
        cancellationToken: new CancellationTokenSource(5000).Token
    );

    Console.WriteLine($"✅ Success: {reply.Message}");
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
{
    Console.WriteLine("⏰ DeadlineExceeded: request timed out!");
    Console.WriteLine($"Message: {ex.Status.Detail}");
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
{
    Console.WriteLine("🚫 Operation cancelled");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Other error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();