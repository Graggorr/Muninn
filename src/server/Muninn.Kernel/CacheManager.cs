using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Shared;

namespace Muninn.Kernel;

internal class CacheManager(IResidentCache residentCache, IEnumerable<IOptionalCacheHandler> handlers) : ICacheManager
{
    private readonly IResidentCache _residentCache = residentCache;
    private readonly IEnumerable<IOptionalCacheHandler> _handlers = handlers;
    
    public async Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.AddAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _handlers.Select(handler => handler.AddAsync(entry, cancellationToken));
        }

        return residentResult;
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.RemoveAsync(key, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _handlers.Select(handler => handler.RemoveAsync(key, cancellationToken));
        }

        return residentResult;
    }

    public Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<MuninnResult>>
        {
            _residentCache.GetAsync(key, cancellationToken),
        };
        tasks.AddRange(_handlers.Select(handler => handler.GetAsync(key, cancellationToken)));
        
        return GetCoreAsync(tasks, result => result.IsSuccessful);
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(bool isTracking, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<IEnumerable<Entry>>>
        {
            _residentCache.GetAllAsync(isTracking, cancellationToken),
        };
        tasks.AddRange(_handlers.Select(handler => handler.GetAllAsync(isTracking, cancellationToken)));
        await Task.WhenAll(tasks);
        var lists = tasks.Select(task => task.Result).ToList();
        var result = lists.First().ToList();
        lists.Remove(result);
        var comparer = new EntryComparer();
        
        foreach (var list in lists)
        {
            result = result.Union(list, comparer).ToList();
        }
        
        return result;
    }
    
    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        var tasks = new List<Task<IEnumerable<Entry>>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.GetEntriesByKeyFilters(chunks, cancellationToken), cancellationToken),
        };
        
        return GetCoreAsync(tasks, entries => entries.Any());
    }

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default)
    {
        var tasks = new List<Task<IEnumerable<Entry>>>(2)
        {
            Task.Factory.StartNew(() => _residentCache.GetEntriesByValueFilters(chunks, cancellationToken), cancellationToken),
        };

        return GetCoreAsync(tasks, entries => entries.Any());
    }

    public async Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.UpdateAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _handlers.Select(handler => handler.UpdateAsync(entry, cancellationToken));
        }

        return residentResult;
    }

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var residentResult = await _residentCache.InsertAsync(entry, cancellationToken);

        if (residentResult.IsSuccessful)
        {
            _ = _handlers.Select(handler => handler.InsertAsync(entry, cancellationToken));
        }

        return residentResult;
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken)
    {
        var tasks = new List<Task<MuninnResult>>
        {
            _residentCache.ClearAsync(cancellationToken),
        };
        tasks.AddRange(_handlers.Select(handler => handler.ClearAsync(cancellationToken)));
        await Task.WhenAll(tasks);

        var failedTask = tasks.FirstOrDefault(task => !task.Result.IsSuccessful);

        return failedTask?.Result ?? tasks.First().Result;
    }

    private static async Task<T> GetCoreAsync<T>(List<Task<T>> tasks, Func<T, bool> successfulCondition)
    {
        var firstTask = await Task.WhenAny(tasks);
        var result = await firstTask;

        if (successfulCondition(result))
        {
            return result;
        }

        tasks.Remove(firstTask);
        var resultTask = tasks.FirstOrDefault(task => successfulCondition(task.Result)) ?? firstTask;

        return resultTask.Result;
    }
}
