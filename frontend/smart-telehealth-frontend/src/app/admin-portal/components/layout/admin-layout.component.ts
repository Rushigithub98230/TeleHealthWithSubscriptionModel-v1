import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, NavigationEnd } from '@angular/router';
import { Subject, takeUntil, filter } from 'rxjs';
import { AdminAuthService } from '../../services/admin-auth.service';

interface NavigationItem {
  title: string;
  route: string;
  icon: string;
  badge?: number;
  children?: NavigationItem[];
  isActive?: boolean;
}

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="admin-layout" [class.sidebar-collapsed]="sidebarCollapsed">
      <!-- Sidebar -->
      <aside class="admin-sidebar" [class.collapsed]="sidebarCollapsed">
        <div class="sidebar-header">
          <div class="logo">
            <i class="fas fa-heartbeat"></i>
            <span *ngIf="!sidebarCollapsed">Smart Telehealth</span>
          </div>
          <button 
            class="sidebar-toggle" 
            (click)="toggleSidebar()"
            [attr.aria-label]="sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
          >
            <i class="fas fa-bars"></i>
          </button>
        </div>

        <nav class="sidebar-nav">
          <ul class="nav-list">
            <li *ngFor="let item of navigationItems" class="nav-item">
              <ng-container [ngSwitch]="!!item.children">
                <!-- Single navigation item -->
                <a 
                  *ngSwitchCase="false"
                  [routerLink]="item.route" 
                  routerLinkActive="active"
                  class="nav-link"
                  [class.has-badge]="item.badge"
                >
                  <i [class]="item.icon"></i>
                  <span *ngIf="!sidebarCollapsed" class="nav-text">{{ item.title }}</span>
                  <span *ngIf="item.badge && !sidebarCollapsed" class="badge">{{ item.badge }}</span>
                </a>

                <!-- Navigation item with children -->
                <div *ngSwitchCase="true" class="nav-item-with-children">
                  <button 
                    class="nav-link nav-link-toggle"
                    (click)="toggleSubmenu(item)"
                    [class.expanded]="item.isActive"
                  >
                    <i [class]="item.icon"></i>
                    <span *ngIf="!sidebarCollapsed" class="nav-text">{{ item.title }}</span>
                    <i *ngIf="!sidebarCollapsed" class="fas fa-chevron-down submenu-arrow"></i>
                  </button>
                  
                  <ul *ngIf="item.isActive && !sidebarCollapsed" class="submenu">
                    <li *ngFor="let child of item.children" class="submenu-item">
                      <a 
                        [routerLink]="child.route" 
                        routerLinkActive="active"
                        class="submenu-link"
                        [class.has-badge]="child.badge"
                      >
                        <i [class]="child.icon"></i>
                        <span class="submenu-text">{{ child.title }}</span>
                        <span *ngIf="child.badge" class="badge">{{ child.badge }}</span>
                      </a>
                    </li>
                  </ul>
                </div>
              </ng-container>
            </li>
          </ul>
        </nav>

        <div class="sidebar-footer" *ngIf="!sidebarCollapsed">
          <div class="user-info">
            <div class="user-avatar">
              <i class="fas fa-user"></i>
            </div>
            <div class="user-details">
              <span class="user-name">{{ currentUser?.firstName }} {{ currentUser?.lastName }}</span>
              <span class="user-role">{{ currentUser?.role }}</span>
            </div>
          </div>
          <button class="logout-btn" (click)="logout()">
            <i class="fas fa-sign-out-alt"></i>
            <span>Logout</span>
          </button>
        </div>
      </aside>

      <!-- Main content area -->
      <main class="admin-main">
        <!-- Header -->
        <header class="admin-header">
          <div class="header-left">
            <button 
              class="mobile-menu-toggle"
              (click)="toggleSidebar()"
              [attr.aria-label]="sidebarCollapsed ? 'Expand sidebar' : 'Collapse sidebar'"
            >
              <i class="fas fa-bars"></i>
            </button>
            <div class="breadcrumb">
              <span *ngFor="let crumb of breadcrumbs; let last = last" class="breadcrumb-item">
                <a *ngIf="!last" [routerLink]="crumb.route">{{ crumb.title }}</a>
                <span *ngIf="last">{{ crumb.title }}</span>
                <i *ngIf="!last" class="fas fa-chevron-right"></i>
              </span>
            </div>
          </div>

          <div class="header-right">
            <div class="header-actions">
              <button class="action-btn" (click)="toggleNotifications()" [class.active]="notificationsOpen">
                <i class="fas fa-bell"></i>
                <span *ngIf="notificationCount > 0" class="notification-badge">{{ notificationCount }}</span>
              </button>
              
              <button class="action-btn" (click)="toggleUserMenu()" [class.active]="userMenuOpen">
                <i class="fas fa-user-circle"></i>
              </button>
            </div>

            <!-- Notifications dropdown -->
            <div *ngIf="notificationsOpen" class="notifications-dropdown">
              <div class="dropdown-header">
                <h3>Notifications</h3>
                <button (click)="markAllAsRead()">Mark all as read</button>
              </div>
              <div class="notifications-list">
                <div *ngFor="let notification of notifications" class="notification-item" [class.unread]="!notification.read">
                  <div class="notification-icon">
                    <i [class]="notification.icon"></i>
                  </div>
                  <div class="notification-content">
                    <p class="notification-text">{{ notification.message }}</p>
                    <span class="notification-time">{{ notification.time | date:'short' }}</span>
                  </div>
                </div>
                <div *ngIf="notifications.length === 0" class="no-notifications">
                  <p>No new notifications</p>
                </div>
              </div>
            </div>

            <!-- User menu dropdown -->
            <div *ngIf="userMenuOpen" class="user-menu-dropdown">
              <div class="user-menu-header">
                <div class="user-avatar">
                  <i class="fas fa-user"></i>
                </div>
                <div class="user-info">
                  <span class="user-name">{{ currentUser?.firstName }} {{ currentUser?.lastName }}</span>
                  <span class="user-email">{{ currentUser?.email }}</span>
                </div>
              </div>
                             <ul class="user-menu-list">
                 <li><a routerLink="profile"><i class="fas fa-user"></i> Profile</a></li>
                 <li><a routerLink="settings"><i class="fas fa-cog"></i> Settings</a></li>
                 <li><a routerLink="help"><i class="fas fa-question-circle"></i> Help</a></li>
                 <li class="divider"></li>
                 <li><button (click)="logout()"><i class="fas fa-sign-out-alt"></i> Logout</button></li>
               </ul>
            </div>
          </div>
        </header>

        <!-- Page content -->
        <div class="page-content">
          <router-outlet></router-outlet>
        </div>
      </main>

      <!-- Overlay for mobile -->
      <div 
        *ngIf="sidebarCollapsed && isMobile" 
        class="sidebar-overlay"
        (click)="closeSidebar()"
      ></div>
    </div>
  `,
  styleUrls: ['./admin-layout.component.scss']
})
export class AdminLayoutComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  sidebarCollapsed = false;
  isMobile = false;
  notificationsOpen = false;
  userMenuOpen = false;
  currentUser: any = null;
  notificationCount = 0;
  breadcrumbs: Array<{ title: string; route: string }> = [];

  navigationItems: NavigationItem[] = [
    {
      title: 'Dashboard',
      route: 'dashboard',
      icon: 'fas fa-tachometer-alt'
    },
    {
      title: 'Subscriptions',
      route: 'subscriptions',
      icon: 'fas fa-list',
      badge: 0
    },
    {
      title: 'Subscription Plans',
      route: 'subscription-plans',
      icon: 'fas fa-cog',
      children: [
        {
          title: 'All Plans',
          route: 'subscription-plans',
          icon: 'fas fa-list'
        },
        {
          title: 'Create Plan',
          route: 'subscription-plans/create',
          icon: 'fas fa-plus'
        }
      ]
    },
    {
      title: 'Users',
      route: 'users',
      icon: 'fas fa-users'
    },
    {
      title: 'Providers',
      route: 'providers',
      icon: 'fas fa-user-md'
    },
    {
      title: 'Billing & Payments',
      route: 'billing',
      icon: 'fas fa-credit-card',
      badge: 0
    },
    {
      title: 'Analytics',
      route: 'analytics',
      icon: 'fas fa-chart-bar'
    },
    {
      title: 'Reports',
      route: 'reports',
      icon: 'fas fa-file-alt'
    },
    {
      title: 'Privileges',
      route: 'privileges',
      icon: 'fas fa-key'
    },
    {
      title: 'Settings',
      route: 'settings',
      icon: 'fas fa-cog'
    },
    {
      title: 'Audit Logs',
      route: 'audit-logs',
      icon: 'fas fa-history'
    }
  ];

  notifications: any[] = [
    {
      id: 1,
      message: 'New subscription created for John Doe',
      time: new Date(),
      read: false,
      icon: 'fas fa-plus'
    },
    {
      id: 2,
      message: 'Payment failed for subscription #12345',
      time: new Date(Date.now() - 1000 * 60 * 30),
      read: false,
      icon: 'fas fa-exclamation-triangle'
    }
  ];

  constructor(
    private router: Router,
    private authService: AdminAuthService
  ) {
    this.checkScreenSize();
    this.setupRouterEvents();
  }

  ngOnInit(): void {
    this.loadCurrentUser();
    this.loadNotifications();
    this.setupResizeListener();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupRouterEvents(): void {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.updateBreadcrumbs();
      this.updateActiveNavigation();
    });
  }

  private setupResizeListener(): void {
    window.addEventListener('resize', () => {
      this.checkScreenSize();
    });
  }

  private checkScreenSize(): void {
    this.isMobile = window.innerWidth < 768;
    if (this.isMobile) {
      this.sidebarCollapsed = true;
    }
  }

  private loadCurrentUser(): void {
    // TODO: Implement when auth service is ready
    this.currentUser = {
      firstName: 'Admin',
      lastName: 'User',
      email: 'admin@smarttelehealth.com',
      role: 'Administrator'
    };
  }

  private loadNotifications(): void {
    // TODO: Implement when notification service is ready
    this.notificationCount = this.notifications.filter(n => !n.read).length;
  }

  private updateBreadcrumbs(): void {
    const url = this.router.url;
    const segments = url.split('/').filter(segment => segment);
    
    this.breadcrumbs = segments.map((segment, index) => {
      const route = '/' + segments.slice(0, index + 1).join('/');
      const title = this.formatBreadcrumbTitle(segment);
      return { title, route };
    });
  }

  private formatBreadcrumbTitle(segment: string): string {
    return segment
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  private updateActiveNavigation(): void {
    const currentRoute = this.router.url;
    
    this.navigationItems.forEach(item => {
      if (item.children) {
        item.isActive = item.children.some(child => 
          currentRoute.startsWith(child.route)
        );
      }
    });
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  closeSidebar(): void {
    if (this.isMobile) {
      this.sidebarCollapsed = true;
    }
  }

  toggleSubmenu(item: NavigationItem): void {
    if (item.children) {
      item.isActive = !item.isActive;
    }
  }

  toggleNotifications(): void {
    this.notificationsOpen = !this.notificationsOpen;
    this.userMenuOpen = false;
  }

  toggleUserMenu(): void {
    this.userMenuOpen = !this.userMenuOpen;
    this.notificationsOpen = false;
  }

  markAllAsRead(): void {
    this.notifications.forEach(notification => notification.read = true);
    this.notificationCount = 0;
  }

  logout(): void {
    // TODO: Implement when auth service is ready
    this.router.navigate(['/webadmin/login']);
  }
}
