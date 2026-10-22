using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;

namespace TechnoSac.FullTank.Platform.Equipment.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Equipment bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyEquipmentConfiguration(this ModelBuilder builder)
    {
        builder.Entity<EquipmentAggregate>().HasKey(e => e.Id);
        builder.Entity<EquipmentAggregate>().Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<EquipmentAggregate>().Property(e => e.Name).IsRequired().HasMaxLength(120);
        builder.Entity<EquipmentAggregate>().Property(e => e.Type).HasMaxLength(80);
        builder.Entity<EquipmentAggregate>().Property(e => e.RequiredFuelType).HasMaxLength(80);
        builder.Entity<EquipmentAggregate>().Property(e => e.Unit).HasMaxLength(20);
        builder.Entity<EquipmentAggregate>().Property(e => e.Status).HasMaxLength(40);
        builder.Entity<EquipmentAggregate>().Property(e => e.Location).HasMaxLength(200);
        builder.Entity<EquipmentAggregate>().Property(e => e.LastRefillDate).HasMaxLength(40);

        builder.Entity<RefillHistory>().HasKey(r => r.Id);
        builder.Entity<RefillHistory>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<RefillHistory>().Property(r => r.FuelType).IsRequired().HasMaxLength(80);
    }
}
