import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AddressDto, CreateUpdateAddressDto, AddressType } from '../../proxy/addresses/models';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal fade show d-block" tabindex="-1" role="dialog" *ngIf="isVisible">
      <div class="modal-dialog modal-lg" role="document">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">
              {{ address ? 'Edit Address' : 'Add New Address' }}
            </h5>
            <button type="button" class="btn-close" (click)="close()" aria-label="Close"></button>
          </div>

          <form [formGroup]="addressForm" (ngSubmit)="onSubmit()">
            <div class="modal-body">
              <div class="row g-3">
                <div class="col-md-6">
                  <label for="fullName" class="form-label">Full Name *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="fullName"
                    formControlName="fullName"
                    [class.is-invalid]="isFieldInvalid('fullName')">
                  <div class="invalid-feedback">Full name is required</div>
                </div>

                <div class="col-md-6">
                  <label for="phoneNumber" class="form-label">Phone Number *</label>
                  <input
                    type="tel"
                    class="form-control"
                    id="phoneNumber"
                    formControlName="phoneNumber"
                    [class.is-invalid]="isFieldInvalid('phoneNumber')">
                  <div class="invalid-feedback">Phone number is required</div>
                </div>

                <div class="col-12">
                  <label for="addressLine1" class="form-label">Address Line 1 *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="addressLine1"
                    formControlName="addressLine1"
                    placeholder="Street address, P.O. box"
                    [class.is-invalid]="isFieldInvalid('addressLine1')">
                  <div class="invalid-feedback">Address line 1 is required</div>
                </div>

                <div class="col-12">
                  <label for="addressLine2" class="form-label">Address Line 2</label>
                  <input
                    type="text"
                    class="form-control"
                    id="addressLine2"
                    formControlName="addressLine2"
                    placeholder="Apartment, suite, unit, building, floor, etc.">
                </div>

                <div class="col-md-6">
                  <label for="city" class="form-label">City *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="city"
                    formControlName="city"
                    [class.is-invalid]="isFieldInvalid('city')">
                  <div class="invalid-feedback">City is required</div>
                </div>

                <div class="col-md-6">
                  <label for="state" class="form-label">State/Province *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="state"
                    formControlName="state"
                    [class.is-invalid]="isFieldInvalid('state')">
                  <div class="invalid-feedback">State is required</div>
                </div>

                <div class="col-md-6">
                  <label for="postalCode" class="form-label">Postal Code *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="postalCode"
                    formControlName="postalCode"
                    [class.is-invalid]="isFieldInvalid('postalCode')">
                  <div class="invalid-feedback">Postal code is required</div>
                </div>

                <div class="col-md-6">
                  <label for="country" class="form-label">Country *</label>
                  <input
                    type="text"
                    class="form-control"
                    id="country"
                    formControlName="country"
                    [class.is-invalid]="isFieldInvalid('country')">
                  <div class="invalid-feedback">Country is required</div>
                </div>

                <div class="col-md-6">
                  <label for="addressType" class="form-label">Address Type *</label>
                  <select
                    class="form-select"
                    id="addressType"
                    formControlName="addressType"
                    [class.is-invalid]="isFieldInvalid('addressType')">
                    <option [value]="AddressType.Shipping">Shipping</option>
                    <option [value]="AddressType.Billing">Billing</option>
                    <option [value]="AddressType.Both">Both</option>
                  </select>
                  <div class="invalid-feedback">Address type is required</div>
                </div>

                <div class="col-md-6">
                  <div class="form-check mt-4">
                    <input
                      class="form-check-input"
                      type="checkbox"
                      id="isDefault"
                      formControlName="isDefault">
                    <label class="form-check-label" for="isDefault">
                      Set as default address
                    </label>
                  </div>
                </div>
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" (click)="close()">
                Cancel
              </button>
              <button type="submit" class="btn btn-primary" [disabled]="loading">
                <span *ngIf="!loading">
                  {{ address ? 'Update Address' : 'Add Address' }}
                </span>
                <span *ngIf="loading">
                  <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                  Saving...
                </span>
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
    <div class="modal-backdrop fade show" *ngIf="isVisible" (click)="close()"></div>
  `,
  styles: [`
    .modal.show {
      background-color: rgba(0, 0, 0, 0.5);
    }

    .form-label {
      font-weight: 500;
      margin-bottom: 0.5rem;
    }

    .modal-content {
      border-radius: 10px;
    }
  `]
})
export class AddressFormComponent implements OnInit {
  @Input() address?: AddressDto;
  @Input() isVisible = false;
  @Output() onSave = new EventEmitter<CreateUpdateAddressDto>();
  @Output() onCancel = new EventEmitter<void>();

  addressForm: FormGroup;
  loading = false;
  AddressType = AddressType;

  constructor(private fb: FormBuilder) {
    this.addressForm = this.fb.group({
      fullName: ['', [Validators.required]],
      phoneNumber: ['', [Validators.required]],
      addressLine1: ['', [Validators.required]],
      addressLine2: [''],
      city: ['', [Validators.required]],
      state: ['', [Validators.required]],
      postalCode: ['', [Validators.required]],
      country: ['', [Validators.required]],
      addressType: [AddressType.Both, [Validators.required]],
      isDefault: [false]
    });
  }

  ngOnInit() {
    if (this.address) {
      this.addressForm.patchValue({
        fullName: this.address.fullName,
        phoneNumber: this.address.phoneNumber,
        addressLine1: this.address.addressLine1,
        addressLine2: this.address.addressLine2,
        city: this.address.city,
        state: this.address.state,
        postalCode: this.address.postalCode,
        country: this.address.country,
        addressType: this.address.addressType,
        isDefault: this.address.isDefault
      });
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.addressForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit() {
    if (this.addressForm.invalid) {
      Object.keys(this.addressForm.controls).forEach(key => {
        this.addressForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;
    const addressData: CreateUpdateAddressDto = {
      fullName: this.addressForm.value.fullName,
      phoneNumber: this.addressForm.value.phoneNumber,
      addressLine1: this.addressForm.value.addressLine1,
      addressLine2: this.addressForm.value.addressLine2,
      city: this.addressForm.value.city,
      state: this.addressForm.value.state,
      postalCode: this.addressForm.value.postalCode,
      country: this.addressForm.value.country,
      addressType: this.addressForm.value.addressType,
      isDefault: this.addressForm.value.isDefault
    };

    this.onSave.emit(addressData);
  }

  close() {
    this.addressForm.reset();
    this.loading = false;
    this.onCancel.emit();
  }
}
