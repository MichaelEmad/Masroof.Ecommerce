using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Customers;

[Authorize]
public class CustomerAppService : CrudAppService<Customer, CustomerDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateCustomerDto>, ICustomerAppService
{
    private readonly ICurrentUser _currentUser;

    public CustomerAppService(
        IRepository<Customer, Guid> repository,
        ICurrentUser currentUser) : base(repository)
    {
        _currentUser = currentUser;
        GetPolicyName = EcommercePermissions.Customers.Default;
        GetListPolicyName = EcommercePermissions.Customers.Default;
        CreatePolicyName = EcommercePermissions.Customers.Create;
        UpdatePolicyName = EcommercePermissions.Customers.Edit;
        DeletePolicyName = EcommercePermissions.Customers.Delete;
    }

    public async Task<CustomerDto> GetMyProfileAsync()
    {
        var userId = _currentUser.GetId();
        var customers = await Repository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            // Create customer profile if doesn't exist
            customer = new Customer(
                GuidGenerator.Create(),
                userId,
                _currentUser.Name?.Split(' ').FirstOrDefault() ?? "User",
                _currentUser.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "",
                _currentUser.Email ?? ""
            );
            customer = await Repository.InsertAsync(customer, autoSave: true);
        }

        return ObjectMapper.Map<Customer, CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateMyProfileAsync(CreateUpdateCustomerDto input)
    {
        var userId = _currentUser.GetId();
        var customers = await Repository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("Customer profile not found");
        }

        customer.UpdateProfile(
            input.FirstName,
            input.LastName,
            input.PhoneNumber,
            input.DateOfBirth
        );

        if (!string.IsNullOrEmpty(input.ProfilePictureUrl))
        {
            customer.ProfilePictureUrl = input.ProfilePictureUrl;
        }

        await Repository.UpdateAsync(customer, autoSave: true);

        return ObjectMapper.Map<Customer, CustomerDto>(customer);
    }

    [Authorize(EcommercePermissions.Customers.Default)]
    public async Task<ListResultDto<CustomerDto>> GetVipCustomersAsync()
    {
        var customers = await Repository.GetListAsync();
        var vipCustomers = customers
            .Where(c => c.IsVipCustomer())
            .OrderByDescending(c => c.TotalSpent)
            .ToList();

        return new ListResultDto<CustomerDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Customer>, System.Collections.Generic.List<CustomerDto>>(vipCustomers)
        );
    }

    protected override async Task<IQueryable<Customer>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await Repository.GetQueryableAsync())
            .OrderByDescending(c => c.CreationTime);
    }
}
