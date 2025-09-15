using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Resident;

namespace Muninn.Kernel;

internal class CacheManager(IPersistentCache persistentCache, IResidentCache residentCache, IBackgroundManager persistentQueue, ISortedResidentCache sortedResidentCache) : ICacheManager
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
            _ = _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!), cancellationToken);
        }

        return residentResult;
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.RemoveAsync(key, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _persistentQueue.EnqueueDeletionAsync(new(new(key, [], null!, TimeSpan.Zero)), cancellationToken);
        }

        return residentResult;
    }

    public async Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<MuninnResult>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.Get(key, cancellationToken), cancellationToken),
        };

        if (!_sortedResidentCache.IsSorting)
        {
            tasks.Add(Task.Factory.StartNew(() => _sortedResidentCache.GetByKey(key), cancellationToken));
        }

        return await GetCoreAsync(tasks, result => result.IsSuccessful && (!result.Message.Equals(SortedResidentCache.MESSAGE) || DateTime.UtcNow - result.Entry!.LastModificationTime > TimeSpan.FromMinutes(5)));
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entries = _residentCache.GetAll(cancellationToken).ToList();

        return entries.Any() ? entries : await _persistentCache.GetAllAsync(cancellationToken);
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

    public Task ClearAsync(CancellationToken cancellationToken) => Task.WhenAll(_residentCache.ClearAsync(cancellationToken), _persistentCache.ClearAsync(cancellationToken));

    private static async Task<T> GetCoreAsync<T>(List<Task<T>> tasks, Func<T, bool> successfulCondition)
    {
        var task = await Task.WhenAny(tasks);
        var result = await task;

        return successfulCondition(result) 
            ? result 
            : await tasks.First(other => !other.Equals(task));
    }
}
