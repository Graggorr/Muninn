using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel;

internal class ResidentCache(ILogger<IResidentCache> logger, ResidentConfiguration configuration, Locker locker, IFilterManager filterManager) : IResidentCache
{
    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly Locker _locker = locker;
    private readonly ResidentConfiguration _configuration = configuration;

    private readonly MuninResult _cancelledResult = new(false, null, "Cancellation has been requested", null, true);

    private Entry?[] _entries = new Entry[1000];
    private const int NOT_VALID_INDEX = -1;

    public MuninResult Add(Entry entry, CancellationToken cancellationToken)
    {
        var index = TryFindIndex(entry.Hashcode, out var freeIndex);

        if (index is not NOT_VALID_INDEX)
        {
            return new MuninResult(false, null, $"Entry with a key {entry.Key} already exists");
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return _cancelledResult;
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
            return _cancelledResult;
        }

        Entry? entry = null;

        try
        {
            _locker.ReadLock();
            entry = _entries[index];
            _locker.WriteLock();
            _entries[index] = null;

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            if (entry is not null && _entries[index] is null)
            {
                return new MuninResult(true, entry);
            }

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
        _locker.ReadLock();
        var entry = _entries[index];
        _locker.ReadReleaseLock();

        return new MuninResult(entry is not null, entry);
    }

    public IEnumerable<Entry?> GetAll(CancellationToken cancellationToken)
    {
        _locker.ReadLock();

        for (var i = 0; i < _entries.Length; i++)
        {
            yield return _entries[i];
        }

        _locker.ReadReleaseLock();
    }

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        yield break;
    }

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        yield break;
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
            return _cancelledResult;
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

        return UpdateCore(entry, freeIndex + 1);
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

        _locker.ReadReleaseLock();

        return NOT_VALID_INDEX;
    }

    private int GetFreeIndex()
    {
        _locker.ReadLock();

        for (var i = _entries.Length - 1; i >= 0; i--)
        {
            if (_entries[i] is null)
            {
                _locker.ReadReleaseLock();

                return i;
            }
        }

        _locker.ReadReleaseLock();

        return NOT_VALID_INDEX;
    }

    private void IncreaseArraySize()
    {
        _locker.ReadLock();
        var array = new Entry?[_entries.Length + _configuration.ArrayIncreasementValue];

        for (var i = 0; i < _entries.Length; i++)
        {
            array[i] = _entries[i];
        }

        _locker.ReadReleaseLock();
        _locker.WriteLock();
        _entries = array;
        _locker.WriteReleaseLock();
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

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
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

            _locker.WriteLock();
            _entries[index] = entry;

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            if (!_locker.IsWriteLocked)
            {
                _locker.WriteLock();
            }

            _entries[index] = oldEntry;

            return new MuninResult(false, null, "Cannot update entry in the cache", exception);
        }
        finally
        {
            _locker.WriteReleaseLock();
        }
    }
}