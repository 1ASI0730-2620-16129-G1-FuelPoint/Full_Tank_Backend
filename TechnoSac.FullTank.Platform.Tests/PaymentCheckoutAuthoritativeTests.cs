using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using TechnoSac.FullTank.Platform.Iam.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Notification.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Ordering.Interfaces.Acl;
using TechnoSac.FullTank.Platform.Payment.Application.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Application.Internal.CommandServices;
using TechnoSac.FullTank.Platform.Payment.Domain.Model;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Commands;
using TechnoSac.FullTank.Platform.Payment.Domain.Model.Events;
using TechnoSac.FullTank.Platform.Resources.Errors;
using TechnoSac.FullTank.Platform.Shared.Application.Internal.EventBus;
using TechnoSac.FullTank.Platform.Shared.Application.Model;
using TechnoSac.FullTank.Platform.Shared.Domain.Repositories;
using Xunit;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Tests;

public class PaymentCheckoutAuthoritativeTests
{
    private readonly Mock<IPaymentCommandService> _paymentCommandServiceMock = new();
    private readonly Mock<IInvoiceCommandService> _invoiceCommandServiceMock = new();
    private readonly Mock<IOrderingContextFacade> _orderingFacadeMock = new();
    private readonly Mock<IIamContextFacade> _iamFacadeMock = new();
    private readonly Mock<INotificationContextFacade> _notificationFacadeMock = new();
    private readonly Mock<IEventBus> _eventBusMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IStringLocalizer<ErrorMessages>> _localizerMock = new();

    public PaymentCheckoutAuthoritativeTests()
    {
        _localizerMock.Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
    }

