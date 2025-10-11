using System.Text;
using Microsoft.AspNetCore.Mvc;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Api;

public static class Endpoints
{
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("muninn");
        group.MapPost("{key}", PostAsync);
        group.MapPost("{key}/insert", InsertAsync);
        group.MapPut("{key}", PutAsync);
        group.MapDelete("{key}", DeleteAsync);
        group.MapDelete(string.Empty, DeleteAllAsync);
        group.MapGet("{key}", GetAsync);

        return group;
    }

    private static async Task<IResult> PostAsync([FromBody] PostRequest request,
        [FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        var encoding = Encoding.GetEncoding(request.Body.EncodingName);
        var entry = new Entry(request.Key, encoding.GetBytes(request.Body.Value), encoding, request.Body.LifeTime);
        var result = await cacheManager.AddAsync(entry, cancellationToken);

        return GetResponse(result, true);
    }

    private static async Task<IResult> InsertAsync([FromBody] InsertRequest request,
        [FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        var encoding = Encoding.GetEncoding(request.Body.EncodingName);
        var entry = new Entry(request.Key, encoding.GetBytes(request.Body.Value), encoding, request.Body.LifeTime);
        var result = await cacheManager.InsertAsync(entry, cancellationToken);

        return GetResponse(result, true);
    }

    private static async Task<IResult> GetAsync([AsParameters] GetRequest request,
        [FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        var result = await cacheManager.GetAsync(request.Key, cancellationToken);

        return GetResponse(result, false);
    }

    private static async Task<IResult> PutAsync([AsParameters] PutRequest request,
        [FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        var encoding = Encoding.GetEncoding(request.Body.EncodingName);
        var entry = new Entry(request.Key, encoding.GetBytes(request.Body.Value), encoding, request.Body.LifeTime);
        var result = await cacheManager.UpdateAsync(entry, cancellationToken);

        return GetResponse(result, true);
    }

    private static async Task<IResult> DeleteAsync([AsParameters] DeleteRequest request,
        [FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        var result = await cacheManager.RemoveAsync(request.Key, cancellationToken);

        return GetResponse(result, true);
    }

    private static async Task<IResult> DeleteAllAsync([FromServices] ICacheManager cacheManager, CancellationToken cancellationToken)
    {
        await cacheManager.ClearAsync(cancellationToken);

        return Results.Ok();
    }

    private static IResult GetResponse(MuninnResult result, bool isCommand)
    {
        if (result is { IsSuccessful: true, Entry: not null })
        {
            return Results.Ok(new Response(result.Entry.Encoding.EncodingName, result.Entry.Value));
        }

        if (result.IsCancelled)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status408RequestTimeout,
                Detail = "Timeout has been risen",
            };

            return Results.Problem(problemDetails);
        }

        if (result.Exception is not null)
        {
            return Results.BadRequest(result.Exception.Message);
        }

        return isCommand ? Results.BadRequest(result.Message) : Results.NotFound(result.Message);
    }
}
