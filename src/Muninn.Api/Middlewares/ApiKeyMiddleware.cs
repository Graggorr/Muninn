using Microsoft.AspNetCore.Authentication;
using Muninn.Api.Extensions;

namespace Muninn.Api.Middlewares;

public class ApiKeyMiddleware(ILogger<IMiddleware> logger, MuninConfiguration configuration) : IMiddleware
{
    private readonly ILogger _logger = logger;

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var apiKey = context.Request.Headers["x-api-key"].ToString();

        if (configuration.ApiKey.Equals(apiKey, StringComparison.CurrentCulture))
        {
            return next(context);
        }

        _logger.LogInvalidApiKey(apiKey);

        throw new AuthenticationFailureException("Invalid authorization");
    }
}
