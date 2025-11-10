using Muninn.Kernel.Common;
using Muninn.Kernel.Resident;
using Muninn.Kernel.Shared;
using Muninn.Server.Tests.Common;
using Muninn.Tests.Shared;
using Muninn.Tests.Shared.Extensions;
using Shouldly;

namespace Muninn.Server.Tests.Resident;

public class SortedResidentCacheTests : BaseTests<ISortedResidentCache>
{
    private readonly SortedResidentCache _sortedResidentCache;

    public SortedResidentCacheTests()
    {
        _sortedResidentCache = new SortedResidentCache(_logger, new FilterService());
    }

    [Fact]
    public async Task SortWhenEntriesExist_ShouldReturnSuccess()
    { 
        // Act

        await AddEntriesAsync(1000, CancellationToken);
        var result = (await _sortedResidentCache.GetAllAsync(false, CancellationToken)).ToList();
        
        // Assert

        result.ShouldBeInOrder(SortDirection.Ascending, new EntryComparer());
    }

    [Fact]
    public async Task SortWhenCancellationIsCalled_ShouldReturnFail()
    {
        // Arrange

        await AddEntriesAsync(10000, CancellationToken);
        
        // Act
        
        Cancel();
        var result = await _sortedResidentCache.SortAsync(CancellationToken);
        
        // Assert
        
        result.ShouldBeCancelled();
    }

    [Fact]
    public async Task GetEntryWhenEntryExists_ShouldReturnSuccess()
    {
        // Arrange

        await AddEntriesAsync(10, CancellationToken);
        var entry = EntryCreator.CreateRandomEntry();
        
        // Act

        var addResult = await _sortedResidentCache.AddAsync(entry, CancellationToken);
        var getResult = await _sortedResidentCache.GetAsync(entry.Key, CancellationToken);
        
        // Assert
        
        addResult.ShouldBeSuccessful(entry);
        getResult.ShouldBeSuccessful(entry);
    }

    [Fact]
    public async Task GetEntryWhenEntryDoesNotExist_ShouldReturnFail()
    {
        // Arrange

        await AddEntriesAsync(1000);
        
        // Act

        var result = await _sortedResidentCache.GetAsync(Guid.CreateVersion7().ToString(), CancellationToken);
        
        // Assert
        
        result.ShouldBeFailed();
    }
    
    private Task AddEntriesAsync(int amount, CancellationToken cancellationToken = default)
    {
        var entries = Enumerable.Repeat(EntryCreator.CreateFixtureEntry, amount).ToList();
        return Task.WhenAll(entries.Select(func => _sortedResidentCache.AddAsync(func(), cancellationToken)));
    }
}
