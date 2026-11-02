using TechnoSac.FullTank.Platform.Iam.Application.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace TechnoSac.FullTank.Platform.Iam.Application.Internal.CommandServices;

/// <summary>Handles user commands: sign-in, sign-up, profile update and password change.</summary>
public class UserCommandService(
    IUserRepository userRepository,
    IBuyerCompanyRepository buyerCompanyRepository,
    IProviderCompanyRepository providerCompanyRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IRegistrationTokenService registrationTokenService,
    IUnitOfWork unitOfWork,
    IStringLocalizer<ErrorMessages> localizer)
    : IUserCommandService
{
    public async Task<Result<(User user, string token)>> Handle(SignInCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByEmailAsync(command.Email, cancellationToken);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return Result<(User user, string token)>.Failure(IamError.InvalidCredentials,
                localizer[nameof(IamError.InvalidCredentials)]);

        var token = tokenService.GenerateToken(user);

        return Result<(User user, string token)>.Success((user, token));
    }

    public async Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken)
    {
        var role = string.IsNullOrWhiteSpace(command.Role) ? "BUYER" : command.Role.Trim().ToUpperInvariant();
        if (role != "BUYER" && role != "PROVIDER")
            return Result<User>.Failure(IamError.ValidationError, "Public sign-up only allows BUYER or PROVIDER roles.");

        if (!command.CompanyId.HasValue || command.CompanyId.Value <= 0)
            return Result<User>.Failure(IamError.ValidationError, "A valid CompanyId is required for registration.");

        // Validate RegistrationToken to ensure user owns or just registered the company
        if (string.IsNullOrWhiteSpace(command.RegistrationToken) ||
            !registrationTokenService.ValidateToken(command.RegistrationToken, command.CompanyId.Value, role))
        {
            return Result<User>.Failure(IamError.ValidationError,
                "Registration token is invalid, expired, or does not match company and role.");
        }

        await unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        try
        {
            if (role == "BUYER")
            {
                var company = await buyerCompanyRepository.FindByIdAsync(command.CompanyId.Value, cancellationToken);
                if (company is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<User>.Failure(IamError.BuyerCompanyNotFound, localizer[nameof(IamError.BuyerCompanyNotFound)]);
                }

                if (await userRepository.ExistsByCompanyIdAndRoleAsync(command.CompanyId.Value, "BUYER", cancellationToken))
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<User>.Failure(IamError.ValidationError, "The specified buyer company is already associated with an account.");
                }
            }
            else
            {
                var company = await providerCompanyRepository.FindByIdAsync(command.CompanyId.Value, cancellationToken);
                if (company is null)
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<User>.Failure(IamError.ProviderCompanyNotFound, localizer[nameof(IamError.ProviderCompanyNotFound)]);
                }

                if (await userRepository.ExistsByCompanyIdAndRoleAsync(command.CompanyId.Value, "PROVIDER", cancellationToken))
                {
                    await unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<User>.Failure(IamError.ValidationError, "The specified provider company is already associated with an account.");
                }
            }

            if (await userRepository.ExistsByEmailAsync(command.Email, cancellationToken))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<User>.Failure(IamError.EmailAlreadyTaken,
                    localizer[nameof(IamError.EmailAlreadyTaken), command.Email]);
            }

            var username = string.IsNullOrWhiteSpace(command.Username) ? command.Email : command.Username;
            if (await userRepository.ExistsByUsernameAsync(username, cancellationToken))
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<User>.Failure(IamError.UsernameAlreadyTaken,
                    localizer[nameof(IamError.UsernameAlreadyTaken), username]);
            }

            var hashedPassword = hashingService.HashPassword(command.Password);
            var sanitizedCommand = command with { Role = role };
            User user;
            try
            {
                user = new User(sanitizedCommand, hashedPassword);
            }
            catch (ArgumentException ex)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<User>.Failure(IamError.ValidationError, ex.Message);
            }

            await userRepository.AddAsync(user, cancellationToken);
            await unitOfWork.CompleteAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result<User>.Success(user);
        }
        catch (OperationCanceledException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<User>.Failure(IamError.OperationCancelled, localizer[nameof(IamError.OperationCancelled)]);
        }
        catch (DbUpdateException)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<User>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result<User>.Failure(IamError.InternalServerError, localizer[nameof(IamError.InternalServerError)]);
        }
    }

    public async Task<Result<User>> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result<User>.Failure(IamError.UserNotFound, localizer[nameof(IamError.UserNotFound)]);

        try
        {
            user.UpdateProfile(command.Name, command.Email);
            userRepository.Update(user);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<User>.Success(user);
        }
        catch (DbUpdateException)
        {
            return Result<User>.Failure(IamError.DatabaseError, localizer[nameof(IamError.DatabaseError)]);
        }
        catch (Exception)
        {
            return Result<User>.Failure(IamError.InternalServerError, localizer[nameof(IamError.InternalServerError)]);
        }
    }

    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(IamError.UserNotFound, localizer[nameof(IamError.UserNotFound)]);

        if (!hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            return Result.Failure(IamError.InvalidCredentials, localizer[nameof(IamError.InvalidCredentials)]);

        try
        {
            user.ChangePasswordHash(hashingService.HashPassword(command.NewPassword));
            userRepository.Update(user);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception)
        {
            return Result.Failure(IamError.PasswordChangeFailed, localizer[nameof(IamError.PasswordChangeFailed)]);
        }
    }
}
