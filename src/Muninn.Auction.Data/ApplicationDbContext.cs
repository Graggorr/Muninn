using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Muninn.Auction.Common.Extensions;
using Muninn.Auction.Data.Common;
using Muninn.Auction.Data.Entities;
using OpenTelemetry;
using static Muninn.Auction.Common.Constants;

namespace Muninn.Auction.Data;

internal sealed class ApplicationDbContext(IConfiguration configuration, DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    private readonly string _serviceName = configuration.GetServiceName();

    public DbSet<User> Users { get; set; }

    public DbSet<Admin> Admins { get; set; }

    public DbSet<Bid> Bids { get; set; }

    public DbSet<Lot> Lots { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditProperties();

        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        SetAuditProperties();

        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges()
    {
        SetAuditProperties();

        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetAuditProperties();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var typeless = typeof(StringToEnumConverter<>);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var type = property.ClrType;

                if (!IsEnumType(type))
                {
                    continue;
                }

                modelBuilder.Entity(((IMutableEntityType)property.DeclaringType).ClrType).Property(property.Name)
                    .HasConversion(typeless.MakeGenericType(type));
            }
        }
    }

    private bool IsEnumType(Type type)
    {
        if (type.IsEnum)
        {
            return true;
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            return type.GetGenericArguments()[0].IsEnum;
        }

        return false;
    }

    private void SetAuditProperties()
    {
        ChangeTracker.DetectChanges();
        var entities = ChangeTracker.Entries().ToList();

        var utcNow = DateTime.UtcNow;
        var actor = Baggage.GetBaggage(UserNameTagKey) ?? Baggage.GetBaggage(CallerNameTagKey) ?? _serviceName;

        foreach (var entityEntry in entities)
        {
            switch (entityEntry.State)
            {
                case EntityState.Added:
                    SetCreated(entityEntry, utcNow, actor);
                    break;
                case EntityState.Modified:
                    SetUpdated(entityEntry, utcNow, actor);
                    break;
                case EntityState.Deleted:
                    SetDeleted(entityEntry, utcNow, actor);
                    break;
            }
        }
    }

    private static void SetCreated(EntityEntry entityEntry, DateTime timeStamp, string actor)
    {
        if (entityEntry.Entity is ICreated created)
        {
            created.CreatedAt = timeStamp;
            created.CreatedBy = actor;
            SetUpdated(entityEntry, timeStamp, actor);
        }
    }

    private static void SetUpdated(EntityEntry entityEntry, DateTime timeStamp, string actor)
    {
        if (entityEntry.Entity is ILastUpdated lastUpdated)
        {
            lastUpdated.LastUpdatedAt = timeStamp;
            lastUpdated.LastUpdatedBy = actor;
        }
    }

    private static void SetDeleted(EntityEntry entityEntry, DateTime timeStamp, string actor)
    {
        if (entityEntry.Entity is IDeleted deleted)
        {
            entityEntry.State = EntityState.Modified;
            deleted.IsDeleted = true;
            SetUpdated(entityEntry, timeStamp, actor);
        }
    }
}
