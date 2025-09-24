using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Options;
using Muninn.Server.Shared;

namespace Muninn.Api.Grpc.Interceptors;

public class MuninnServiceInterceptor(ILogger<Interceptor> logger, IOptions<MuninnConfiguration> configuration)
    : Interceptor
{
    private readonly ILogger _logger = logger;
    private readonly MuninnConfiguration _configuration = configuration.Value;

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var key = context.RequestHeaders.GetValue("x-api-key");

        if (_configuration.ApiKey.Equals(key))
        {
            return base.UnaryServerHandler(request, context, continuation);
        }

        throw new UnauthorizedAccessException("Invalid API key");
    }
}