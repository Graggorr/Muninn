using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Muninn;

/// <summary>
/// Represents an implementation of <see cref="IMuninnClient"/>
/// </summary>
/// <param name="logger">A configured instance for logs</param>
/// <param name="httpClientFactory">Factory of the <see cref="HttpClient"/></param>
/// <param name="muninnConfiguration">Configuration for server connection</param>
internal class MuninnClient(ILogger<IMuninnClient> logger, IHttpClientFactory httpClientFactory,
    IOptions<MuninnConfiguration> muninnConfiguration) : IMuninnClient
{
    private readonly ILogger _logger = logger;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly Encoding _encoding = Encoding.GetEncoding(muninnConfiguration.Value.EncodingName);
    private readonly TimeSpan _defaultLifeTime = TimeSpan.FromHours(1);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default) 
        => SendCommandAsync($"muninn/{key}", HttpMethod.Post, value, lifeTime, encoding, cancellationToken);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Post, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Post, value, _defaultLifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default) 
        => SendCommandAsync($"muninn/insert/{key}", HttpMethod.Post, value, lifeTime, encoding, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/insert/{key}", HttpMethod.Post, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/insert/{key}", HttpMethod.Post, value, _defaultLifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default) 
        => SendCommandAsync($"muninn/{key}", HttpMethod.Put, value, lifeTime, encoding, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Put, value, lifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        => SendCommandAsync($"muninn/{key}", HttpMethod.Put, value, _defaultLifeTime, _encoding, cancellationToken);

    public Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default) => SendBodyLessAsync<T>($"muninn/{key}", HttpMethod.Delete, cancellationToken);

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        var path = "muninn";
        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response is null)
        {
            return new MuninnResult(false, "Cannot get response from the Muninn cache service.");
        }

        var message = string.Empty;
        
        if (!response.IsSuccessStatusCode)
        {
            message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogBadRequest(path, message);
        }

        return new MuninnResult(response.IsSuccessStatusCode, message);
    }

    public Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default) => SendBodyLessAsync<T>($"muninn/{key}", HttpMethod.Get, cancellationToken);

    private async Task<MuninnResult<T>> SendBodyLessAsync<T>(string path, HttpMethod httpMethod, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(httpMethod, path);
        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await GetMuninnResultAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MuninnResult<T>> SendCommandAsync<T>(string path, HttpMethod httpMethod, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken)
    {
        var body = new RequestBody(BinarySerializer.Serialize(value, encoding), lifeTime, encoding.EncodingName);
        var serializedBody = JsonSerializer.Serialize(body, MuninnJsonSerializerContext.Default.RequestBody);
        var request = new HttpRequestMessage(httpMethod, path)
        {
            Content = new StringContent(serializedBody, encoding, "application/json")
        };

        var response = await SendAsync(request, cancellationToken).ConfigureAwait(false);

        return await GetMuninnResultAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MuninnResult<T>> GetMuninnResultAsync<T>(HttpResponseMessage? response, CancellationToken cancellationToken)
    {
        if (response is null)
        {
            return new MuninnResult<T>(false, "Cannot get response from the Muninn cache service.", default);
        }

        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var responseBody = (await JsonSerializer.DeserializeAsync(stream, MuninnJsonSerializerContext.Default.ResponseBody, cancellationToken)
                .ConfigureAwait(false))!;
            var value = BinarySerializer.Deserialize<T>(responseBody.value, Encoding.GetEncoding(responseBody.EncodingName));

            return new MuninnResult<T>(true, string.Empty, value);
        }

        var message = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogBadRequest(response.RequestMessage!.RequestUri!.AbsolutePath, message);

        return new MuninnResult<T>(false, message, default);
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
