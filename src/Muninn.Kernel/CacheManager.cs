using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel;

internal class CacheManager(IPersistentCache persistentCache, IResidentCache residentCache,
    IBackgroundManager persistentQueue, ISortedResidentCache sortedResidentCache) : ICacheManager
{
    private readonly IPersistentCache _persistentCache = persistentCache;
    private readonly IResidentCache _residentCache = residentCache;
    private readonly IBackgroundManager _persistentQueue = persistentQueue;
    private readonly ISortedResidentCache _sortedResidentCache = sortedResidentCache;

    private bool _isInitialized;

    public async Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.AddAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _sortedResidentCache.AddAsync(entry, cancellationToken);
            _ = _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!), cancellationToken);
        }

        return residentResult;
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.RemoveAsync(key, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _sortedResidentCache.RemoveAsync(key, cancellationToken);
            _ = _persistentQueue.EnqueueDeletionAsync(new(new(key, [], null!, TimeSpan.Zero)), cancellationToken);
        }

        return residentResult;
    }

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<MuninnResult>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.Get(key, cancellationToken), cancellationToken),
            Task.Factory.StartNew(() => _sortedResidentCache.Get(key), cancellationToken)
        };

        return GetCoreAsync(tasks, result => result.IsSuccessful);
    }

    public async Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<IEnumerable<Entry>>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.GetEntriesByKeyFilters(chunks, cancellationToken), cancellationToken),
            _persistentCache.GetEntriesByKeyFiltersAsync(chunks, cancellationToken)
        };

        return await GetCoreAsync(tasks, entries => entries.Any());
    }

    public async Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<IEnumerable<Entry>>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.GetEntriesByValueFilters(chunks, cancellationToken), cancellationToken),
            _persistentCache.GetEntriesByValueFiltersAsync(chunks, cancellationToken)
        };

        return await GetCoreAsync(tasks, entries => entries.Any());
    }

    public async Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.UpdateAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!), cancellationToken);
        }

        return residentResult;
    }

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.InsertAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!), cancellationToken);
        }

        return residentResult;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        _persistentCache.Initialize();
        var entries = await _persistentCache.GetAllAsync(CancellationToken.None);
        await _residentCache.InitializeAsync(entries.ToArray());
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken)
    {
        var tasks = new List<Task<MuninnResult>>
        {
            _residentCache.ClearAsync(cancellationToken),
            _persistentCache.ClearAsync(cancellationToken),
        };
        await Task.WhenAll(tasks);

        var failedTask = tasks.FirstOrDefault(task => !task.Result.IsSuccessful);

        return failedTask?.Result ?? new(true, null);
    }

    private static async Task<T> GetCoreAsync<T>(List<Task<T>> tasks, Func<T, bool> successfulCondition)
    {
        var task = await Task.WhenAny(tasks);
        var result = await task;

        return successfulCondition(result) 
            ? result 
            : await tasks.First(other => !other.Equals(task));
    }
}
