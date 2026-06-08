using GrpcStreamingDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Register gRPC and configure specific options for GreeterService
builder.Services.AddGrpc()
    .AddServiceOptions<GreeterService>(options =>
    {
        options.Interceptors.Add<LoggingInterceptor>();
        options.Interceptors.Add<ValidationInterceptor>();
    });

builder.Services.AddLogging();

// Register interceptors in DI container
builder.Services.AddScoped<LoggingInterceptor>();
builder.Services.AddScoped<ValidationInterceptor>();

var app = builder.Build();

// 2. Map the service cleanly
app.MapGrpcService<GreeterService>();

app.MapGet("/", () => "gRPC service is running. Use a gRPC client to call it.");

app.Run();