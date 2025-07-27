using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Extensions;
using Muninn.Kernel.Models;

namespace Muninn.Kernel.Persistent;

internal class PersistentCache(ILogger<IPersistentCache> logger, PersistentConfiguration persistentConfiguration, IFilterManager filterManager) : IPersistentCache
{
    private readonly ref struct StreamResult(IDisposable? stream, Exception? exception)
    {
        public IDisposable? Stream { get; } = stream;

        public Exception? Exception { get; } = exception;

        public StreamWriter StreamWriter => (Stream as StreamWriter)!;

        public StreamReader StreamReader => (Stream as StreamReader)!;
    }

    private const string FILE_EXTENSION = ".munin";

    private readonly ILogger _logger = logger;
    private readonly IFilterManager _filterManager = filterManager;
    private readonly string _directoryPath = persistentConfiguration.DirectoryPath;
    private readonly int _defaultBufferSize = persistentConfiguration.DefaultBufferSize;
    private readonly ConcurrentDictionary<int, ConcurrentHashSet<IDisposable>> _registeredStreams = [];

    private bool _isBlocked;

    public async Task<MuninResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var fullPath = BuildFullPath(entry.Key, entry.EncodingName);
        var encoding = Encoding.GetEncoding(entry.EncodingName);
        var streamResult = CreateStream(entry.Hashcode, fullPath, encoding, true, entry.Value.Length);

