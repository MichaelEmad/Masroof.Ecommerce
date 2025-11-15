using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace Masroof.Ecommerce.Emails;

public class EmailService : IEmailService, ITransientDependency
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;
    private readonly string _templateBasePath;

    public EmailService(
        IEmailSender emailSender,
        ILogger<EmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;

        // Get the base path for email templates
        _templateBasePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Emails",
            "Templates"
        );
    }

    public async Task SendOrderConfirmationEmailAsync(Order order, Customer customer)
    {
        try
        {
            var subject = $"Order Confirmation - {order.OrderNumber}";
            var body = await GetOrderConfirmationEmailBodyAsync(order, customer);

            await _emailSender.SendAsync(
                customer.Email,
                subject,
                body
            );

            _logger.LogInformation(
                "Order confirmation email sent successfully to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send order confirmation email to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
            // Don't throw - we don't want email failures to break the main operation
        }
    }

    public async Task SendOrderShippedEmailAsync(Order order, Customer customer)
    {
        try
        {
            var subject = $"Your Order Has Been Shipped - {order.OrderNumber}";
            var body = await GetOrderShippedEmailBodyAsync(order, customer);

            await _emailSender.SendAsync(
                customer.Email,
                subject,
                body
            );

            _logger.LogInformation(
                "Order shipped email sent successfully to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send order shipped email to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
            // Don't throw - we don't want email failures to break the main operation
        }
    }

    public async Task SendOrderDeliveredEmailAsync(Order order, Customer customer)
    {
        try
        {
            var subject = $"Your Order Has Been Delivered - {order.OrderNumber}";
            var body = await GetOrderDeliveredEmailBodyAsync(order, customer);

            await _emailSender.SendAsync(
                customer.Email,
                subject,
                body
            );

            _logger.LogInformation(
                "Order delivered email sent successfully to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send order delivered email to {Email} for order {OrderNumber}",
                customer.Email,
                order.OrderNumber
            );
            // Don't throw - we don't want email failures to break the main operation
        }
    }

    public async Task SendWelcomeEmailAsync(Customer customer)
    {
        try
        {
            var subject = "Welcome to Masroof E-Commerce!";
            var body = await GetWelcomeEmailBodyAsync(customer);

            await _emailSender.SendAsync(
                customer.Email,
                subject,
                body
            );

            _logger.LogInformation(
                "Welcome email sent successfully to {Email}",
                customer.Email
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email to {Email}",
                customer.Email
            );
            // Don't throw - we don't want email failures to break the main operation
        }
    }

    private async Task<string> GetOrderConfirmationEmailBodyAsync(Order order, Customer customer)
    {
        var template = await LoadTemplateAsync("OrderConfirmation.html");

        var itemsHtml = new StringBuilder();
        foreach (var item in order.Items)
        {
            itemsHtml.Append($@"
                <tr>
                    <td style='padding: 10px; border-bottom: 1px solid #eee;'>
                        {(string.IsNullOrEmpty(item.ImageUrl) ? "" : $"<img src='{item.ImageUrl}' alt='{item.ProductName}' style='width: 50px; height: 50px; object-fit: cover; margin-right: 10px; vertical-align: middle;'>")}
                        {item.ProductName}
                    </td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>${item.UnitPrice:F2}</td>
                    <td style='padding: 10px; border-bottom: 1px solid #eee; text-align: right;'>${item.GetTotalPrice():F2}</td>
                </tr>
            ");
        }

        return template
            .Replace("{{CustomerName}}", customer.GetFullName())
            .Replace("{{OrderNumber}}", order.OrderNumber)
            .Replace("{{OrderDate}}", order.CreationTime.ToString("MMMM dd, yyyy"))
            .Replace("{{OrderItems}}", itemsHtml.ToString())
            .Replace("{{SubTotal}}", $"${order.SubTotal:F2}")
            .Replace("{{Discount}}", order.DiscountAmount > 0 ? $"-${order.DiscountAmount:F2}" : "$0.00")
            .Replace("{{ShippingCost}}", $"${order.ShippingCost:F2}")
            .Replace("{{Tax}}", $"${order.Tax:F2}")
            .Replace("{{TotalAmount}}", $"${order.TotalAmount:F2}")
            .Replace("{{ShippingAddress}}", FormatAddress(
                order.ShippingFullName,
                order.ShippingAddressLine1,
                order.ShippingAddressLine2,
                order.ShippingCity,
                order.ShippingState,
                order.ShippingPostalCode,
                order.ShippingCountry
            ));
    }

    private async Task<string> GetOrderShippedEmailBodyAsync(Order order, Customer customer)
    {
        var template = await LoadTemplateAsync("OrderShipped.html");

        return template
            .Replace("{{CustomerName}}", customer.GetFullName())
            .Replace("{{OrderNumber}}", order.OrderNumber)
            .Replace("{{ShippedDate}}", order.ShippedDate?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy"))
            .Replace("{{TrackingNumber}}", order.TrackingNumber ?? "N/A")
            .Replace("{{ShippingCarrier}}", order.ShippingCarrier ?? "N/A")
            .Replace("{{ShippingAddress}}", FormatAddress(
                order.ShippingFullName,
                order.ShippingAddressLine1,
                order.ShippingAddressLine2,
                order.ShippingCity,
                order.ShippingState,
                order.ShippingPostalCode,
                order.ShippingCountry
            ))
            .Replace("{{TotalAmount}}", $"${order.TotalAmount:F2}");
    }

    private async Task<string> GetOrderDeliveredEmailBodyAsync(Order order, Customer customer)
    {
        var template = await LoadTemplateAsync("OrderDelivered.html");

        return template
            .Replace("{{CustomerName}}", customer.GetFullName())
            .Replace("{{OrderNumber}}", order.OrderNumber)
            .Replace("{{DeliveredDate}}", order.DeliveredDate?.ToString("MMMM dd, yyyy") ?? DateTime.UtcNow.ToString("MMMM dd, yyyy"))
            .Replace("{{TotalAmount}}", $"${order.TotalAmount:F2}");
    }

    private async Task<string> GetWelcomeEmailBodyAsync(Customer customer)
    {
        var template = await LoadTemplateAsync("WelcomeEmail.html");

        return template
            .Replace("{{CustomerName}}", customer.GetFullName())
            .Replace("{{Email}}", customer.Email);
    }

    private async Task<string> LoadTemplateAsync(string templateName)
    {
        try
        {
            var templatePath = Path.Combine(_templateBasePath, templateName);

            if (!File.Exists(templatePath))
            {
                _logger.LogWarning("Email template not found at {TemplatePath}, using fallback", templatePath);
                return GetFallbackTemplate(templateName);
            }

            return await File.ReadAllTextAsync(templatePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading email template {TemplateName}", templateName);
            return GetFallbackTemplate(templateName);
        }
    }

    private string GetFallbackTemplate(string templateName)
    {
        // Simple fallback templates in case files are not found
        return templateName switch
        {
            "OrderConfirmation.html" => "<html><body><h1>Order Confirmation</h1><p>Thank you for your order!</p></body></html>",
            "OrderShipped.html" => "<html><body><h1>Order Shipped</h1><p>Your order has been shipped!</p></body></html>",
            "OrderDelivered.html" => "<html><body><h1>Order Delivered</h1><p>Your order has been delivered!</p></body></html>",
            "WelcomeEmail.html" => "<html><body><h1>Welcome!</h1><p>Welcome to Masroof E-Commerce!</p></body></html>",
            _ => "<html><body><p>Email content</p></body></html>"
        };
    }

    private string FormatAddress(
        string fullName,
        string addressLine1,
        string? addressLine2,
        string city,
        string state,
        string postalCode,
        string country)
    {
        var address = new StringBuilder();
        address.AppendLine($"<strong>{fullName}</strong><br>");
        address.AppendLine($"{addressLine1}<br>");

        if (!string.IsNullOrEmpty(addressLine2))
        {
            address.AppendLine($"{addressLine2}<br>");
        }

        address.AppendLine($"{city}, {state} {postalCode}<br>");
        address.Append(country);

        return address.ToString();
    }
}
