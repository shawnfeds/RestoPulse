namespace RestoPulse.OrderService.Domain.Enums;

public enum OrderStatus
{
    New,
    Preparing,
    Served,
    Billed,
    Void
}