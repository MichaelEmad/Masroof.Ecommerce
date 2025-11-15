using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Orders;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Payments;

[Authorize]
public class PaymentAppService : ApplicationService, IPaymentAppService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly ICurrentUser _currentUser;
    private readonly IPaymentService _paymentService;

    public PaymentAppService(
        IRepository<Payment, Guid> paymentRepository,
        IRepository<Order, Guid> orderRepository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentUser currentUser,
        IPaymentService paymentService)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _currentUser = currentUser;
        _paymentService = paymentService;
    }

    [Authorize(EcommercePermissions.Payments.Default)]
    public async Task<PaymentDto> GetAsync(Guid id)
    {
        var payment = await _paymentRepository.GetAsync(id);
        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }

    [Authorize(EcommercePermissions.Payments.Default)]
    public async Task<PagedResultDto<PaymentDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var payments = await _paymentRepository.GetListAsync();
        var totalCount = payments.Count;

        var orderedList = payments
            .OrderByDescending(p => p.CreationTime)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .ToList();

        return new PagedResultDto<PaymentDto>(
            totalCount,
            ObjectMapper.Map<System.Collections.Generic.List<Payment>, System.Collections.Generic.List<PaymentDto>>(orderedList)
        );
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var order = await _orderRepository.GetAsync(input.OrderId);

        // Validate order belongs to customer
        if (order.CustomerId != customer.Id && !_currentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("Invalid order");
        }

        // Check if payment already exists for this order
        var payments = await _paymentRepository.GetListAsync();
        var existingPayment = payments.FirstOrDefault(p => p.OrderId == input.OrderId && p.Status != PaymentStatus.Failed);

        if (existingPayment != null)
        {
            throw new UserFriendlyException("Payment already exists for this order");
        }

        // Create payment
        var payment = new Payment(
            GuidGenerator.Create(),
            input.OrderId,
            customer.Id,
            order.TotalAmount,
            input.PaymentMethod
        );

        if (!string.IsNullOrEmpty(input.PaymentGateway))
        {
            payment.PaymentGateway = input.PaymentGateway;
        }

        await _paymentRepository.InsertAsync(payment, autoSave: true);

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }

    [Authorize(EcommercePermissions.Payments.Process)]
    public async Task<PaymentDto> ProcessPaymentAsync(Guid id, ProcessPaymentDto input)
    {
        var payment = await _paymentRepository.GetAsync(id);

        // Mark as processing
        payment.MarkAsProcessing();

        // TODO: Integrate with real payment gateway
        // For now, simulate successful payment

        // Mark as succeeded
        payment.MarkAsSucceeded(
            input.TransactionId ?? Guid.NewGuid().ToString("N"),
            input.CardLast4Digits,
            input.CardBrand
        );

        await _paymentRepository.UpdateAsync(payment, autoSave: true);

        // Update order status
        var order = await _orderRepository.GetAsync(payment.OrderId);
        if (order.Status == OrderStatus.Pending)
        {
            order.Confirm();
            await _orderRepository.UpdateAsync(order, autoSave: true);
        }

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }

    [Authorize(EcommercePermissions.Payments.Refund)]
    public async Task<PaymentDto> RefundAsync(Guid id)
    {
        var payment = await _paymentRepository.GetAsync(id);

        if (!payment.CanBeRefunded())
        {
            throw new UserFriendlyException("This payment cannot be refunded");
        }

        // TODO: Integrate with payment gateway for refund
        // For now, just mark as refunded

        payment.Refund();

        await _paymentRepository.UpdateAsync(payment, autoSave: true);

        // Update order status to cancelled
        var order = await _orderRepository.GetAsync(payment.OrderId);
        if (order.CanBeCancelled())
        {
            order.Cancel();
            await _orderRepository.UpdateAsync(order, autoSave: true);
        }

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }

    public async Task<PaymentDto> GetByOrderIdAsync(Guid orderId)
    {
        var payments = await _paymentRepository.GetListAsync();
        var payment = payments.FirstOrDefault(p => p.OrderId == orderId);

        if (payment == null)
        {
            throw new UserFriendlyException("Payment not found for this order");
        }

        // Verify access
        var customer = await GetCurrentCustomerAsync();
        if (payment.CustomerId != customer.Id && !_currentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("Access denied");
        }

        return ObjectMapper.Map<Payment, PaymentDto>(payment);
    }

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(CreatePaymentIntentDto input)
    {
        var customer = await GetCurrentCustomerAsync();
        var order = await _orderRepository.GetAsync(input.OrderId);

        // Validate order belongs to customer
        if (order.CustomerId != customer.Id && !_currentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("Invalid order");
        }

        // Check if payment already exists for this order
        var payments = await _paymentRepository.GetListAsync();
        var existingPayment = payments.FirstOrDefault(p => p.OrderId == input.OrderId && p.Status != PaymentStatus.Failed);

        if (existingPayment != null && !string.IsNullOrEmpty(existingPayment.TransactionId))
        {
            // Return existing payment intent
            var confirmResult = await _paymentService.ConfirmPaymentAsync(existingPayment.TransactionId);
            return new PaymentIntentResultDto
            {
                PaymentIntentId = confirmResult.PaymentIntentId,
                ClientSecret = confirmResult.ClientSecret,
                Status = confirmResult.Status,
                IsSuccessful = true
            };
        }

        // Create Stripe PaymentIntent
        var result = await _paymentService.CreatePaymentIntentAsync(order, customer.Email);

        if (!result.IsSuccessful)
        {
            throw new UserFriendlyException(result.ErrorMessage ?? "Failed to create payment intent");
        }

        // Create or update payment record
        Payment payment;
        if (existingPayment != null)
        {
            payment = existingPayment;
            payment.TransactionId = result.PaymentIntentId;
            payment.PaymentGateway = "Stripe";
            payment.MarkAsProcessing();
            await _paymentRepository.UpdateAsync(payment, autoSave: true);
        }
        else
        {
            payment = new Payment(
                GuidGenerator.Create(),
                input.OrderId,
                customer.Id,
                order.TotalAmount,
                PaymentMethod.CreditCard
            );
            payment.TransactionId = result.PaymentIntentId;
            payment.PaymentGateway = "Stripe";
            payment.MarkAsProcessing();
            await _paymentRepository.InsertAsync(payment, autoSave: true);
        }

        return new PaymentIntentResultDto
        {
            PaymentIntentId = result.PaymentIntentId,
            ClientSecret = result.ClientSecret,
            Status = result.Status,
            IsSuccessful = result.IsSuccessful,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<PaymentIntentResultDto> ConfirmPaymentAsync(ConfirmPaymentDto input)
    {
        var result = await _paymentService.ConfirmPaymentAsync(input.PaymentIntentId);

        // Get the payment record and update it
        var payments = await _paymentRepository.GetListAsync();
        var payment = payments.FirstOrDefault(p => p.TransactionId == input.PaymentIntentId);

        if (payment != null && result.IsSuccessful)
        {
            payment.MarkAsSucceeded(
                input.PaymentIntentId,
                result.CardLast4,
                result.CardBrand
            );
            await _paymentRepository.UpdateAsync(payment, autoSave: true);

            // Update order status
            var order = await _orderRepository.GetAsync(payment.OrderId);
            if (order.Status == OrderStatus.Pending)
            {
                order.Confirm();
                await _orderRepository.UpdateAsync(order, autoSave: true);
            }
        }

        return new PaymentIntentResultDto
        {
            PaymentIntentId = result.PaymentIntentId,
            ClientSecret = result.ClientSecret,
            Status = result.Status,
            IsSuccessful = result.IsSuccessful,
            ErrorMessage = result.ErrorMessage
        };
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = _currentUser.GetId();
        var customers = await _customerRepository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("Customer profile not found");
        }

        return customer;
    }
}
