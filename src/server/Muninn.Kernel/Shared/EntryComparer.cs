using Muninn.Kernel.Models;

namespace Muninn.Kernel.Shared;

public struct EntryComparer : IComparer<Entry>, IEqualityComparer<Entry>
{
    public int Compare(Entry? first, Entry? second)
    {
        if (ReferenceEquals(first, second))
        {
            return 0;
        }

        if (second is null)
        {
            return 1;
        }

        if (first is null)
        {
            return -1;
        }

        return first.Hashcode.CompareTo(second.Hashcode);
    }

    public bool Equals(Entry? x, Entry? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        return GetHashCode(x).Equals(GetHashCode(y));
    }

    public int GetHashCode(Entry obj)
    {
        return obj.Hashcode;
    }
}
