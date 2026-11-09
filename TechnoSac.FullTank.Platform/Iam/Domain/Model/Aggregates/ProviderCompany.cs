using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a fuel provider company.</summary>
/// <remarks>
///     <see cref="FuelTypesOffered" /> is persisted as a comma-separated string and exposed as a list
///     at the REST boundary through <see cref="FuelTypesAsList" />.
/// </remarks>
public class ProviderCompany : IAuditableEntity
{
    /// <summary>Protected parameterless constructor for EF Core.</summary>
    protected ProviderCompany()
    {
        Name = string.Empty;
        Ruc = string.Empty;
        Address = string.Empty;
        Phone = string.Empty;
        FuelTypesOffered = string.Empty;
        Description = string.Empty;
    }

    public ProviderCompany(CreateProviderCompanyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        Ruc = command.Ruc;
        Rating = command.Rating;
        Address = command.Address;
        Phone = command.Phone;
        FuelTypesOffered = JoinFuelTypes(command.FuelTypesOffered);
        Description = command.Description;
    }

    public void Update(UpdateProviderCompanyCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Name = command.Name;
        Ruc = command.Ruc;
        Rating = command.Rating;
        Address = command.Address;
        Phone = command.Phone;
        FuelTypesOffered = JoinFuelTypes(command.FuelTypesOffered);
        Description = command.Description;
    }

    private static string JoinFuelTypes(IEnumerable<string>? fuelTypes)
    {
        return fuelTypes is null
            ? string.Empty
            : string.Join(',', fuelTypes.Where(f => !string.IsNullOrWhiteSpace(f)));
    }

    /// <summary>Returns the offered fuel types as a list (split from the stored comma-separated value).</summary>
    public IReadOnlyList<string> FuelTypesAsList()
    {
        return string.IsNullOrWhiteSpace(FuelTypesOffered)
            ? []
            : FuelTypesOffered.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Ruc { get; private set; }
    public decimal Rating { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public string FuelTypesOffered { get; private set; }
    public string Description { get; private set; }

    /// <inheritdoc />
    public DateTimeOffset? CreatedAt { get; set; }

    /// <inheritdoc />
    public DateTimeOffset? UpdatedAt { get; set; }
}
