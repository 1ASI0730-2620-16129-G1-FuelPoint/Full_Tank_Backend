using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Payment bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyPaymentConfiguration(this ModelBuilder builder)
    {
        // Payment aggregate. Stores only foreign-key ids to other bounded contexts (no navigation references).
        builder.Entity<PaymentAggregate>().HasKey(p => p.Id);
        builder.Entity<PaymentAggregate>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<PaymentAggregate>().Property(p => p.Method).IsRequired().HasMaxLength(40);
        builder.Entity<PaymentAggregate>().Property(p => p.Amount).HasPrecision(14, 2);
        builder.Entity<PaymentAggregate>().Property(p => p.Status).HasMaxLength(40);
        builder.Entity<PaymentAggregate>().Property(p => p.MaskedCard).HasMaxLength(40);
        builder.Entity<PaymentAggregate>().Property(p => p.CardHolder).HasMaxLength(120);
        builder.Entity<PaymentAggregate>().Property(p => p.Reference).HasMaxLength(120);
        builder.Entity<PaymentAggregate>().HasIndex(p => p.OrderId).IsUnique();

        // Invoice aggregate. Stores only foreign-key ids to other bounded contexts (no navigation references).
        builder.Entity<Invoice>().HasKey(i => i.Id);
        builder.Entity<Invoice>().Property(i => i.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Invoice>().Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(60);
        builder.Entity<Invoice>().Property(i => i.ProviderRuc).HasMaxLength(20);
        builder.Entity<Invoice>().Property(i => i.ProviderName).HasMaxLength(160);
        builder.Entity<Invoice>().Property(i => i.BuyerRuc).HasMaxLength(20);
        builder.Entity<Invoice>().Property(i => i.BuyerName).HasMaxLength(160);
        builder.Entity<Invoice>().Property(i => i.FuelType).HasMaxLength(80);
        builder.Entity<Invoice>().Property(i => i.Unit).HasMaxLength(20);
        builder.Entity<Invoice>().Property(i => i.UnitPrice).HasPrecision(12, 2);
        builder.Entity<Invoice>().Property(i => i.Subtotal).HasPrecision(14, 2);
        builder.Entity<Invoice>().Property(i => i.Igv).HasPrecision(14, 2);
        builder.Entity<Invoice>().Property(i => i.Total).HasPrecision(14, 2);
        builder.Entity<Invoice>().Property(i => i.IssueDate).HasMaxLength(40);
        builder.Entity<Invoice>().Property(i => i.Status).HasMaxLength(40);
        builder.Entity<Invoice>().HasIndex(i => i.PaymentId).IsUnique();
        builder.Entity<Invoice>().HasIndex(i => i.OrderId).IsUnique();
        builder.Entity<Invoice>().HasIndex(i => i.InvoiceNumber).IsUnique();
    }
}
