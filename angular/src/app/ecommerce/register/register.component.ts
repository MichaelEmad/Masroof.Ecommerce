import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ConfigStateService, AuthService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { AccountService, RegisterDto } from '@volo/abp.ng.account/public/proxy';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  loading = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private accountService: AccountService,
    private authService: AuthService,
    private configState: ConfigStateService,
    private toasterService: ToasterService
  ) {
    this.registerForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
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
    return !!(field && field.invalid && (field.dirty || field.touched || this.submitted));
  }

  onSubmit() {
    this.submitted = true;

    if (this.registerForm.invalid) {
      Object.keys(this.registerForm.controls).forEach(key => {
        this.registerForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;

    const formValue = this.registerForm.value;

    // Create ABP RegisterDto
    const registerDto: RegisterDto = {
      userName: formValue.userName,
      emailAddress: formValue.email,
      password: formValue.password,
      appName: 'Angular',
      extraProperties: {
        FirstName: formValue.firstName,
        LastName: formValue.lastName,
        PhoneNumber: formValue.phoneNumber || ''
      }
    };

    // Call ABP Account registration API
    this.accountService.register(registerDto)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: () => {
          this.toasterService.success(
            'Registration successful! You can now log in with your credentials.',
            'Success',
            { life: 5000 }
          );

          // Auto-login after registration
          this.performLogin(formValue.userName, formValue.password);
        },
        error: (error) => {
          const errorMessage = error?.error?.error?.message || 'Registration failed. Please try again.';
          this.toasterService.error(errorMessage, 'Registration Error', { life: 10000 });
          this.loading = false;
        }
      });
  }

  private performLogin(username: string, password: string) {
    this.loading = true;

    this.authService.login({
      username,
      password,
      rememberMe: true
    }).subscribe({
      next: () => {
        this.toasterService.success(
          'Welcome! Your account has been created successfully.',
          'Welcome',
          { life: 3000 }
        );

        // Customer profile is automatically created by CustomerUserCreatedEventHandler
        // on the backend when the user is created
        setTimeout(() => {
          this.router.navigate(['/ecommerce/products']);
        }, 500);
      },
      error: () => {
        this.toasterService.info(
          'Registration successful! Please log in to continue.',
          'Please Log In'
        );
        this.router.navigate(['/ecommerce/login']);
        this.loading = false;
      }
    });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);

    if (!field || !field.errors || !this.isFieldInvalid(fieldName)) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    if (field.errors['email']) {
      return 'Invalid email format';
    }

    if (field.errors['minlength']) {
      const minLength = field.errors['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} must be at least ${minLength} characters`;
    }

    if (field.errors['passwordMismatch']) {
      return 'Passwords do not match';
    }

    return 'Invalid value';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      userName: 'Username',
      firstName: 'First name',
      lastName: 'Last name',
      email: 'Email',
      phoneNumber: 'Phone number',
      password: 'Password',
      confirmPassword: 'Confirm password'
    };
    return labels[fieldName] || fieldName;
  }
}
