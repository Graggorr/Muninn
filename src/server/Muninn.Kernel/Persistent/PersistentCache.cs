using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;
using Muninn.Kernel.Shared;

namespace Muninn.Kernel.Persistent;

internal class PersistentCache(ILogger<IPersistentCache> logger, IFilterService filterService) :
    BaseCache<IPersistentCache>(logger, filterService), IPersistentCache
{
    private class StreamResult(IDisposable? stream, Exception? exception)
    {
        public IDisposable? Stream { get; } = stream;

        public Exception? Exception { get; } = exception;

        public T GetStream<T>() => (T)Stream!;
    }

    private const string FileExtension = ".muninn";

    private readonly string _directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
    private readonly ConcurrentHashSet<StreamWriter> _streamWriters = new();

    private const char Separator = '%';
    private const int ParallelLoopSize = 100;
    private const int DefaultBufferSize = 65_536; // 64KB

    private const int KeyPosition = 0;
    private const int EncodingPosition = 1;
    private const int LifetimePosition = 2;
    private const int CreationTimePosition = 3;
    private const int LastModificationTimePosition = 4;

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken = default)
    {
        var fullPath = BuildFullPath(entry);
        StreamWriter? streamWriter = null;

        try
        {
            await _semaphoreSlim.WaitAsync(cancellationToken);
            var streamResult = CreateStream(fullPath, entry.Encoding, true, entry.Value.Length);

            if (streamResult.Exception is not null)
            {
                return GetFailedResult(streamResult.Exception.Message, false, streamResult.Exception);
            }

            streamWriter = streamResult.GetStream<StreamWriter>();

            var value = entry.Encoding.GetChars(entry.Value);
            await streamWriter.WriteAsync(value, cancellationToken);

            return GetSuccessfulResult(entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(InsertAsync), entry.Key, operationCanceledException);
        }
        catch (ObjectDisposedException objectDisposedException)
        {
            // this happens only if ClearAsync() has been called

            return GetFailedResult("Clear all has been called", false, objectDisposedException);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileInsert(entry.Key, exception);

            return GetFailedResult("Cannot write data into the file", false, exception);
        }
        finally
        {
            if (streamWriter is null)
            {
                _semaphoreSlim.Release(1);
            }
            else
            {
                await ReleaseStreamAsync(streamWriter);
            }
        }
    }

    public async Task<MuninnResult> ClearAsync(CancellationToken cancellationToken = default)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);

        foreach (var streamWriter in _streamWriters)
        {
            await streamWriter.DisposeAsync();
        }

        var filePaths = Directory.GetFiles(_directoryPath, $"*{FileExtension}");

        try
        {
            foreach (var filePath in filePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    DeleteFile(filePath, filePath.Split(Separator).First());
                }
                catch (Exception exception)
                {
                    _logger.LogFailedFileDelete(Path.GetFileName(filePath), exception);
                }
            }

            return GetSuccessfulResult();
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogClearAsyncError(exception);

            return GetFailedResult("Cannot clear all files", false, exception);
        }
        finally
        {
            _semaphoreSlim.Release(1);
        }
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(key);

        if (string.IsNullOrEmpty(fullPath))
        {
            return new MuninnResult(true, null, $"File {key} is not found");
        }

        var entry = GetEntryWithoutValue(fullPath);
        var streamResult = CreateStream(fullPath, entry.Encoding, false, DefaultBufferSize);

        if (streamResult.Exception is not null)
        {
            return GetFailedResult(streamResult.Exception.Message, false, streamResult.Exception);
        }

        var streamReader = streamResult.GetStream<StreamReader>();
        await _semaphoreSlim.WaitAsync(cancellationToken);

        try
        {
            var result = await ReadFileAsync(streamReader, entry, cancellationToken);

            if (result)
            {
                DeleteFile(fullPath, entry.Key);
            }

            return GetSuccessfulResult(entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(RemoveAsync), key, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileDelete(key, exception);

            return GetFailedResult($"Cannot delete file {key}", false, exception);
        }
        finally
        {
            await ReleaseStreamAsync(streamReader);
        }
    }

    public async Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(key);

        if (string.IsNullOrEmpty(fullPath))
        {
            return GetFailedResult($"File {key} is not found", false);
        }

        var entry = GetEntryWithoutValue(fullPath);
        var streamResult = CreateStream(fullPath, entry.Encoding, false, DefaultBufferSize);

        if (streamResult.Exception is not null)
        {
            return GetFailedResult(streamResult.Exception.Message, false, streamResult.Exception);
        }

        var streamReader = streamResult.GetStream<StreamReader>();

        try
        {
            await ReadFileAsync(streamReader, entry, cancellationToken);

            return GetSuccessfulResult(entry);
        }
        catch (OperationCanceledException operationCanceledException)
        {
            return GetCancelledResult(nameof(GetAsync), key, operationCanceledException);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileRead(key, exception);

            return GetFailedResult($"Cannot get file {key}", false, exception);
        }
        finally
        {
            await ReleaseStreamAsync(streamReader);
        }
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filePaths = Directory.GetFiles(_directoryPath).Where(filePath => filePath.EndsWith(FileExtension))
            .ToArray();

        if (filePaths.Length is 0)
        {
            return [];
        }

        var entries = new ConcurrentBag<Entry>();
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = filePaths.Length >= ParallelLoopSize ? ParallelLoopSize : filePaths.Length
        };

        await Parallel.ForEachAsync(filePaths, parallelOptions, async (filePath, token) =>
        {
            var entry = GetEntryWithoutValue(filePath);
            var key = entry.Key;
            var streamResult = CreateStream(filePath, entry.Encoding, false, DefaultBufferSize);

            if (streamResult.Stream is null)
            {
                return;
            }

            var streamReader = streamResult.GetStream<StreamReader>();

            try
            {
                var result = await ReadFileAsync(streamReader, entry, token);

                if (result)
                {
                    entries.Add(entry);
                }
            }
            catch (Exception exception)
            {
                _logger.LogFailedFileRead(key, exception);
            }
            finally
            {
                await ReleaseStreamAsync(streamReader);
            }
        });

        return entries.OrderBy(entry => entry.Key).ToArray();
    }

    public  IEnumerable<Entry> GetEntriesByKeyFilters(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken = default)
        => GetEntriesByKeyFiltersAsync(chunks, cancellationToken).GetAwaiter().GetResult();

    public IEnumerable<Entry> GetEntriesByValueFilters(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken = default)
        => GetEntriesByValueFiltersAsync(chunks, cancellationToken).GetAwaiter().GetResult();
    
    public async Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks,
        CancellationToken cancellationToken = default)
    {
        var entries = await GetAllAsync(cancellationToken);

        return _filterService.FilterEntryKeys(entries.ToArray(), chunks, cancellationToken);
    }

    public async Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks,
        CancellationToken cancellationToken = default)
    {
        var entries = await GetAllAsync(cancellationToken);

        return _filterService.FilterEntryValues(entries.ToArray(), chunks, cancellationToken);
    }

    public void Initialize()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Directory.CreateDirectory(_directoryPath);
        }
    }

    Task<MuninnResult> IBaseCache.AddAsync(Entry entry, CancellationToken cancellationToken) 
        => InsertAsync(entry, cancellationToken);
    
    Task<MuninnResult> IBaseCache.UpdateAsync(Entry entry, CancellationToken cancellationToken) 
        => InsertAsync(entry, cancellationToken);
    
    private static async Task<bool> ReadFileAsync(StreamReader streamReader, Entry entry,
        CancellationToken cancellationToken)
    {
        var result = await streamReader.ReadToEndAsync(cancellationToken);
        entry.Value = entry.Encoding.GetBytes(result);

        return true;
    }

    private void DeleteFile(string fullPath, string key)
    {
        File.Delete(fullPath);
        _logger.LogFileDelete(key);
    }

    private StreamResult CreateStream(string fullPath, Encoding encoding, bool isWriting, int bufferSize)
    {
        try
        {
            FileStream fileStream;
            IDisposable stream;

            if (isWriting)
            {
                fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite,
                    bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous);
                var streamWriter = new StreamWriter(fileStream, encoding, bufferSize)
                {
                    AutoFlush = true,
                };
                _streamWriters.Add(streamWriter);
                stream = streamWriter;
            }
            else
            {
                fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read,
                    FileShare.ReadWrite | FileShare.Delete, bufferSize, FileOptions.Asynchronous);
                stream = new StreamReader(fileStream, encoding);
            }

            return new StreamResult(stream, null);
        }
        catch (Exception exception)
        {
            var operation = isWriting ? "writing" : "reading";
            _logger.LogFailedStreamCreate(operation, Path.GetFileName(fullPath), exception);

            return new StreamResult(null, exception);
        }
    }

    private async ValueTask ReleaseStreamAsync(IDisposable disposable)
    {
        if (disposable is StreamWriter streamWriter)
        {
            await streamWriter.DisposeAsync();
            _semaphoreSlim.Release(1);
            _streamWriters.Remove(streamWriter);

            return;
        }

        disposable.Dispose();
    }

    private static Entry GetEntryWithoutValue(string fullPath)
    {
        var split = Path.GetFileNameWithoutExtension(fullPath).Split(Separator);
        var key = split[KeyPosition];
        var encoding = Encoding.GetEncoding(int.Parse(split[EncodingPosition]));
        var lifeTime = TimeSpan.FromTicks(long.Parse(split[LifetimePosition]));
        var creationTime = new DateTime(long.Parse(split[CreationTimePosition]));
        var lastModificationTime = new DateTime(long.Parse(split[LastModificationTimePosition]));

        return new(key, [], encoding, lifeTime)
        {
            CreationTime = creationTime,
            LastModificationTime = lastModificationTime,
        };
    }

    internal string BuildFullPath(Entry entry)
    {
        var fileNameBuilder = new StringBuilder()
            .Append(entry.Key)
            .Append(Separator)
            .Append(entry.Encoding.CodePage)
            .Append(Separator)
            .Append(entry.LifeTime.Ticks)
            .Append(Separator)
            .Append(entry.CreationTime.Ticks)
            .Append(Separator)
            .Append(entry.LastModificationTime.Ticks)
            .Append(FileExtension);

        return Path.Combine(_directoryPath, fileNameBuilder.ToString());
    }

    private string GetFullPath(string key)
    {
        var allFiles = Directory.GetFiles(_directoryPath);

        return allFiles.FirstOrDefault(file => Path.GetFileName(file).Split(Separator)[0].Equals(key)) ?? string.Empty;
    }
}
