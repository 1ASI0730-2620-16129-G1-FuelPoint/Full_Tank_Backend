using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Inventory.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Inventory bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyInventoryConfiguration(this ModelBuilder builder)
    {
        builder.Entity<InventoryItem>().HasKey(i => i.Id);
        builder.Entity<InventoryItem>().Property(i => i.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<InventoryItem>().Property(i => i.Name).IsRequired().HasMaxLength(120);
        builder.Entity<InventoryItem>().Property(i => i.Type).HasMaxLength(80);
        builder.Entity<InventoryItem>().Property(i => i.Description).HasMaxLength(400);
        builder.Entity<InventoryItem>().Property(i => i.PricePerLiter).HasPrecision(12, 2);
        builder.Entity<InventoryItem>().Property(i => i.Stock).IsRequired().IsConcurrencyToken();
        builder.Entity<InventoryItem>().Property(i => i.Reserved).IsRequired().IsConcurrencyToken();
        builder.Entity<InventoryItem>().Property(i => i.Unit).HasMaxLength(20);
        builder.Entity<InventoryItem>().Property(i => i.Status).HasMaxLength(40);

        builder.Entity<InventoryMovement>().HasKey(m => m.Id);
        builder.Entity<InventoryMovement>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<InventoryMovement>().Property(m => m.Type).IsRequired().HasMaxLength(20);
        builder.Entity<InventoryMovement>().Property(m => m.Reason).HasMaxLength(240);
    }
}
