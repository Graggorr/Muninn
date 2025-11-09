using Muninn.Kernel.Common;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Handlers;

public class PersistentCacheHandler(IPersistentCache persistentCache) : IOptionalCacheHandler
{
    private const int MaxCount = 3;
    
    private readonly IPersistentCache _cache = persistentCache;
    private readonly TimeSpan _delay = TimeSpan.FromSeconds(1);

    public async Task InsertAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < MaxCount)
        {
            var result = await _cache.InsertAsync(entry, cancellationToken);

            if (result.IsSuccessful)
            {
                return;
            }

            if (result.IsCancelled)
            {
                throw result.Exception!;
            }
            
            count++;
        }
    }
    
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var count = 0;

        while (count < MaxCount)
        {
            var result = await _cache.RemoveAsync(key, cancellationToken);

            if (result.IsSuccessful)
            {
                return;
            }

            if (result.IsCancelled)
            {
                throw result.Exception!;
            }

            count++;
        }
    }

    public Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default) => _cache.ClearAsync(cancellationToken);

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default) => _cache.GetAsync(key, cancellationToken);
    
    public Task UpdateAsync(Entry entry, CancellationToken cancellationToken = default) => InsertAsync(entry, cancellationToken);

    public Task AddAsync(Entry entry, CancellationToken cancellationToken = default) => InsertAsync(entry, cancellationToken);
}
