using System;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;

namespace Masroof.Ecommerce.Emails;

public interface IEmailService
{
    /// <summary>
    /// Sends order confirmation email to customer
    /// </summary>
    Task SendOrderConfirmationEmailAsync(Order order, Customer customer);

    /// <summary>
    /// Sends order shipped notification email to customer
    /// </summary>
    Task SendOrderShippedEmailAsync(Order order, Customer customer);

    /// <summary>
    /// Sends order delivered notification email to customer
    /// </summary>
    Task SendOrderDeliveredEmailAsync(Order order, Customer customer);

    /// <summary>
    /// Sends welcome email to newly registered customer
    /// </summary>
    Task SendWelcomeEmailAsync(Customer customer);
}
