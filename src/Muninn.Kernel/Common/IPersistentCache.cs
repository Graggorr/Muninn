using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IPersistentCache
{
    public Task<bool> AddAsync(Entry entry);

    public Entry? Remove(string key);

    public Task<Entry?> GetAsync(string key);

    public Task<IEnumerable<Entry>> GetAllAsync();

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks);

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks);

    public Task<bool> UpdateAsync(Entry entry);

    public Task<bool> InsertAsync(Entry entry);
}
