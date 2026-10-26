using TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates;
using PaymentAggregate = TechnoSac.FullTank.Platform.Payment.Domain.Model.Aggregates.Payment;

namespace TechnoSac.FullTank.Platform.Payment.Domain.Model;

public record PaymentCheckout(PaymentAggregate Payment, Invoice Invoice);
