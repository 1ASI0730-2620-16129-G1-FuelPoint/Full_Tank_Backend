using TechnoSac.FullTank.Platform.Notification.Application.CommandServices;
using TechnoSac.FullTank.Platform.Notification.Domain.Model;
using TechnoSac.FullTank.Platform.Notification.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Notification.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using NotificationAggregate = TechnoSac.FullTank.Platform.Notification.Domain.Model.Aggregates.Notification;

namespace TechnoSac.FullTank.Platform.Notification.Application.Internal.CommandServices;

public class NotificationCommandService(
    INotificationRepository repository,
    IIamContextFacade iamContextFacade,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : INotificationCommandService
{
    public async Task<Result<NotificationAggregate>> Handle(CreateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        if (!await IsValid(command.RecipientType, command.BuyerCompanyId, command.ProviderId, command.Type,
                command.Title, command.Message, cancellationToken))
            return ValidationFailure();

        var notification = new NotificationAggregate(command);
        try
        {
            await repository.AddAsync(notification, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<NotificationAggregate>.Success(notification);
        }
        catch (DbUpdateException)
        {
            return Result<NotificationAggregate>.Failure(NotificationError.DatabaseError,
                localizer[nameof(NotificationError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<NotificationAggregate>.Failure(NotificationError.InternalServerError,
                localizer[nameof(NotificationError.InternalServerError)]);
        }
    }

    public async Task<Result<NotificationAggregate>> Handle(UpdateNotificationCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (notification is null)
            return Result<NotificationAggregate>.Failure(NotificationError.NotFound,
                localizer[nameof(NotificationError.NotFound)]);
        if (!await IsValid(command.RecipientType, command.BuyerCompanyId, command.ProviderId, command.Type,
                command.Title, command.Message, cancellationToken))
            return ValidationFailure();

        try
        {
            notification.Update(command);
            repository.Update(notification);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<NotificationAggregate>.Success(notification);
        }
        catch (Exception)
        {
            return Result<NotificationAggregate>.Failure(NotificationError.InternalServerError,
                localizer[nameof(NotificationError.InternalServerError)]);
        }
    }

    public async Task<Result<NotificationAggregate>> Handle(MarkNotificationAsReadCommand command,
        CancellationToken cancellationToken)
    {
        var notification = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (notification is null)
            return Result<NotificationAggregate>.Failure(NotificationError.NotFound,
                localizer[nameof(NotificationError.NotFound)]);

        try
        {
            notification.MarkAsRead();
            repository.Update(notification);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<NotificationAggregate>.Success(notification);
        }
        catch (Exception)
        {
            return Result<NotificationAggregate>.Failure(NotificationError.InternalServerError,
                localizer[nameof(NotificationError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(MarkAllBuyerNotificationsAsReadCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var notifications = await repository.FindByBuyerCompanyIdAsync(command.BuyerCompanyId, cancellationToken);
            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
                repository.Update(notification);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationError.InternalServerError,
                localizer[nameof(NotificationError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(MarkAllProviderNotificationsAsReadCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var notifications = await repository.FindByProviderIdAsync(command.ProviderId, cancellationToken);
            foreach (var notification in notifications)
            {
                notification.MarkAsRead();
                repository.Update(notification);
            }

            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(NotificationError.InternalServerError,
                localizer[nameof(NotificationError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(DeleteNotificationCommand command, CancellationToken cancellationToken)
    {
        var notification = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (notification is null)
            return Result.Failure(NotificationError.NotFound, localizer[nameof(NotificationError.NotFound)]);

        repository.Remove(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }

    private async Task<bool> IsValid(string recipientType, int? buyerCompanyId, int? providerId, string type,
        string title, string message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
            return false;

        return recipientType switch
        {
            "BUYER" => buyerCompanyId is > 0
                       && providerId is null
                       && await iamContextFacade.ExistsBuyerCompany(buyerCompanyId.Value, cancellationToken),
            "PROVIDER" => providerId is > 0
                          && buyerCompanyId is null
                          && await iamContextFacade.ExistsProviderCompany(providerId.Value, cancellationToken),
            _ => false
        };
    }

    private Result<NotificationAggregate> ValidationFailure()
    {
        return Result<NotificationAggregate>.Failure(NotificationError.ValidationError,
            localizer[nameof(NotificationError.ValidationError)]);
    }
}
