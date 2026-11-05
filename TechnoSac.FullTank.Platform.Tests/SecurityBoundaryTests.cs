using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using TechnoSac.FullTank.Platform.Equipment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Equipment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Application.CommandServices;
using TechnoSac.FullTank.Platform.Inventory.Application.QueryServices;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Queries;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Resources.Shared;
using Xunit;
using AspNetCoreProblemDetailsFactory = Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory;
using CustomProblemDetailsFactory = TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;
using InventoryItemAggregate = TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates.InventoryItem;
using UserAggregate = TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates.User;

namespace TechnoSac.FullTank.Platform.Tests;

public class SecurityBoundaryTests
{
    private readonly Mock<IStringLocalizer<ErrorMessages>> _errorLocalizerMock = new();
    private readonly Mock<IStringLocalizer<CommonMessages>> _commonLocalizerMock = new();
    private readonly Mock<AspNetCoreProblemDetailsFactory> _aspNetCoreProblemDetailsFactoryMock = new();
    private readonly CustomProblemDetailsFactory _problemDetailsFactory;

    public SecurityBoundaryTests()
    {
        _problemDetailsFactory = new CustomProblemDetailsFactory(
            errorLocalizer: _errorLocalizerMock.Object,
            commonLocalizer: _commonLocalizerMock.Object,
            aspNetCoreProblemDetailsFactory: _aspNetCoreProblemDetailsFactoryMock.Object);
    }

