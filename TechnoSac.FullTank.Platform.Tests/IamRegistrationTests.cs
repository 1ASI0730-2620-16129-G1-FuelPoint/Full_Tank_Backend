using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Moq;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Iam.Application.Internal.OutboundServices;
using TechnoSac.FullTank.Platform.Iam.Domain.Model;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Domain.Repositories;
using TechnoSac.FullTank.Platform.Iam.Infrastructure.Tokens.Registration;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Xunit;

namespace TechnoSac.FullTank.Platform.Tests;

public class IamRegistrationTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IBuyerCompanyRepository> _buyerCompanyRepositoryMock = new();
    private readonly Mock<IProviderCompanyRepository> _providerCompanyRepositoryMock = new();
    private readonly Mock<IHashingService> _hashingServiceMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly Mock<IRegistrationTokenService> _registrationTokenServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock = new();
    private readonly UserCommandService _service;

    public IamRegistrationTests()
    {
        _localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        _hashingServiceMock.Setup(h => h.HashPassword(It.IsAny<string>()))
            .Returns("hashed_secret");

        _service = new UserCommandService(
            _userRepositoryMock.Object,
            _buyerCompanyRepositoryMock.Object,
            _providerCompanyRepositoryMock.Object,
            _tokenServiceMock.Object,
            _hashingServiceMock.Object,
            _registrationTokenServiceMock.Object,
            _unitOfWorkMock.Object,
            _localizerMock.Object);
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("SUPERADMIN")]
    [InlineData("ROOT")]
    [InlineData("GUEST")]
    public async Task SignUp_WithUnauthorizedRole_FailsWithValidationError(string role)
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Attacker",
            Email: "attacker@test.com",
            Username: "attacker",
            Password: "Password123!",
            Role: role,
            CompanyId: 1,
            RegistrationToken: "token");

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(IamError.ValidationError, result.Error);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SignUp_WithNonPositiveCompanyId_FailsWithValidationError()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Buyer User",
            Email: "buyer@test.com",
            Username: "buyer",
            Password: "Password123!",
            Role: "BUYER",
            CompanyId: 0,
            RegistrationToken: "token");

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(IamError.ValidationError, result.Error);
    }

    [Fact]
    public async Task SignUp_WithMissingOrInvalidToken_FailsWithValidationError()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Buyer User",
            Email: "buyer@test.com",
            Username: "buyer",
            Password: "Password123!",
            Role: "BUYER",
            CompanyId: 10,
            RegistrationToken: "invalid-token");
        _registrationTokenServiceMock.Setup(r => r.ValidateToken("invalid-token", 10, "BUYER")).Returns(false);

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(IamError.ValidationError, result.Error);
    }

    [Fact]
    public async Task SignUp_WhenCompanyDoesNotExist_FailsWithBuyerCompanyNotFound()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Buyer User",
            Email: "buyer@test.com",
            Username: "buyer",
            Password: "Password123!",
            Role: "BUYER",
            CompanyId: 999,
            RegistrationToken: "valid-token");
        _registrationTokenServiceMock.Setup(r => r.ValidateToken("valid-token", 999, "BUYER")).Returns(true);
        _buyerCompanyRepositoryMock.Setup(r => r.FindByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BuyerCompany?)null);

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(IamError.BuyerCompanyNotFound, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignUp_WhenCompanyAlreadyHasUser_FailsWithValidationError()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Buyer User 2",
            Email: "buyer2@test.com",
            Username: "buyer2",
            Password: "Password123!",
            Role: "BUYER",
            CompanyId: 5,
            RegistrationToken: "valid-token");
        _registrationTokenServiceMock.Setup(r => r.ValidateToken("valid-token", 5, "BUYER")).Returns(true);
        var dummyBuyer = new BuyerCompany(new CreateBuyerCompanyCommand(
            Name: "Buyer",
            Ruc: "20100000001",
            Sector: "Transport",
            Address: "Address",
            ContactEmail: "b@b.com",
            Phone: "999999999"));
        _buyerCompanyRepositoryMock.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyBuyer);
        _userRepositoryMock.Setup(r => r.ExistsByCompanyIdAndRoleAsync(5, "BUYER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(IamError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignUp_WithValidBuyerAndUnassociatedCompany_SucceedsUnderSerializableTransaction()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Buyer New",
            Email: "buyer_new@test.com",
            Username: "buyer_new",
            Password: "Password123!",
            Role: "BUYER",
            CompanyId: 10,
            RegistrationToken: "valid-token");
        _registrationTokenServiceMock.Setup(r => r.ValidateToken("valid-token", 10, "BUYER")).Returns(true);
        var dummyBuyer = new BuyerCompany(new CreateBuyerCompanyCommand(
            Name: "Buyer",
            Ruc: "20100000002",
            Sector: "Mining",
            Address: "Address",
            ContactEmail: "b2@b.com",
            Phone: "999999998"));
        _buyerCompanyRepositoryMock.Setup(r => r.FindByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyBuyer);
        _userRepositoryMock.Setup(r => r.ExistsByCompanyIdAndRoleAsync(10, "BUYER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync("buyer_new", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("BUYER", result.Value.Role);
        Assert.Equal(10, result.Value.CompanyId);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SignUp_WithValidProviderAndUnassociatedCompany_Succeeds()
    {
        // Arrange
        var command = new SignUpCommand(
            Name: "Provider New",
            Email: "provider_new@test.com",
            Username: "provider_new",
            Password: "Password123!",
            Role: "PROVIDER",
            CompanyId: 20,
            RegistrationToken: "valid-token");
        _registrationTokenServiceMock.Setup(r => r.ValidateToken("valid-token", 20, "PROVIDER")).Returns(true);
        var dummyProvider = new ProviderCompany(new CreateProviderCompanyCommand(
            Name: "Provider",
            Ruc: "20999999999",
            Address: "Address",
            Phone: "999999997",
            Rating: 5.0m,
            FuelTypesOffered: new[] { "DIESEL" },
            Description: "Description"));
        _providerCompanyRepositoryMock.Setup(r => r.FindByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyProvider);
        _userRepositoryMock.Setup(r => r.ExistsByCompanyIdAndRoleAsync(20, "PROVIDER", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.ExistsByUsernameAsync("provider_new", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("PROVIDER", result.Value.Role);
        Assert.Equal(20, result.Value.CompanyId);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Real Cryptographic DataProtection Tests ---

    [Fact]
    public void RealRegistrationTokenService_GenerateAndValidate_Succeeds()
    {
        var provider = new EphemeralDataProtectionProvider();
        var tokenService = new RegistrationTokenService(provider);

        var token = tokenService.GenerateToken(42, "BUYER");

        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(tokenService.ValidateToken(token, 42, "BUYER"));
    }

    [Fact]
    public void RealRegistrationTokenService_RoleMismatch_ReturnsFalse()
    {
        var provider = new EphemeralDataProtectionProvider();
        var tokenService = new RegistrationTokenService(provider);

        var token = tokenService.GenerateToken(42, "BUYER");

        Assert.False(tokenService.ValidateToken(token, 42, "PROVIDER"));
    }

    [Fact]
    public void RealRegistrationTokenService_CompanyMismatch_ReturnsFalse()
    {
        var provider = new EphemeralDataProtectionProvider();
        var tokenService = new RegistrationTokenService(provider);

        var token = tokenService.GenerateToken(42, "BUYER");

        Assert.False(tokenService.ValidateToken(token, 999, "BUYER"));
    }

    [Fact]
    public void RealRegistrationTokenService_WrongPurpose_ReturnsFalse()
    {
        var provider = new EphemeralDataProtectionProvider();
        var alienProtector = provider.CreateProtector("Alien.Purpose.v1");
        var payloadJson = JsonSerializer.Serialize(new
        {
            CompanyId = 42,
            Role = "BUYER",
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()
        });
        var alienToken = alienProtector.Protect(payloadJson);

        var tokenService = new RegistrationTokenService(provider);
        Assert.False(tokenService.ValidateToken(alienToken, 42, "BUYER"));
    }

    [Fact]
    public void RealRegistrationTokenService_TamperedPayload_ReturnsFalse()
    {
        var provider = new EphemeralDataProtectionProvider();
        var tokenService = new RegistrationTokenService(provider);

        var token = tokenService.GenerateToken(42, "BUYER");
        var tamperedToken = token.Substring(0, token.Length - 4) + "XXXX";

        Assert.False(tokenService.ValidateToken(tamperedToken, 42, "BUYER"));
        Assert.False(tokenService.ValidateToken("not-a-valid-token", 42, "BUYER"));
        Assert.False(tokenService.ValidateToken(null, 42, "BUYER"));
        Assert.False(tokenService.ValidateToken("", 42, "BUYER"));
        Assert.False(tokenService.ValidateToken("   ", 42, "BUYER"));
    }

    [Fact]
    public void RealRegistrationTokenService_ExpiredPayload_ReturnsFalse()
    {
        var provider = new EphemeralDataProtectionProvider();
        var protector = provider.CreateProtector("FullTank.CompanyRegistrationToken.v1");
        var tokenService = new RegistrationTokenService(provider);

        var expiredPayload = JsonSerializer.Serialize(new
        {
            CompanyId = 42,
            Role = "BUYER",
            ExpiresAtUnix = DateTimeOffset.UtcNow.AddSeconds(-10).ToUnixTimeSeconds()
        });
        var expiredToken = protector.Protect(expiredPayload);

        Assert.False(tokenService.ValidateToken(expiredToken, 42, "BUYER"));
    }
}
