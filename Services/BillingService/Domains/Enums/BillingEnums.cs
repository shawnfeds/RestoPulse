namespace RestoPulse.BillingService.Domain.Enums;

public enum BillStatus
{
    Pending,
    Settled,
    Voided
}

public enum PaymentMethod
{
    Cash,
    Card,
    UPI
}