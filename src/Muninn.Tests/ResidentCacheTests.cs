using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Muninn.Kernel.Common;
using Muninn.Kernel.Models;
using Muninn.Kernel.Resident;
using Muninn.Kernel.Shared;

namespace Muninn.Tests;

public class ResidentCacheTests
{
    private readonly ResidentConfiguration _configuration = new();
    private readonly FilterManager _filterManager = new();
    private readonly ResidentCache _residentCache;

    public ResidentCacheTests()
    {
        var loggerFactory = LoggerFactory.Create(factory =>
        {
            factory.SetMinimumLevel(LogLevel.Information);
        });
        var logger = new Logger<IResidentCache>(loggerFactory);
        _residentCache = new ResidentCache(logger, _configuration, _filterManager);
    }

    [Fact]
    public async Task AddEntryWhenNoDuplicate_ShouldReturnSuccess()
    {
        // Arrange

        var entry = new Entry("Key", [0, 1, 2], Encoding.ASCII, TimeSpan.FromHours(1));
        
        // Act

        var result = await _residentCache.AddAsync(entry, CancellationToken.None);

        // Assert

        Assert.True(result.IsSuccessful);
        Assert.True(result.Entry is not null);
    }

    [Fact]
    public async Task AddEntryWhenKeyIsDuplicated_ShouldReturnFail()
    {
        // Arrange

        var entry = new Entry("Key", [0, 1, 2], Encoding.ASCII, TimeSpan.FromHours(1));
        var duplicatedEntry = new Entry("Key", [0, 1, 2], Encoding.ASCII, TimeSpan.FromHours(1));

        // Act

        var successfulResult = await _residentCache.AddAsync(entry, CancellationToken.None);
        var failedResult = await _residentCache.AddAsync(duplicatedEntry, CancellationToken.None);

        // Assert

        Assert.True(successfulResult.IsSuccessful);
        Assert.False(failedResult.IsSuccessful);
    }
}
