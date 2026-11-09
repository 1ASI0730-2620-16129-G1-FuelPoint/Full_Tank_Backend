namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Queries;

public record GetAllProviderRatingsQuery(int? CompanyId = null, int? ProviderId = null);
