namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;

public record CreateProviderRatingCommand(int CompanyId, int ProviderId, int Rating);
