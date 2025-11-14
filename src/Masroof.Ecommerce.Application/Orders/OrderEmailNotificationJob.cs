using System.Text;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing;

namespace Masroof.Ecommerce.Orders;

public class OrderEmailNotificationJob :
    AsyncBackgroundJob<OrderEmailNotificationArgs>,
    ITransientDependency
{
    private readonly IEmailSender _emailSender;

    public OrderEmailNotificationJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public override async Task ExecuteAsync(OrderEmailNotificationArgs args)
    {
        var subject = "Order Confirmation";
        var body = new StringBuilder();
        body.AppendLine($"<h2>Hello {args.CustomerName},</h2>");
        body.AppendLine($"<p>Thank you for your order!</p>");
        body.AppendLine($"<p><strong>Order ID:</strong> {args.OrderId}</p>");
        body.AppendLine($"<p><strong>Total Amount:</strong> ${args.TotalAmount:F2}</p>");
        body.AppendLine($"<p>Your order is being processed and will be shipped soon.</p>");
        body.AppendLine($"<p>Best regards,<br/>E-commerce Team</p>");

        await _emailSender.SendAsync(
            args.CustomerEmail,
            subject,
            body.ToString()
        );
    }
}

public class OrderPaymentConfirmationJob :
    AsyncBackgroundJob<OrderPaymentConfirmationArgs>,
    ITransientDependency
{
    private readonly IEmailSender _emailSender;

    public OrderPaymentConfirmationJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public override async Task ExecuteAsync(OrderPaymentConfirmationArgs args)
    {
        var subject = "Payment Confirmed";
        var body = new StringBuilder();
        body.AppendLine($"<h2>Hello {args.CustomerName},</h2>");
        body.AppendLine($"<p>Your payment has been confirmed!</p>");
        body.AppendLine($"<p><strong>Order ID:</strong> {args.OrderId}</p>");
        body.AppendLine($"<p>Your order will be shipped soon.</p>");
        body.AppendLine($"<p>Best regards,<br/>E-commerce Team</p>");

        await _emailSender.SendAsync(
            args.CustomerEmail,
            subject,
            body.ToString()
        );
    }
}
