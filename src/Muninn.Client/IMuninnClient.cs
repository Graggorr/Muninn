namespace Muninn;

public interface IMuninnClient
{
    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> AddAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> InsertAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, TimeSpan lifeTime, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> UpdateAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> RemoveAsync<T>(string key, CancellationToken cancellationToken = default);

    public Task<MuninnResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    public Task<IEnumerable<T>> GetAllAsync<T>(CancellationToken cancellationToken = default);
}
