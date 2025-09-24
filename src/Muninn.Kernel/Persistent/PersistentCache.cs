using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;
using Muninn.Kernel.Shared;

namespace Muninn.Kernel.Persistent;

internal class PersistentCache(ILogger<IPersistentCache> logger, PersistentConfiguration persistentConfiguration, IFilterManager filterManager) : IPersistentCache
{
    private class StreamResult(IDisposable? stream, Exception? exception)
    {
        public IDisposable? Stream { get; } = stream;

        public Exception? Exception { get; } = exception;

        public T GetStream<T>() => (T)Stream!;
    }

    private class EntryFileData(string key, Encoding encoding, TimeSpan lifeTime)
    {
        public string Key { get; init; } = key;

        public Encoding Encoding { get; init; } = encoding;

        public TimeSpan LifeTime { get; init; } = lifeTime;
    }

    private const string FileExtension = ".muninn";

    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly string _directoryPath = persistentConfiguration.DirectoryPath;
    private readonly int _defaultBufferSize = persistentConfiguration.DefaultBufferSize;
    private readonly ConcurrentHashSet<StreamWriter> _streamWriters = new();
    private readonly SemaphoreSlim _semaphore = new(1);

    private const string LifetimeStringFormat = "G";
    private const int KeyPosition = 0;
    private const int EncodingPosition = 1;
    private const int LifetimePosition = 2;

    public async Task<MuninnResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var fullPath = BuildFullPath(entry.Key, entry.Encoding.EncodingName, entry.LifeTime.ToString(LifetimeStringFormat));
        await _semaphore.WaitAsync(cancellationToken);
        var streamResult = CreateStream(fullPath, entry.Encoding, true, entry.Value.Length);

