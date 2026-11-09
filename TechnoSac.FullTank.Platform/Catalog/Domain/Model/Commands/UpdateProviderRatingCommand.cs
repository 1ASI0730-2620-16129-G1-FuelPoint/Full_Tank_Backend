namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;

public record UpdateProviderRatingCommand(int Id, int CompanyId, int ProviderId, int Rating);
