import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { AuthService, ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  loading = false;
  submitted = false;
  returnUrl: string = '/ecommerce/products';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private configState: ConfigStateService,
    private toasterService: ToasterService
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]],
      rememberMe: [true]
    });
  }

  ngOnInit() {
    // Get return URL from query params or default to products
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/ecommerce/products';

    // If user is already logged in, redirect to return URL
    if (this.configState.getOne('currentUser')?.id) {
      this.router.navigate([this.returnUrl]);
    }
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched || this.submitted));
  }

  onSubmit() {
    this.submitted = true;

    if (this.loginForm.invalid) {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.loading = true;

    const { username, password, rememberMe } = this.loginForm.value;

    this.authService.login({
      username,
      password,
      rememberMe,
      redirectUrl: this.returnUrl
    })
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: () => {
          this.toasterService.success(
            'Welcome back! You have successfully logged in.',
            'Login Successful',
            { life: 3000 }
          );

          // Navigate to return URL
          setTimeout(() => {
            this.router.navigate([this.returnUrl]);
          }, 500);
        },
        error: (error) => {
          const errorMessage = error?.error?.error_description ||
                              error?.error?.error?.message ||
                              'Invalid username or password. Please try again.';
          this.toasterService.error(errorMessage, 'Login Failed', { life: 5000 });
        }
      });
  }

  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);

    if (!field || !field.errors || !this.isFieldInvalid(fieldName)) {
      return '';
    }

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }

    return 'Invalid value';
  }

  private getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      username: 'Username',
      password: 'Password'
    };
    return labels[fieldName] || fieldName;
  }
}
