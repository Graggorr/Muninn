using Muninn.Kernel.Models;

namespace Muninn.Kernel.Shared;

public struct EntryComparer : IComparer<Entry>
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
}
