import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { AuthService, ConfigStateService } from '@abp/ng.core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container mt-5">
      <div class="row justify-content-center">
        <div class="col-md-6">
          <div class="card shadow">
            <div class="card-body p-5">
              <div class="text-center mb-4">
                <h2>Welcome Back!</h2>
                <p class="text-muted">Sign in to continue shopping</p>
              </div>

              <div class="d-grid gap-3">
                <button
                  class="btn btn-primary btn-lg"
                  (click)="login()">
                  <i class="bi bi-box-arrow-in-right me-2"></i>
                  Sign In
                </button>

                <div class="text-center">
                  <span class="text-muted">Don't have an account?</span>
                  <a [routerLink]="['/ecommerce/register']" class="ms-2 fw-bold">
                    Register Now
                  </a>
                </div>

                <hr class="my-3">

                <div class="text-center">
                  <a [routerLink]="['/ecommerce/products']" class="text-muted">
                    <i class="bi bi-arrow-left me-2"></i>
                    Continue Shopping
                  </a>
                </div>
              </div>
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
  `]
})
export class LoginComponent implements OnInit {
  returnUrl: string = '/ecommerce/products';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private configState: ConfigStateService
  ) {}

  ngOnInit() {
    // Get return URL from query params or default to products
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/ecommerce/products';

    // If user is already logged in, redirect to return URL
    if (this.configState.getOne('currentUser')?.id) {
      this.router.navigate([this.returnUrl]);
    }
  }

  login() {
    // Redirect to ABP's built-in login page
    // Store the return URL so we can redirect after login
    const loginUrl = '/account/login';
    const returnUrl = encodeURIComponent(this.returnUrl);
    this.router.navigateByUrl(`${loginUrl}?returnUrl=${returnUrl}`);
  }
}
