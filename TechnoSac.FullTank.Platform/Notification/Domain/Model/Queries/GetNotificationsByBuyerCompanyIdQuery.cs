namespace TechnoSac.FullTank.Platform.Notification.Domain.Model.Queries;

/// <summary>Query to get notifications addressed to a given buyer company id.</summary>
public record GetNotificationsByBuyerCompanyIdQuery(int BuyerCompanyId);
