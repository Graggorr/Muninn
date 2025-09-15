using System.Text;
using System.Text.Json;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muninn.Grpc.Contracts.Extensions;
using static Muninn.Grpc.MuninnStream;
using static Muninn.Grpc.MuninnService;

namespace Muninn.Grpc;

internal class MuninnClientGrpc(ILogger<IMuninnClient> logger, IOptions<MuninnConfiguration> configuration) : IMuninnClientGrpc
{
    private readonly ILogger _logger = logger;
    private readonly MuninnConfiguration _configuration = configuration.Value;
    private readonly Encoding _encoding = Encoding.GetEncoding(configuration.Value.EncodingName);
    private readonly TimeSpan _defaultLifeTime = TimeSpan.FromHours(1);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default) 
        => AddCoreAsync(key, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => AddCoreAsync(key, value, _defaultLifeTime, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => InsertCoreAsync(key, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => InsertCoreAsync(key, value, _defaultLifeTime, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => UpdateCoreAsync(key, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => UpdateCoreAsync(key, value, _defaultLifeTime, cancellationToken);

    public async Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var client = new MuninnServiceClient(GetGrpcChannel());
        var request = new DeleteRequest
        {
            Key = key,
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = await client.DeleteAsync(request, callOptions).ConfigureAwait(false);

        return GetResult<T>(reply.IsSuccessful, reply.Value, Encoding.GetEncoding(reply.EncodingName));
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        var client = new MuninnStreamClient(GetGrpcChannel());
        var request = new DeleteAllRequest
        {
            ReturnValues = false,
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = client.DeleteAllAsync(request, callOptions).ResponseStream.ReadAllAsync(cancellationToken).ConfigureAwait(false);
        await using var asyncEnumerator = reply.GetAsyncEnumerator();
        await asyncEnumerator.MoveNextAsync();

        return new MuninnResult(true);
    }

    public async Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var client = new MuninnServiceClient(GetGrpcChannel());
        var request = new GetRequest
        {
            Key = key,
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = await client.GetAsync(request, callOptions);

        return GetResult<T>(reply.IsSuccessful, reply.Value, Encoding.GetEncoding(reply.EncodingName));
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken cancellationToken = default)
    {
        var client = new MuninnStreamClient(GetGrpcChannel());
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var replies = client.GetAllAsync(new(), callOptions).ResponseStream.ReadAllAsync(cancellationToken);
        var list = new List<T>();

        await foreach (var reply in replies)
        {
            var encoding = Encoding.GetEncoding(reply.EncodingName);
            list.Add(JsonSerializer.Deserialize<T>(encoding.GetString(reply.Value.ToByteArray()), JsonSerializerOptions.Web)!);
        }

        return list;
    }

    private async Task<MuninnResult<T>> AddCoreAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken)
    {
        var client = new MuninnServiceClient(GetGrpcChannel());
        var request = new AddRequest
        {
            EncodingName = _encoding.EncodingName,
            Key = key,
            LifeTime = lifeTime.ToDuration(),
            Value = SerializeValue(value).ToByteString(),
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = await client.AddAsync(request, callOptions).ConfigureAwait(false);

        return GetResult<T>(reply.IsSuccessful, reply.Value);
    }

    private async Task<MuninnResult<T>> InsertCoreAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken)
    {
        var client = new MuninnServiceClient(GetGrpcChannel());
        var request = new InsertRequest
        {
            EncodingName = _encoding.EncodingName,
            Key = key,
            LifeTime = lifeTime.ToDuration(),
            Value = SerializeValue(value).ToByteString(),
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = await client.InsertAsync(request, callOptions).ConfigureAwait(false);

        return GetResult<T>(reply.IsSuccessful, reply.Value);
    }

    private async Task<MuninnResult<T>> UpdateCoreAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken)
    {
        var client = new MuninnServiceClient(GetGrpcChannel());
        var request = new UpdateRequest
        {
            EncodingName = _encoding.EncodingName,
            Key = key,
            LifeTime = lifeTime.ToDuration(),
            Value = SerializeValue(value).ToByteString(),
        };
        var callOptions = new CallOptions(cancellationToken: cancellationToken);
        var reply = await client.UpdateAsync(request, callOptions).ConfigureAwait(false);

        return GetResult<T>(reply.IsSuccessful, reply.Value);
    }

    private ChannelBase GetGrpcChannel()
    {
        return GrpcChannel.ForAddress(_configuration.HostName);
    }

    private MuninnResult<T> GetResult<T>(bool isSuccessful, ByteString encodedValue) => GetResult<T>(isSuccessful, encodedValue, _encoding);

    private static MuninnResult<T> GetResult<T>(bool isSuccessful, ByteString encodedValue, Encoding encoding)
    {
        var value = isSuccessful
            ? JsonSerializer.Deserialize<T>(encoding.GetString(encodedValue.ToByteArray()), JsonSerializerOptions.Web)
            : default;

        return new MuninnResult<T>(isSuccessful, value);
    }

    private byte[] SerializeValue<T>(T value)
    {
        var serializedValue = JsonSerializer.Serialize(value, JsonSerializerOptions.Web);

        return _encoding.GetBytes(serializedValue);
    }
}
