using Microsoft.EntityFrameworkCore;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>EF Core model configuration for the Notification bounded context.</summary>
public static class ModelBuilderExtensions
{
    public static void ApplyNotificationConfiguration(this ModelBuilder builder)
    {
        // Notification aggregate. Stores only foreign-key ids and routes (no navigation references to other contexts).
        builder.Entity<NotificationAggregate>().ToTable("notifications");
        builder.Entity<NotificationAggregate>().HasKey(n => n.Id);
        builder.Entity<NotificationAggregate>().Property(n => n.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<NotificationAggregate>().Property(n => n.RecipientType)
            .HasColumnName("recipient_type").HasMaxLength(40);
        builder.Entity<NotificationAggregate>().Property(n => n.BuyerCompanyId).HasColumnName("buyer_company_id");
        builder.Entity<NotificationAggregate>().Property(n => n.ProviderId).HasColumnName("provider_id");
        builder.Entity<NotificationAggregate>().Property(n => n.Type).HasColumnName("type").HasMaxLength(80);
        builder.Entity<NotificationAggregate>().Property(n => n.Title).HasColumnName("title").HasMaxLength(160);
        builder.Entity<NotificationAggregate>().Property(n => n.Message).HasColumnName("message").HasMaxLength(1000);
        builder.Entity<NotificationAggregate>().Property(n => n.IsRead).HasColumnName("is_read");
        builder.Entity<NotificationAggregate>().Property(n => n.RelatedId).HasColumnName("related_id");
        builder.Entity<NotificationAggregate>().Property(n => n.TargetRoute)
            .HasColumnName("target_route").HasMaxLength(240);
        builder.Entity<NotificationAggregate>().Property(n => n.CreatedAt).HasColumnName("created_at");
        builder.Entity<NotificationAggregate>().Property(n => n.UpdatedAt).HasColumnName("updated_at");
    }
}
