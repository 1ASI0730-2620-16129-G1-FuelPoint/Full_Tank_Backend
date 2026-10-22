using TechnoSac.FullTank.Platform.Catalog.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Shared.Domain.Model.Entities;

namespace TechnoSac.FullTank.Platform.Catalog.Domain.Model.Aggregates;

/// <summary>Aggregate root representing one buyer company's rating of a provider.</summary>
public class ProviderRating : IAuditableEntity
{
    protected ProviderRating()
    {
    }

    public ProviderRating(CreateProviderRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        CompanyId = command.CompanyId;
        ProviderId = command.ProviderId;
        Rating = ValidateRating(command.Rating);
    }

    public void Update(UpdateProviderRatingCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        Rating = ValidateRating(command.Rating);
    }

    private static int ValidateRating(int rating)
    {
        return rating is >= 1 and <= 5
            ? rating
            : throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
    }

    public int Id { get; private set; }
    public int CompanyId { get; private set; }
    public int ProviderId { get; private set; }
    public int Rating { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