        if (streamResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamResult.Exception);
        }

        var streamWriter = streamResult.StreamWriter;

        BlockExecutionWhileTrue(() => _isBlocked);

        try
        {
            var value = encoding.GetChars(entry.Value);
            await streamWriter.WriteAsync(value, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileInsert(entry.Key, exception);

            return CreateFailedMuninResult(exception, "Cannot write data into the file");
        }
        finally
        {
            await ReleaseStreamAsync(streamWriter, entry.Hashcode);
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        _isBlocked = true;

        foreach (var hashSet in _registeredStreams.Values)
        {
            foreach (var disposable in hashSet)
            {
                if (disposable is StreamWriter streamWriter)
                {
                    await streamWriter.DisposeAsync();
                    continue;
                }

                ((StreamReader)disposable).Close();
            }
        }

        var filePaths = Directory.GetFiles(_directoryPath, $"*{FILE_EXTENSION}");

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                File.Delete(filePath);
            }
            catch (Exception exception)
            {
                _logger.LogFailedFileDelete(Path.GetFileName(filePath), exception);
            }
        }

        _registeredStreams.Clear();
    }

    public async Task<MuninResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var fullPath = Directory.GetFiles(_directoryPath, key).FirstOrDefault();

        if (string.IsNullOrEmpty(fullPath))
        {
            return new MuninResult(true, null, $"File {key} is not found");
        }

        const int dummyBufferSize = 0;
        var encoding = GetEncoding(fullPath);
        var hashCode = key.GetHashCode();

        var dummyStreamResult = CreateStream(hashCode, fullPath, encoding, true, dummyBufferSize);
        var streamReaderResult = CreateStream(hashCode, fullPath, encoding, false, _defaultBufferSize);

        if (dummyStreamResult.Exception is not null)
        {
            return CreateFailedMuninResult(dummyStreamResult.Exception);
        }

        if (streamReaderResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamReaderResult.Exception);
        }

        var streamReader = streamReaderResult.StreamReader;
        var dummyStream = dummyStreamResult.StreamWriter;

        try
        {
            var entry = await ReadFileAsync(streamReader, key, encoding, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileDelete(key, exception);

            return CreateFailedMuninResult(exception);
        }
        finally
        {
            await ReleaseStreamAsync(dummyStream, hashCode);
            await ReleaseStreamAsync(streamReader, hashCode);
        }
    }

    public async Task<MuninResult> GetAsync(string key, CancellationToken cancellationToken)
    {
        var fullPath = Directory.GetFiles(_directoryPath, key).FirstOrDefault();

        if (string.IsNullOrEmpty(fullPath))
        {
            return new MuninResult(true, null, "File is not found");
        }

        var encoding = GetEncoding(fullPath);
        var hashCode = key.GetHashCode();
        var streamReaderResult = CreateStream(hashCode, fullPath, encoding, false, _defaultBufferSize);

        if (streamReaderResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamReaderResult.Exception);
        }

        var streamReader = streamReaderResult.StreamReader;

        try
        {
            var entry = await ReadFileAsync(streamReader, key, encoding, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            _logger.LogFailedFileRead(key, exception);

            return CreateFailedMuninResult(exception);
        }
        finally
        {
            await ReleaseStreamAsync(streamReader, hashCode);
        }
    }

    public async Task<IEnumerable<Entry>> GetAllAsync(CancellationToken cancellationToken)
    {
        var filePaths = Directory.GetFiles(_directoryPath, $"*{FILE_EXTENSION}");
        var entries = new List<Entry>(filePaths.Length);

        foreach (var filePath in filePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileNameSplit = Path.GetFileNameWithoutExtension(filePath).Split('-');
            var key = fileNameSplit[0];
            var encoding = Encoding.GetEncoding(fileNameSplit[^1]);
            var hashCode = key.GetHashCode();
            var streamResult = CreateStream(hashCode, filePath, encoding, false, _defaultBufferSize);

            if (streamResult.Stream is null)
            {
                continue;
            }

            var streamReader = streamResult.StreamReader;

            try
            {
                entries.Add(await ReadFileAsync(streamReader, key, encoding, cancellationToken));
            }
            catch (Exception exception)
            {
                _logger.LogFailedFileRead(key, exception);
            }
            finally
            {
                await ReleaseStreamAsync(streamReader, hashCode);
            }
        }

        return entries;
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

    private static async Task<Entry> ReadFileAsync(StreamReader streamReader, string key, Encoding encoding, CancellationToken cancellationToken)
    {
        var result = await streamReader.ReadToEndAsync(cancellationToken);
        var value = encoding.GetBytes(result);

        return new Entry(key, value)
        {
            EncodingName = encoding.EncodingName
        };
    }

    private StreamResult CreateStream(int hashcode, string fullPath, Encoding encoding, bool isWriting, int bufferSize)
    {
        var hashSet = _registeredStreams.GetOrAdd(hashcode, _ => new ConcurrentHashSet<IDisposable>());

        try
        {
            FileStream fileStream;
            IDisposable stream;

            if (isWriting)
            {
                BlockExecutionWhileTrue(() => hashSet.Any(item => item.GetType() == typeof(StreamWriter)));
                fileStream = new FileStream(fullPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, bufferSize, FileOptions.WriteThrough | FileOptions.Asynchronous);
                stream = new StreamWriter(fileStream, encoding, bufferSize)
                {
                    AutoFlush = true
                };
            }
            else
            {
                fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.Asynchronous);
                stream = new StreamReader(fileStream, encoding);
            }

            hashSet.Add(stream);

            return new StreamResult(stream, null);
        }
        catch (Exception exception)
        {
            var operation = isWriting ? "writing" : "reading";
            _logger.LogFailedStreamCreate(operation, Path.GetFileName(fullPath), exception);

            return new StreamResult(null, exception);
        }
    }

    private async ValueTask ReleaseStreamAsync(IDisposable disposable, int hashcode)
    {
        if (disposable is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            disposable.Dispose();
        }

        _registeredStreams[hashcode].Remove(disposable);
    }

    private string BuildFullPath(string key, string encoding) => Path.Combine(_directoryPath, $"{key}-{encoding}{FILE_EXTENSION}");

    private static Encoding GetEncoding(string fullPath)
    {
        var encodingName = Path.GetFileNameWithoutExtension(fullPath).Split('-')[^1];

        return Encoding.GetEncoding(encodingName);
    }

    private static void BlockExecutionWhileTrue(Func<bool> predicate)
    {
        while (predicate())
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }
    }

    private static MuninResult CreateFailedMuninResult(Exception exception, string message = "") => new(false, null, message, exception);
}
