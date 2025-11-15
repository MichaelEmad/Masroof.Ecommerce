import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { CustomerService } from '../../proxy/customers/customer.service';
import { AddressService } from '../../proxy/addresses/address.service';
import { CustomerDto } from '../../proxy/customers/models';
import { AddressDto, CreateUpdateAddressDto } from '../../proxy/addresses/models';
import { AddressFormComponent } from './address-form.component';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, AddressFormComponent],
  template: `
    <div class="container mt-4 mb-5">
      <div class="row">
        <!-- Sidebar -->
        <div class="col-md-3">
          <div class="card">
            <div class="card-body">
              <div class="text-center mb-3">
                <div class="profile-avatar mb-2">
                  <i class="bi bi-person-circle" style="font-size: 4rem;"></i>
                </div>
                <h5 class="mb-0">{{ customer?.fullName }}</h5>
                <small class="text-muted">{{ customer?.email }}</small>
                <div class="mt-2" *ngIf="customer?.isVipCustomer">
                  <span class="badge bg-warning text-dark">
                    <i class="bi bi-star-fill me-1"></i>VIP Customer
                  </span>
                </div>
              </div>
              <hr>
              <nav class="nav flex-column">
                <a class="nav-link" [class.active]="activeTab === 'profile'" (click)="activeTab = 'profile'">
                  <i class="bi bi-person me-2"></i>Profile
                </a>
                <a class="nav-link" [class.active]="activeTab === 'addresses'" (click)="activeTab = 'addresses'">
                  <i class="bi bi-geo-alt me-2"></i>Addresses
                </a>
                <a class="nav-link" [routerLink]="['/ecommerce/my-orders']">
                  <i class="bi bi-box-seam me-2"></i>My Orders
                </a>
                <a class="nav-link" href="/account/manage">
                  <i class="bi bi-key me-2"></i>Change Password
                </a>
              </nav>
            </div>
          </div>
        </div>

        <!-- Main Content -->
        <div class="col-md-9">
          <!-- Profile Tab -->
          <div *ngIf="activeTab === 'profile'">
            <!-- Customer Stats -->
            <div class="row mb-4">
              <div class="col-md-4">
                <div class="card text-center">
                  <div class="card-body">
                    <i class="bi bi-cart-check text-primary" style="font-size: 2rem;"></i>
                    <h3 class="mt-2 mb-0">{{ customer?.totalOrders || 0 }}</h3>
                    <p class="text-muted mb-0">Total Orders</p>
                  </div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="card text-center">
                  <div class="card-body">
                    <i class="bi bi-currency-dollar text-success" style="font-size: 2rem;"></i>
                    <h3 class="mt-2 mb-0">\${{ customer?.totalSpent || 0 | number: '1.2-2' }}</h3>
                    <p class="text-muted mb-0">Total Spent</p>
                  </div>
                </div>
              </div>
              <div class="col-md-4">
                <div class="card text-center">
                  <div class="card-body">
                    <i class="bi bi-calendar-event text-info" style="font-size: 2rem;"></i>
                    <h3 class="mt-2 mb-0">{{ getMemberSince() }}</h3>
                    <p class="text-muted mb-0">Member Since</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Profile Form -->
            <div class="card">
              <div class="card-header">
                <h5 class="mb-0">Personal Information</h5>
              </div>
              <div class="card-body">
                <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
                  <div class="row g-3">
                    <div class="col-md-6">
                      <label for="firstName" class="form-label">First Name *</label>
                      <input
                        type="text"
                        class="form-control"
                        id="firstName"
                        formControlName="firstName"
                        [class.is-invalid]="isFieldInvalid('firstName')">
                      <div class="invalid-feedback">First name is required</div>
                    </div>

                    <div class="col-md-6">
                      <label for="lastName" class="form-label">Last Name *</label>
                      <input
                        type="text"
                        class="form-control"
                        id="lastName"
                        formControlName="lastName"
                        [class.is-invalid]="isFieldInvalid('lastName')">
                      <div class="invalid-feedback">Last name is required</div>
                    </div>

                    <div class="col-md-6">
                      <label for="email" class="form-label">Email Address</label>
                      <input
                        type="email"
                        class="form-control"
                        id="email"
                        [value]="customer?.email"
                        disabled>
                      <small class="text-muted">Contact support to change your email</small>
                    </div>

                    <div class="col-md-6">
                      <label for="phoneNumber" class="form-label">Phone Number</label>
                      <input
                        type="tel"
                        class="form-control"
                        id="phoneNumber"
                        formControlName="phoneNumber">
                    </div>

                    <div class="col-md-6">
                      <label for="dateOfBirth" class="form-label">Date of Birth</label>
                      <input
                        type="date"
                        class="form-control"
                        id="dateOfBirth"
                        formControlName="dateOfBirth">
                    </div>
                  </div>

                  <div class="mt-4">
                    <button type="submit" class="btn btn-primary" [disabled]="profileLoading || profileForm.invalid">
                      <span *ngIf="!profileLoading">
                        <i class="bi bi-save me-2"></i>Save Changes
                      </span>
                      <span *ngIf="profileLoading">
                        <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                        Saving...
                      </span>
                    </button>
                    <button type="button" class="btn btn-secondary ms-2" (click)="loadProfile()">
                      <i class="bi bi-x-circle me-2"></i>Cancel
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>

          <!-- Addresses Tab -->
          <div *ngIf="activeTab === 'addresses'">
            <div class="card">
              <div class="card-header d-flex justify-content-between align-items-center">
                <h5 class="mb-0">My Addresses</h5>
                <button class="btn btn-primary btn-sm" (click)="openAddressForm()">
                  <i class="bi bi-plus-circle me-2"></i>Add Address
                </button>
              </div>
              <div class="card-body">
                <div *ngIf="addresses.length === 0" class="text-center py-5">
                  <i class="bi bi-geo-alt" style="font-size: 3rem; color: #ccc;"></i>
                  <p class="text-muted mt-3">No addresses found</p>
                  <button class="btn btn-primary" (click)="openAddressForm()">
                    <i class="bi bi-plus-circle me-2"></i>Add Your First Address
                  </button>
                </div>

                <div class="row" *ngIf="addresses.length > 0">
                  <div class="col-md-6 mb-3" *ngFor="let address of addresses">
                    <div class="card h-100" [class.border-primary]="address.isDefault">
                      <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                          <h6 class="mb-0">
                            {{ address.fullName }}
                            <span class="badge bg-primary ms-2" *ngIf="address.isDefault">Default</span>
                          </h6>
                          <span class="badge bg-secondary">
                            {{ getAddressTypeName(address.addressType) }}
                          </span>
                        </div>
                        <p class="mb-1 small text-muted">
                          <i class="bi bi-telephone me-2"></i>{{ address.phoneNumber }}
                        </p>
                        <p class="mb-2 small">
                          {{ address.formattedAddress }}
                        </p>
                        <div class="btn-group btn-group-sm" role="group">
                          <button
                            class="btn btn-outline-primary"
                            (click)="editAddress(address)"
                            [disabled]="addressLoading">
                            <i class="bi bi-pencil"></i> Edit
                          </button>
                          <button
                            class="btn btn-outline-success"
                            (click)="setDefaultAddress(address.id)"
                            [disabled]="address.isDefault || addressLoading"
                            *ngIf="!address.isDefault">
                            <i class="bi bi-check-circle"></i> Set Default
                          </button>
                          <button
                            class="btn btn-outline-danger"
                            (click)="deleteAddress(address.id)"
                            [disabled]="addressLoading">
                            <i class="bi bi-trash"></i> Delete
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Address Form Modal -->
    <app-address-form
      [address]="selectedAddress"
      [isVisible]="showAddressForm"
      (onSave)="saveAddress($event)"
      (onCancel)="closeAddressForm()">
    </app-address-form>
  `,
  styles: [`
    .nav-link {
      color: #495057;
      cursor: pointer;
      border-radius: 0.25rem;
      padding: 0.5rem 1rem;
      margin-bottom: 0.25rem;
    }

    .nav-link:hover,
    .nav-link.active {
      background-color: #e9ecef;
      color: #0d6efd;
    }

    .nav-link i {
      width: 20px;
    }

    .profile-avatar {
      color: #6c757d;
    }

    .form-label {
      font-weight: 500;
      margin-bottom: 0.5rem;
    }

    .card {
      border-radius: 10px;
      margin-bottom: 1.5rem;
    }
  `]
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
    private fb: FormBuilder
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
      error: (error) => {
        console.error('Error loading profile:', error);
        alert('Failed to load profile. Please try again.');
      }
    });
  }

  loadAddresses() {
    this.addressService.getMyAddresses().subscribe({
      next: (data) => {
        this.addresses = data;
      },
      error: (error) => {
        console.error('Error loading addresses:', error);
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
        alert('Profile updated successfully!');
      },
      error: (error) => {
        console.error('Error updating profile:', error);
        this.profileLoading = false;
        alert('Failed to update profile. Please try again.');
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
        alert(this.selectedAddress ? 'Address updated successfully!' : 'Address added successfully!');
      },
      error: (error) => {
        console.error('Error saving address:', error);
        this.addressLoading = false;
        alert('Failed to save address. Please try again.');
      }
    });
  }

  setDefaultAddress(id: string) {
    this.addressLoading = true;
    this.addressService.setAsDefault(id).subscribe({
      next: () => {
        this.addressLoading = false;
        this.loadAddresses();
        alert('Default address updated successfully!');
      },
      error: (error) => {
        console.error('Error setting default address:', error);
        this.addressLoading = false;
        alert('Failed to set default address. Please try again.');
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
        alert('Address deleted successfully!');
      },
      error: (error) => {
        console.error('Error deleting address:', error);
        this.addressLoading = false;
        alert('Failed to delete address. Please try again.');
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
