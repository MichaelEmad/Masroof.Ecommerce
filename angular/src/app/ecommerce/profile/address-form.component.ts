import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AddressDto, CreateUpdateAddressDto, AddressType } from '../../proxy/addresses/models';

@Component({
  selector: 'app-address-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './address-form.component.html',
  styleUrls: ['./address-form.component.scss']
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
    this.addressForm.reset({
      addressType: AddressType.Both,
      isDefault: false
    });
    this.loading = false;
    this.onCancel.emit();
  }
}
