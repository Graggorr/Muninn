using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class SortedResidentCache(ILogger<ISortedResidentCache> logger, ResidentConfiguration configuration) : ISortedResidentCache
{
    private struct EntryComparer : IComparer<Entry>
    {
        public int Compare(Entry? x, Entry? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is null)
            {
                return -1;
            }

            return x.Hashcode.CompareTo(y.Hashcode);
        }
    }

    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly ILogger _logger = logger;
    private Entry?[] _entries = [];
    internal const string Message = "Sorted";

    public bool IsSorting { get; private set; }

    public async Task SortAsync(Entry[] entries)
    {
        var oldReference = _entries;

        try
        {
            await _semaphore.WaitAsync();
            IsSorting = true;
            Array.Sort(entries, new EntryComparer());
            _entries = entries;
        }
        catch (Exception exception)
        {
            _logger.LogSortError(exception);
            _entries = oldReference;
        }
        finally
        {
            IsSorting = false;
            _semaphore.Release(1);
        }
    }

    public MuninnResult GetByKey(string key)
    {
        if (IsSorting)
        {
            return new MuninnResult(false, null);
        }

        var index = Array.BinarySearch(_entries, Entry.CreateFilterEntry(key));

        return int.IsNegative(index) ? new MuninnResult(false, null) : new MuninnResult(true, _entries[index], Message);
    }
}