        if (streamResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamResult.Exception);
        }

        var streamWriter = streamResult.GetStream<StreamWriter>();

        try
        {
            var value = entry.Encoding.GetChars(entry.Value).AsMemory();
            await streamWriter.WriteAsync(value, cancellationToken);

            return new MuninnResult(true, entry);
        }
        catch (ObjectDisposedException) 
        {
            // this happens only if ClearAsync() has been called

            return CreateFailedMuninResult(null!, "Clear all has been called");
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileInsert(entry.Key, exception);

            return CreateFailedMuninResult(exception, "Cannot write data into the file");
        }
        finally
        {
            await ReleaseStreamAsync(streamWriter);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        foreach (var streamWriter in _streamWriters)
        {
            await streamWriter.DisposeAsync();
        }

        var filePaths = Directory.GetFiles(_directoryPath, $"*{FileExtension}");

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                DeleteFile(filePath);
            }
            catch (Exception exception)
            {
                _logger.LogFailedFileDelete(Path.GetFileName(filePath), exception);
            }
        }

        _streamWriters.Clear();
        _semaphore.Release(1);
    }

    public async Task<MuninnResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var fullPath = GetFullPath(key);

        if (string.IsNullOrEmpty(fullPath))
        {
            return new MuninnResult(true, null, $"File {key} is not found");
        }

        var entryFileData = GetEntryFileData(fullPath);
        var streamResult = CreateStream(fullPath, entryFileData.Encoding, false, _defaultBufferSize);

        if (streamResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamResult.Exception);
        }

        var streamReader = streamResult.GetStream<StreamReader>();
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            var entry = await ReadFileAsync(streamReader, key, entryFileData.Encoding, entryFileData.LifeTime, cancellationToken);
            DeleteFile(fullPath);

            return new MuninnResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileDelete(key, exception);

            return CreateFailedMuninResult(exception);
        }
        finally
        {
            await ReleaseStreamAsync(streamReader);
        }
    }

    public async Task<MuninnResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var fullPath = Directory.GetFiles(_directoryPath, key).FirstOrDefault();

        if (string.IsNullOrEmpty(fullPath))
        {
            return new MuninnResult(true, null, "File is not found");
        }

        var entryFileData = GetEntryFileData(fullPath);
        var streamResult = CreateStream(fullPath, entryFileData.Encoding, false, _defaultBufferSize);

        if (streamResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamResult.Exception);
        }

        var streamReader = streamResult.GetStream<StreamReader>();

        try
        {
            var entry = await ReadFileAsync(streamReader, key, entryFileData.Encoding, entryFileData.LifeTime, cancellationToken);

            return new MuninnResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileRead(key, exception);

            return CreateFailedMuninResult(exception);
        }
        finally
        {
            await ReleaseStreamAsync(streamReader);
        }
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken)
    {
        var filePaths = Directory.GetFiles(_directoryPath, $"*{FileExtension}");

        if (!filePaths.Any())
        {
            return [];
        }

        var entries = new ConcurrentBag<Entry>();
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = filePaths.Length >= 100 ? 100 : filePaths.Length
        };

        await Parallel.ForEachAsync(filePaths, parallelOptions, async (filePath, token) =>
        {
            var entryFileData = GetEntryFileData(filePath);
            var key = entryFileData.Key;
            var streamResult = CreateStream(filePath, entryFileData.Encoding, false, _defaultBufferSize);

            if (streamResult.Stream is null)
            {
                return;
            }

            var streamReader = streamResult.GetStream<StreamReader>();

            try
            {
                entries.Add(await ReadFileAsync(streamReader, key, entryFileData.Encoding, entryFileData.LifeTime, token));
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

        return entries.OrderBy(entry => entry.Key).ToList();
    }

    public async Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        var entries = await GetAllAsync(cancellationToken);

        return _filterManager.FilterEntryKeys(entries.ToArray(), chunks, cancellationToken);
    }

    public async Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<ValueFilter>> chunks, CancellationToken cancellationToken)
    {
        var entries = await GetAllAsync(cancellationToken);

        return _filterManager.FilterEntryValues(entries.ToArray(), chunks, cancellationToken);
    }

    public void Initialize()
    {
        if (!Directory.Exists(_directoryPath))
        {
            Directory.CreateDirectory(_directoryPath, UnixFileMode.None);
        }
    }

    private static async Task<Entry> ReadFileAsync(StreamReader streamReader, string key, Encoding encoding, TimeSpan lifeTime, CancellationToken cancellationToken)
    {
        var result = await streamReader.ReadToEndAsync(cancellationToken);
        var value = encoding.GetBytes(result);

        return new Entry(key, value, encoding, lifeTime);
    }

    private StreamResult CreateStream(string fullPath, Encoding encoding, bool isWriting, int bufferSize)
    {
        try
        {
            FileStream fileStream;
            IDisposable stream;

            if (isWriting)
            { 
                fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous);
                var streamWriter = new StreamWriter(fileStream, encoding, bufferSize)
                {
                    AutoFlush = true,
                };
                _streamWriters.Add(streamWriter);
                stream = streamWriter;
            }
            else
            {
                fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, bufferSize, FileOptions.Asynchronous);
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
            _semaphore.Release(1);
            _streamWriters.Remove(streamWriter);

            return;
        }

        disposable.Dispose();
    }

    private static EntryFileData GetEntryFileData(string fullPath)
    {
        var split = Path.GetFileNameWithoutExtension(fullPath).Split('-');
        var key = split[KeyPosition];
        var encoding = Encoding.GetEncoding(split[EncodingPosition]);
        var lifeTime = TimeSpan.Parse(split[LifetimePosition]);

        return new EntryFileData(key, encoding, lifeTime);
    }

    private string BuildFullPath(string key, string encoding, string lifeTime) => Path.Combine(_directoryPath, $"{key}-{encoding}-{lifeTime}{FileExtension}");

    private string GetFullPath(string key) => Directory.GetFiles(_directoryPath, key).FirstOrDefault() ?? string.Empty;

    private static void DeleteFile(string fullPath) => File.Delete(fullPath);

    private static MuninnResult CreateFailedMuninResult(Exception exception, string message = "") => new(false, null, message, exception);
}
