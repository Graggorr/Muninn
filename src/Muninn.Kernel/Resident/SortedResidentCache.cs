using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class SortedResidentCache(ILogger<ISortedResidentCache> logger, ResidentConfiguration configuration) : ISortedResidentCache
{
    public class EntryComparer : IComparer<Entry>
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

    private readonly ILogger _logger = logger;
    private Entry?[] _entries = [];
    internal const string MESSAGE = "Sorted";

    public bool IsSorting { get; private set; }

    public void Sort(Entry[] entries)
    {
        var oldReference = _entries;

        try
        {
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
        }
    }

    public MuninResult GetByKey(string key)
    {
        var index = Array.BinarySearch(_entries, new Entry(key, []));

        return int.IsNegative(index) ? new MuninResult(false, null) : new MuninResult(true, _entries[index], MESSAGE);
    }
}