    [Fact]
    public async Task EquipmentController_Update_WhenEntityBelongsToCompanyBAndBodyClaimsCompanyA_Returns403AndNeverHandles()
    {
        // Arrange
        var commandServiceMock = new Mock<IEquipmentCommandService>();
        var queryServiceMock = new Mock<IEquipmentQueryService>();

        // Persisted entity belongs to Company B (Id: 20)
        var persistedEquipment = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 20,
            Name: "Heavy Generator 20",
            Type: "GENERATOR",
            RequiredFuelType: "DIESEL",
            Capacity: 1000,
            CurrentLevel: 400,
            Unit: "GAL",
            Status: "ACTIVE",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 100,
            Location: "Site B",
            LastRefillDate: null));

        queryServiceMock.Setup(q => q.Handle(It.Is<GetEquipmentByIdQuery>(query => query.Id == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedEquipment);

        // Authenticated user belongs to Company A (Id: 10)
        var userCompanyA = new UserAggregate(
            command: new SignUpCommand(
                Name: "Buyer User A",
                Email: "buyer_a@companya.com",
                Username: "buyer_a",
                Password: "Password123!",
                Role: "BUYER",
                CompanyId: 10,
                RegistrationToken: "token-a"),
            passwordHash: "securehash");

        var controller = new EquipmentController(
            commandService: commandServiceMock.Object,
            queryService: queryServiceMock.Object,
            errorLocalizer: _errorLocalizerMock.Object,
            problemDetailsFactory: _problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["User"] = userCompanyA;

        // Request body claims Actor Company A (Id: 10)
        var requestBody = new UpdateEquipmentResource(
            CompanyId: 10,
            Name: "Malicious Tamper Name",
            Type: "GENERATOR",
            RequiredFuelType: "DIESEL",
            Capacity: 1000,
            CurrentLevel: 400,
            Unit: "GAL",
            Status: "ACTIVE",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 100,
            Location: "Site A",
            LastRefillDate: null);

        // Act
        var result = await controller.Update(42, requestBody, CancellationToken.None);

        // Assert - BOLA prevention: 403 Forbidden and command handler NEVER executed
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
        commandServiceMock.Verify(c => c.Handle(It.IsAny<UpdateEquipmentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EquipmentController_GetById_WhenEntityBelongsToCompanyB_Returns403()
    {
        // Arrange
        var commandServiceMock = new Mock<IEquipmentCommandService>();
        var queryServiceMock = new Mock<IEquipmentQueryService>();

        var persistedEquipment = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 20,
            Name: "Heavy Generator 20",
            Type: "GENERATOR",
            RequiredFuelType: "DIESEL",
            Capacity: 1000,
            CurrentLevel: 400,
            Unit: "GAL",
            Status: "ACTIVE",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 100,
            Location: "Site B",
            LastRefillDate: null));

        queryServiceMock.Setup(q => q.Handle(It.Is<GetEquipmentByIdQuery>(query => query.Id == 42), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedEquipment);

        var userCompanyA = new UserAggregate(
            command: new SignUpCommand(
                Name: "Buyer User A",
                Email: "buyer_a@companya.com",
                Username: "buyer_a",
                Password: "Password123!",
                Role: "BUYER",
                CompanyId: 10,
                RegistrationToken: "token-a"),
            passwordHash: "securehash");

        var controller = new EquipmentController(
            commandService: commandServiceMock.Object,
            queryService: queryServiceMock.Object,
            errorLocalizer: _errorLocalizerMock.Object,
            problemDetailsFactory: _problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["User"] = userCompanyA;

        // Act
        var result = await controller.GetById(42, CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task InventoryItemsController_Update_WhenEntityBelongsToCompanyBAndBodyClaimsCompanyA_Returns403AndNeverHandles()
    {
        // Arrange
        var commandServiceMock = new Mock<IInventoryItemCommandService>();
        var queryServiceMock = new Mock<IInventoryItemQueryService>();

        // Persisted entity belongs to Provider/Company B (ProviderId: 20)
        var persistedItem = new InventoryItemAggregate(new CreateInventoryItemCommand(
            ProviderId: 20,
            Name: "Diesel B5",
            Type: "DIESEL",
            Description: "Ultra low sulfur diesel",
            PricePerLiter: 4.80m,
            Stock: 5000,
            Reserved: 200,
            Capacity: 10000,
            LowStockThreshold: 1000,
            Unit: "L",
            Status: "AVAILABLE"));

        queryServiceMock.Setup(q => q.Handle(It.Is<GetInventoryItemByIdQuery>(query => query.Id == 84), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedItem);

        // Authenticated user belongs to Provider A (CompanyId: 10)
        var userProviderA = new UserAggregate(
            command: new SignUpCommand(
                Name: "Provider User A",
                Email: "provider_a@companya.com",
                Username: "provider_a",
                Password: "Password123!",
                Role: "PROVIDER",
                CompanyId: 10,
                RegistrationToken: "token-provider-a"),
            passwordHash: "securehash");

        var controller = new InventoryItemsController(
            commandService: commandServiceMock.Object,
            queryService: queryServiceMock.Object,
            errorLocalizer: _errorLocalizerMock.Object,
            problemDetailsFactory: _problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["User"] = userProviderA;

        // Request body claims Actor Provider A (ProviderId: 10)
        var requestBody = new UpdateInventoryItemResource(
            ProviderId: 10,
            Name: "Tampered Inventory Name",
            Type: "DIESEL",
            Description: "Tampered description",
            PricePerLiter: 1.00m,
            Stock: 99999,
            Reserved: 0,
            Capacity: 10000,
            LowStockThreshold: 1000,
            Unit: "L",
            Status: "AVAILABLE");

        // Act
        var result = await controller.Update(84, requestBody, CancellationToken.None);

        // Assert - BOLA prevention: 403 Forbidden and command handler NEVER executed
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
        commandServiceMock.Verify(c => c.Handle(It.IsAny<UpdateInventoryItemCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InventoryItemsController_Delete_WhenEntityBelongsToCompanyB_Returns403AndNeverHandles()
    {
        // Arrange
        var commandServiceMock = new Mock<IInventoryItemCommandService>();
        var queryServiceMock = new Mock<IInventoryItemQueryService>();

        var persistedItem = new InventoryItemAggregate(new CreateInventoryItemCommand(
            ProviderId: 20,
            Name: "Diesel B5",
            Type: "DIESEL",
            Description: "Ultra low sulfur diesel",
            PricePerLiter: 4.80m,
            Stock: 5000,
            Reserved: 200,
            Capacity: 10000,
            LowStockThreshold: 1000,
            Unit: "L",
            Status: "AVAILABLE"));

        queryServiceMock.Setup(q => q.Handle(It.Is<GetInventoryItemByIdQuery>(query => query.Id == 84), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedItem);

        var userProviderA = new UserAggregate(
            command: new SignUpCommand(
                Name: "Provider User A",
                Email: "provider_a@companya.com",
                Username: "provider_a",
                Password: "Password123!",
                Role: "PROVIDER",
                CompanyId: 10,
                RegistrationToken: "token-provider-a"),
            passwordHash: "securehash");

        var controller = new InventoryItemsController(
            commandService: commandServiceMock.Object,
            queryService: queryServiceMock.Object,
            errorLocalizer: _errorLocalizerMock.Object,
            problemDetailsFactory: _problemDetailsFactory)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["User"] = userProviderA;

        // Act
        var result = await controller.Delete(84, CancellationToken.None);

        // Assert
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status403Forbidden, statusCodeResult.StatusCode);
        commandServiceMock.Verify(c => c.Handle(It.IsAny<DeleteInventoryItemCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
