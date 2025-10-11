using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IResidentCache
{
    public int Count { get; }
    
    public int Length { get; }
    
    public Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken);

    public MuninnResult Get(string key, CancellationToken cancellationToken);

    public IEnumerable<Entry> GetAll(CancellationToken cancellationToken);

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken);

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken);

    public Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken);

    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken);

    public Task<bool> IncreaseArraySizeAsync(CancellationToken cancellationToken);
    
    public Task<bool> DecreaseArraySizeAsync(CancellationToken cancellationToken);
    
    internal Task InitializeAsync(Entry[] entries);
}
