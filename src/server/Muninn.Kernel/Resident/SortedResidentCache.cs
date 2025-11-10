using System.Text;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;
using Muninn.Kernel.Shared;

namespace Muninn.Kernel.Resident;

internal class SortedResidentCache(ILogger<ISortedResidentCache> logger, IFilterService filterService)
    : ResidentCache(logger, filterService), ISortedResidentCache
{
    public override Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken = default) =>
        SortIfSuccessfulAsync(base.AddAsync(entry, cancellationToken));

    public override Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        SortIfSuccessfulAsync(base.RemoveAsync(key, cancellationToken));

    public override Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default) => 
        SortIfSuccessfulAsync(base.InsertAsync(entry, cancellationToken));

    public async Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            var index = Array.BinarySearch(_entries, new(key, [], Encoding.Default), new EntryComparer());

            return int.IsNegative(index)
                ? GetFailedResult($"Key {key} is not found", false)
                : GetSuccessfulResult(_entries[index]);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(Get), key, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogGetError(key, exception);

            return GetFailedResult(exception.Message, false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    public override async Task<IEnumerable<Entry>> GetAllAsync(bool isTracking, CancellationToken cancellationToken = default)
    {
        var result = await base.GetAllAsync(isTracking, cancellationToken);

        if (!isTracking)
        {
            var array = result.ToArray();
            await SortAsync(array, cancellationToken);
            result = new List<Entry>(array);
        }
        
        return result;
    }

    public Task<MuninnResult> SortAsync(CancellationToken cancellationToken = default) => SortAsync(_entries, cancellationToken);

    private async Task<MuninnResult> SortAsync(Entry?[] entries, CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            Array.Sort(entries, new EntryComparer());

            return GetSuccessfulResult();
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(SortAsync), string.Empty, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogSortError(exception);

            return GetFailedResult(exception.Message, false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }
    
    private async Task<MuninnResult> SortIfSuccessfulAsync(Task<MuninnResult> task)
    {
        var result = await task;
        
        if (result.IsSuccessful)
        {
            var sortResult = await SortAsync();

            if (!sortResult.IsSuccessful)
            {
                return sortResult;
            }
        }
        
        return result;
    }
}
