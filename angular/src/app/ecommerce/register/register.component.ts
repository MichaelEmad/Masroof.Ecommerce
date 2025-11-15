import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ConfigStateService } from '@abp/ng.core';
import { CustomerService } from '../../proxy/customers/customer.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-8 col-lg-6">
          <div class="card shadow">
            <div class="card-body p-5">
              <div class="text-center mb-4">
                <h2>Create Account</h2>
                <p class="text-muted">Join us and start shopping!</p>
              </div>

              <div class="alert alert-info" role="alert">
                <i class="bi bi-info-circle me-2"></i>
                <strong>Note:</strong> This will redirect you to the ABP registration page. After successful registration,
                a customer profile will be automatically created for you.
              </div>

              <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
                <div class="row g-3">
                  <div class="col-md-6">
                    <label for="firstName" class="form-label">First Name *</label>
                    <input
                      type="text"
                      class="form-control"
                      id="firstName"
                      formControlName="firstName"
                      [class.is-invalid]="isFieldInvalid('firstName')">
                    <div class="invalid-feedback" *ngIf="isFieldInvalid('firstName')">
                      First name is required
                    </div>
                  </div>

                  <div class="col-md-6">
                    <label for="lastName" class="form-label">Last Name *</label>
                    <input
                      type="text"
                      class="form-control"
                      id="lastName"
                      formControlName="lastName"
                      [class.is-invalid]="isFieldInvalid('lastName')">
                    <div class="invalid-feedback" *ngIf="isFieldInvalid('lastName')">
                      Last name is required
                    </div>
                  </div>

                  <div class="col-12">
                    <label for="email" class="form-label">Email Address *</label>
                    <input
                      type="email"
                      class="form-control"
                      id="email"
                      formControlName="email"
                      [class.is-invalid]="isFieldInvalid('email')">
                    <div class="invalid-feedback" *ngIf="isFieldInvalid('email')">
                      <span *ngIf="registerForm.get('email')?.errors?.['required']">Email is required</span>
                      <span *ngIf="registerForm.get('email')?.errors?.['email']">Invalid email format</span>
                    </div>
                  </div>

                  <div class="col-12">
                    <label for="phoneNumber" class="form-label">Phone Number</label>
                    <input
                      type="tel"
                      class="form-control"
                      id="phoneNumber"
                      formControlName="phoneNumber"
                      placeholder="+1 (555) 123-4567">
                  </div>

                  <div class="col-12">
                    <label for="password" class="form-label">Password *</label>
                    <input
                      type="password"
                      class="form-control"
                      id="password"
                      formControlName="password"
                      [class.is-invalid]="isFieldInvalid('password')">
                    <div class="invalid-feedback" *ngIf="isFieldInvalid('password')">
                      <span *ngIf="registerForm.get('password')?.errors?.['required']">Password is required</span>
                      <span *ngIf="registerForm.get('password')?.errors?.['minlength']">
                        Password must be at least 6 characters
                      </span>
                    </div>
                  </div>

                  <div class="col-12">
                    <label for="confirmPassword" class="form-label">Confirm Password *</label>
                    <input
                      type="password"
                      class="form-control"
                      id="confirmPassword"
                      formControlName="confirmPassword"
                      [class.is-invalid]="isFieldInvalid('confirmPassword')">
                    <div class="invalid-feedback" *ngIf="isFieldInvalid('confirmPassword')">
                      <span *ngIf="registerForm.get('confirmPassword')?.errors?.['required']">
                        Please confirm your password
                      </span>
                      <span *ngIf="registerForm.get('confirmPassword')?.errors?.['passwordMismatch']">
                        Passwords do not match
                      </span>
                    </div>
                  </div>
                </div>

                <div class="d-grid gap-3 mt-4">
                  <button
                    type="submit"
                    class="btn btn-primary btn-lg"
                    [disabled]="loading">
                    <span *ngIf="!loading">
                      <i class="bi bi-person-plus me-2"></i>
                      Create Account
                    </span>
                    <span *ngIf="loading">
                      <span class="spinner-border spinner-border-sm me-2" role="status"></span>
                      Redirecting...
                    </span>
                  </button>

                  <div class="text-center">
                    <span class="text-muted">Already have an account?</span>
                    <a [routerLink]="['/ecommerce/login']" class="ms-2 fw-bold">
                      Sign In
                    </a>
                  </div>

                  <hr class="my-2">

                  <div class="text-center">
                    <a [routerLink]="['/ecommerce/products']" class="text-muted">
                      <i class="bi bi-arrow-left me-2"></i>
                      Continue Shopping
                    </a>
                  </div>
                </div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .card {
      border: none;
      border-radius: 15px;
    }

    .btn-lg {
      padding: 12px 24px;
      font-size: 1.1rem;
    }

    .form-label {
      font-weight: 500;
      margin-bottom: 0.5rem;
    }
  `]
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private customerService: CustomerService,
    private configState: ConfigStateService
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, {
      validators: this.passwordMatchValidator
    });
  }

  ngOnInit() {
    // If user is already logged in, redirect to products
    if (this.configState.getOne('currentUser')?.id) {
      this.router.navigate(['/ecommerce/products']);
    }
  }

  passwordMatchValidator(form: FormGroup) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    return null;
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  onSubmit() {
    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;

    // Store registration data in sessionStorage to create customer profile after ABP registration
    const registrationData = {
      firstName: this.registerForm.value.firstName,
      lastName: this.registerForm.value.lastName,
      phoneNumber: this.registerForm.value.phoneNumber,
      email: this.registerForm.value.email
    };
    sessionStorage.setItem('pendingCustomerProfile', JSON.stringify(registrationData));

    // Redirect to ABP's registration page
    // In a real implementation, you would need to:
    // 1. Call ABP's registration API
    // 2. Create customer profile after successful registration
    // 3. Handle the redirect properly

    // For now, redirect to ABP's register page
    const registerUrl = '/account/register';
    const returnUrl = encodeURIComponent('/ecommerce/products');

    // In production, you would want to integrate with ABP's account module
    // and handle the customer profile creation in a registration event handler
    window.location.href = `${registerUrl}?returnUrl=${returnUrl}`;
  }
}
