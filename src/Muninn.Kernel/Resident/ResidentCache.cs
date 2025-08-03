using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class ResidentCache(ILogger<IResidentCache> logger, ResidentConfiguration configuration, IFilterManager filterManager) : IResidentCache
{
    private class ResidentCacheIndex
    {
        public bool IsUsed { get; set; }
    }

    private const int NOT_VALID_INDEX = -1;

    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly SemaphoreSlim _semaphoreSlim = new(1);
    private readonly ResidentConfiguration _configuration = configuration;

    private ResidentCacheIndex[] _indexes = new ResidentCacheIndex[configuration.ArrayIncreasementValue];
    private Entry?[] _entries = new Entry[configuration.ArrayIncreasementValue];
    private bool _isResizeCalled;

    public async Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is not NOT_VALID_INDEX)
        {
            return GetFailedResult($"Entry with a key {entry.Key} already exists", false);
        }

        if (freeIndex is NOT_VALID_INDEX)
        {
            await IncreaseArraySizeAsync(cancellationToken);
            freeIndex = GetFreeIndex();
        }

        return await AddCoreAsync(entry, freeIndex, cancellationToken);
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var hashcode = key.GetHashCode();
        var index = TryFindIndex(hashcode, out _);

        if (index is NOT_VALID_INDEX)
        {
            return new MuninnResult(true, null);
        }

        try
        {
            var entry = _entries[index];
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = null;
            _indexes[index].IsUsed = false;

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

        if (index is NOT_VALID_INDEX)
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

        return index is NOT_VALID_INDEX ? GetFailedResultAsync("Entry is not found in the cache") : UpdateCoreAsync(entry, index, cancellationToken);
    }

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is NOT_VALID_INDEX)
        {
            return await AddCoreAsync(entry, index, cancellationToken);
        }

        if (freeIndex is NOT_VALID_INDEX)
        {
            await IncreaseArraySizeAsync(cancellationToken);
            freeIndex = GetFreeIndex();
        }

        return await UpdateCoreAsync(entry, freeIndex, cancellationToken);
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        _entries = new Entry[_configuration.ArrayIncreasementValue];
        _indexes = new ResidentCacheIndex[_configuration.ArrayIncreasementValue];
        _semaphoreSlim.Release(1);
        GC.Collect();
    }

    public async Task InitializeAsync(Entry[] entries)
    {
        await _semaphoreSlim.WaitAsync();

        if (entries.Length < _configuration.ArrayIncreasementValue)
        {
            _entries = new Entry[_configuration.ArrayIncreasementValue];

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


    private int TryFindIndex(int hashcode, out int freeIndex)
    {
        freeIndex = NOT_VALID_INDEX;

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

        if (freeIndex is not NOT_VALID_INDEX)
        {
            if (!_indexes[freeIndex].IsUsed)
            {
                _indexes[freeIndex].IsUsed = true;
            }
            else
            {
                freeIndex = NOT_VALID_INDEX;

                for (var i = freeIndex + 1; i < _indexes.Length; i++)
                {
                    if (!_indexes[i].IsUsed)
                    {
                        _indexes[i].IsUsed = true;
                        freeIndex = i;
                        break;
                    }
                }
            }
        }

        return freeIndex;
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

    private async Task<bool> IncreaseArraySizeAsync(CancellationToken cancellationToken)
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
            var entries = new Entry?[_entries.Length + _configuration.ArrayIncreasementValue];
            var indexes = new ResidentCacheIndex[_indexes.Length + _configuration.ArrayIncreasementValue];
            await _semaphoreSlim.WaitAsync(cancellationToken);

            for (var i = 0; i < _entries.Length; i++)
            {
                entries[i] = _entries[i];
                indexes[i] = _indexes[i];
                indexes[i + _indexes.Length - 1] = new();
            }

            _entries = entries;

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

    private async Task<bool> DecreaseArraySizeAsync(CancellationToken cancellationToken)
    {
        if (_isResizeCalled)
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _semaphoreSlim.Release(1);

            return true;
        }

        var count = _entries.Count();
        var array = new Entry?[count + _configuration.ArrayIncreasementValue];

        try
        {
            for (var i = 0; i < count; i++)
            {
                array[i] = _entries[i];
            }

            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries = array;

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

    private async Task<MuninnResult> AddCoreAsync(Entry entry, int index, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            _entries[index] = entry;
            _logger.LogKeyAdd(entry.Key, entry.DecodeValue());

            return new MuninnResult(true, entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogCancelledRequest(entry.Key, operationCanceledException);

            return await GetCancelledResultAsync(entry.Key);
        }
        catch (Exception exception)
        {
            _logger.LogFailedKeyInsert(entry.Key, entry.DecodeValue(), exception);
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
            _logger.LogKeyUpdate(entry.Key, oldEntry.DecodeValue(), entry.DecodeValue());

            return new MuninnResult(true, entry);
        }
        catch (Exception exception)
        {
            if (oldEntry is not null)
            {
                _entries[index] = oldEntry;
            }

            _logger.LogFailedKeyUpdate(entry.Key, entry.DecodeValue(), exception);

            return new MuninnResult(false, null, "Cannot update entry in the cache", exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    private MuninnResult GetCancelledResult(string key)
    {
        _logger.LogCancelledRequest(key);

        return GetFailedResult("Cancellation has been requested", true);
    }

    private static MuninnResult GetFailedResult(string message, bool isCancelled, Exception? exception = null) => new(false, null, message, exception, isCancelled);

    private Task<MuninnResult> GetCancelledResultAsync(string key) => Task.FromResult(GetCancelledResult(key));

    private static Task<MuninnResult> GetFailedResultAsync(string message) => Task.FromResult(GetFailedResult(message, false));
}
