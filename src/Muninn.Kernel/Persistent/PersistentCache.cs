using Microsoft.Extensions.Logging;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using System.Collections.Concurrent;
using System.Text;

namespace Muninn.Kernel.Persistent;

internal class PersistentCache(ILogger<IPersistentCache> logger, PersistentConfiguration persistentConfiguration) : IPersistentCache
{
    private readonly ref struct StreamResult(IDisposable? stream, Exception? exception)
    {
        public IDisposable? Stream { get; } = stream;

        public Exception? Exception { get; init; } = exception;

        public StreamWriter StreamWriter => (Stream as StreamWriter)!;

        public StreamReader StreamReader => (Stream as StreamReader)!;
    }

    private static readonly Encoding _encoding = Encoding.ASCII;
    private const string FILE_EXTENSION = ".munin";

    private readonly ILogger _logger = logger;
    private readonly string _directoryPath = persistentConfiguration.DirectoryPath;
    private readonly int _defaultBufferSize = persistentConfiguration.DefaultBufferSize;
    private readonly ConcurrentDictionary<int, ConcurrentHashSet<IDisposable>> _registeredStreams = [];

    private bool _isBlocked = false;

    public async Task<MuninResult> InsertAsync(Entry entry, CancellationToken cancellationToken)
    {
        var fullPath = BuildFullPath(entry.Key);
        var streamResult = CreateStream(entry.Hashcode, fullPath, true, entry.Value.Length);

        if (streamResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamResult.Exception);
        }

        var streamWriter = streamResult.StreamWriter;

        BlockExecutionWhileTrue(() => _isBlocked);

        try
        {
            var value = _encoding.GetChars(entry.Value);

            await streamWriter.WriteAsync(value, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
            return new MuninResult(false, null, "Cannot write data into the file", exception);
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

            }
        }

        _registeredStreams.Clear();
    }

    public async Task<MuninResult> RemoveAsync(string key, CancellationToken cancellationToken)
    {
        var fullPath = BuildFullPath(key);
        var hashCode = key.GetHashCode();

        var dummyStreamResult = CreateStream(hashCode, fullPath, true, 0);
        var streamReaderResult = CreateStream(hashCode, fullPath, false, _defaultBufferSize);

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
            var entry = await ReadFileAsync(streamReader, key, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
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
        var hashCode = key.GetHashCode();
        var fullPath = BuildFullPath(key);
        var streamReaderResult = CreateStream(hashCode, fullPath, false, _defaultBufferSize);

        if (streamReaderResult.Exception is not null)
        {
            return CreateFailedMuninResult(streamReaderResult.Exception);
        }

        var streamReader = streamReaderResult.StreamReader;

        try
        {
            var entry = await ReadFileAsync(streamReader, key, cancellationToken);

            return new MuninResult(true, entry);
        }
        catch (Exception exception)
        {
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
            var key = Path.GetFileName(filePath);
            var hashCode = key.GetHashCode();
            var streamResult = CreateStream(hashCode, filePath, false, _defaultBufferSize);

            if (streamResult.Stream is null)
            {
                continue;
            }

            var streamReader = streamResult.StreamReader;
            try
            {
                entries.Add(await ReadFileAsync(streamReader, key, cancellationToken));
            }
            finally
            {
                await ReleaseStreamAsync(streamReader, hashCode);
            }
        }

        return entries;
    }

    public Task<IEnumerable<Entry>> GetEntriesByKeyFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        return null;
    }

    public Task<IEnumerable<Entry>> GetEntriesByValueFiltersAsync(IEnumerable<IEnumerable<KeyFilter>> chunks, CancellationToken cancellationToken)
    {
        return null;
    }

    private static async Task<Entry> ReadFileAsync(StreamReader streamReader, string key, CancellationToken cancellationToken)
    {
        var result = await streamReader.ReadToEndAsync(cancellationToken);
        var value = _encoding.GetBytes(result);

        return new Entry(key, value);
    }

    private StreamResult CreateStream(int hashcode, string fullPath, bool isWriting, int bufferSize)
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
                stream = new StreamWriter(fileStream, _encoding, bufferSize)
                {
                    AutoFlush = true
                };
            }
            else
            {
                fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, bufferSize, FileOptions.Asynchronous);
                stream = new StreamReader(fileStream, _encoding);
            }

            hashSet.Add(stream);

            return new StreamResult(stream, null);
        }
        catch (Exception exception)
        {
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

    private string BuildFullPath(string key) => Path.Combine(_directoryPath, $"{key}{FILE_EXTENSION}");

    private static void BlockExecutionWhileTrue(Func<bool> predicate)
    {
        while (predicate())
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(10));
        }
    }

    private static MuninResult CreateFailedMuninResult(Exception exception) => new(false, null, string.Empty, exception);
}
