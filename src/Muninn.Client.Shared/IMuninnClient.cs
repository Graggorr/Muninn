using System.Text;

namespace Muninn;

/// <summary>
/// Represents an interlayer between client and Muninn server
/// </summary>
public interface IMuninnClient
{
    /// <summary>
    /// Adds a new item into the cache under the specified key with chosen <paramref name="encoding"/>
    /// </summary>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">Period of lifetime of the <paramref name="value"/></param>
    /// <param name="encoding">An instance for encoding of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new item into the cache under the specified key with default <see cref="Encoding"/>
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">A time which determines living period of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new item into the cache under the specified key with default <see cref="Encoding"/>
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a new item into the cache under the specified <paramref name="key"/> with specified <paramref name="encoding"/>
    /// </summary>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">A time which determines living period of <paramref name="value"/></param>
    /// <param name="encoding">An instance for encoding of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <typeparam name="T"></typeparam>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inserts an item into the cache either it exists there or not
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">A time which determines living period of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an item into the cache either it exists there or not
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an item under the existing <paramref name="key"/> in the cache
    /// </summary>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">Period of lifetime of the <paramref name="value"/></param>
    /// <param name="encoding">An instance for encoding of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, Encoding encoding, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an item under the existing key in the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="lifeTime">A time which determines living period of <paramref name="value"/></param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an item under the existing key in the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the <paramref name="value"/></typeparam>
    /// <param name="key">A unique identity for <paramref name="value"/></param>
    /// <param name="value">Data to be saved in the cache</param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears cache
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>Completed <see cref="Task"/></returns>
    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an item from the cache by the specified <paramref name="key"/>
    /// </summary>
    /// <typeparam name="T">Generic type of the returned value</typeparam>
    /// <param name="key">A unique identity for returned value</param>
    /// <param name="cancellationToken">A specified instance to cancel operation due to timeout</param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);
}
