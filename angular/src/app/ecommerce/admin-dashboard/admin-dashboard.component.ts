import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../proxy/dashboard/dashboard.service';
import { DashboardStatsDto } from '../../proxy/dashboard/models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  stats: DashboardStatsDto | null = null;
  loading = true;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading = true;
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading dashboard stats:', err);
        this.loading = false;
      }
    });
  }

  getStatusColor(status: string): string {
    const colorMap: { [key: string]: string } = {
      'Pending': 'warning',
      'Confirmed': 'info',
      'Processing': 'primary',
      'Shipped': 'success',
      'Delivered': 'success',
      'Cancelled': 'danger'
    };
    return colorMap[status] || 'secondary';
  }

  getGrowthClass(growth: number): string {
    return growth >= 0 ? 'text-success' : 'text-danger';
  }

  getGrowthIcon(growth: number): string {
    return growth >= 0 ? '↑' : '↓';
  }
}
