using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Shared;

internal class FilterManager : IFilterManager
{
    public IEnumerable<Entry> FilterEntryKeys(Entry?[] entries, IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        yield break;
    }

    public IEnumerable<Entry> FilterEntryValues(Entry?[] entries, IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken)
    {
        yield break;
    }
}
