using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IFilterService
{
    public IEnumerable<Entry> FilterEntryKeys(Entry?[] entries, IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public IEnumerable<Entry> FilterEntryValues(Entry?[] entries, IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);
}
