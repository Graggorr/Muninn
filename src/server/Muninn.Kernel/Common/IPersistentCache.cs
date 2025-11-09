using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IPersistentCache : IBaseCache
{
    public Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken = default);
    
    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken = default);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default);
    
    public void Initialize();
}
