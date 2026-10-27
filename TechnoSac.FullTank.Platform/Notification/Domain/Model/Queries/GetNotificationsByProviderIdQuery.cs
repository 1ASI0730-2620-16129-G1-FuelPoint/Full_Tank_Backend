namespace TechnoSac.FullTank.Platform.Notification.Domain.Model.Queries;

/// <summary>Query to get notifications addressed to a given provider id.</summary>
public record GetNotificationsByProviderIdQuery(int ProviderId);
