using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IResidentCache : IBaseCache
{
    public int Count { get; }
    
    public int Length { get; }
    
    public MuninnResult Get(string key, CancellationToken cancellationToken = default);

    public IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken = default);

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default);
    
    public IEnumerable<Entry> GetAll(CancellationToken cancellationToken = default);

    public Task<MuninnResult> IncreaseArraySizeAsync(CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> DecreaseArraySizeAsync(CancellationToken cancellationToken = default);
    
    internal Task InitializeAsync(Entry[] entries, CancellationToken cancellationToken = default);
}
