using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using EquipmentAggregate = TechnoSac.FullTank.Platform.Equipment.Domain.Model.Aggregates.Equipment;
using TechnoSac.FullTank.Platform.Equipment.Application.Acl;
using TechnoSac.FullTank.Platform.Equipment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Equipment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Equipment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Application.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Fulfillment.Application.QueryServices;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Fulfillment.Domain.Repositories;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest;
using TechnoSac.FullTank.Platform.Fulfillment.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Iam.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Inventory.Application.Acl;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Inventory.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Inventory.Domain.Repositories;
using TechnoSac.FullTank.Platform.Inventory.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Ordering.Application.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Ordering.Application.QueryServices;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Ordering.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Ordering.Domain.Repositories;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Rest.Resources;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Resources.Shared;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using TechnoSac.FullTank.Platform.Shared.Domain.Services;
using TechnoSac.FullTank.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Xunit;

namespace TechnoSac.FullTank.Platform.Tests;

public class OrderWorkflowOrchestrationTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock = new();
    private readonly Mock<IRequestRepository> _requestRepositoryMock = new();
    private readonly Mock<IIamContextFacade> _iamContextFacadeMock = new();
    private readonly Mock<IFulfillmentContextFacade> _fulfillmentFacadeMock = new();
    private readonly Mock<IInventoryContextFacade> _inventoryFacadeMock = new();
    private readonly Mock<IEquipmentContextFacade> _equipmentFacadeMock = new();
    private readonly Mock<INotificationContextFacade> _notificationFacadeMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IStringLocalizer<ErrorMessages>> _errorLocalizerMock = new();
    private readonly Mock<IStringLocalizer<CommonMessages>> _commonLocalizerMock = new();
    private readonly Mock<Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory> _aspNetProblemDetailsFactoryMock = new();
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly OrderCommandService _orderCommandService;

    public OrderWorkflowOrchestrationTests()
    {
        _errorLocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        _commonLocalizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _aspNetProblemDetailsFactoryMock.Setup(f => f.CreateProblemDetails(
            It.IsAny<HttpContext>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns((HttpContext httpContext, int? statusCode, string title, string type, string detail, string instance) =>
                new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = detail,
                    Instance = instance
                });

        _problemDetailsFactory = new ProblemDetailsFactory(
            _errorLocalizerMock.Object,
            _commonLocalizerMock.Object,
            _aspNetProblemDetailsFactoryMock.Object);

        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int?>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _orderCommandService = new OrderCommandService(
            _orderRepositoryMock.Object,
            _requestRepositoryMock.Object,
            _iamContextFacadeMock.Object,
            _fulfillmentFacadeMock.Object,
            _inventoryFacadeMock.Object,
            _equipmentFacadeMock.Object,
            _notificationFacadeMock.Object,
            _eventBusMock.Object,
            _unitOfWorkMock.Object,
            _errorLocalizerMock.Object);
    }

    private static void SetupControllerContext(ControllerBase controller, string role, int companyId)
    {
        var httpContext = new DefaultHttpContext();
        var signUpCmd = new SignUpCommand(
            Name: "Test User",
            Email: "user@test.com",
            Username: "user@test.com",
            Password: "Password123!",
            Role: role,
            CompanyId: companyId,
            RegistrationToken: "token");
        httpContext.Items["User"] = new User(signUpCmd, "hashed_password");
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private static void SetEntityId<T>(T entity, int id)
    {
        if (entity is null) return;
        var prop = entity.GetType().GetProperty("Id", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(entity, id);
        }
        else
        {
            var field = entity.GetType().GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        ?? entity.GetType().BaseType?.GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(entity, id);
            }
            else
            {
                prop?.SetValue(entity, id);
            }
        }
    }

    private static Request CreateTestRequest(int id = 1, int buyerCompanyId = 1, int providerId = 2, int equipmentId = 3,
        string fuelType = "DIESEL", int quantity = 100, string unit = "L", decimal unitPrice = 4.5m, string status = "PENDING")
    {
        var request = new Request(new CreateRequestCommand(
            BuyerCompanyId: buyerCompanyId,
            ProviderId: providerId,
            EquipmentId: equipmentId,
            FuelType: fuelType,
            ProductName: "Diesel B5",
            Quantity: quantity,
            Unit: unit,
            UnitPrice: unitPrice,
            DeliveryAddress: "Address",
            DeliveryDate: "2026-09-10",
            Status: status,
            Source: "MANUAL",
            RejectionReasonCode: null,
            RejectionReasonNote: null));
        SetEntityId(request, id);
        return request;
    }

    private static Order CreateTestOrder(int id = 10, int requestId = 1, int buyerCompanyId = 1, int providerId = 2,
        int equipmentId = 3, string fuelType = "DIESEL", int quantity = 100, string unit = "L", decimal unitPrice = 4.5m,
        decimal totalAmount = 450m, string status = "ACCEPTED")
    {
        var order = new Order(new CreateOrderCommand(
            RequestId: requestId,
            BuyerCompanyId: buyerCompanyId,
            ProviderId: providerId,
            EquipmentId: equipmentId,
            FuelType: fuelType,
            Quantity: quantity,
            Unit: unit,
            UnitPrice: unitPrice,
            TotalAmount: totalAmount,
            DeliveryAddress: "Address",
            Status: status,
            PaymentStatus: "PENDING",
            DriverId: null,
            VehicleId: null,
            EstimatedDeliveryDate: "2026-09-10",
            DispatchedAt: null,
            DeliveredAt: null,
            PaidAt: null,
            ClosedAt: null,
            CancelledAt: null,
            CancelReason: null));
        SetEntityId(order, id);
        return order;
    }

    [Fact]
    public async Task ApproveRequest_WhenStockInsufficient_RollsBackAndFails()
    {
        // Arrange
        var request = CreateTestRequest();
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _inventoryFacadeMock.Setup(i => i.FetchPricePerLiterAsync(2, "DIESEL", It.IsAny<CancellationToken>())).ReturnsAsync(4.5m);
        _inventoryFacadeMock.Setup(i => i.ReserveStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveRequest_WhenAuthoritativePriceMissingOrZero_RejectsAndRollsBack()
    {
        // Arrange
        var request = CreateTestRequest();
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _inventoryFacadeMock.Setup(i => i.FetchPricePerLiterAsync(2, "DIESEL", It.IsAny<CancellationToken>())).ReturnsAsync((decimal?)null);

        // Act
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _inventoryFacadeMock.Verify(i => i.ReserveStockAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveRequest_WhenStockAvailable_ReservesStockCreatesOrderAndCommits()
    {
        // Arrange
        var request = CreateTestRequest();
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _inventoryFacadeMock.Setup(i => i.FetchPricePerLiterAsync(2, "DIESEL", It.IsAny<CancellationToken>())).ReturnsAsync(4.5m);
        _inventoryFacadeMock.Setup(i => i.ReserveStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderRepositoryMock.Setup(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => SetEntityId(o, 10))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10, result.Value.Id);
        Assert.Equal("ACCEPTED", result.Value.Status);
        Assert.Equal(450m, result.Value.TotalAmount);
        Assert.Equal("APPROVED", request.Status);

        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _orderRepositoryMock.Verify(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationFacadeMock.Verify(n => n.CreateNotificationAsync(
            "BUYER",
            1,
            2,
            "ORDER_ACCEPTED",
            "Fuel Request Approved",
            It.IsAny<string>(),
            10,
            "/ordering/my-orders/10",
            It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<RequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequest_GallonConversionCeilingRule_ReservesStockAndCalculatesAuthoritativePrice()
    {
        // Arrange: 100 GALLONS requested at $3.50/liter authoritative inventory price
        var request = CreateTestRequest(quantity: 100, unit: "GALLONS", unitPrice: 0m);
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _inventoryFacadeMock.Setup(i => i.FetchPricePerLiterAsync(2, "DIESEL", It.IsAny<CancellationToken>())).ReturnsAsync(3.50m);
        _inventoryFacadeMock.Setup(i => i.ReserveStockAsync(2, "DIESEL", 100, "GALLONS", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderRepositoryMock.Setup(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => SetEntityId(o, 10))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // Unit price: 3.50 * 3.78541 = 13.248935 -> rounded to 13.25
        Assert.Equal(13.25m, result.Value!.UnitPrice);
        // Total amount: 13.25 * 100 = 1325.00
        Assert.Equal(1325.00m, result.Value.TotalAmount);
        Assert.Equal("GALLONS", result.Value.Unit);

        _inventoryFacadeMock.Verify(i => i.ReserveStockAsync(2, "DIESEL", 100, "GALLONS", It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveRequest_WhenNotificationReturnsFalse_RollsBackAndNeverCommits()
    {
        // Arrange
        var request = CreateTestRequest();
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);
        _inventoryFacadeMock.Setup(i => i.FetchPricePerLiterAsync(2, "DIESEL", It.IsAny<CancellationToken>())).ReturnsAsync(4.5m);
        _inventoryFacadeMock.Setup(i => i.ReserveStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderRepositoryMock.Setup(o => o.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => SetEntityId(o, 10))
            .Returns(Task.CompletedTask);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<RequestApprovedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveRequest_WrongProvider_FailsWithoutReservingStock()
    {
        // Arrange
        var request = CreateTestRequest();
        _requestRepositoryMock.Setup(r => r.FindByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(request);

        // Act (Actor company is 99, but request.ProviderId is 2)
        var result = await _orderCommandService.Handle(new ApproveFuelRequestCommand(RequestId: 1, ActorRole: "PROVIDER", ActorCompanyId: 99), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _inventoryFacadeMock.Verify(i => i.ReserveStockAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchOrder_ValidProviderAndAvailableFleet_DispatchesAndCommits()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _inventoryFacadeMock.Setup(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("DISPATCHED", result.Value!.Status);
        Assert.Equal(5, result.Value.DriverId);
        Assert.Equal(8, result.Value.VehicleId);
        _fulfillmentFacadeMock.Verify(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>()), Times.Once);
        _inventoryFacadeMock.Verify(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>()), Times.Once);
        _notificationFacadeMock.Verify(n => n.CreateNotificationAsync(
            "BUYER",
            1,
            2,
            "ORDER_DISPATCHED",
            "Order Dispatched",
            It.IsAny<string>(),
            10,
            "/ordering/my-orders/10",
            It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderDispatchedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DispatchOrder_WhenFleetUnavailable_RollsBack()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ACCEPTED", order.Status);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchOrder_WhenStockFulfillmentFails_RollsBack()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _inventoryFacadeMock.Setup(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        Assert.Equal("ACCEPTED", order.Status);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchOrder_WhenNotificationFails_RollsBackAndDoesNotCommit()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _inventoryFacadeMock.Setup(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Notification gateway failed"));

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchOrder_WhenNotificationReturnsFalse_RollsBackAndNeverCommits()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _inventoryFacadeMock.Setup(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderDispatchedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchOrder_WhenEventPublishFails_RollsBackAndDoesNotCommit()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CreateDeliveryAsync(10, 2, 5, 8, "Address", 100, "L", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _inventoryFacadeMock.Setup(i => i.FulfillStockAsync(2, "DIESEL", 100, "L", 10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventBusMock.Setup(e => e.PublishAsync(It.IsAny<OrderDispatchedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Event bus connection failed"));

        // Act
        var result = await _orderCommandService.Handle(new DispatchOrderCommand(OrderId: 10, DriverId: 5, VehicleId: 8, ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReceiveOrder_ValidBuyer_CompletesDeliveryAndRefillsEquipment()
    {
        // Arrange
        var order = CreateTestOrder(status: "DISPATCHED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CompleteDeliveryAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _equipmentFacadeMock.Setup(e => e.RefillEquipmentAsync(3, 1, 100, "L", "DIESEL", 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _orderCommandService.Handle(new ReceiveOrderCommand(OrderId: 10, ActorRole: "BUYER", ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("PENDING_PAYMENT", result.Value!.Status);
        _fulfillmentFacadeMock.Verify(f => f.CompleteDeliveryAsync(10, It.IsAny<CancellationToken>()), Times.Once);
        _equipmentFacadeMock.Verify(e => e.RefillEquipmentAsync(3, 1, 100, "L", "DIESEL", 2, 1, It.IsAny<CancellationToken>()), Times.Once);
        _notificationFacadeMock.Verify(n => n.CreateNotificationAsync(
            "PROVIDER",
            1,
            2,
            "ORDER_RECEIVED",
            "Order Received",
            It.IsAny<string>(),
            10,
            "/ordering/orders/10",
            It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReceiveOrder_WhenEquipmentRefillFails_RollsBack()
    {
        // Arrange
        var order = CreateTestOrder(status: "DISPATCHED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CompleteDeliveryAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _equipmentFacadeMock.Setup(e => e.RefillEquipmentAsync(3, 1, 100, "L", "DIESEL", 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new ReceiveOrderCommand(OrderId: 10, ActorRole: "BUYER", ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        Assert.Equal("DISPATCHED", order.Status);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReceiveOrder_WhenNotificationReturnsFalse_RollsBackAndNeverCommits()
    {
        // Arrange
        var order = CreateTestOrder(status: "DISPATCHED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CompleteDeliveryAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _equipmentFacadeMock.Setup(e => e.RefillEquipmentAsync(3, 1, 100, "L", "DIESEL", 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new ReceiveOrderCommand(OrderId: 10, ActorRole: "BUYER", ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReceiveOrder_WhenEventPublishFails_RollsBackAndDoesNotCommit()
    {
        // Arrange
        var order = CreateTestOrder(status: "DISPATCHED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _fulfillmentFacadeMock.Setup(f => f.CompleteDeliveryAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _equipmentFacadeMock.Setup(e => e.RefillEquipmentAsync(3, 1, 100, "L", "DIESEL", 2, 1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _eventBusMock.Setup(e => e.PublishAsync(It.IsAny<OrderReceivedEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Broker connection failed"));

        // Act
        var result = await _orderCommandService.Handle(new ReceiveOrderCommand(OrderId: 10, ActorRole: "BUYER", ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_ByProviderWhenAccepted_ReleasesStockAndCancels()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryFacadeMock.Setup(i => i.ReleaseStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _orderCommandService.Handle(new CancelOrderCommand(OrderId: 10, Reason: "Out of stock", ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("CANCELLED", result.Value!.Status);
        _inventoryFacadeMock.Verify(i => i.ReleaseStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>()), Times.Once);
        _notificationFacadeMock.Verify(n => n.CreateNotificationAsync(
            "BUYER",
            1,
            2,
            "ORDER_CANCELLED",
            "Order Cancelled",
            It.IsAny<string>(),
            10,
            "/ordering/my-orders/10",
            It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_ByBuyer_IsRejected()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Act (Buyer attempts cancellation)
        var result = await _orderCommandService.Handle(new CancelOrderCommand(OrderId: 10, Reason: "Buyer cancelled", ActorRole: "BUYER", ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("ACCEPTED", order.Status);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelOrder_WhenNotificationFails_RollsBackAndDoesNotCommit()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryFacadeMock.Setup(i => i.ReleaseStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Notification transport down"));

        // Act
        var result = await _orderCommandService.Handle(new CancelOrderCommand(OrderId: 10, Reason: "Out of stock", ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelOrder_WhenNotificationReturnsFalse_RollsBackAndNeverCommits()
    {
        // Arrange
        var order = CreateTestOrder(status: "ACCEPTED");
        _orderRepositoryMock.Setup(o => o.FindByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _inventoryFacadeMock.Setup(i => i.ReleaseStockAsync(2, "DIESEL", 100, "L", It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _orderCommandService.Handle(new CancelOrderCommand(OrderId: 10, Reason: "Out of stock", ActorRole: "PROVIDER", ActorCompanyId: 2), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderingError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<OrderCancelledEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void OrdersController_GenericUpdate_IsRejectedWithBadRequest()
    {
        // Arrange
        var orderCommandMock = new Mock<IOrderCommandService>();
        var orderQueryMock = new Mock<IOrderQueryService>();
        var controller = new OrdersController(
            orderCommandMock.Object,
            orderQueryMock.Object,
            _errorLocalizerMock.Object,
            _problemDetailsFactory);
        SetupControllerContext(controller, "PROVIDER", 2);

        var updateResource = new UpdateOrderResource(
            RequestId: 1,
            BuyerCompanyId: 1,
            ProviderId: 2,
            EquipmentId: 3,
            FuelType: "DIESEL",
            Quantity: 100,
            Unit: "L",
            UnitPrice: 4.5m,
            TotalAmount: 450m,
            DeliveryAddress: "Address",
            Status: "DISPATCHED",
            PaymentStatus: "PENDING",
            DriverId: 1,
            VehicleId: 2,
            EstimatedDeliveryDate: null,
            DispatchedAt: null,
            DeliveredAt: null,
            PaidAt: null,
            ClosedAt: null,
            CancelledAt: null,
            CancelReason: null);

        // Act
        var result = controller.Update(1, updateResource, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task RequestsController_Approve_DelegatesToOrderCommandService()
    {
        // Arrange
        var requestCommandMock = new Mock<IRequestCommandService>();
        var requestQueryMock = new Mock<IRequestQueryService>();
        var orderCommandMock = new Mock<IOrderCommandService>();

        var createdOrder = CreateTestOrder(status: "ACCEPTED");
        orderCommandMock.Setup(o => o.Handle(It.IsAny<ApproveFuelRequestCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Order>.Success(createdOrder));

        var controller = new RequestsController(
            requestCommandMock.Object,
            requestQueryMock.Object,
            orderCommandMock.Object,
            _errorLocalizerMock.Object,
            _problemDetailsFactory);
        SetupControllerContext(controller, "PROVIDER", 2);

        // Act
        var result = await controller.Approve(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var res = Assert.IsType<OrderResource>(okResult.Value);
        Assert.Equal("ACCEPTED", res.Status);
        orderCommandMock.Verify(o => o.Handle(It.Is<ApproveFuelRequestCommand>(c => c.RequestId == 1 && c.ActorRole == "PROVIDER" && c.ActorCompanyId == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeliveriesController_DirectMutations_RejectedWithBadRequest()
    {
        // Arrange
        var deliveryCommandMock = new Mock<IDeliveryCommandService>();
        var deliveryQueryMock = new Mock<IDeliveryQueryService>();
        var orderingFacadeMock = new Mock<IOrderingContextFacade>();
        var controller = new DeliveriesController(
            deliveryCommandMock.Object,
            deliveryQueryMock.Object,
            orderingFacadeMock.Object,
            _errorLocalizerMock.Object,
            _problemDetailsFactory);
        SetupControllerContext(controller, "PROVIDER", 2);

        var createResource = new CreateDeliveryResource(
            OrderId: 1,
            ProviderId: 2,
            DriverId: 3,
            VehicleId: 4,
            Status: "on_the_way",
            OriginLocation: "Depot",
            DestinationLocation: "Site",
            DispatchedAt: null,
            DeliveredAt: null,
            Notes: null);

        // Act & Assert Create
        var createResult = await controller.Create(createResource, CancellationToken.None);
        var createBadReq = Assert.IsType<ObjectResult>(createResult);
        Assert.Equal(StatusCodes.Status400BadRequest, createBadReq.StatusCode);

        // Act & Assert Complete
        var completeResult = await controller.Complete(10, CancellationToken.None);
        var completeBadReq = Assert.IsType<ObjectResult>(completeResult);
        Assert.Equal(StatusCodes.Status400BadRequest, completeBadReq.StatusCode);
    }

    [Fact]
    public async Task InventoryFacade_ReserveStock_InactiveTank_RejectsReservation()
    {
        // Arrange
        var itemRepo = new Mock<IInventoryItemRepository>();
        var movRepo = new Mock<IInventoryMovementRepository>();
        var uow = new Mock<IUnitOfWork>();

        var inactiveItem = new InventoryItem(new CreateInventoryItemCommand(
            ProviderId: 2,
            Name: "Diesel Tank",
            Type: "DIESEL",
            Description: "Main",
            PricePerLiter: 4.5m,
            Stock: 1000,
            Reserved: 0,
            Capacity: 5000,
            LowStockThreshold: 500,
            Unit: "LITERS",
            Status: "MAINTENANCE"));
        itemRepo.Setup(r => r.FindByProviderIdAndTypeAsync(2, "DIESEL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveItem);

        var facade = new InventoryContextFacade(itemRepo.Object, movRepo.Object, uow.Object);

        // Act
        var result = await facade.ReserveStockAsync(2, "DIESEL", 100, "GALLONS");

        // Assert
        Assert.False(result);
        uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InventoryFacade_ReserveStock_GallonToLiterCeilingConversion_ReservesCorrectAmount()
    {
        // Arrange
        var itemRepo = new Mock<IInventoryItemRepository>();
        var movRepo = new Mock<IInventoryMovementRepository>();
        var uow = new Mock<IUnitOfWork>();

        var activeItem = new InventoryItem(new CreateInventoryItemCommand(
            ProviderId: 2,
            Name: "Diesel Tank",
            Type: "DIESEL",
            Description: "Main",
            PricePerLiter: 4.5m,
            Stock: 1000,
            Reserved: 0,
            Capacity: 5000,
            LowStockThreshold: 500,
            Unit: "LITERS",
            Status: "ACTIVE"));
        itemRepo.Setup(r => r.FindByProviderIdAndTypeAsync(2, "DIESEL", It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeItem);

        var facade = new InventoryContextFacade(itemRepo.Object, movRepo.Object, uow.Object);

        // Act: 100 gallons into Liters -> 100 * 3.78541 = 378.541 -> ceiling = 379 liters reserved
        var result = await facade.ReserveStockAsync(2, "DIESEL", 100, "GALLONS");

        // Assert
        Assert.True(result);
        Assert.Equal(379, activeItem.Reserved);
        uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EquipmentFacade_RefillEquipment_FrontendStatuses_AllowsAndDerives()
    {
        // Arrange: Frontend low_fuel status is compatible and derives to operational after refill
        var equipRepo = new Mock<IEquipmentRepository>();
        var refillRepo = new Mock<IRefillHistoryRepository>();
        var uow = new Mock<IUnitOfWork>();

        var equipment = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 1,
            Name: "Truck 1",
            Type: "TRUCK",
            RequiredFuelType: "DIESEL",
            Capacity: 500,
            CurrentLevel: 30,
            Unit: "L",
            Status: "low_fuel",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 50,
            Location: "Site",
            LastRefillDate: null));
        equipRepo.Setup(r => r.FindByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        var facade = new EquipmentContextFacade(equipRepo.Object, refillRepo.Object, uow.Object);

        // Act: Refill 100 liters
        var result = await facade.RefillEquipmentAsync(10, 1, 100, "L", "DIESEL", 2, 1);

        // Assert
        Assert.True(result);
        Assert.Equal(130, equipment.CurrentLevel);
        Assert.Equal("operational", equipment.Status);
        uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EquipmentFacade_RefillEquipment_IncompatibleFuelOrGenuinelyInactive_RejectsRefill()
    {
        // Arrange
        var equipRepo = new Mock<IEquipmentRepository>();
        var refillRepo = new Mock<IRefillHistoryRepository>();
        var uow = new Mock<IUnitOfWork>();

        // Genuinely inactive equipment
        var inactiveEquip = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 1,
            Name: "Truck 1",
            Type: "TRUCK",
            RequiredFuelType: "DIESEL",
            Capacity: 500,
            CurrentLevel: 100,
            Unit: "L",
            Status: "INACTIVE",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 50,
            Location: "Site",
            LastRefillDate: null));
        equipRepo.Setup(r => r.FindByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveEquip);

        var facade = new EquipmentContextFacade(equipRepo.Object, refillRepo.Object, uow.Object);
        var result = await facade.RefillEquipmentAsync(10, 1, 50, "L", "DIESEL", 2, 1);
        Assert.False(result);

        // Fuel type mismatch on operational equipment
        var activeEquip = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 1,
            Name: "Truck 2",
            Type: "TRUCK",
            RequiredFuelType: "DIESEL",
            Capacity: 500,
            CurrentLevel: 100,
            Unit: "L",
            Status: "operational",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 50,
            Location: "Site",
            LastRefillDate: null));
        equipRepo.Setup(r => r.FindByIdAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEquip);

        var mismatchResult = await facade.RefillEquipmentAsync(20, 1, 50, "L", "GASOLINE", 2, 1);
        Assert.False(mismatchResult);

        // Already at capacity
        var fullEquip = new EquipmentAggregate(new CreateEquipmentCommand(
            CompanyId: 1,
            Name: "Truck 3",
            Type: "TRUCK",
            RequiredFuelType: "DIESEL",
            Capacity: 500,
            CurrentLevel: 500,
            Unit: "L",
            Status: "operational",
            FavoriteProviderId: null,
            AutoRefill: false,
            RefillThreshold: 50,
            Location: "Site",
            LastRefillDate: null));
        equipRepo.Setup(r => r.FindByIdAsync(30, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fullEquip);

        var fullResult = await facade.RefillEquipmentAsync(30, 1, 50, "L", "DIESEL", 2, 1);
        Assert.False(fullResult);
    }

    [Fact]
    public async Task FulfillmentFacade_CreateDelivery_RequiresAvailableAndSetsAssigned()
    {
        // Arrange
        var deliveryRepo = new Mock<IDeliveryRepository>();
        var driverRepo = new Mock<IDriverRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();
        var iamFacade = new Mock<IIamContextFacade>();

        iamFacade.Setup(i => i.FetchProviderCompanyInfo(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("PetroPeru Callao", "20100012345"));

        var driver = new Driver(new CreateDriverCommand(
            Name: "Carlos Vega",
            LicenseNumber: "DL12345",
            Phone: "555-1234",
            Email: "carlos@test.com",
            Status: "AVAILABLE",
            ProviderId: 2));
        SetEntityId(driver, 5);

        var vehicle = new Vehicle(new CreateVehicleCommand(
            Plate: "ABC-123",
            Brand: "Volvo",
            Model: "FH",
            Capacity: 1000,
            Unit: "LITERS",
            Status: "AVAILABLE",
            ProviderId: 2));
        SetEntityId(vehicle, 8);

        driverRepo.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(driver);
        vehicleRepo.Setup(r => r.FindByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);

        var facade = new FulfillmentContextFacade(deliveryRepo.Object, driverRepo.Object, vehicleRepo.Object, uow.Object, iamFacade.Object);

        // Act
        var result = await facade.CreateDeliveryAsync(10, 2, 5, 8, "Buyer Site", 500, "LITERS");

        // Assert
        Assert.True(result);
        Assert.Equal("ASSIGNED", driver.Status);
        Assert.Equal("ASSIGNED", vehicle.Status);
        deliveryRepo.Verify(d => d.AddAsync(It.Is<Delivery>(del => del.OriginLocation == "PetroPeru Callao"), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FulfillmentFacade_CreateDelivery_RejectsExceedingVehicleCapacityOrBusyFleet()
    {
        // Arrange
        var deliveryRepo = new Mock<IDeliveryRepository>();
        var driverRepo = new Mock<IDriverRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();

        var driver = new Driver(new CreateDriverCommand(
            Name: "Carlos Vega",
            LicenseNumber: "DL12345",
            Phone: "555-1234",
            Email: "carlos@test.com",
            Status: "ASSIGNED", // Already assigned
            ProviderId: 2));
        SetEntityId(driver, 5);

        var vehicle = new Vehicle(new CreateVehicleCommand(
            Plate: "ABC-123",
            Brand: "Volvo",
            Model: "FH",
            Capacity: 100, // Small capacity
            Unit: "LITERS",
            Status: "AVAILABLE",
            ProviderId: 2));
        SetEntityId(vehicle, 8);

        driverRepo.Setup(r => r.FindByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(driver);
        vehicleRepo.Setup(r => r.FindByIdAsync(8, It.IsAny<CancellationToken>())).ReturnsAsync(vehicle);

        var facade = new FulfillmentContextFacade(deliveryRepo.Object, driverRepo.Object, vehicleRepo.Object, uow.Object);

        // Act 1: Driver is ASSIGNED (not AVAILABLE)
        var busyDriverResult = await facade.CreateDeliveryAsync(10, 2, 5, 8, "Buyer Site", 50, "LITERS");
        Assert.False(busyDriverResult);

        // Set driver to AVAILABLE, but order 200 GALLONS into 100 LITERS capacity vehicle (200 gal = 757 liters > 100 liters)
        driver.SetStatus("AVAILABLE");
        var overCapacityResult = await facade.CreateDeliveryAsync(10, 2, 5, 8, "Buyer Site", 200, "GALLONS");
        Assert.False(overCapacityResult);
    }

    [Fact]
    public async Task FulfillmentFacade_CompleteDelivery_MissingDelivery_RejectsPhantomReceipt()
    {
        // Arrange: Missing delivery
        var deliveryRepo = new Mock<IDeliveryRepository>();
        var driverRepo = new Mock<IDriverRepository>();
        var vehicleRepo = new Mock<IVehicleRepository>();
        var uow = new Mock<IUnitOfWork>();

        deliveryRepo.Setup(r => r.FindActiveByOrderIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Delivery?)null);

        var facade = new FulfillmentContextFacade(deliveryRepo.Object, driverRepo.Object, vehicleRepo.Object, uow.Object);

        // Act
        var result = await facade.CompleteDeliveryAsync(10);

        // Assert: Must return false to prevent phantom receipt
        Assert.False(result);
        uow.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
