using System;
using System.Linq;
using System.Threading.Tasks;
using Masroof.Ecommerce.Customers;
using Masroof.Ecommerce.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace Masroof.Ecommerce.Addresses;

[Authorize]
public class AddressAppService : CrudAppService<Address, AddressDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateAddressDto>, IAddressAppService
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<Customer, Guid> _customerRepository;

    public AddressAppService(
        IRepository<Address, Guid> repository,
        IRepository<Customer, Guid> customerRepository,
        ICurrentUser currentUser) : base(repository)
    {
        _customerRepository = customerRepository;
        _currentUser = currentUser;
        GetPolicyName = EcommercePermissions.Addresses.Default;
        GetListPolicyName = EcommercePermissions.Addresses.Default;
        CreatePolicyName = EcommercePermissions.Addresses.Create;
        UpdatePolicyName = EcommercePermissions.Addresses.Edit;
        DeletePolicyName = EcommercePermissions.Addresses.Delete;
    }

    public async Task<ListResultDto<AddressDto>> GetMyAddressesAsync()
    {
        var customer = await GetCurrentCustomerAsync();
        var addresses = await Repository.GetListAsync();
        var myAddresses = addresses
            .Where(a => a.CustomerId == customer.Id)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.CreationTime)
            .ToList();

        return new ListResultDto<AddressDto>(
            ObjectMapper.Map<System.Collections.Generic.List<Address>, System.Collections.Generic.List<AddressDto>>(myAddresses)
        );
    }

    public async Task<AddressDto> GetDefaultAddressAsync(Guid customerId)
    {
        var addresses = await Repository.GetListAsync();
        var defaultAddress = addresses.FirstOrDefault(a => a.CustomerId == customerId && a.IsDefault);

        if (defaultAddress == null)
        {
            throw new UserFriendlyException("No default address found");
        }

        return ObjectMapper.Map<Address, AddressDto>(defaultAddress);
    }

    public async Task SetAsDefaultAsync(Guid id)
    {
        var address = await Repository.GetAsync(id);
        var customer = await GetCurrentCustomerAsync();

        // Ensure user owns this address
        if (address.CustomerId != customer.Id && !_currentUser.IsInRole("admin"))
        {
            throw new UserFriendlyException("You don't have permission to modify this address");
        }

        // Remove default from all other addresses
        var addresses = await Repository.GetListAsync();
        var customerAddresses = addresses.Where(a => a.CustomerId == address.CustomerId).ToList();

        foreach (var addr in customerAddresses)
        {
            if (addr.Id != id && addr.IsDefault)
            {
                addr.RemoveDefault();
                await Repository.UpdateAsync(addr, autoSave: false);
            }
        }

        // Set current address as default
        address.SetAsDefault();
        await Repository.UpdateAsync(address, autoSave: true);
    }

    public override async Task<AddressDto> CreateAsync(CreateUpdateAddressDto input)
    {
        var customer = await GetCurrentCustomerAsync();

        var address = new Address(
            GuidGenerator.Create(),
            customer.Id,
            input.FullName,
            input.PhoneNumber,
            input.AddressLine1,
            input.City,
            input.State,
            input.PostalCode,
            input.Country,
            input.AddressType
        );

        if (!string.IsNullOrEmpty(input.AddressLine2))
        {
            address.AddressLine2 = input.AddressLine2;
        }

        if (input.IsDefault)
        {
            // Remove default from all other addresses
            var addresses = await Repository.GetListAsync();
            var customerAddresses = addresses.Where(a => a.CustomerId == customer.Id).ToList();

            foreach (var addr in customerAddresses)
            {
                if (addr.IsDefault)
                {
                    addr.RemoveDefault();
                    await Repository.UpdateAsync(addr, autoSave: false);
                }
            }

            address.SetAsDefault();
        }

        await Repository.InsertAsync(address, autoSave: true);

        return ObjectMapper.Map<Address, AddressDto>(address);
    }

    private async Task<Customer> GetCurrentCustomerAsync()
    {
        var userId = _currentUser.GetId();
        var customers = await _customerRepository.GetListAsync();
        var customer = customers.FirstOrDefault(c => c.UserId == userId);

        if (customer == null)
        {
            throw new UserFriendlyException("Customer profile not found. Please create your profile first.");
        }

        return customer;
    }

    protected override async Task<IQueryable<Address>> CreateFilteredQueryAsync(PagedAndSortedResultRequestDto input)
    {
        return (await Repository.GetQueryableAsync())
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreationTime);
    }
}
