using Grpc.Net.Client;
using Grpc.Core;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Error Handling Demo - Day 7 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

try
{
    // Test 1: Valid request
    Console.WriteLine("Test 1: Valid request");
    var validRequest = new HelloRequest
    {
        Name = "Mohammad Jenabi",
        Email = "mohammad@example.com"
    };

    var validReply = await client.SayHelloWithValidationAsync(validRequest);
    Console.WriteLine($"✅ Success: {validReply.Message}\n");

    // Test 2: Invalid request (error case)
    Console.WriteLine("Test 2: Invalid request");
    var invalidRequest = new HelloRequest { Name = "ab" }; // too short name

    await client.SayHelloWithValidationAsync(invalidRequest);
}
catch (RpcException ex)
{
    Console.WriteLine($"❌ RpcException occurred!");
    Console.WriteLine($"StatusCode: {ex.StatusCode}");
    Console.WriteLine($"Message: {ex.Status.Detail}");

    foreach (var entry in ex.Trailers)
    {
        Console.WriteLine($"Metadata: {entry.Key} = {entry.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();