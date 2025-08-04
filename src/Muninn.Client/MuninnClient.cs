using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Muninn;

internal class MuninnClient(ILogger<IMuninnClient> logger, IHttpClientFactory httpClientFactory,
    IOptions<MuninnConfiguration> muninnConfiguration) : IMuninnClient
{
    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
    private readonly record struct RequestBody<T>(T Value, TimeSpan LifeTime, string EncodingName);

    private readonly ILogger _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly MuninnConfiguration _muninnConfiguration = muninnConfiguration.Value;
    private readonly Encoding _encoding = Encoding.GetEncoding(muninnConfiguration.Value.EncodingName);
    private readonly TimeSpan _defaultLifeTime = TimeSpan.FromHours(1);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Post, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Post, value, _defaultLifeTime, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/insert/{key}", HttpMethod.Post, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/insert/{key}", HttpMethod.Post, value, _defaultLifeTime, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Put, value, lifeTime, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Put, value, _defaultLifeTime, cancellationToken);

    public Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default) => SendBodyLessAsync<T>($"muninn/{key}", HttpMethod.Delete, cancellationToken);

    public Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default) => SendBodyLessAsync<T>($"muninn/{key}", HttpMethod.Get, cancellationToken);

    public async Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "muninn");
        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response is null || !response.IsSuccessStatusCode)
        {
            return [];
        }

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return (await JsonSerializer.DeserializeAsync<IEnumerable<T>>(stream, JsonSerializerOptions.Web, cancellationToken).ConfigureAwait(false))!;
    }

    private async Task<MuninnResult<T>> SendBodyLessAsync<T>(string path, HttpMethod httpMethod, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(httpMethod, path);
        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await GetMuninnResultAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MuninnResult<T>> SendCommandAsync<T>(string path, HttpMethod httpMethod, T value, TimeSpan lifeTime, CancellationToken cancellationToken)
    {
        var body = new RequestBody<T>(value, lifeTime, _encoding.EncodingName);
        var serializedBody = JsonSerializer.Serialize(body);
        var request = new HttpRequestMessage(httpMethod, path)
        {
            Content = new StringContent(serializedBody, _encoding, "application/json")
        };

        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await GetMuninnResultAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MuninnResult<T>> GetMuninnResultAsync<T>(HttpResponseMessage? response, CancellationToken cancellationToken)
    {
        if (response is null)
        {
            return new MuninnResult<T>(false, default);
        }

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var value = await JsonSerializer.DeserializeAsync(stream, JsonTypeInfo.CreateJsonTypeInfo<T>(JsonSerializerOptions.Web), cancellationToken).ConfigureAwait(false);

            return new MuninnResult<T>(true, value);
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogBadRequest(response.RequestMessage!.RequestUri!.AbsolutePath, message);

        return new MuninnResult<T>(false, default);
    }

    private async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            using var httpClient = _httpClientFactory.CreateClient(nameof(MuninnClient));

            return await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            _logger.LogSendAsyncException(exception);

            return null;
        }
    }
}
