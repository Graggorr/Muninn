using Muninn.Kernel.Models;

namespace Muninn.Kernel.Common;

public interface IBaseCache
{
    public Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken = default);

    public Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default);
    
    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default);
    
}
