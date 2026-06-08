using Grpc.Net.Client;
using GrpcStreamingDemo;

using var channel = GrpcChannel.ForAddress("https://localhost:7007");
var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(
    new HelloRequest { Name = "Dear User" }
);

Console.WriteLine("Server response: " + reply.Message);

Console.WriteLine("Press any key to exit...");
Console.ReadKey();