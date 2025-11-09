using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Muninn.Grpc.Contracts.Extensions;
using Muninn.Grpc.Extensions;
using static Muninn.Grpc.MuninnService;

namespace Muninn.Grpc;

internal class MuninnClientGrpc(ILogger<IMuninnClient> logger, IOptions<MuninnConfiguration> configuration,
    MuninnServiceClient client) : IMuninnClientGrpc
{
    private readonly ILogger _logger = logger;
    private readonly Encoding _encoding = Encoding.GetEncoding(configuration.Value.EncodingName);
    private readonly MuninnServiceClient _client = client;
    private readonly TimeSpan _defaultLifeTime = TimeSpan.FromHours(1);

    public async Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new AddRequest
            {
                EncodingName = encoding.EncodingName,
                Key = key,
                LifeTime = lifeTime.ToDuration(),
                Value = BinarySerializer.Serialize(value, encoding).ToByteString(),
            };
            var reply = await _client.AddAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return GetResult<T>(reply.IsSuccessful, reply.Value);
        }
        catch (Exception exception)
        {
            _logger.LogAddAsyncError(key, exception);
            
            return GetResult<T>(exception);
        }
    }

    public async Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InsertRequest
            {
                EncodingName = encoding.EncodingName,
                Key = key,
                LifeTime = lifeTime.ToDuration(),
                Value = BinarySerializer.Serialize(value, encoding).ToByteString(),
            };
            var reply = await _client.InsertAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return GetResult<T>(reply.IsSuccessful, reply.Value);
        }
        catch (Exception exception)
        {
            _logger.LogInsertAsyncError(key, exception);
            
            return GetResult<T>(exception);
        }
    }

    public async Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new UpdateRequest
            {
                EncodingName = encoding.EncodingName,
                Key = key,
                LifeTime = lifeTime.ToDuration(),
                Value = BinarySerializer.Serialize(value, encoding).ToByteString(),
            };
            var reply = await _client.UpdateAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return GetResult<T>(reply.IsSuccessful, reply.Value);
        }
        catch (Exception exception)
        {
            _logger.LogUpdateAsyncError(key, exception);
            
            return GetResult<T>(exception);
        }
    }

    public async Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteRequest
            {
                Key = key,
            };
            var reply = await _client.DeleteAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return GetResult<T>(reply.IsSuccessful, reply.Value, Encoding.GetEncoding(reply.EncodingName));
        }
        catch (Exception exception)
        {
            _logger.LogRemoveAsyncError(key, exception);
            
            return GetResult<T>(exception);
        }
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteAllRequest();
            var reply = await _client.DeleteAllAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            return new MuninnResult(reply.IsSuccessful, reply.Message);
        }
        catch (Exception exception)
        {
            _logger.LogClearAsyncError(exception);
            
            return GetResult(exception);
        }
    }

    public async Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetRequest
            {
                Key = key,
            };
            var reply = await _client.GetAsync(request, cancellationToken: cancellationToken);

            return GetResult<T>(reply.IsSuccessful, reply.Value, Encoding.GetEncoding(reply.EncodingName));
        }
        catch (Exception exception)
        {
            return GetResult<T>(exception);
        }
    }

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => AddAsync(key, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => AddAsync(key, value, _defaultLifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default) 
        => InsertAsync(key, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => InsertAsync(key, value, _defaultLifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => UpdateAsync(key, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => UpdateAsync(key, value, _defaultLifeTime, _encoding, cancellationToken);

    private static MuninnResult<T> GetResult<T>(Exception exception) => new(false, exception.Message, default);

    private static MuninnResult GetResult(Exception exception) => new(false, exception.Message);

    private MuninnResult<T> GetResult<T>(bool isSuccessful, ByteString encodedValue) =>
        GetResult<T>(isSuccessful, encodedValue, _encoding);

    private static MuninnResult<T> GetResult<T>(bool isSuccessful, ByteString encodedValue, Encoding encoding)
    {
        var value = default(T);
        var errorMessage = string.Empty;
        var array = encodedValue.ToByteArray();
        
        if (isSuccessful)
        {
            value = BinarySerializer.Deserialize<T>(array, encoding);
        }
        else
        {
            errorMessage = BinarySerializer.Deserialize<string>(array, encoding);
        }

        return new MuninnResult<T>(isSuccessful, errorMessage, value);
    }
}