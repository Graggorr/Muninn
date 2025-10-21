using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Persistent;
using Muninn.Kernel.Shared;
using Muninn.Server.Tests.Common;
using Muninn.Tests.Shared;
using Muninn.Tests.Shared.Extensions;
using Shouldly;

namespace Muninn.Server.Tests.Persistent;

public class PersistentCacheTests : BaseTests<IPersistentCache>
{
    private readonly PersistentCache _persistentCache;

    public PersistentCacheTests()
    {
        _persistentCache = new PersistentCache(_logger, new FilterManager());
        _persistentCache.Initialize();
        _persistentCache.ClearAsync(CancellationToken).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task InsertFile_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        var result = await _persistentCache.InsertAsync(entry, CancellationToken);
        
        // Assert

        result.ShouldBeSuccessful(entry);
        FileShouldExist(entry);
    }

    [Fact]
    public async Task InsertFileWhenCancelIsRequested_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        Cancel();
        var result = await _persistentCache.InsertAsync(entry, CancellationToken);
        
        // Assert

        result.ShouldBeCancelled();
        FileShouldNotExist(entry);
    }

    [Fact]
    public async Task GetEntryFromFileWhenItExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act 

        var insertResult = await _persistentCache.InsertAsync(entry, CancellationToken);
        var getResult = await _persistentCache.GetAsync(entry.Key, CancellationToken);
        
        // Assert
        
        insertResult.ShouldBeSuccessful(entry);
        getResult.ShouldBeSuccessfulAndEquivalentTo(entry);
    }

    [Fact]
    public async Task GetEntryFromFileWhenItDoesNotExist_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        var result = await _persistentCache.GetAsync(entry.Key, CancellationToken);
        
        // Assert
        
        result.ShouldBeFailed();
    }

    [Fact]
    public async Task Clear_ShouldReturnSuccess()
    {
        // Act

        var result = await _persistentCache.ClearAsync(CancellationToken);
        
        // Assert
        
        result.ShouldBeSuccessful();
    }

    [Fact]
    public async Task RemoveFileWhenItExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        var addResult = await _persistentCache.InsertAsync(entry, CancellationToken);
        var removeResult = await _persistentCache.RemoveAsync(entry.Key, CancellationToken);
        
        // Assert
        
        addResult.ShouldBeSuccessful(entry);
        removeResult.ShouldBeSuccessfulAndEquivalentTo(entry);
    }
    
    [Fact]
    public async Task RemoveFileWhenItDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        var result = await _persistentCache.RemoveAsync(entry.Key, CancellationToken);
        
        // Assert
        
        result.ShouldBeSuccessful();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllFiles()
    {
        // Arrange

        var entries = Enumerable.Repeat(EntryCreator.CreateRandomEntry, 1000).Select(func => func()).ToList();
        
        // Act

        await Parallel.ForEachAsync(entries, async (entry, cancellationToken) =>
        {
            await _persistentCache.InsertAsync(entry, cancellationToken);
        });
        var result = await _persistentCache.GetAllAsync(CancellationToken);
        
        // Assert
        
        result.ToList().Count.ShouldBeEquivalentTo(entries.Count);
    }
    
    private void FileShouldExist(Entry entry)
    {
        var fullPath = _persistentCache.BuildFullPath(entry);
        File.Exists(fullPath).ShouldBeTrue();
    }

    private void FileShouldNotExist(Entry entry)
    {
        var fullPath = _persistentCache.BuildFullPath(entry);
        File.Exists(fullPath).ShouldBeFalse();
    }
}
