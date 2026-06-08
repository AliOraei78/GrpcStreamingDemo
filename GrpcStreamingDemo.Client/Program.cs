using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Interceptors Demo - Day 11 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

Console.WriteLine("1. Validation test");
Console.WriteLine("------------------------------------------");

// Test 1: Invalid name (should fail)
try
{
    var invalidRequest = new HelloRequest { Name = "Al" };
    var reply1 = await client.SayHelloAsync(invalidRequest);
    Console.WriteLine("Unexpected success!");
}
catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
{
    Console.WriteLine($"✅ Validation Interceptor worked correctly: {ex.Status.Detail}");
}

// Test 2: Valid request
try
{
    var validRequest = new HelloRequest
    {
        Name = "Ali Jenabi",
        Email = "ali@example.com"
    };

    Console.WriteLine("\n2. Valid request (with Logging Interceptor)");
    var reply2 = await client.SayHelloAsync(validRequest);

    Console.WriteLine($"Success: {reply2.Message}");
    Console.WriteLine($"Timestamp: {reply2.Timestamp}");
}
catch (RpcException ex)
{
    Console.WriteLine($"Error: {ex.Status.Detail}");
}

Console.WriteLine("\n3. Metadata test (Day 10)");
try
{
    var metadata = new Metadata
    {
        { "authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." },
        { "x-custom-header", "Test-Interceptor-Value" },
        { "accept-language", "fa-IR" },
        { "user-agent", "GrpcClient-Day11" }
    };

    var reply3 = await client.SayHelloWithMetadataAsync(
        new HelloRequest { Name = "Reza Ahmadi" },
        headers: metadata
    );

    Console.WriteLine($"Metadata test success: {reply3.Message}");
}
catch (RpcException ex)
{
    Console.WriteLine($"Metadata error: {ex.Status.Detail}");
}

Console.WriteLine("\nInterceptor tests completed successfully.");
Console.WriteLine("Check server logs for LoggingInterceptor and ValidationInterceptor output.");

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();