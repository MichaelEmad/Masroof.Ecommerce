using System;
using System.Threading.Tasks;

namespace Masroof.Ecommerce.Invoices;

public interface IInvoiceService
{
    Task<byte[]> GenerateInvoicePdfAsync(Guid orderId);
}
