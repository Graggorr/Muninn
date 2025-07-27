using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

internal interface IResidentCache
{
    public MuninResult Add(Entry entry, CancellationToken cancellationToken);

    public MuninResult Remove(string key, CancellationToken cancellationToken);

    public MuninResult Get(string key, CancellationToken cancellationToken);

    public IEnumerable<Entry> GetAll(CancellationToken cancellationToken);

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);

    public MuninResult Update(Entry entry, CancellationToken cancellationToken);

    public MuninResult Insert(Entry entry, CancellationToken cancellationToken);

    public void Clear();

    internal void Initialize(Entry[] entries);
}
