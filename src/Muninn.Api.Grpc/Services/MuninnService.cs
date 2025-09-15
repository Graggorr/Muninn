using System.Text;
using Google.Protobuf;
using Grpc.Core;
using Muninn.Grpc;
using Muninn.Grpc.Contracts.Extensions;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using static Muninn.Grpc.MuninnService;

namespace Muninn.Api.Grpc.Services;

public class MuninnService(ICacheManager cacheManager) : MuninnServiceBase
{
    private readonly ICacheManager _cacheManager = cacheManager;

    public override async Task<AddReply> Add(AddRequest request, ServerCallContext context)
    {
        var encoding = Encoding.GetEncoding(request.EncodingName);
        var entry = new Entry(request.Key, request.Value.ToByteArray(), encoding, request.LifeTime.ToTimeSpan());
        var result = await _cacheManager.AddAsync(entry, context.CancellationToken);

        return new()
        {
            IsSuccessful = result.IsSuccessful,
            Value = encoding.GetBytes(result.Message).ToByteString(),
        };
    }

    public override async Task<InsertReply> Insert(InsertRequest request, ServerCallContext context)
    {
        var encoding = Encoding.GetEncoding(request.EncodingName);
        var entry = new Entry(request.Key, request.Value.ToByteArray(), encoding, request.LifeTime.ToTimeSpan());
        var result = await _cacheManager.InsertAsync(entry, context.CancellationToken);

        return new()
        {
            IsSuccessful = result.IsSuccessful,
            Value = encoding.GetBytes(result.Message).ToByteString(),
        };
    }

    public override async Task<GetReply> Get(GetRequest request, ServerCallContext context)
    {
        var result = await _cacheManager.GetAsync(request.Key, context.CancellationToken);
        var reply = new GetReply
        {
            IsSuccessful = result.IsSuccessful
        };

        if (result.Entry is not null)
        {
            reply.EncodingName = result.Entry.Encoding.EncodingName;
            reply.Value = result.Entry.Value.ToByteString();
        }
        else
        {
            reply.Value = ByteString.Empty;
            reply.EncodingName = string.Empty;
        }

        return reply;
    }

    public override async Task<UpdateReply> Update(UpdateRequest request, ServerCallContext context)
    {
        var encoding = Encoding.GetEncoding(request.EncodingName);
        var entry = new Entry(request.Key, request.Value.ToByteArray(), encoding, request.LifeTime.ToTimeSpan());
        var result = await _cacheManager.UpdateAsync(entry, context.CancellationToken);

        return new()
        {
            IsSuccessful = result.IsSuccessful,
            Value = encoding.GetBytes(result.Message).ToByteString(),
        };
    }

    public override async Task<DeleteReply> Delete(DeleteRequest request, ServerCallContext context)
    {
        var result = await _cacheManager.RemoveAsync(request.Key, context.CancellationToken);
        var reply = new DeleteReply
        {
            IsSuccessful = result.IsSuccessful
        };

        if (result.Entry is not null)
        {
            reply.EncodingName = result.Entry.Encoding.EncodingName;
            reply.Value = result.Entry.Value.ToByteString();
        }
        else
        {
            reply.Value = ByteString.Empty;
            reply.EncodingName = string.Empty;
        }

        return reply;
    }
}
