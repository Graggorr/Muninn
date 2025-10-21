using Microsoft.Extensions.Logging;
using Muninn.Kernel;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Persistent;
using Muninn.Kernel.Resident;
using Muninn.Kernel.Shared;
using Muninn.Server.Tests.Common;
using Muninn.Tests.Shared;
using Muninn.Tests.Shared.Extensions;
using Shouldly;

namespace Muninn.Server.Tests;

public class CacheManagerTests : BaseTests<ICacheManager>
{
    private readonly CacheManager _cacheManager;
    private readonly PersistentCache _persistentCache;
    
    public CacheManagerTests()
    {
        var loggerFactory = LoggerFactory.Create(factory => { factory.SetMinimumLevel(LogLevel.Debug); });
        var filterManager = new FilterManager();
        _persistentCache = new PersistentCache(loggerFactory.CreateLogger<IPersistentCache>(), filterManager);
        var residentCache = new ResidentCache(loggerFactory.CreateLogger<IResidentCache>(), filterManager);
        var sortedResidentCache = new SortedResidentCache(loggerFactory.CreateLogger<ISortedResidentCache>(), filterManager);
        _cacheManager = new CacheManager(_persistentCache, residentCache, new BackgroundManager(), sortedResidentCache);
    }

    // [Fact]
    // public async Task AddAsync()
    // {
    //     // Arrange
    //
    //     var entry = EntryCreator.CreateRandomEntry();
    //     
    //     // Act
    //
    //     var result = await _cacheManager.AddAsync(entry, CancellationToken);
    //     
    //     // Assert
    //     
    //     result.ShouldBeSuccessful();
    //     FileShouldExist(entry);
    // }
    
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
