namespace Muninn;

/// <summary>
/// Represents an interlayer between client and muninn server
/// </summary>
public interface IMuninnClient
{
    /// <summary>
    /// Adds a new item into the cache under the specified key
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="lifeTime">Period of lifetime of the value</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new item into the cache under the specified key
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an item into the cache either it exists there or not
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="lifeTime">Period of lifetime of the value</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts an item into the cache either it exists there or not
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an item under the existing key in the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="lifeTime">Period of lifetime of the value</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an item under the existing key in the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an item from the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="key">A unique identity for value</param>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="MuninnResult{T}"/></returns>
    public Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all existed items from the cache
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="cancellationToken"></param>
    /// <returns>An instance of <see cref="IEnumerable{T}"/></returns>
    public Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken cancellationToken = default);
}
