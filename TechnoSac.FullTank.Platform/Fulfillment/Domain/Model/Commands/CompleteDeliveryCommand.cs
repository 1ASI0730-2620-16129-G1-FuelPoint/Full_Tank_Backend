namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;

/// <summary>Command to mark a delivery as completed (delivered).</summary>
public record CompleteDeliveryCommand(int Id, string? DeliveredAt = null);
