using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class ResidentCache(ILogger<IResidentCache> logger, IFilterService filterService)
    : BaseCache<IResidentCache>(logger, filterService), IResidentCache
{
    private class ResidentCacheIndex
    {
        public bool IsUsed { get; set; }
    }

    private const int NotValidIndex = -1;

    protected Entry?[] _entries = new Entry[DefaultIncreaseValue];
    private ResidentCacheIndex[] _indexes = CreateIndexes(DefaultIncreaseValue);
    
    private bool _isResizeCalled;
    private int _count;

    public int Count
    {
        get
        {
            _semaphoreSlim.Wait();
            var value = _count;
            _semaphoreSlim.Release(1);

            return value;
        }
    }

    public int Length => _entries.Length;
    
    public virtual async Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);
        
        if (index is not NotValidIndex)
        {
            return GetFailedResult($"Entry with a key {entry.Key} already exists", false);
        }

        if (freeIndex is NotValidIndex)
        {
            var increaseResult = await IncreaseArraySizeAsync(cancellationToken);

            if (!increaseResult.IsSuccessful)
            {
                return increaseResult;
            }
            
            freeIndex = GetFreeIndex();
        }

        return await AddCoreAsync(entry, freeIndex, cancellationToken);
    }

    public virtual async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var hashcode = key.GetHashCode();
        var index = TryFindIndex(hashcode, out _);

        if (index is NotValidIndex)
        {
            return new MuninnResult(true, null);
        }

        try
        {
            var entry = _entries[index];
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = null;
            _indexes[index].IsUsed = false;
            _count--;
            
            return new MuninnResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedKeyDelete(key, exception);

            return GetFailedResult($"Cannot remove entry with key {key}", false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    public virtual MuninnResult Get(string key, CancellationToken cancellationToken = default)
    {
        var index = TryFindIndex(key.GetHashCode(), out _);

        if (cancellationToken.IsCancellationRequested)
        {
            return GetCancelledResult(nameof(Get), key, new("Cancellation has been requested"));
        }
        
        if (index is NotValidIndex)
        {
            return GetFailedResult($"Key {key} is not found", false);
        }

        var entry = _entries[index];

        return new MuninnResult(entry is not null, entry);
    }

    public virtual IEnumerable<Entry> GetAll(CancellationToken cancellationToken = default)
    {
        return _entries.Where(entry => entry is not null)!;
    }

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken = default)
    {
        return _filterService.FilterEntryKeys(_entries, chunks, cancellationToken);
    }

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default)
    {
        return _filterService.FilterEntryValues(_entries, chunks, cancellationToken);
    }

    public virtual async Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var index = TryFindIndex(entry.Hashcode, out _);

        return index is NotValidIndex 
            ? GetFailedResult("Entry is not found in the cache", false)
            : await UpdateCoreAsync(entry, index, cancellationToken);
    }

    public virtual async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is NotValidIndex)
        {
            if (freeIndex is NotValidIndex)
            {
                var increaseResult = await IncreaseArraySizeAsync(cancellationToken);

                if (!increaseResult.IsSuccessful)
                {
                    return increaseResult;
                }
            
                freeIndex = GetFreeIndex();
            }
            
            return await AddCoreAsync(entry, freeIndex, cancellationToken);
        }

        return await UpdateCoreAsync(entry, index, cancellationToken);
    }

    public virtual async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries = new Entry[DefaultIncreaseValue];
            _indexes = CreateIndexes(DefaultIncreaseValue);
            _count = 0;
            GC.Collect();

            return new(true, null);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(nameof(ClearAsync), operationCanceledException);

            return GetFailedResult(operationCanceledException.Message, true, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogClearAsyncError(exception);

            return GetFailedResult(exception.Message, false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    public async Task InitializeAsync(Entry[] entries, CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        if (entries.Length < DefaultIncreaseValue)
        {
            _entries = new Entry[DefaultIncreaseValue];

            for (var i = 0; i < entries.Length; i++)
            {
                _entries[i] = entries[i];
            }
        }
        else
        {
            _entries = entries;
        }

        _indexes = new ResidentCacheIndex[_entries.Length];

        for (var i = 0; i < _indexes.Length; i++)
        {
            _indexes[i] = new ResidentCacheIndex
            {
                IsUsed = _entries[i] is not null
            };
        }

        _semaphoreSlim.Release(1);
        GC.Collect();
    }

    public async Task<MuninnResult> IncreaseArraySizeAsync(CancellationToken cancellationToken = default)
    {
        if (_isResizeCalled)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _semaphoreSlim.Release(1);

            return GetSuccessfulResult();
        }

        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _isResizeCalled = true;
            var size = _count + DefaultIncreaseValue;
            var entries = new Entry?[size];
            var indexes = new ResidentCacheIndex[size];

            for (var i = 0; i < _entries.Length; i++)
            {
                entries[i] = _entries[i];
                indexes[i] = _indexes[i];
                indexes[i + _indexes.Length] = new();
            }

            _entries = entries;
            _indexes = indexes;
            _logger.LogIncreasedSize(size);

            return GetSuccessfulResult();
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(IncreaseArraySizeAsync), operationCanceledException.Message, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogIncreaseArraySizeAsyncError(exception);
            
            return GetFailedResult("Cannot increase size of array", false, exception);
        }
        finally
        {
            _isResizeCalled = false;
            _semaphoreSlim.Release(1);
        }
    }
    
    public async Task<MuninnResult> DecreaseArraySizeAsync(CancellationToken cancellationToken = default)
    {
        if (_isResizeCalled)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _semaphoreSlim.Release(1);

            return GetSuccessfulResult();
        }
        
        var size = _count + DefaultIncreaseValue;
        var array = new Entry?[size];
        var indexes = new ResidentCacheIndex[size];

        try
        {
            for (var i = 0; i < _count; i++)
            {
                array[i] = _entries[i];
                indexes[i] = new()
                {
                    IsUsed = array[i] is not null
                };
            }

            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries = array;
            _indexes = indexes;
            _logger.LogDecreasedSize(size);

            return GetSuccessfulResult();
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(DecreaseArraySizeAsync), operationCanceledException.Message, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogDecreaseArraySizeAsyncError(exception);
            
            return GetFailedResult("Cannot decrease array size", false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }
    
    private int TryFindIndex(int hashcode, out int freeIndex)
    {
        freeIndex = NotValidIndex;
        var entries = _entries;
        var indexes = _indexes;
        
        for (var i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];

            if (entry is not null)
            {
                if (entry.Hashcode == hashcode)
                {
                    return i;
                }

                continue;
            }

            freeIndex = i;
        }

        if (freeIndex is not NotValidIndex)
        {
            if (!indexes[freeIndex].IsUsed)
            {
                indexes[freeIndex].IsUsed = true;
            }
            else
            {
                freeIndex = NotValidIndex;

                for (var i = freeIndex + 1; i < indexes.Length; i++)
                {
                    if (!indexes[i].IsUsed)
                    {
                        indexes[i].IsUsed = true;
                        freeIndex = i;

                        return NotValidIndex;
                    }
                }
            }
        }

        return NotValidIndex;
    }

    private int GetFreeIndex()
    {
        for (var i = _indexes.Length - 1; i >= 0; i--)
        {
            if (!_indexes[i].IsUsed)
            {
                _indexes[i].IsUsed = true;

                return i;
            }
        }

        var oldEntry = _entries.OrderBy(entry => entry?.LastModificationTime).First();
        var index = Array.IndexOf(_entries, oldEntry);

        return index;
    }

    private async Task<MuninnResult> AddCoreAsync(Entry entry, int index, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = entry;
            _count++;
            _logger.LogKeyAdd(entry.Key);

            return new MuninnResult(true, entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(entry.Key, operationCanceledException);

            return GetCancelledResult(nameof(AddCoreAsync), entry.Key, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogFailedKeyInsert(entry.Key, exception);
            _indexes[index].IsUsed = false;

            return new MuninnResult(false, null, "Cannot add new entry into the cache", exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    private async Task<MuninnResult> UpdateCoreAsync(Entry entry, int index, CancellationToken cancellationToken = default)
    {
        Entry? oldEntry = null;

        try
        {
            oldEntry = _entries[index]!;
            entry.CreationTime = oldEntry.CreationTime;
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = entry;
            _logger.LogKeyUpdate(entry.Key, entry.Value);

            return new MuninnResult(true, entry);
        }
        catch (Exception exception)
        {
            if (oldEntry is not null)
            {
                _entries[index] = oldEntry;
            }

            _logger.LogFailedKeyUpdate(entry.Key, exception);

            return new MuninnResult(false, null, "Cannot update entry in the cache", exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    private static ResidentCacheIndex[] CreateIndexes(int count)
    {
        var indexes = new ResidentCacheIndex[count];

        for (var i = 0; i < count; i++)
        {
            indexes[i] = new ResidentCacheIndex();
        }

        return indexes;
    }
    
    Task<MuninnResult> IBaseCache.GetAsync(string key, CancellationToken cancellationToken) => Task.Factory.StartNew(() => Get(key, cancellationToken), cancellationToken);
}
