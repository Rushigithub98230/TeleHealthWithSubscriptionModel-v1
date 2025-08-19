# 🏥 Smart Telehealth Frontend Application

## 📋 Table of Contents
- [Overview](#overview)
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Routing Architecture](#routing-architecture)
- [Development Guidelines](#development-guidelines)
- [API Integration Standards](#api-integration-standards)
- [Admin Portal Structure](#admin-portal-structure)
- [Implementation Plan](#implementation-plan)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)

## 🎯 Overview

Smart Telehealth is a comprehensive subscription-based healthcare application that provides three distinct portals:

- **User Portal** (`/web/*`) - For patients/clients to access healthcare services
- **Provider Portal** (`/web/*`) - For healthcare providers to manage patients and services
- **Admin Portal** (`/webadmin/*`) - For system administrators to manage subscriptions, users, and system settings

## 🏗️ Architecture Overview

### Technology Stack
- **Framework**: Angular 18
- **Styling**: SCSS with responsive design
- **State Management**: RxJS with BehaviorSubject
- **HTTP Client**: Angular HttpClient with interceptors
- **Routing**: Angular Router with lazy loading
- **Authentication**: JWT-based with role-based access control

### Core Principles
- **Modular Architecture**: Feature-based module organization
- **Separation of Concerns**: Clear separation between components, services, and models
- **Reusability**: Shared components and services across portals
- **Scalability**: Lazy loading and efficient routing
- **Security**: Role-based access control and secure authentication

## 📁 Project Structure

```
src/
├── app/
│   ├── core/                           # Core functionality (singletons)
│   │   ├── services/                   # Shared services
│   │   │   ├── common.service.ts       # Centralized API service
│   │   │   ├── auth.service.ts         # Authentication service
│   │   │   ├── loading.service.ts      # Loading state management
│   │   │   └── toast.service.ts        # Notification service
│   │   ├── guards/                     # Route guards
│   │   │   ├── auth.guard.ts           # Authentication guard
│   │   │   └── role.guard.ts           # Role-based access guard
│   │   ├── interceptors/               # HTTP interceptors
│   │   │   ├── auth.interceptor.ts     # JWT token interceptor
│   │   │   ├── error.interceptor.ts    # Error handling interceptor
│   │   │   └── loading.interceptor.ts  # Loading state interceptor
│   │   ├── models/                     # Shared interfaces/types
│   │   │   ├── json-model.interface.ts # API response model
│   │   │   ├── user.interface.ts       # User model
│   │   │   ├── subscription.interface.ts # Subscription model
│   │   │   └── index.ts                # Barrel export
│   │   └── constants/                  # App constants
│   │       ├── app.constants.ts        # Application constants
│   │       └── api-endpoints.ts        # API endpoint constants
│   ├── shared/                         # Shared components/modules
│   │   ├── components/                 # Reusable UI components
│   │   │   ├── loading-spinner/        # Loading spinner component
│   │   │   ├── confirm-dialog/         # Confirmation dialog
│   │   │   ├── pagination/             # Pagination component
│   │   │   ├── search-filter/          # Search and filter component
│   │   │   └── data-table/             # Data table component
│   │   ├── directives/                 # Custom directives
│   │   │   ├── click-outside.directive.ts
│   │   │   └── debounce.directive.ts
│   │   ├── pipes/                      # Custom pipes
│   │   │   ├── format-date.pipe.ts
│   │   │   └── format-currency.pipe.ts
│   │   └── shared.module.ts            # Shared module
│   ├── user-portal/                    # User Portal (Patient/Client)
│   │   ├── components/                 # User-specific components
│   │   │   ├── dashboard/              # User dashboard
│   │   │   ├── profile/                # User profile management
│   │   │   ├── subscriptions/          # User subscription management
│   │   │   ├── appointments/           # Appointment booking/management
│   │   │   ├── consultations/          # Video consultation interface
│   │   │   ├── medications/            # Medication management
│   │   │   ├── health-assessments/     # Health assessment forms
│   │   │   ├── messages/               # Messaging system
│   │   │   ├── notifications/          # Notification center
│   │   │   ├── billing/                # Billing and payment
│   │   │   └── support/                # Support and help
│   │   ├── services/                   # User-specific services
│   │   ├── models/                     # User-specific interfaces
│   │   └── user-portal.module.ts       # User portal module
│   ├── provider-portal/                # Provider Portal (Healthcare)
│   │   ├── components/                 # Provider-specific components
│   │   │   ├── dashboard/              # Provider dashboard
│   │   │   ├── profile/                # Provider profile management
│   │   │   ├── patients/               # Patient management
│   │   │   ├── appointments/           # Appointment management
│   │   │   ├── consultations/          # Consultation management
│   │   │   ├── schedule/               # Schedule management
│   │   │   ├── payouts/                # Payout and earnings
│   │   │   ├── analytics/              # Provider analytics
│   │   │   ├── messages/               # Messaging system
│   │   │   └── notifications/          # Notification center
│   │   ├── services/                   # Provider-specific services
│   │   ├── models/                     # Provider-specific interfaces
│   │   └── provider-portal.module.ts   # Provider portal module
│   ├── admin-portal/                   # Admin Portal (System Admin)
│   │   ├── components/                 # Admin-specific components
│   │   │   ├── dashboard/              # Admin dashboard
│   │   │   ├── subscriptions/          # Subscription management
│   │   │   │   ├── subscription-list/  # List all subscriptions
│   │   │   │   ├── subscription-detail/ # Subscription details
│   │   │   │   └── subscription-form/  # Create/edit subscriptions
│   │   │   ├── subscription-plans/     # Subscription plan management
│   │   │   │   ├── plan-list/          # List all plans
│   │   │   │   ├── plan-create/        # Create new plan
│   │   │   │   ├── plan-edit/          # Edit existing plan
│   │   │   │   └── plan-detail/        # Plan details
│   │   │   ├── users/                  # User management
│   │   │   │   ├── user-list/          # List all users
│   │   │   │   └── user-detail/        # User details
│   │   │   ├── providers/              # Provider management
│   │   │   │   ├── provider-list/      # List all providers
│   │   │   │   └── provider-detail/    # Provider details
│   │   │   ├── billing/                # Billing management
│   │   │   ├── analytics/              # System analytics
│   │   │   ├── reports/                # Report generation
│   │   │   ├── settings/               # System settings
│   │   │   ├── privileges/             # Privilege management
│   │   │   └── audit-logs/             # Audit log management
│   │   ├── services/                   # Admin-specific services
│   │   ├── models/                     # Admin-specific interfaces
│   │   └── admin-portal.module.ts      # Admin portal module
│   ├── app.component.ts                # Root component
│   ├── app.module.ts                   # Root module
│   ├── app-routing.module.ts           # Root routing
│   └── app.config.ts                   # App configuration
├── assets/                             # Static assets
│   ├── images/                         # Application images
│   ├── icons/                          # Icon sets
│   └── styles/                         # Global styles
├── environments/                       # Environment configurations
│   ├── environment.ts                  # Development environment
│   └── environment.prod.ts             # Production environment
└── styles/                             # Global styles
    ├── _variables.scss                 # SCSS variables
    ├── _mixins.scss                    # SCSS mixins
    ├── _utilities.scss                 # Utility classes
    └── main.scss                       # Main stylesheet
```

## 🛣️ Routing Architecture

### Root App Routing
```typescript
const routes: Routes = [
  { path: '', redirectTo: '/web/login', pathMatch: 'full' },
  
  // User & Provider Portal Routes (/web/*)
  {
    path: 'web',
    loadChildren: () => import('./user-portal/user-portal.module').then(m => m.UserPortalModule),
    canActivate: [AuthGuard],
    data: { roles: ['User', 'Provider'] }
  },
  
  // Admin Portal Routes (/webadmin/*)
  {
    path: 'webadmin',
    loadChildren: () => import('./admin-portal/admin-portal.module').then(m => m.AdminPortalModule),
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  
  { path: '**', redirectTo: '/web/login' }
];
```

### URL Structure Examples

#### User & Provider Portal (`/web/*`)
- `/web/login` - Login page
- `/web/dashboard` - User/Provider dashboard
- `/web/subscriptions` - Subscription management
- `/web/appointments` - Appointment booking
- `/web/consultations` - Video consultations
- `/web/profile` - Profile management
- `/web/billing` - Billing and payments

#### Admin Portal (`/webadmin/*`)
- `/webadmin/login` - Admin login
- `/webadmin/dashboard` - Admin dashboard
- `/webadmin/subscriptions` - Subscription management
- `/webadmin/subscription-plans` - Plan management
- `/webadmin/users` - User management
- `/webadmin/providers` - Provider management
- `/webadmin/analytics` - System analytics
- `/webadmin/settings` - System settings

## 📚 Development Guidelines

### 1. API Call Standards

**✅ Always use a centralized API service layer (Common.service.ts) for all HTTP calls.**

```typescript
// ❌ WRONG - Direct HttpClient usage in component
constructor(private http: HttpClient) {}

ngOnInit() {
  this.http.get('/api/subscriptions').subscribe(...);
}

// ✅ CORRECT - Use CommonService
constructor(private subscriptionService: SubscriptionService) {}

ngOnInit() {
  this.subscriptionService.getAllSubscriptions().subscribe(...);
}
```

**✅ Never call HttpClient directly inside components.**

**✅ Define base URL in environment.ts (environment.apiUrl).**

```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api',
  appName: 'Smart Telehealth'
};

// services/common.service.ts
private readonly baseUrl = environment.apiUrl;
```

**✅ Use interceptors for authentication (JWT Token), error handling, and loading indicators.**

### 2. Request Handling

**✅ Always send requests through a service, not directly from a component.**

**✅ Use typed request models (e.g., LoginRequest, AppointmentRequest).**

```typescript
// models/login-request.interface.ts
export interface LoginRequest {
  email: string;
  password: string;
  rememberMe?: boolean;
}

// services/auth.service.ts
login(credentials: LoginRequest): Observable<LoginResponse> {
  return this.commonService.post<LoginResponse>('/auth/login', credentials);
}
```

**✅ Ensure correct HTTP methods (GET, POST, PUT, DELETE) are used.**

**✅ Pass query params using HttpParams, not string concatenation.**

```typescript
// ❌ WRONG - String concatenation
const url = `/api/subscriptions?page=${page}&size=${size}`;

// ✅ CORRECT - Use HttpParams
const params = new HttpParams()
  .set('page', page.toString())
  .set('size', size.toString());

this.commonService.get<Subscription[]>('/api/subscriptions', params);
```

### 3. Response Handling

**✅ Use a generic response model (JsonModel → { data, message, statusCode }).**

```typescript
// models/json-model.interface.ts
export interface JsonModel<T> {
  data: T;
  message: string;
  statusCode: number;
}

// services/common.service.ts
private handleResponse<T>(response: JsonModel<T>): T {
  if (response.statusCode >= 200 && response.statusCode < 300) {
    return response.data;
  } else {
    throw new Error(response.message || 'API request failed');
  }
}
```

**✅ Always map API response to a strongly typed interface.**

**✅ Check for statusCode before using data.**

**✅ Show error messages from backend (message) in UI.**

```typescript
this.subscriptionService.getAllSubscriptions().subscribe({
  next: (subscriptions) => {
    this.subscriptions = subscriptions;
  },
  error: (error) => {
    this.toastService.showError(error.message || 'Failed to load subscriptions');
  }
});
```

### 4. Error Handling

**✅ Use a global error handler interceptor to catch 401, 403, 500 errors.**

**✅ Redirect to /web/login or /webadmin/login if token expired (401).**

**✅ Show toast/snackbar for user-friendly error messages.**

**✅ Log detailed error only in dev mode (not production).**

```typescript
// interceptors/error.interceptor.ts
private handleError(error: HttpErrorResponse): void {
  switch (error.status) {
    case 401: // Unauthorized
      this.handleUnauthorized();
      break;
    case 403: // Forbidden
      this.handleForbidden();
      break;
    case 500: // Internal Server Error
      this.handleServerError();
      break;
    default:
      this.handleGenericError(error);
  }
}

private handleUnauthorized(): void {
  const currentRoute = this.router.url;
  if (currentRoute.startsWith('/webadmin')) {
    this.router.navigate(['/webadmin/login']);
  } else {
    this.router.navigate(['/web/login']);
  }
  this.toastService.showError('Session expired. Please login again.');
}
```

### 5. Authentication & Security

**✅ Store token in HttpOnly Cookie (preferred).**

**✅ Never hardcode secrets or API keys in frontend.**

**✅ Attach Authorization header in every secured request via interceptor.**

```typescript
// interceptors/auth.interceptor.ts
intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
  const currentUser = this.authService.getCurrentUser();
  
  if (currentUser && currentUser.token) {
    request = request.clone({
      setHeaders: {
        Authorization: `Bearer ${currentUser.token}`
      }
    });
  }

  return next.handle(request);
}
```

**✅ Validate role-based access (User, Provider, Admin) on routing guards.**

```typescript
// guards/role.guard.ts
canActivate(route: ActivatedRouteSnapshot): boolean {
  const requiredRoles = route.data['roles'] as string[];
  
  if (!requiredRoles || requiredRoles.length === 0) {
    return true;
  }

  if (this.authService.hasAnyRole(requiredRoles)) {
    return true;
  }

  this.router.navigate(['/unauthorized']);
  return false;
}
```

### 6. Code & Performance

**✅ Use async/await or RxJS operators (pipe, map, catchError) instead of nested .subscribe().**

```typescript
// ❌ WRONG - Nested subscriptions
this.authService.login(credentials).subscribe(response => {
  this.userService.getUserProfile().subscribe(profile => {
    this.router.navigate(['/dashboard']);
  });
});

// ✅ CORRECT - Use RxJS operators
this.authService.login(credentials).pipe(
  switchMap(response => this.userService.getUserProfile()),
  tap(profile => this.router.navigate(['/dashboard']))
).subscribe();
```

**✅ Cancel API subscriptions on component destroy (takeUntil).**

```typescript
export class SubscriptionListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  ngOnInit() {
    this.subscriptionService.getAllSubscriptions().pipe(
      takeUntil(this.destroy$)
    ).subscribe(subscriptions => {
      this.subscriptions = subscriptions;
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
```

**✅ Implement loading spinners for API calls.**

**✅ Paginate API calls when handling large datasets (e.g., notifications, patients).**

### 7. Best Practices

**✅ Keep services reusable (don't duplicate API calls).**

**✅ Use environment variables for switching between dev, staging, and prod.**

## 🏛️ Admin Portal Structure

### Core Sections

#### 1. Dashboard (`/webadmin/dashboard`)
- System overview and key metrics
- Recent activities and alerts
- Quick action buttons
- Revenue and subscription statistics

#### 2. Subscription Management (`/webadmin/subscriptions`)
- **List View**: All subscriptions with filtering and search
- **Detail View**: Individual subscription details
- **Actions**: Create, edit, cancel, pause, resume subscriptions
- **Bulk Operations**: Bulk actions for multiple subscriptions

#### 3. Subscription Plans (`/webadmin/subscription-plans`)
- **Plan List**: All available subscription plans
- **Plan Creation**: Create new subscription plans
- **Plan Editing**: Modify existing plans
- **Plan Details**: Comprehensive plan information
- **Privilege Management**: Manage plan features and limits

#### 4. User Management (`/webadmin/users`)
- **User List**: All registered users
- **User Details**: Individual user information
- **User Actions**: Activate, deactivate, edit users
- **Role Management**: Assign and modify user roles

#### 5. Provider Management (`/webadmin/providers`)
- **Provider List**: All healthcare providers
- **Provider Details**: Provider information and credentials
- **Provider Actions**: Approve, reject, suspend providers
- **Onboarding**: Manage provider registration process

#### 6. Billing Management (`/webadmin/billing`)
- **Billing Records**: All billing transactions
- **Payment Processing**: Manual payment processing
- **Refund Management**: Handle refunds and adjustments
- **Financial Reports**: Revenue and billing analytics

#### 7. Analytics (`/webadmin/analytics`)
- **Subscription Analytics**: Growth, churn, retention metrics
- **Revenue Analytics**: Revenue trends and projections
- **User Analytics**: User behavior and engagement
- **Provider Analytics**: Provider performance metrics

#### 8. System Settings (`/webadmin/settings`)
- **General Settings**: Application configuration
- **Privilege Management**: Define and manage system privileges
- **Audit Logs**: System activity and security logs
- **Integration Settings**: Third-party service configurations

### Required Controllers

#### Subscription Management Controllers
- `AdminSubscriptionsComponent` - Main subscription management
- `SubscriptionListComponent` - List and filter subscriptions
- `SubscriptionDetailComponent` - View subscription details
- `SubscriptionFormComponent` - Create/edit subscriptions
- `SubscriptionActionsComponent` - Bulk operations

#### Subscription Plan Controllers
- `AdminSubscriptionPlansComponent` - Plan management overview
- `PlanListComponent` - List all plans
- `PlanCreateComponent` - Create new plan
- `PlanEditComponent` - Edit existing plan
- `PlanDetailComponent` - Plan details and privileges

#### User Management Controllers
- `AdminUsersComponent` - User management overview
- `UserListComponent` - List and filter users
- `UserDetailComponent` - User details and actions
- `UserFormComponent` - Create/edit users

#### Provider Management Controllers
- `AdminProvidersComponent` - Provider management overview
- `ProviderListComponent` - List and filter providers
- `ProviderDetailComponent` - Provider details and actions
- `ProviderOnboardingComponent` - Manage onboarding process

#### Analytics Controllers
- `AdminAnalyticsComponent` - Analytics dashboard
- `SubscriptionAnalyticsComponent` - Subscription metrics
- `RevenueAnalyticsComponent` - Revenue analysis
- `UserAnalyticsComponent` - User behavior analytics

## 📋 Implementation Plan

### Phase 1: Foundation (Week 1-2)
- [x] Set up Angular 18 project structure
- [ ] Implement core services (CommonService, AuthService)
- [ ] Set up routing architecture
- [ ] Implement interceptors and guards
- [ ] Create shared components

### Phase 2: Admin Portal - Core (Week 3-4)
- [ ] Admin portal layout and navigation
- [ ] Dashboard component
- [ ] Basic subscription management
- [ ] User authentication and authorization

### Phase 3: Admin Portal - Subscription Management (Week 5-6)
- [ ] Subscription CRUD operations
- [ ] Subscription plan management
- [ ] Billing and payment integration
- [ ] Analytics and reporting

### Phase 4: User & Provider Portals (Week 7-8)
- [ ] User portal basic structure
- [ ] Provider portal basic structure
- [ ] Cross-portal authentication
- [ ] Role-based access control

### Phase 5: Integration & Testing (Week 9-10)
- [ ] Backend API integration
- [ ] End-to-end testing
- [ ] Performance optimization
- [ ] Security hardening

## 🚀 Getting Started

### Prerequisites
- Node.js 18+ and npm
- Angular CLI 18+
- Backend API running (Smart Telehealth Backend)

### Installation
```bash
# Clone the repository
git clone <repository-url>
cd smart-telehealth-frontend

# Install dependencies
npm install

# Start development server
npm start

# Build for production
npm run build
```

### Environment Configuration
1. Copy `environments/environment.ts.example` to `environments/environment.ts`
2. Update `apiUrl` to point to your backend API
3. Configure other environment variables as needed

### Development Commands
```bash
# Start development server
npm start

# Build application
npm run build

# Run tests
npm test

# Run linting
npm run lint

# Generate component
ng generate component components/component-name

# Generate service
ng generate service services/service-name
```

## 🔄 Development Workflow

### 1. Feature Development
1. Create feature branch from `develop`
2. Implement feature following guidelines
3. Write unit tests
4. Update documentation
5. Create pull request

### 2. Code Review Process
1. Self-review before submitting PR
2. Peer review by team member
3. Address feedback and comments
4. Merge after approval

### 3. Testing Strategy
- **Unit Tests**: Component and service testing
- **Integration Tests**: API integration testing
- **E2E Tests**: User workflow testing
- **Performance Tests**: Load and stress testing

### 4. Deployment Pipeline
1. **Development**: Local development and testing
2. **Staging**: Integration testing and QA
3. **Production**: Live deployment with monitoring

## 📱 Responsive Design

### Breakpoints
- **Mobile**: < 768px
- **Tablet**: 768px - 1024px
- **Desktop**: > 1024px

### Design Principles
- Mobile-first approach
- Touch-friendly interfaces
- Consistent spacing and typography
- Accessible color schemes

## 🔒 Security Considerations

### Authentication
- JWT token-based authentication
- Secure token storage (HttpOnly cookies)
- Automatic token refresh
- Session timeout handling

### Authorization
- Role-based access control (RBAC)
- Route-level security
- Component-level permissions
- API endpoint protection

### Data Protection
- Input validation and sanitization
- XSS protection
- CSRF protection
- Secure communication (HTTPS)

## 📊 Performance Optimization

### Lazy Loading
- Feature modules loaded on demand
- Route-based code splitting
- Preloading strategies

### Caching
- HTTP response caching
- Local storage for user preferences
- Service worker for offline support

### Bundle Optimization
- Tree shaking
- Code splitting
- Asset optimization
- Gzip compression

## 🧪 Testing Strategy

### Unit Testing
- Component testing with Angular TestBed
- Service testing with mocks
- Guard and interceptor testing
- Model validation testing

### Integration Testing
- API integration testing
- Route testing
- Authentication flow testing
- Error handling testing

### E2E Testing
- User workflow testing
- Cross-browser testing
- Performance testing
- Accessibility testing

## 📚 Additional Resources

### Documentation
- [Angular Official Documentation](https://angular.io/docs)
- [Angular Material](https://material.angular.io/)
- [RxJS Documentation](https://rxjs.dev/)

### Tools
- [Angular DevTools](https://angular.io/dev-tools)
- [Angular Language Service](https://angular.io/guide/language-service)
- [Angular CLI](https://cli.angular.io/)

### Best Practices
- [Angular Style Guide](https://angular.io/guide/styleguide)
- [Angular Architecture Patterns](https://angular.io/guide/architecture)
- [Angular Security](https://angular.io/guide/security)

---

## 🤝 Contributing

Please read our [Contributing Guidelines](CONTRIBUTING.md) before submitting pull requests.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 📞 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

---

**Last Updated**: August 2025  
**Version**: 1.0.0  
**Angular Version**: 18.x
