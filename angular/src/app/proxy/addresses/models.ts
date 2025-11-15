export interface AddressDto {
  id: string;
  customerId: string;
  fullName: string;
  phoneNumber: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
  addressType: AddressType;
  formattedAddress: string;
}

export interface CreateUpdateAddressDto {
  fullName: string;
  phoneNumber: string;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
  addressType: AddressType;
}

export enum AddressType {
  Shipping = 0,
  Billing = 1,
  Both = 2
}
