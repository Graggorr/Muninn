using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IFilterManager
{
    public IEnumerable<Entry> FilterEntryKeys(ref Entry[] entries, IEnumerable<IEnumerable<KeyFilter>> chunks);

    public IEnumerable<Entry> FilterEntryValues(ref Entry[] entries, IEnumerable<IEnumerable<ValueFilter>> chunks);
}
