namespace Masroof.Ecommerce.Orders;

public enum OrderStatus
{
    /// <summary>
    /// Order has been created but payment is pending
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Order has been paid and is being processed
    /// </summary>
    Paid = 1,

    /// <summary>
    /// Order has been cancelled
    /// </summary>
    Cancelled = 2
}
