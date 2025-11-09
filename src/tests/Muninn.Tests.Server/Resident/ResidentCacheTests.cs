using System.Security.Cryptography;
using Muninn.Kernel.Common;
using Muninn.Kernel.Resident;
using Muninn.Kernel.Shared;
using Muninn.Server.Tests.Common;
using Muninn.Server.Tests.Extensions.cs;
using Muninn.Tests.Shared;
using Muninn.Tests.Shared.Extensions;
using Shouldly;

namespace Muninn.Server.Tests.Resident;

public class ResidentCacheTests : BaseTests<IResidentCache>
{
    private readonly FilterService _filterService = new();
    private readonly ResidentCache _residentCache;

    public ResidentCacheTests()
    {
        _residentCache = new ResidentCache(_logger, _filterService);
    }

    [Fact]
    public async Task AddEntryWhenNoDuplicated_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateFixtureEntry();

        // Act

        var result = await _residentCache.AddAsync(entry);

        // Assert

        result.ShouldBeSuccessful(entry);
    }

    [Fact]
    public async Task AddEntryWhenKeyIsDuplicated_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        var duplicatedEntry = entry.Clone();

        // Act

        var successfulResult = await _residentCache.AddAsync(entry);
        var failedResult = await _residentCache.AddAsync(duplicatedEntry);

        // Assert

        successfulResult.ShouldBeSuccessful(entry);
        failedResult.ShouldBeFailed();
    }

    [Fact]
    public async Task AddEntryWhenArrayCannotBeResized_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateFixtureEntry();

        // Act

        await AddEntriesAsync(ResidentCache.DefaultIncreaseValue, CancellationToken);
        Cancel();
        var result = await _residentCache.AddAsync(entry, CancellationToken);

        // Assert

        result.ShouldBeCancelled();
    }

    [Fact]
    public async Task AddEntryWhenArrayIsResized_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateFixtureEntry();

        // Act

        await AddEntriesAsync(1000);
        var isMaxArraySize = _residentCache.Length is ResidentCache.DefaultIncreaseValue;
        var result = await _residentCache.AddAsync(entry);

        // Assert

        result.ShouldBeSuccessful(entry);
        isMaxArraySize.ShouldBeTrue();
    }

    [Fact]
    public async Task GetEntryByKeyWhenItExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();

        // Act

        var addResult = await _residentCache.AddAsync(entry);
        var getResult = _residentCache.Get(entry.Key);

        // Assert

        addResult.ShouldBeSuccessful(entry);
        getResult.ShouldBeSuccessful(entry);
    }

    [Fact]
    public async Task GetEntryByKeyWhenItDoesNotExist_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();

        // Act

        var addResult = await _residentCache.AddAsync(entry);
        var getResult = _residentCache.Get(Guid.CreateVersion7().ToString());

        // Assert

        addResult.ShouldBeSuccessful(entry);
        getResult.ShouldBeFailed();
    }

    [Fact]
    public async Task GetAll_ShouldReturnSuccess()
    {
        // Arrange

        var count = RandomNumberGenerator.GetInt32(1, 1000);

        // Act

        await AddEntriesAsync(count);
        var entries = _residentCache.GetAll(CancellationToken.None).ToList();

        // Assert

        entries.ShouldNotContain(entry => false);
        entries.Count.ShouldBeEquivalentTo(count);
    }

    [Fact]
    public async Task RemoveEntryWhenItExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();

        // Act

        await AddEntriesAsync(RandomNumberGenerator.GetInt32(1, 1000));
        var addResult = await _residentCache.AddAsync(entry);
        var removeResult = await _residentCache.RemoveAsync(entry.Key);
        
        // Assert
        
        addResult.ShouldBeSuccessful(entry);
        removeResult.ShouldBeSuccessful(entry);
    }

    [Fact]
    public async Task UpdateEntryWhenItExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        var updateEntry = entry.Clone([0, 1, 3]);

        // Act

        var addResult = await _residentCache.AddAsync(entry);
        var updateResult = await _residentCache.UpdateAsync(updateEntry);

        // Assert

        addResult.ShouldBeSuccessful(entry);
        updateResult.ShouldBeSuccessful(updateEntry);
        addResult.Entry!.Value.ShouldNotBe(updateResult.Entry!.Value);
    }

    [Fact]
    public async Task UpdateEntryWhenItDoesNotExist_ShouldReturnFail()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();

        // Act

        var result = await _residentCache.UpdateAsync(entry);

        // Assert

        result.ShouldBeFailed();
    }

    [Fact]
    public async Task InsertEntryWhenEntryExists_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();
        var insertEntry = entry.Clone([0, 1, 8]);

        // Act

        var addResult = await _residentCache.AddAsync(entry);
        var insertResult = await _residentCache.InsertAsync(insertEntry);

        // Assert

        addResult.ShouldBeSuccessful(entry);
        insertResult.ShouldBeSuccessful(insertEntry);
        insertResult.Entry.ShouldNotBe(addResult.Entry);
    }

    [Fact]
    public async Task InsertEntryWhenEntryDoesNotExist_ShouldReturnSuccess()
    {
        // Arrange

        var entry = EntryCreator.CreateRandomEntry();

        // Act

        var result = await _residentCache.InsertAsync(entry);

        // Assert

        result.ShouldBeSuccessful(entry);
    }

    [Fact]
    public async Task ClearEntries_ShouldReturnSuccess()
    { 
        // Act

        await AddEntriesAsync(1000, CancellationToken);
        var result = await _residentCache.ClearAsync(CancellationToken);

        // Assert

        result.ShouldBeSuccessful();
    }

    [Fact]
    public async Task ClearEntriesWhenCancellationTokenHasBeenCalled_ShouldReturnFail()
    {
        // Act

        await AddEntriesAsync(1000, CancellationToken);
        Cancel();
        var result = await _residentCache.ClearAsync(CancellationToken);

        // Assert

        result.ShouldBeCancelled();
    }

    [Fact]
    public async Task InitializeAsync_ShouldReturnSuccess()
    {
        // Arrange 

        var entries = Enumerable.Repeat(EntryCreator.CreateFixtureEntry, 1000);

        // Act & Assert

        await _residentCache.InitializeAsync(entries.Select(entry => entry()).ToArray()).ShouldNotThrowAsync();
    }

    private Task AddEntriesAsync(int amount, CancellationToken cancellationToken = default)
    {
        var entries = Enumerable.Repeat(EntryCreator.CreateFixtureEntry, amount);

        return Task.WhenAll(entries.Select(func => _residentCache.AddAsync(func(), cancellationToken)));
    }
}