using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a buyer (transport) company.</summary>
public class BuyerCompany : IAuditableEntity
{
    /// <summary>Protected parameterless constructor for EF Core.</summary>
    protected BuyerCompany()
    {
        Name = string.Empty;
        Ruc = string.Empty;
        Sector = string.Empty;
        Address = string.Empty;
        ContactEmail = string.Empty;
        Phone = string.Empty;
    }

    public BuyerCompany(CreateBuyerCompanyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        Ruc = command.Ruc;
        Sector = command.Sector;
        Address = command.Address;
        ContactEmail = command.ContactEmail;
        Phone = command.Phone;
    }

    public void Update(UpdateBuyerCompanyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        Ruc = command.Ruc;
        Sector = command.Sector;
        Address = command.Address;
        ContactEmail = command.ContactEmail;
        Phone = command.Phone;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Ruc { get; private set; }
    public string Sector { get; private set; }
    public string Address { get; private set; }
    public string ContactEmail { get; private set; }
    public string Phone { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }
}
