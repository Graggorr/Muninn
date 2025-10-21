using System.Text;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Resident;

internal class SortedResidentCache(ILogger<ISortedResidentCache> logger, IFilterManager filterManager)
    : ResidentCache(logger, filterManager), ISortedResidentCache
{
    internal struct EntryComparer : IComparer<Entry>
    {
        public int Compare(Entry? x, Entry? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is null)
            {
                return -1;
            }

            return x.Hashcode.CompareTo(y.Hashcode);
        }
    }

    public override Task<MuninnResult> AddAsync(Entry entry, CancellationToken cancellationToken = default) =>
        SortIfSuccessfulAsync(base.AddAsync(entry, cancellationToken));

    public override Task<MuninnResult> UpdateAsync(Entry entry, CancellationToken cancellationToken = default) =>
        SortIfSuccessfulAsync(base.UpdateAsync(entry, cancellationToken));

    public override Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        SortIfSuccessfulAsync(base.RemoveAsync(key, cancellationToken));

    public override Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default) => 
        SortIfSuccessfulAsync(base.InsertAsync(entry, cancellationToken));

    public async Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            var index = Array.BinarySearch(_entries, new Entry(key, [], Encoding.Default, TimeSpan.Zero), new EntryComparer());

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
            _logger.LogGerError(key, exception);

            return GetFailedResult(exception.Message, false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    public async Task<MuninnResult> SortAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            Array.Sort(_entries, new EntryComparer());

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
                result = sortResult;
            }
        }

        return result;
    }
}