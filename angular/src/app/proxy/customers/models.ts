export interface CustomerDto {
  id: string;
  userId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  profilePictureUrl?: string;
  isEmailVerified: boolean;
  isPhoneVerified: boolean;
  lastLoginDate?: string;
  totalOrders: number;
  totalSpent: number;
  isVipCustomer: boolean;
  creationTime: string;
}

export interface CreateUpdateCustomerDto {
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  dateOfBirth?: string;
  profilePictureUrl?: string;
}
