using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class ResidentCache(ILogger<IResidentCache> logger, IFilterManager filterManager) : IResidentCache
{
    private class ResidentCacheIndex
    {
        public bool IsUsed { get; set; }
    }

    private const int NotValidIndex = -1;
    private const int DefaultIncreaseValue = 1000;
    
    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    private ResidentCacheIndex[] _indexes = CreateIndexes(DefaultIncreaseValue);
    private Entry?[] _entries = new Entry[DefaultIncreaseValue];
    private bool _isResizeCalled;
    private int _count;

    public int Count
    {
        get
        {
            _semaphoreSlim.Wait();
            
            return _count;
        }
        private set => _count = value;
    }

    public int Length => _entries.Length;
    
    public async Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is not NotValidIndex)
        {
            return GetFailedResult($"Entry with a key {entry.Key} already exists", false);
        }

        if (freeIndex is NotValidIndex)
        {
            if (!await IncreaseArraySizeAsync(cancellationToken))
            {
                return GetFailedResult("Cannot increase array size", false);
            }
            
            freeIndex = GetFreeIndex();
        }

        return await AddCoreAsync(entry, freeIndex, cancellationToken);
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
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
            Count--;
            
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

    public MuninnResult Get(string key, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(key.GetHashCode(), out _);

        if (index is NotValidIndex)
        {
            return new MuninnResult(false, null, $"Key {key} is not found");
        }

        var entry = _entries[index];

        return new MuninnResult(entry is not null, entry);
    }

    public IEnumerable<Entry> GetAll(CancellationToken cancellationToken)
    {
        return _entries.Where(entry => entry is not null)!;
    }

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        return _filterManager.FilterEntryKeys(_entries, chunks, cancellationToken);
    }

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken)
    {
        return _filterManager.FilterEntryValues(_entries, chunks, cancellationToken);
    }

    public Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out _);

        return index is NotValidIndex ? GetFailedResultAsync("Entry is not found in the cache") : UpdateCoreAsync(entry, index, cancellationToken);
    }

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is NotValidIndex)
        {
            return await AddCoreAsync(entry, index, cancellationToken);
        }

        if (freeIndex is NotValidIndex)
        {
            if (!await IncreaseArraySizeAsync(cancellationToken))
            {
                return GetFailedResult("Cannot increase array size", false);
            }
            
            freeIndex = GetFreeIndex();
        }

        return await UpdateCoreAsync(entry, freeIndex, cancellationToken);
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries = new Entry[DefaultIncreaseValue];
            _indexes = CreateIndexes(DefaultIncreaseValue);
            Count = 0;
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

    public async Task InitializeAsync(Entry[] entries)
    {
        await _semaphoreSlim.WaitAsync();

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

    public async Task<bool> IncreaseArraySizeAsync(CancellationToken cancellationToken)
    {
        if (_isResizeCalled)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _semaphoreSlim.Release(1);

            return true;
        }

        _isResizeCalled = true;

        try
        {
            var size = Count + DefaultIncreaseValue;
            var entries = new Entry?[size];
            var indexes = new ResidentCacheIndex[size];
            await _semaphoreSlim.WaitAsync(cancellationToken);

            for (var i = 0; i < _entries.Length; i++)
            {
                entries[i] = _entries[i];
                indexes[i] = _indexes[i];
                indexes[i + _indexes.Length - 1] = new();
            }

            _entries = entries;
            _logger.LogIncreasedSize(size);

            return true;
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(operationCanceledException);

            return false;
        }
        finally
        {
            _isResizeCalled = false;
            _semaphoreSlim.Release(1);
        }
    }
    
    public async Task<bool> DecreaseArraySizeAsync(CancellationToken cancellationToken)
    {
        if (_isResizeCalled)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _semaphoreSlim.Release(1);

            return true;
        }
        
        var size = Count + DefaultIncreaseValue;
        var array = new Entry?[size];
        var indexes = new ResidentCacheIndex[size];

        try
        {
            for (var i = 0; i < Count; i++)
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
            
            return true;
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(operationCanceledException);

            return false;
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    private int TryFindIndex(int hashcode, out int freeIndex)
    {
        freeIndex = NotValidIndex;

        for (var i = 0; i < _entries.Length; i++)
        {
            var entry = _entries[i];

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
            if (!_indexes[freeIndex].IsUsed)
            {
                _indexes[freeIndex].IsUsed = true;
            }
            else
            {
                freeIndex = NotValidIndex;

                for (var i = freeIndex + 1; i < _indexes.Length; i++)
                {
                    if (!_indexes[i].IsUsed)
                    {
                        _indexes[i].IsUsed = true;
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

    private async Task<MuninnResult> AddCoreAsync(Entry entry, int index, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = entry;
            Count++;
            _logger.LogKeyAdd(entry.Key);

            return new MuninnResult(true, entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(entry.Key, operationCanceledException);

            return await GetCancelledResultAsync(nameof(AddCoreAsync), entry.Key);
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

    private async Task<MuninnResult> UpdateCoreAsync(Entry entry, int index, CancellationToken cancellationToken)
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

    private MuninnResult GetCancelledResult(string methodName, string key)
    {
        _logger.LogCancelledRequest(methodName, key);

        return GetFailedResult("Cancellation has been requested", true);
    }

    private static MuninnResult GetFailedResult(string message, bool isCancelled, Exception? exception = null) => new(false, null, message, exception, isCancelled);

    private Task<MuninnResult> GetCancelledResultAsync(string methodName, string key) => Task.FromResult(GetCancelledResult(methodName, key));

    private static Task<MuninnResult> GetFailedResultAsync(string message) => Task.FromResult(GetFailedResult(message, false));

    private static ResidentCacheIndex[] CreateIndexes(int count)
    {
        var indexes = new ResidentCacheIndex[count];

        for (var i = 0; i < count; i++)
        {
            indexes[i] = new ResidentCacheIndex();
        }

        return indexes;
    }
}
