using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace TechnoSac.FullTank.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     EF Core model configuration for the IAM bounded context (User, BuyerCompany, ProviderCompany).
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>Applies the IAM entity mappings to the shared model builder.</summary>
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        // User
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Name).IsRequired().HasMaxLength(120);
        builder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(160);
        builder.Entity<User>().Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Entity<User>().Property(u => u.Role).IsRequired().HasMaxLength(40);
        builder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        builder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        builder.Entity<User>().HasIndex(u => new { u.CompanyId, u.Role }).IsUnique();

        // BuyerCompany
        builder.Entity<BuyerCompany>().HasKey(c => c.Id);
        builder.Entity<BuyerCompany>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<BuyerCompany>().Property(c => c.Name).IsRequired().HasMaxLength(160);
        builder.Entity<BuyerCompany>().Property(c => c.Ruc).IsRequired().HasMaxLength(11);
        builder.Entity<BuyerCompany>().Property(c => c.Sector).HasMaxLength(80);
        builder.Entity<BuyerCompany>().Property(c => c.Address).HasMaxLength(200);
        builder.Entity<BuyerCompany>().Property(c => c.ContactEmail).HasMaxLength(160);
        builder.Entity<BuyerCompany>().Property(c => c.Phone).HasMaxLength(30);
        builder.Entity<BuyerCompany>().HasIndex(c => c.Ruc).IsUnique();

        // ProviderCompany
        builder.Entity<ProviderCompany>().HasKey(c => c.Id);
        builder.Entity<ProviderCompany>().Property(c => c.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<ProviderCompany>().Property(c => c.Name).IsRequired().HasMaxLength(160);
        builder.Entity<ProviderCompany>().Property(c => c.Ruc).IsRequired().HasMaxLength(11);
        builder.Entity<ProviderCompany>().Property(c => c.Address).HasMaxLength(200);
        builder.Entity<ProviderCompany>().Property(c => c.Phone).HasMaxLength(30);
        builder.Entity<ProviderCompany>().Property(c => c.Rating).HasPrecision(3, 2);
        builder.Entity<ProviderCompany>().Property(c => c.FuelTypesOffered).HasMaxLength(255);
        builder.Entity<ProviderCompany>().Property(c => c.Description).HasMaxLength(500);
        builder.Entity<ProviderCompany>().HasIndex(c => c.Ruc).IsUnique();
    }
}
