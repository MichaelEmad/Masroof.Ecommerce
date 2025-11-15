import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';
import { CustomerService } from '../../proxy/customers/customer.service';
import { AddressService } from '../../proxy/addresses/address.service';
import { CustomerDto } from '../../proxy/customers/models';
import { AddressDto, CreateUpdateAddressDto } from '../../proxy/addresses/models';
import { AddressFormComponent } from './address-form.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, AddressFormComponent],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  customer?: CustomerDto;
  addresses: AddressDto[] = [];
  activeTab: 'profile' | 'addresses' = 'profile';
  profileForm: FormGroup;
  profileLoading = false;
  addressLoading = false;
  showAddressForm = false;
  selectedAddress?: AddressDto;

  constructor(
    private customerService: CustomerService,
    private addressService: AddressService,
    private fb: FormBuilder,
    private toasterService: ToasterService
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phoneNumber: [''],
      dateOfBirth: ['']
    });
  }

  ngOnInit() {
    this.loadProfile();
    this.loadAddresses();
  }

  loadProfile() {
    this.customerService.getMyProfile().subscribe({
      next: (data) => {
        this.customer = data;
        this.profileForm.patchValue({
          firstName: data.firstName,
          lastName: data.lastName,
          phoneNumber: data.phoneNumber,
          dateOfBirth: data.dateOfBirth ? data.dateOfBirth.split('T')[0] : ''
        });
      },
      error: () => {
        this.toasterService.error('Failed to load profile', 'Error');
      }
    });
  }

  loadAddresses() {
    this.addressService.getMyAddresses().subscribe({
      next: (data) => {
        this.addresses = data;
      },
      error: () => {
        this.toasterService.error('Failed to load addresses', 'Error');
      }
    });
  }

  updateProfile() {
    if (this.profileForm.invalid) {
      Object.keys(this.profileForm.controls).forEach(key => {
        this.profileForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.profileLoading = true;
    const profileData = {
      firstName: this.profileForm.value.firstName,
      lastName: this.profileForm.value.lastName,
      phoneNumber: this.profileForm.value.phoneNumber || null,
      dateOfBirth: this.profileForm.value.dateOfBirth || null
    };

    this.customerService.updateMyProfile(profileData).subscribe({
      next: (data) => {
        this.customer = data;
        this.profileLoading = false;
        this.toasterService.success('Profile updated successfully!', 'Success');
      },
      error: () => {
        this.profileLoading = false;
        this.toasterService.error('Failed to update profile', 'Error');
      }
    });
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.profileForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  openAddressForm(address?: AddressDto) {
    this.selectedAddress = address;
    this.showAddressForm = true;
  }

  closeAddressForm() {
    this.showAddressForm = false;
    this.selectedAddress = undefined;
  }

  editAddress(address: AddressDto) {
    this.openAddressForm(address);
  }

  saveAddress(addressData: CreateUpdateAddressDto) {
    this.addressLoading = true;

    const request = this.selectedAddress
      ? this.addressService.update(this.selectedAddress.id, addressData)
      : this.addressService.create(addressData);

    request.subscribe({
      next: () => {
        this.addressLoading = false;
        this.closeAddressForm();
        this.loadAddresses();
        this.toasterService.success(
          this.selectedAddress ? 'Address updated successfully!' : 'Address added successfully!',
          'Success'
        );
      },
      error: () => {
        this.addressLoading = false;
        this.toasterService.error('Failed to save address', 'Error');
      }
    });
  }

  setDefaultAddress(id: string) {
    this.addressLoading = true;
    this.addressService.setAsDefault(id).subscribe({
      next: () => {
        this.addressLoading = false;
        this.loadAddresses();
        this.toasterService.success('Default address updated successfully!', 'Success');
      },
      error: () => {
        this.addressLoading = false;
        this.toasterService.error('Failed to set default address', 'Error');
      }
    });
  }

  deleteAddress(id: string) {
    if (!confirm('Are you sure you want to delete this address?')) {
      return;
    }

    this.addressLoading = true;
    this.addressService.delete(id).subscribe({
      next: () => {
        this.addressLoading = false;
        this.loadAddresses();
        this.toasterService.success('Address deleted successfully!', 'Success');
      },
      error: () => {
        this.addressLoading = false;
        this.toasterService.error('Failed to delete address', 'Error');
      }
    });
  }

  getMemberSince(): string {
    if (!this.customer?.creationTime) return 'N/A';
    const date = new Date(this.customer.creationTime);
    return date.toLocaleDateString('en-US', { month: 'short', year: 'numeric' });
  }

  getAddressTypeName(type: number): string {
    switch (type) {
      case 0:
        return 'Shipping';
      case 1:
        return 'Billing';
      case 2:
        return 'Both';
      default:
        return 'Unknown';
    }
  }
}