    [Theory]
    [InlineData("")]
    [InlineData("SIMULATION")]
    [InlineData("VISA_GATEWAY_XYZ")]
    public async Task Handle_WhenSimulationDisabled_FailsRegardlessOfReference(string reference)
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "false" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        var paymentCmd = new CreatePaymentCommand(
            OrderId: 1,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: reference);

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 1,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20123456789",
            ProviderName: "Provider",
            BuyerRuc: "20987654321",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 100,
            Unit: "L",
            UnitPrice: 4.5m,
            Subtotal: 400m,
            Igv: 50m,
            Total: 450m,
            IssueDate: null,
            Status: "PAID");

        // Act
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Simulated checkout is disabled", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_WithValidOrder_DerivesAllAuthoritativeMetadataAndDeterministicReceipt()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(590.00m);
        _orderingFacadeMock.Setup(o => o.FetchOrderLineInfo(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("DIESEL B5", 100, "GAL", 5.90m, 590.00m));
        _orderingFacadeMock.Setup(o => o.MarkOrderPaid(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _iamFacadeMock.Setup(i => i.FetchBuyerCompanyInfo(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Authoritative Buyer SAC", "20100000001"));
        _iamFacadeMock.Setup(i => i.FetchProviderCompanyInfo(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Authoritative Provider SAC", "20200000002"));

        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(true);

        CreatePaymentCommand? capturedPayment = null;
        _paymentCommandServiceMock.Setup(p => p.Handle(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreatePaymentCommand, CancellationToken>((cmd, _) => capturedPayment = cmd)
            .ReturnsAsync((CreatePaymentCommand cmd, CancellationToken _) => Result<PaymentAggregate>.Success(new PaymentAggregate(cmd)));

        CreateInvoiceCommand? capturedInvoice = null;
        _invoiceCommandServiceMock.Setup(i => i.Handle(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateInvoiceCommand, CancellationToken>((cmd, _) => capturedInvoice = cmd)
            .ReturnsAsync((CreateInvoiceCommand cmd, CancellationToken _) => Result<Invoice>.Success(new Invoice(cmd)));

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        // Client attempts to spoof metadata and price
        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 1.00m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: "CLIENT-SPOOF");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "CLIENT-FAKE-INV",
            ProviderRuc: "99999999999",
            ProviderName: "Fake Provider",
            BuyerRuc: "11111111111",
            BuyerName: "Fake Buyer",
            FuelType: "WATER",
            Quantity: 999,
            Unit: "KG",
            UnitPrice: 0.01m,
            Subtotal: 1.00m,
            Igv: 0.18m,
            Total: 1.18m,
            IssueDate: null,
            Status: "PAID");

        // Act
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedPayment);
        Assert.NotNull(capturedInvoice);

        // Authoritative overwrites for financial figures
        Assert.Equal(590.00m, capturedPayment.Amount);
        Assert.Equal(590.00m, capturedInvoice.Total);
        Assert.Equal(500.00m, capturedInvoice.Subtotal);
        Assert.Equal(90.00m, capturedInvoice.Igv);

        // Deterministic unique academic receipt format tied to OrderId
        Assert.Equal("SIM-INV-00000010", capturedInvoice.InvoiceNumber);
        Assert.Equal("SIM-00000010", capturedPayment.Reference);

        // Authoritative party and product metadata from server facades
        Assert.Equal("20200000002", capturedInvoice.ProviderRuc);
        Assert.Equal("Authoritative Provider SAC", capturedInvoice.ProviderName);
        Assert.Equal("20100000001", capturedInvoice.BuyerRuc);
        Assert.Equal("Authoritative Buyer SAC", capturedInvoice.BuyerName);
        Assert.Equal("DIESEL B5", capturedInvoice.FuelType);
        Assert.Equal(100, capturedInvoice.Quantity);
        Assert.Equal("GAL", capturedInvoice.Unit);
        Assert.Equal(5.90m, capturedInvoice.UnitPrice);

        // Transaction and events
        _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        _orderingFacadeMock.Verify(o => o.MarkOrderPaid(10, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<PaymentCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenLateNotificationFails_RollsBackTransactionAndFails()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(100.00m);
        _orderingFacadeMock.Setup(o => o.FetchOrderLineInfo(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("DIESEL", 10, "L", 10.00m, 100.00m));
        _orderingFacadeMock.Setup(o => o.MarkOrderPaid(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        _iamFacadeMock.Setup(i => i.FetchBuyerCompanyInfo(1, It.IsAny<CancellationToken>())).ReturnsAsync(("Buyer", "20100000001"));
        _iamFacadeMock.Setup(i => i.FetchProviderCompanyInfo(2, It.IsAny<CancellationToken>())).ReturnsAsync(("Provider", "20200000002"));

        _paymentCommandServiceMock.Setup(p => p.Handle(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreatePaymentCommand cmd, CancellationToken _) => Result<PaymentAggregate>.Success(new PaymentAggregate(cmd)));
        _invoiceCommandServiceMock.Setup(i => i.Handle(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreateInvoiceCommand cmd, CancellationToken _) => Result<Invoice>.Success(new Invoice(cmd)));

        // Notification failure inside transaction
        _notificationFacadeMock.Setup(n => n.CreateNotificationAsync(
            It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: "SIM");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20200000002",
            ProviderName: "Provider",
            BuyerRuc: "20100000001",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 10,
            Unit: "L",
            UnitPrice: 10m,
            Subtotal: 84.75m,
            Igv: 15.25m,
            Total: 100m,
            IssueDate: null,
            Status: "PAID");

        // Act
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<PaymentCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenActorCompanyDoesNotMatchOrderBuyerCompany_FailsWithValidationError()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1); // Real buyer company is 1
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(100.00m);

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        // Actor belongs to company 99, attempting to pay for company 1's order
        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 99,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "Attacker",
            Reference: "SIM");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20200000002",
            ProviderName: "Provider",
            BuyerRuc: "20100000001",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 10,
            Unit: "L",
            UnitPrice: 10m,
            Subtotal: 84.75m,
            Igv: 15.25m,
            Total: 100m,
            IssueDate: null,
            Status: "PAID");

        // Act
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 99), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("does not match", result.Message, StringComparison.OrdinalIgnoreCase);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenActorCompanyIsNull_FailsWithValidationErrorAndPerformsNoMutations()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(100.00m);

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: "SIM");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20200000002",
            ProviderName: "Provider",
            BuyerRuc: "20100000001",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 10,
            Unit: "L",
            UnitPrice: 10m,
            Subtotal: 84.75m,
            Igv: 15.25m,
            Total: 100m,
            IssueDate: null,
            Status: "PAID");

        // Act - null ActorCompanyId
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: null), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PaymentError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _paymentCommandServiceMock.Verify(p => p.Handle(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceCommandServiceMock.Verify(i => i.Handle(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenOrderLineInfoIsNull_FailsWithValidationErrorAndPerformsNoMutations()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(100.00m);
        _orderingFacadeMock.Setup(o => o.FetchOrderLineInfo(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string FuelType, int Quantity, string Unit, decimal UnitPrice, decimal TotalAmount)?)null);

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: "SIM");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20200000002",
            ProviderName: "Provider",
            BuyerRuc: "20100000001",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 10,
            Unit: "L",
            UnitPrice: 10m,
            Subtotal: 84.75m,
            Igv: 15.25m,
            Total: 100m,
            IssueDate: null,
            Status: "PAID");

        // Act - Authoritative line details missing
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PaymentError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _paymentCommandServiceMock.Verify(p => p.Handle(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceCommandServiceMock.Verify(i => i.Handle(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCompanyInfoIsMissing_FailsWithValidationErrorAndPerformsNoMutations()
    {
        // Arrange
        var inMemoryConfig = new Dictionary<string, string?>
        {
            { "PaymentSettings:EnableSimulation", "true" }
        };
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(inMemoryConfig).Build();

        _orderingFacadeMock.Setup(o => o.ExistsOrder(10, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _orderingFacadeMock.Setup(o => o.FetchOrderStatus(10, It.IsAny<CancellationToken>())).ReturnsAsync("PENDING_PAYMENT");
        _orderingFacadeMock.Setup(o => o.FetchOrderBuyerCompanyId(10, It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _orderingFacadeMock.Setup(o => o.FetchOrderProviderId(10, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _orderingFacadeMock.Setup(o => o.FetchOrderTotalAmount(10, It.IsAny<CancellationToken>())).ReturnsAsync(100.00m);
        _orderingFacadeMock.Setup(o => o.FetchOrderLineInfo(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("DIESEL", 10, "L", 10.00m, 100.00m));
        _iamFacadeMock.Setup(i => i.FetchBuyerCompanyInfo(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(((string?)null, (string?)null));
        _iamFacadeMock.Setup(i => i.FetchProviderCompanyInfo(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("Provider SAC", "20200000002"));

        var service = new PaymentCheckoutService(
            paymentCommandService: _paymentCommandServiceMock.Object,
            invoiceCommandService: _invoiceCommandServiceMock.Object,
            orderingContextFacade: _orderingFacadeMock.Object,
            iamContextFacade: _iamFacadeMock.Object,
            notificationContextFacade: _notificationFacadeMock.Object,
            eventBus: _eventBusMock.Object,
            unitOfWork: _unitOfWorkMock.Object,
            configuration: configuration,
            localizer: _localizerMock.Object);

        var paymentCmd = new CreatePaymentCommand(
            OrderId: 10,
            CompanyId: 1,
            ProviderId: 2,
            Method: "CARD",
            Amount: 100m,
            Status: "COMPLETED",
            MaskedCard: "4111********1111",
            CardHolder: "John Doe",
            Reference: "SIM");

        var invoiceCmd = new CreateInvoiceCommand(
            PaymentId: null,
            OrderId: 10,
            InvoiceNumber: "INV-001",
            ProviderRuc: "20200000002",
            ProviderName: "Provider",
            BuyerRuc: "20100000001",
            BuyerName: "Buyer",
            FuelType: "DIESEL",
            Quantity: 10,
            Unit: "L",
            UnitPrice: 10m,
            Subtotal: 84.75m,
            Igv: 15.25m,
            Total: 100m,
            IssueDate: null,
            Status: "PAID");

        // Act - Authoritative buyer company info missing
        var result = await service.Handle(new CheckoutPaymentCommand(
            Payment: paymentCmd,
            Invoice: invoiceCmd,
            ActorCompanyId: 1), CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PaymentError.ValidationError, result.Error);
        _unitOfWorkMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _paymentCommandServiceMock.Verify(p => p.Handle(It.IsAny<CreatePaymentCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        _invoiceCommandServiceMock.Verify(i => i.Handle(It.IsAny<CreateInvoiceCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
