using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muninn.Auction.Common.Extensions;
using Npgsql;
using static Muninn.Auction.Common.Constants;

namespace Muninn.Auction.Data.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterNpgsql(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
        var dataSource = new NpgsqlDataSourceBuilder(configuration.GetDatabaseConnectionString())
            .EnableDynamicJson()
            .Build();

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            options.UseNpgsql(dataSource)
                .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
                .UseSnakeCaseNamingConvention();
        });

        services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>(tags: [ReadyTag, LiveTag, PgsqlConnectivityTag]);
        
        return services;
    }
}
