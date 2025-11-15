using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace Masroof.Ecommerce.Invoices;

public class InvoiceService : IInvoiceService
{
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public InvoiceService(
        IRepository<Order, Guid> orderRepository,
        IRepository<Customer, Guid> customerRepository)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;

        // Set QuestPDF license to Community (free for non-commercial use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(Guid orderId)
    {
        var order = await _orderRepository.GetAsync(orderId);
        var customer = await _customerRepository.GetAsync(order.CustomerId);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                page.Header()
                    .Element(c => ComposeHeader(c, order, customer));

                page.Content()
                    .Element(c => ComposeContent(c, order, customer));

                page.Footer()
                    .Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, Order order, Customer customer)
    {
        container.Column(column =>
        {
            // Company Header
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("MASROOF E-COMMERCE")
                        .FontSize(24)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().Text("Your Trusted Online Store")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Email: ").SemiBold();
                        text.Span("support@masroof.com");
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Phone: ").SemiBold();
                        text.Span("+1 (555) 123-4567");
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Website: ").SemiBold();
                        text.Span("www.masroof.com");
                    });
                });

                row.ConstantItem(150).AlignRight().Column(col =>
                {
                    col.Item().Text("INVOICE")
                        .FontSize(20)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("Invoice #: ").SemiBold();
                        text.Span($"INV-{order.OrderNumber}");
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Date: ").SemiBold();
                        text.Span(order.CreationTime.ToString("MMM dd, yyyy"));
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Status: ").SemiBold();
                        text.Span(GetStatusText(order.Status)).FontColor(GetStatusColor(order.Status));
                    });
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            // Customer Information
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(5).Text(order.BillingFullName).Bold();
                    col.Item().Text(customer.Email);
                    col.Item().Text(order.BillingAddressLine1);
                    if (!string.IsNullOrEmpty(order.BillingAddressLine2))
                    {
                        col.Item().Text(order.BillingAddressLine2);
                    }
                    col.Item().Text($"{order.BillingCity}, {order.BillingState} {order.BillingPostalCode}");
                    col.Item().Text(order.BillingCountry);
                    if (!string.IsNullOrEmpty(order.BillingPhone))
                    {
                        col.Item().Text($"Phone: {order.BillingPhone}");
                    }
                });

                row.ConstantItem(20);

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("SHIP TO:")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(5).Text(order.ShippingFullName).Bold();
                    col.Item().Text(order.ShippingAddressLine1);
                    if (!string.IsNullOrEmpty(order.ShippingAddressLine2))
                    {
                        col.Item().Text(order.ShippingAddressLine2);
                    }
                    col.Item().Text($"{order.ShippingCity}, {order.ShippingState} {order.ShippingPostalCode}");
                    col.Item().Text(order.ShippingCountry);
                    if (!string.IsNullOrEmpty(order.ShippingPhone))
                    {
                        col.Item().Text($"Phone: {order.ShippingPhone}");
                    }
                });
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, Order order, Customer customer)
    {
        container.PaddingVertical(10).Column(column =>
        {
            // Order Items Table
            column.Item().Text("ORDER ITEMS")
                .FontSize(12)
                .SemiBold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().PaddingVertical(5).Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(40);  // #
                    columns.RelativeColumn(3);   // Product Name
                    columns.ConstantColumn(80);  // Unit Price
                    columns.ConstantColumn(60);  // Quantity
                    columns.ConstantColumn(90);  // Total
                });

                // Table Header
                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#").SemiBold();
                    header.Cell().Element(CellStyle).Text("Product").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Unit Price").SemiBold();
                    header.Cell().Element(CellStyle).AlignCenter().Text("Qty").SemiBold();
                    header.Cell().Element(CellStyle).AlignRight().Text("Total").SemiBold();

                    static IContainer CellStyle(IContainer container)
                    {
                        return container
                            .BorderBottom(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .PaddingVertical(5)
                            .Background(Colors.Grey.Lighten3);
                    }
                });

                // Table Rows
                var itemNumber = 1;
                foreach (var item in order.Items)
                {
                    var backgroundColor = itemNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Element(c => CellStyle(c, backgroundColor)).Text(itemNumber.ToString());
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).Text(item.ProductName);
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text($"${item.UnitPrice:N2}");
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(c => CellStyle(c, backgroundColor)).AlignRight().Text($"${item.GetTotalPrice():N2}");

                    itemNumber++;
                }

                static IContainer CellStyle(IContainer container, string backgroundColor)
                {
                    return container
                        .BorderBottom(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Background(backgroundColor)
                        .PaddingVertical(5)
                        .PaddingHorizontal(5);
                }
            });

            // Summary Section
            column.Item().PaddingTop(20).AlignRight().Column(summaryColumn =>
            {
                summaryColumn.Item().Width(250).Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Subtotal:");
                        row.ConstantItem(100).AlignRight().Text($"${order.SubTotal:N2}");
                    });

                    if (order.DiscountAmount > 0)
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Discount:");
                            row.ConstantItem(100).AlignRight().Text($"-${order.DiscountAmount:N2}").FontColor(Colors.Green.Darken2);
                        });

                        if (!string.IsNullOrEmpty(order.CouponCode))
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"  (Coupon: {order.CouponCode})").FontSize(8).Italic();
                            });
                        }
                    }

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Shipping:");
                        row.ConstantItem(100).AlignRight().Text($"${order.ShippingCost:N2}");
                    });

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Tax:");
                        row.ConstantItem(100).AlignRight().Text($"${order.Tax:N2}");
                    });

                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Darken1);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("TOTAL:").FontSize(12).Bold();
                        row.ConstantItem(100).AlignRight().Text($"${order.TotalAmount:N2}").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    });
                });
            });

            // Payment Information
            if (!string.IsNullOrEmpty(order.TrackingNumber))
            {
                column.Item().PaddingTop(20).Column(col =>
                {
                    col.Item().Text("SHIPPING INFORMATION")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Tracking Number: ").SemiBold();
                        text.Span(order.TrackingNumber);
                    });

                    if (!string.IsNullOrEmpty(order.ShippingCarrier))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Carrier: ").SemiBold();
                            text.Span(order.ShippingCarrier);
                        });
                    }

                    if (order.ShippedDate.HasValue)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Shipped Date: ").SemiBold();
                            text.Span(order.ShippedDate.Value.ToString("MMM dd, yyyy"));
                        });
                    }
                });
            }

            // Customer Notes
            if (!string.IsNullOrEmpty(order.CustomerNotes))
            {
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("CUSTOMER NOTES")
                        .FontSize(11)
                        .SemiBold()
                        .FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(5)
                        .Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Text(order.CustomerNotes);
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Column(column =>
        {
            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item().PaddingTop(5).Text("Thank you for your business!")
                .FontSize(11)
                .SemiBold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().PaddingTop(5).Text(text =>
            {
                text.DefaultTextStyle(TextStyle.Default.FontSize(8).FontColor(Colors.Grey.Darken1));
                text.Span("Masroof E-Commerce | 123 Business Street, Suite 100, Business City, BC 12345 | ");
                text.Span("Email: support@masroof.com | Phone: +1 (555) 123-4567");
            });

            column.Item().PaddingTop(5).Text("This is a computer-generated invoice and does not require a signature.")
                .FontSize(7)
                .Italic()
                .FontColor(Colors.Grey.Medium);
        });
    }

    private string GetStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pending",
            OrderStatus.Confirmed => "Confirmed",
            OrderStatus.Processing => "Processing",
            OrderStatus.Shipped => "Shipped",
            OrderStatus.Delivered => "Delivered",
            OrderStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };
    }

    private string GetStatusColor(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => Colors.Orange.Darken1,
            OrderStatus.Confirmed => Colors.Blue.Darken1,
            OrderStatus.Processing => Colors.Blue.Darken2,
            OrderStatus.Shipped => Colors.Green.Darken1,
            OrderStatus.Delivered => Colors.Green.Darken2,
            OrderStatus.Cancelled => Colors.Red.Darken1,
            _ => Colors.Grey.Darken1
        };
    }
}
