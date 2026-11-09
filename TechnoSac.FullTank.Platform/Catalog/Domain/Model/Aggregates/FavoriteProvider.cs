using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;

/// <summary>Aggregate root representing a buyer company's favorite fuel provider (referenced by IDs only).</summary>
public class FavoriteProvider : IAuditableEntity
{
    protected FavoriteProvider()
    {
    }

    public FavoriteProvider(CreateFavoriteProviderCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        CompanyId = command.CompanyId;
        ProviderId = command.ProviderId;
    }

    public int Id { get; private set; }
    public int CompanyId { get; private set; }
    public int ProviderId { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
