using GrpcStreamingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddLogging(); // Enables ILogger support

var app = builder.Build();

app.MapGrpcService<GreeterService>();

app.MapGet("/", () => "gRPC service is running. Use a gRPC client to call it.");

app.Run();