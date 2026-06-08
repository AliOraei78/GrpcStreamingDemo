using Grpc.Core;
using Grpc.Core.Interceptors;

namespace GrpcStreamingDemo.Services;

public class ValidationInterceptor : Interceptor
{
    private readonly ILogger<ValidationInterceptor> _logger;

    public ValidationInterceptor(ILogger<ValidationInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        // General validation (e.g., Name field)
        if (request is HelloRequest helloRequest)
        {
            if (string.IsNullOrWhiteSpace(helloRequest.Name))
            {
                _logger.LogWarning("Validation failed: Name is empty");
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Username cannot be empty."));
            }

            if (helloRequest.Name.Length < 3)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Name must be at least 3 characters long."));
            }
        }

        return await continuation(request, context);
    }
}