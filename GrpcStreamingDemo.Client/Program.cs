using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingDemo;

Console.WriteLine("=== gRPC Metadata & Headers Demo - Day 10 ===\n");

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

var request = new HelloRequest
{
    Name = "Ali Jenabi",
    Email = "ali@example.com"
};

var headers = new Metadata
{
    { "authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." },
    { "x-custom-header", "Grpc-Demo-Value" },
    { "accept-language", "fa-IR" },
    { "user-agent", "GrpcClient-Day10" }
};

try
{
    var reply = await client.SayHelloWithMetadataAsync(request, headers: headers);

    Console.WriteLine($"Response: {reply.Message}");

    // Display response headers (typically accessed via CallOptions)
    Console.WriteLine("Metadata processing completed.");
}
catch (RpcException ex)
{
    Console.WriteLine($"Error: {ex.Status.Detail}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();