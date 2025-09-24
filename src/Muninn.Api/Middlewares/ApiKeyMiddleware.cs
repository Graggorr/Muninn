using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Muninn.Api.Extensions;
using Muninn.Server.Shared;

namespace Muninn.Api.Middlewares;

public class ApiKeyMiddleware(ILogger<IMiddleware> logger, IOptions<MuninnConfiguration> configuration) : IMiddleware
{
    private readonly ILogger _logger = logger;
    private readonly MuninnConfiguration _configuration = configuration.Value;
    private const string InvalidIpAddress = "Cannot determine an IP address";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var apiKey = context.Request.Headers["x-api-key"].ToString();

        if (_configuration.ApiKey.Equals(apiKey, StringComparison.CurrentCulture))
        {
            return next(context);
        }

        _logger.LogInvalidApiKey(context.Connection.RemoteIpAddress?.ToString() ?? InvalidIpAddress);

        throw new AuthenticationFailureException("Invalid authorization");
    }
}
