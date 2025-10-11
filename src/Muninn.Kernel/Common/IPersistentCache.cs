using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IPersistentCache
{
    public Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken);

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);

    public Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken);

    public void Initialize();
}
