namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

public record FavoriteProviderResource(
    int Id,
    int CompanyId,
    int ProviderId,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
