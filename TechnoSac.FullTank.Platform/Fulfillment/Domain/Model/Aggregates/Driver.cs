using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a delivery driver.</summary>
public class Driver : IAuditableEntity
{
    protected Driver()
    {
        Name = string.Empty;
        LicenseNumber = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Status = string.Empty;
    }

    public Driver(CreateDriverCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        LicenseNumber = command.LicenseNumber;
        Phone = command.Phone;
        Email = command.Email;
        Status = command.Status;
        ProviderId = command.ProviderId;
    }

    public void Update(UpdateDriverCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        LicenseNumber = command.LicenseNumber;
        Phone = command.Phone;
        Email = command.Email;
        Status = command.Status;
    }

    public void SetStatus(string status)
    {
        if (!string.IsNullOrWhiteSpace(status))
            Status = status;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string LicenseNumber { get; private set; }
    public string Phone { get; private set; }
    public string Email { get; private set; }
    public string Status { get; private set; }
    public int? ProviderId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
