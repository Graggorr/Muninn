using Grpc.Core;
using Muninn.Kernel.Common;
using System.Collections.Immutable;
using Muninn.Grpc.Contracts.Extensions;
using Muninn.Grpc;

namespace Muninn.Api.Grpc.Services;

public class MuninnStreamService(ICacheManager cacheManager) : MuninnStream.MuninnStreamBase
{
    private readonly ICacheManager _cacheManager = cacheManager;
    private const int MAX_PARALLEL_THREADS = 30;

    public override async Task GetAllAsync(GetAllRequest request, IServerStreamWriter<GetAllReply> responseStream, ServerCallContext context)
    {
        var entries = await _cacheManager.GetAllAsync(context.CancellationToken);
        var sortedSet = entries.ToImmutableSortedSet();

        if (sortedSet.IsEmpty)
        {
            return;
        }

        await Parallel.ForEachAsync(sortedSet, new ParallelOptions
        {
            CancellationToken = context.CancellationToken,
            MaxDegreeOfParallelism = sortedSet.Count > MAX_PARALLEL_THREADS ? MAX_PARALLEL_THREADS : sortedSet.Count,
        }, async (entry, cancellationToken) =>
        {
            await responseStream.WriteAsync(new()
            {
                EncodingName = entry.Encoding.EncodingName,
                Key = entry.Key,
                Value = entry.Value.ToByteString(),
            }, cancellationToken);
        });
    }

    public override async Task DeleteAllAsync(DeleteAllRequest request, IServerStreamWriter<DeleteAllReply> responseStream, ServerCallContext context)
    {
        if (request.ReturnValues)
        {
            var entities = await _cacheManager.GetAllAsync(context.CancellationToken);
            var mappedResponses = entities.Select(entity => new DeleteAllReply
            {
                EncodingName = entity.Encoding.EncodingName,
                Key = entity.Key,
                Value = entity.Value.ToByteString(),
            }).ToList();

            if (mappedResponses.Count > 0)
            {
                await Parallel.ForEachAsync(mappedResponses, new ParallelOptions
                {
                    CancellationToken = context.CancellationToken,
                    MaxDegreeOfParallelism = mappedResponses.Count > MAX_PARALLEL_THREADS ? MAX_PARALLEL_THREADS : mappedResponses.Count,
                }, async (mappedResponse, cancellationToken) => await responseStream.WriteAsync(mappedResponse, cancellationToken));
            }
        }

        await _cacheManager.ClearAsync(context.CancellationToken);
    }
}
