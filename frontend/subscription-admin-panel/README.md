# Subscription Management Admin Panel

A comprehensive Angular-based admin panel for managing subscription plans, user subscriptions, billing, and payment processing with Stripe integration.

## 🚀 Features

### Admin Dashboard
- **Real-time Statistics**: Total subscriptions, active subscriptions, revenue metrics
- **Quick Actions**: Create subscriptions, manage plans, trigger billing
- **Subscription Management**: View, pause, resume, cancel subscriptions
- **Plan Management**: Create, edit, delete subscription plans
- **Payment Processing**: Process payments, handle failed payments
- **Analytics**: Revenue analytics, usage statistics, growth metrics

### Authentication & Security
- **JWT Token Management**: Secure authentication with refresh tokens
- **Role-based Access**: Admin and SuperAdmin roles
- **Session Management**: Automatic token refresh and logout

### Stripe Integration
- **Payment Processing**: Secure payment processing through Stripe
- **Webhook Handling**: Real-time payment status updates
- **Subscription Management**: Automated billing and renewal
- **Payment Methods**: Add, remove, and manage payment methods

## 🛠️ Technology Stack

- **Frontend**: Angular 17+ with TypeScript
- **UI Framework**: Angular Material
- **HTTP Client**: Angular HttpClient with interceptors
- **State Management**: RxJS Observables
- **Payment Processing**: Stripe.js
- **Styling**: SCSS with responsive design

## 📋 Prerequisites

- Node.js (v18 or higher)
- npm or yarn
- Angular CLI
- Backend API running on localhost:5000

## 🚀 Quick Start

### 1. Install Dependencies
```bash
npm install
```

### 2. Configure Environment
Update `src/environments/environment.ts`:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000', // Your backend API URL
  stripePublishableKey: 'pk_test_your_stripe_key_here'
};
```

### 3. Start Development Server
```bash
ng serve
```

The application will be available at `http://localhost:4200`

## 🔐 Default Admin Credentials

For testing purposes, use these credentials:
- **Email**: admin@test.com
- **Password**: Admin123!

## 📊 Dashboard Features

### Statistics Overview
- Total Subscriptions
- Active Subscriptions
- Total Revenue
- Monthly Recurring Revenue (MRR)
- Total Users
- Pending Payments

### Quick Actions
- **Create Subscription**: Add new user subscriptions
- **Create Plan**: Create new subscription plans
- **Trigger Billing**: Manually trigger billing cycles
- **View Analytics**: Access detailed analytics

### Subscription Management
- View all subscriptions in a data table
- Filter by status (Active, Paused, Cancelled, Trial)
- Perform actions: Pause, Resume, Cancel, Upgrade
- View subscription details and billing history

### Plan Management
- Create new subscription plans
- Set pricing, trial periods, billing cycles
- Edit existing plans
- Delete inactive plans

## 🔧 API Integration

The frontend is designed to work with the SmartTelehealth backend API. Key endpoints:

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Token refresh

### Dashboard
- `GET /api/admin/dashboard` - Dashboard statistics

### Subscriptions
- `GET /api/subscription-management/subscriptions` - Get all subscriptions
- `POST /api/subscriptions` - Create subscription
- `POST /api/subscriptions/{id}/cancel` - Cancel subscription
- `POST /api/subscriptions/{id}/pause` - Pause subscription
- `POST /api/subscriptions/{id}/resume` - Resume subscription

### Plans
- `GET /api/subscription-management/plans` - Get all plans
- `POST /api/subscription-management/plans` - Create plan
- `PUT /api/subscription-management/plans/{id}` - Update plan
- `DELETE /api/subscription-management/plans/{id}` - Delete plan

### Billing
- `GET /api/billing/records` - Get billing records
- `POST /api/billing/{id}/process-payment` - Process payment
- `POST /api/payments/retry-payment/{id}` - Retry failed payment

### Analytics
- `GET /api/subscription-analytics/revenue` - Revenue analytics
- `GET /api/subscriptions/{id}/usage` - Usage statistics

## 🎨 UI Components

### Material Design Components
- **Cards**: Statistics cards, plan cards
- **Tables**: Subscription data table with sorting and filtering
- **Forms**: Login form, subscription creation forms
- **Buttons**: Action buttons with icons
- **Menus**: Dropdown menus for actions
- **Progress Indicators**: Loading spinners

### Responsive Design
- Mobile-first approach
- Responsive grid layouts
- Touch-friendly interfaces
- Adaptive navigation

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **HTTP Interceptors**: Automatic token injection
- **Route Guards**: Protected routes for admin access
- **Error Handling**: Comprehensive error handling and user feedback

## 📱 Mobile Support

- Responsive design for all screen sizes
- Touch-friendly interface elements
- Mobile-optimized navigation
- Progressive Web App capabilities

## 🚀 Deployment

### Build for Production
```bash
ng build --configuration production
```

### Environment Configuration
Update `src/environments/environment.prod.ts`:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api-url.com',
  stripePublishableKey: 'pk_live_your_stripe_key_here'
};
```

## 🧪 Testing

### Unit Tests
```bash
ng test
```

### E2E Tests
```bash
ng e2e
```

## 📝 Development Notes

### Project Structure
```
src/
├── app/
│   ├── components/
│   │   ├── dashboard/
│   │   └── login/
│   ├── models/
│   ├── services/
│   └── app.component.ts
├── environments/
└── assets/
```

### Key Services
- **AuthService**: Authentication and token management
- **SubscriptionService**: Subscription and plan management
- **HTTP Interceptors**: Automatic token handling

### State Management
- RxJS Observables for reactive state
- BehaviorSubject for user authentication state
- Local storage for token persistence

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🆘 Support

For support and questions:
- Check the documentation
- Review the API integration guide
- Contact the development team

---

**Built with ❤️ using Angular and Material Design**
