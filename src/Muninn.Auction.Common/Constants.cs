namespace Muninn.Auction.Common;

public static class Constants
{
    public const string ServiceNameTagKey = "ServiceName";

    public const string CallerNameTagKey = "CallerName";
    
    public const string CallerIdTagKey = "CallerId";
    
    public const string UserNameTagKey = "UserName";
    
    public const string ConnectionStringTagKey = "ConnectionString";
    
    /// <summary>
    /// Ready HealthCheck tag.
    /// </summary>
    public const string ReadyTag = "ready";

    /// <summary>
    /// Live HealthCheck tag.
    /// </summary>
    public const string LiveTag = "live";
    

    /// <summary>
    /// Tag marking a Health Check that probes PostgreSQL connectivity. Used to de-duplicate the
    /// DbContext connectivity check and the Orleans Npgsql grain-storage connectivity check when a
    /// service registers both against the same database.
    /// </summary>
    public const string PgsqlConnectivityTag = "db-connectivity:pgsql";
    
    /// <summary>
    /// Tag marking a Health Check that probes Valkey connectivity. The Orleans Valkey grain-storage
    /// connectivity check carries it so it is only de-duplicated against another Valkey probe, never
    /// against a PostgreSQL one.
    /// </summary>
    public const string OrleansConnectivityTag = "db-connectivity:orleans";
    
    /// <summary>
    /// Combined HealthCheck Url.
    /// </summary>
    public const string CombinedHealthCheckUrl = "/actuator/health";
}
