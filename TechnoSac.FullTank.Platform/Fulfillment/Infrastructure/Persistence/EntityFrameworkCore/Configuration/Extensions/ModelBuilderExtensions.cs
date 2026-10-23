using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Fulfillment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Fulfillment bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyFulfillmentConfiguration(this ModelBuilder builder)
    {
        // Driver aggregate.
        builder.Entity<Driver>().HasKey(d => d.Id);
        builder.Entity<Driver>().Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Driver>().Property(d => d.Name).IsRequired().HasMaxLength(120);
        builder.Entity<Driver>().Property(d => d.LicenseNumber).HasMaxLength(60);
        builder.Entity<Driver>().Property(d => d.Phone).HasMaxLength(40);
        builder.Entity<Driver>().Property(d => d.Email).HasMaxLength(160);
        builder.Entity<Driver>().Property(d => d.Status).HasMaxLength(40).IsConcurrencyToken();

        // Vehicle aggregate.
        builder.Entity<Vehicle>().HasKey(v => v.Id);
        builder.Entity<Vehicle>().Property(v => v.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Vehicle>().Property(v => v.Plate).IsRequired().HasMaxLength(20);
        builder.Entity<Vehicle>().Property(v => v.Brand).HasMaxLength(80);
        builder.Entity<Vehicle>().Property(v => v.Model).HasMaxLength(80);
        builder.Entity<Vehicle>().Property(v => v.Unit).HasMaxLength(20);
        builder.Entity<Vehicle>().Property(v => v.Status).HasMaxLength(40).IsConcurrencyToken();

        // Delivery aggregate. Stores only foreign-key ids to other bounded contexts (no navigation references).
        builder.Entity<Delivery>().HasKey(d => d.Id);
        builder.Entity<Delivery>().Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Delivery>().Property(d => d.Status).HasMaxLength(40);
        builder.Entity<Delivery>().Property(d => d.OriginLocation).HasMaxLength(200);
        builder.Entity<Delivery>().Property(d => d.DestinationLocation).HasMaxLength(200);
        builder.Entity<Delivery>().Property(d => d.DispatchedAt).HasMaxLength(40);
        builder.Entity<Delivery>().Property(d => d.DeliveredAt).HasMaxLength(40);
        builder.Entity<Delivery>().Property(d => d.Notes).HasMaxLength(500);
        builder.Entity<Delivery>().HasIndex(d => d.OrderId).IsUnique();
    }
}
