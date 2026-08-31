using Microsoft.Extensions.Configuration;
using static System.ArgumentException;
using static Muninn.Auction.Common.Constants;

namespace Muninn.Auction.Common.Extensions;

public static class ConfigurationExtensions
{
    extension(IConfiguration configuration)
    {
        public string GetServiceName()
        {
            var serviceName = configuration[ServiceNameTagKey];
            ThrowIfNullOrWhiteSpace(serviceName);

            return serviceName;
        }
    }
}
