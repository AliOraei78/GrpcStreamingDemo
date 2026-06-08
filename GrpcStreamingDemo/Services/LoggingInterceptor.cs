using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcStreamingDemo.Services;

public class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;

    public LoggingInterceptor(ILogger<LoggingInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var methodName = context.Method;
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Starting call to {Method}", methodName);

        try
        {
            var response = await continuation(request, context);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Completed {Method} in {Duration}ms",
                methodName,
                duration.TotalMilliseconds);

            return response;
        }
        catch (RpcException ex)
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogWarning(
                "RpcException in {Method} | Status: {Status} | Detail: {Detail} | Duration: {Duration}ms",
                methodName,
                ex.StatusCode,
                ex.Status.Detail,
                duration.TotalMilliseconds);

            throw; // Important: must rethrow
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            _logger.LogError(
                ex,
                "Unexpected error in {Method} | Duration: {Duration}ms",
                methodName,
                duration.TotalMilliseconds);

            throw;
        }
    }

    public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
        TRequest request,
        IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context,
        ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        _logger.LogInformation("🌊 Starting Server Streaming: {Method}", context.Method);
        await continuation(request, responseStream, context);
        _logger.LogInformation("🌊 Server Streaming completed: {Method}", context.Method);
    }
}