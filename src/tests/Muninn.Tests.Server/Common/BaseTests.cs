using Microsoft.Extensions.Logging;

namespace Muninn.Server.Tests.Common;

public abstract class BaseTests<TService>
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    
    protected readonly ILogger<TService> _logger;
    protected CancellationToken CancellationToken => _cancellationTokenSource.Token;
    
    protected BaseTests()
    {
        var loggerFactory = LoggerFactory.Create(factory => { factory.SetMinimumLevel(LogLevel.Information); });
        _cancellationTokenSource = new CancellationTokenSource();
        _logger = new Logger<TService>(loggerFactory);
    }

    protected void CancelAfter(TimeSpan time) => _cancellationTokenSource.CancelAfter(time);

    protected void Cancel() => _cancellationTokenSource.Cancel();
}
