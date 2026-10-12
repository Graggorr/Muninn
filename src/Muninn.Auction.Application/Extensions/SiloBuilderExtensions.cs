using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Muninn.Auction.Application.Filters;
using Orleans.Configuration;
using StackExchange.Redis;
using static System.ArgumentException;
using static System.Int32;
using static Muninn.Auction.Common.Constants;
using static Muninn.Auction.Application.Contracts.Constants;

namespace Muninn.Auction.Application.Extensions;

public static class SiloBuilderExtensions
{
    public static ISiloBuilder RegisterOrleans(this ISiloBuilder builder, IConfiguration configuration)
    {
        var redis = configuration.GetSection(RedisSectionKey);
        var connectionString = redis[ConnectionStringTagKey];
        ThrowIfNullOrWhiteSpace(connectionString);

        var clusteringDatabase = TryParse(redis["ClusteringDatabase"], out var clustering) ? clustering : 0;
        var storageDatabase = TryParse(redis["GrainStorageDatabase"], out var storage) ? storage : 1;

        builder.UseRedisClustering(options => options.ConfigurationOptions = Build(connectionString, clusteringDatabase));

        builder.AddRedisGrainStorage(GrainStorageName,
            options => options.ConfigurationOptions = Build(connectionString, storageDatabase));

        builder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = configuration[$"{OrleansSectionKey}:ClusterId"] ?? "auction";
            options.ServiceId = configuration[$"{OrleansSectionKey}:ServiceId"] ?? "muninn-auction";
        });

        builder.AddIncomingGrainCallFilter<BaggageIncomingCallFilter>();
        builder.AddOutgoingGrainCallFilter<BaggageOutgoingCallFilter>();

        return builder;
    }

    private static ConfigurationOptions Build(string connectionString, int database)
    {
        var options = ConfigurationOptions.Parse(connectionString);
        options.DefaultDatabase = database;

        return options;
    }
}
