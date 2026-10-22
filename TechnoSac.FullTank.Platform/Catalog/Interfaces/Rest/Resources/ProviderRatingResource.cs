namespace TechnoSac.FullTank.Platform.Catalog.Interfaces.Rest.Resources;

public record ProviderRatingResource(
    int Id,
    int CompanyId,
    int ProviderId,
    int Rating,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt);
