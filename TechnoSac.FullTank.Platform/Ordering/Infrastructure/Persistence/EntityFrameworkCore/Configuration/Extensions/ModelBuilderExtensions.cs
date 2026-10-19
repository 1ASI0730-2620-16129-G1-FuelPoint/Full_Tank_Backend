using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Ordering.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Ordering bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyOrderingConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Request>().ToTable("fuel_requests");
        builder.Entity<Request>().HasKey(r => r.Id);
        builder.Entity<Request>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Request>().Property(r => r.FuelType).HasMaxLength(80);
        builder.Entity<Request>().Property(r => r.ProductName).HasMaxLength(120);
        builder.Entity<Request>().Property(r => r.Unit).HasMaxLength(20);
        builder.Entity<Request>().Property(r => r.UnitPrice).HasPrecision(12, 2);
        builder.Entity<Request>().Property(r => r.DeliveryAddress).HasMaxLength(240);
        builder.Entity<Request>().Property(r => r.DeliveryDate).HasMaxLength(40);
        builder.Entity<Request>().Property(r => r.Status).HasMaxLength(40);
        builder.Entity<Request>().Property(r => r.Source).HasMaxLength(40);
        builder.Entity<Request>().Property(r => r.RejectionReasonCode).HasMaxLength(60);
        builder.Entity<Request>().Property(r => r.RejectionReasonNote).HasMaxLength(240);

        builder.Entity<Order>().HasKey(o => o.Id);
        builder.Entity<Order>().Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Order>().Property(o => o.FuelType).HasMaxLength(80);
        builder.Entity<Order>().Property(o => o.Unit).HasMaxLength(20);
        builder.Entity<Order>().Property(o => o.UnitPrice).HasPrecision(12, 2);
        builder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(14, 2);
        builder.Entity<Order>().Property(o => o.DeliveryAddress).HasMaxLength(240);
        builder.Entity<Order>().Property(o => o.Status).HasMaxLength(40).IsConcurrencyToken();
        builder.Entity<Order>().Property(o => o.PaymentStatus).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.EstimatedDeliveryDate).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.DispatchedAt).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.DeliveredAt).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.PaidAt).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.ClosedAt).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.CancelledAt).HasMaxLength(40);
        builder.Entity<Order>().Property(o => o.CancelReason).HasMaxLength(240);
        builder.Entity<Order>().HasIndex(o => o.RequestId).IsUnique();
    }
}
