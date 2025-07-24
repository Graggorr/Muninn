using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Persistent;

namespace Muninn.Kernel;

internal class CacheManager(IPersistentCache persistentCache, IResidentCache residentCache, IPersistentQueue persistentQueue) : ICacheManager
{
    private readonly IPersistentCache _persistentCache = persistentCache;
    private readonly IResidentCache _residentCache = residentCache;
    private readonly IPersistentQueue _persistentQueue = persistentQueue;
    private bool _isInitialized = false;

    public Task<MuninResult> AddAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = _residentCache.Add(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!));
        }

        return Task.FromResult(residentResult);
    }

    public Task<MuninResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var residentResult = _residentCache.Remove(key, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _persistentQueue.EnqueueDeletionAsync(new(new(key, [])));
        }

        return Task.FromResult(residentResult);
    }

    public async Task<MuninResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var residentResult = _residentCache.Get(key, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            return residentResult;
        }

        return await _persistentCache.GetAsync(key, cancellationToken);
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken)
    {
        var entries = _residentCache.GetAll(cancellationToken).ToList();

        return !entries.Any() ? await _persistentCache.GetAllAsync(cancellationToken) : entries;
    }

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        return null;
    }

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        return null;
    }

    public Task<MuninResult> UpdateAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = _residentCache.Update(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!));
        }

        return Task.FromResult(residentResult);
    }

    public Task<MuninResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = _residentCache.Insert(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _persistentQueue.EnqueueInsertionAsync(new(residentResult.Entry!));
        }

        return Task.FromResult(residentResult);
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        _isInitialized = true;
        var entries = await _persistentCache.GetAllAsync(CancellationToken.None);
        _residentCache.Initialize(entries.ToArray());
    }

    public void Clear(CancellationToken cancellationToken)
    {
        _residentCache.Clear();
        _persistentCache.ClearAsync(cancellationToken);
    }
}
