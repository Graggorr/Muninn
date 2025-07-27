using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class ResidentCache(ILogger<IResidentCache> logger, ResidentConfiguration configuration, Locker locker, IFilterManager filterManager) : IResidentCache
{
    private class ResidentCacheIndex
    {
        public bool IsUsed { get; set; }
    }

    private const int NOT_VALID_INDEX = -1;

    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly Locker _locker = locker;
    private readonly ResidentConfiguration _configuration = configuration;

    private ResidentCacheIndex[] _indexes = new ResidentCacheIndex[configuration.ArrayIncreasementValue];
    private Entry?[] _entries = new Entry[configuration.ArrayIncreasementValue];
    private bool _isResizeCalled;

    public MuninResult Add(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is not NOT_VALID_INDEX)
        {
            return new MuninResult(false, null, $"Entry with a key {entry.Key} already exists");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return GetCancelledResult(entry.Key);
        }

        if (freeIndex is NOT_VALID_INDEX)
        {
            IncreaseArraySize();
            freeIndex = GetFreeIndex();
        }

        return AddCore(entry, freeIndex);
    }

    public MuninResult Remove(string key, CancellationToken cancellationToken)
    {
        var hashcode = key.GetHashCode();
        var index = TryFindIndex(hashcode, out _);

        if (index is NOT_VALID_INDEX)
        {
            return new MuninResult(true, null);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return GetCancelledResult(key);
        }

        try
        {
            _locker.ReadLock();
            var entry = _entries[index];
            _locker.ReadReleaseLock();
            _locker.WriteLock();
            _entries[index] = null;
            _indexes[index].IsUsed = false;

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            return new MuninResult(false, null, $"Cannot remove entry with key {key}", exception);
        }
        finally
        {
            _locker.WriteReleaseLock();
        }
    }

    public MuninResult Get(string key, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(key.GetHashCode(), out _);

        if (index is NOT_VALID_INDEX)
        {
            return new MuninResult(false, null, $"Key {key} is not found");
        }

        _locker.ReadLock();
        var entry = _entries[index];
        _locker.ReadReleaseLock();

        return new MuninResult(entry is not null, entry);
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

    public MuninResult Update(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out _);

        return index is NOT_VALID_INDEX ? new MuninResult(false, null, "Entry is not found in the cache") : UpdateCore(entry, index);
    }

    public MuninResult Insert(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (cancellationToken.IsCancellationRequested)
        {
            return GetCancelledResult(entry.Key);
        }

        if (index is NOT_VALID_INDEX)
        {
            return AddCore(entry, index);
        }

        if (freeIndex is NOT_VALID_INDEX)
        {
            IncreaseArraySize();
            freeIndex = GetFreeIndex();
        }

        return UpdateCore(entry, freeIndex);
    }

    public void Clear()
    {
        _locker.WriteLock();
        _entries = new Entry[_configuration.ArrayIncreasementValue];
        _indexes = new ResidentCacheIndex[_configuration.ArrayIncreasementValue];
        _locker.WriteReleaseLock();
        GC.Collect();
    }

    public void Initialize(Entry[] entries)
    {
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

        GC.Collect();
    }


    private int TryFindIndex(int hashcode, out int freeIndex)
    {
        freeIndex = NOT_VALID_INDEX;
        _locker.ReadLock();

        for (var i = 0; i < _entries.Length; i++)
        {
            var entry = _entries[i];

            if (entry is not null)
            {
                if (entry.Hashcode == hashcode)
                {
                    _locker.ReadReleaseLock();

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

        _locker.ReadReleaseLock();

        return freeIndex;
    }

    private int GetFreeIndex()
    {
        _locker.ReadLock();

        for (var i = _indexes.Length - 1; i >= 0; i--)
        {
            if (!_indexes[i].IsUsed)
            {
                _locker.ReadReleaseLock();
                _indexes[i].IsUsed = true;

                return i;
            }
        }

        var oldEntry = _entries.OrderBy(entry => entry?.LastModificationTime).First();
        var index = Array.IndexOf(_entries, oldEntry);
        _locker.ReadReleaseLock();

        return index;
    }

    private void IncreaseArraySize()
    {
        if (_isResizeCalled)
        {
            while (_isResizeCalled)
            {

            }

            return;
        }

        _isResizeCalled = true;

        try
        {
            _locker.ReadLock();
            var entries = new Entry?[_entries.Length + _configuration.ArrayIncreasementValue];
            var indexes = new ResidentCacheIndex[_indexes.Length + _configuration.ArrayIncreasementValue];
            _locker.ReadReleaseLock();
            _locker.WriteLockLong();

            for (var i = 0; i < _entries.Length; i++) // _entries.Length has to be equal to _indexes.Length
            {
                entries[i] = _entries[i];
                indexes[i] = _indexes[i];
                indexes[i + _indexes.Length - 1] = new();
            }

            _entries = entries;
            _locker.WriteReleaseLock();
        }
        finally
        {
            _isResizeCalled = false;
        }
    }

    private void DecreaseArraySize()
    {
        _locker.ReadLock();
        var count = _entries.Count();
        var array = new Entry?[count + _configuration.ArrayIncreasementValue];

        for (var i = 0; i < count; i++)
        {
            array[i] = _entries[i];
        }

        _locker.ReadReleaseLock();
        _locker.WriteLock();
        _entries = array;
        _locker.WriteReleaseLock();
    }

    private MuninResult AddCore(Entry entry, int index)
    {
        try
        {
            _locker.WriteLock();
            _entries[index] = entry;
            _logger.LogKeyAdd(entry.Key, entry.DecodeValue());

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedKeyInsert(entry.Key, entry.DecodeValue(), exception);
            _indexes[index].IsUsed = false;

            return new MuninResult(false, null, "Cannot add new entry into the cache", exception);
        }
        finally
        {
            _locker.WriteReleaseLock();
        }
    }

    private MuninResult UpdateCore(Entry entry, int index)
    {
        Entry? oldEntry = null;

        try
        {
            _locker.ReadLock();
            oldEntry = _entries[index];
            _locker.ReadReleaseLock();
            _locker.WriteLock();
            _entries[index] = entry;
            _logger.LogKeyUpdate(entry.Key, oldEntry!.DecodeValue(), entry.DecodeValue());

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            if (!_locker.IsWriteLocked)
            {
                _locker.WriteLock();
            }

            _entries[index] = oldEntry;
            _indexes[index].IsUsed = false;
            _logger.LogFailedKeyUpdate(entry.Key, entry.DecodeValue(), exception);

            return new MuninResult(false, null, "Cannot update entry in the cache", exception);
        }
        finally
        {
            _locker.WriteReleaseLock();
        }
    }

    private MuninResult GetCancelledResult(string key)
    {
        _logger.LogCancelledRequest(key);

        return new(false, null, "Cancellation has been requested", null, true);
    }
}