using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Catalog.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Catalog bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyCatalogConfiguration(this ModelBuilder builder)
    {
        builder.Entity<ProviderProduct>().HasKey(p => p.Id);
        builder.Entity<ProviderProduct>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<ProviderProduct>().Property(p => p.FuelType).IsRequired().HasMaxLength(80);
        builder.Entity<ProviderProduct>().Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Entity<ProviderProduct>().Property(p => p.Description).HasMaxLength(400);
        builder.Entity<ProviderProduct>().Property(p => p.PricePerLiter).HasPrecision(12, 2);
        builder.Entity<ProviderProduct>().Property(p => p.Unit).HasMaxLength(20);

        builder.Entity<FavoriteProvider>().HasKey(f => f.Id);
        builder.Entity<FavoriteProvider>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<FavoriteProvider>().Property(f => f.CompanyId).IsRequired();
        builder.Entity<FavoriteProvider>().Property(f => f.ProviderId).IsRequired();
        builder.Entity<FavoriteProvider>().HasIndex(f => new { f.CompanyId, f.ProviderId }).IsUnique();

        builder.Entity<ProviderRating>().HasKey(r => r.Id);
        builder.Entity<ProviderRating>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<ProviderRating>().Property(r => r.CompanyId).IsRequired();
        builder.Entity<ProviderRating>().Property(r => r.ProviderId).IsRequired();
        builder.Entity<ProviderRating>().Property(r => r.Rating).IsRequired();
        builder.Entity<ProviderRating>().HasIndex(r => new { r.CompanyId, r.ProviderId }).IsUnique();
    }
}
