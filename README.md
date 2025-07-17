# TeleHealth Platform - Complete Solution

A comprehensive telehealth platform with subscription-based model, featuring patient portals, provider management, admin controls, real-time communication, and video consultations.

## 🚀 Features

### Patient Portal
- **Beautiful Landing Page**: Modern, attractive design inspired by Hims.com
- **Multi-step Onboarding**: Personal info, medical history, subscription selection, payment setup
- **Dashboard**: Stats, quick actions, appointments, subscriptions, recent activity
- **Appointment Booking**: Easy consultation scheduling with providers
- **Real-time Chat**: Secure messaging with healthcare providers
- **Video Consultations**: Integrated OpenTok video calls
- **Subscription Management**: Plan selection, billing, payment methods
- **Health Records**: Medical history, prescriptions, test results

### Provider Portal
- **Professional Login**: Secure admin access for healthcare providers
- **Registration**: Comprehensive provider onboarding with credentials
- **Dashboard**: Patient management, appointments, earnings, analytics
- **Patient Communication**: Chat, video calls, file sharing
- **Schedule Management**: Availability, appointment scheduling
- **Medical Records**: Patient history, prescriptions, notes
- **Billing**: Consultation fees, payment processing

### Admin Portal
- **Secure Access**: Protected admin panel with role-based permissions
- **User Management**: Patient and provider oversight
- **Platform Analytics**: Usage statistics, revenue tracking
- **Content Management**: Health tips, announcements
- **System Monitoring**: Performance, security, compliance
- **Financial Reports**: Revenue, subscriptions, payments

### Core Features
- **Authentication**: JWT-based secure login/logout
- **Real-time Communication**: SignalR chat and notifications
- **Video Calls**: OpenTok integration for consultations
- **Payment Processing**: Stripe integration for subscriptions
- **Responsive Design**: Mobile-first, accessible UI
- **Security**: HTTPS, data encryption, HIPAA compliance

## 🛠 Technology Stack

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8.0
- **Architecture**: Clean Architecture with CQRS
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT Bearer Tokens
- **Real-time**: SignalR for chat and notifications
- **Video**: OpenTok SDK integration
- **Payment**: Stripe API integration
- **Testing**: xUnit with Moq

### Frontend (Angular 18)
- **Framework**: Angular 18 with TypeScript
- **UI**: Custom SCSS with modern design system
- **State Management**: RxJS with BehaviorSubject
- **HTTP**: Angular HttpClient with interceptors
- **Real-time**: SignalR client for chat
- **Video**: OpenTok client for video calls
- **Forms**: Reactive Forms with validation
- **Routing**: Lazy-loaded feature modules

### Infrastructure
- **Database**: SQL Server with migrations
- **File Storage**: Azure Blob Storage
- **Email**: SendGrid for notifications
- **SMS**: Twilio for appointment reminders
- **Monitoring**: Application Insights
- **Deployment**: Azure App Service

## 📁 Project Structure

```
TeleHealthWithSubscriptionModel/
├── src/
│   ├── SmartTelehealth.API/           # Backend API
│   │   ├── Controllers/               # API endpoints
│   │   ├── Hubs/                     # SignalR hubs
│   │   ├── Migrations/               # Database migrations
│   │   └── wwwroot/                  # Static files
│   ├── SmartTelehealth.Application/   # Business logic
│   │   ├── DTOs/                     # Data transfer objects
│   │   ├── Interfaces/               # Service contracts
│   │   └── Services/                 # Business services
│   ├── SmartTelehealth.Core/         # Domain entities
│   │   ├── Entities/                 # Domain models
│   │   └── Interfaces/               # Repository contracts
│   ├── SmartTelehealth.Infrastructure/ # Data access
│   │   ├── Data/                     # DbContext
│   │   ├── Repositories/             # Repository implementations
│   │   └── Services/                 # External services
│   └── SmartTelehealth.Tests/        # Unit & integration tests
├── telehealth-frontend/               # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                 # Core services & guards
│   │   │   ├── shared/               # Shared components
│   │   │   └── features/             # Feature modules
│   │   │       ├── patient/          # Patient portal
│   │   │       ├── provider/         # Provider portal
│   │   │       └── admin/            # Admin portal
│   │   ├── assets/                   # Static assets
│   │   └── environments/             # Environment configs
└── docs/                             # Documentation
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ and npm
- SQL Server (LocalDB or Azure)
- Azure Storage Account (for file uploads)

### Backend Setup
```bash
# Clone repository
git clone <repository-url>
cd TeleHealthWithSubscriptionModel

# Restore dependencies
dotnet restore

# Update database connection string in appsettings.json
# Run migrations
dotnet ef database update --project src/SmartTelehealth.Infrastructure --startup-project src/SmartTelehealth.API

# Start backend API
dotnet run --project src/SmartTelehealth.API
```

### Frontend Setup
```bash
# Navigate to frontend directory
cd telehealth-frontend

# Install dependencies
npm install

# Start development server
npm start
```

### Environment Configuration

#### Backend (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TeleHealthDB;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-here",
    "Issuer": "TeleHealth",
    "Audience": "TeleHealthUsers",
    "ExpirationHours": 24
  },
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_..."
  },
  "OpenTokSettings": {
    "ApiKey": "your-opentok-api-key",
    "ApiSecret": "your-opentok-api-secret"
  }
}
```

#### Frontend (environment.ts)
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  signalRUrl: 'http://localhost:5000',
  stripePublishableKey: 'pk_test_...',
  openTokApiKey: 'your-opentok-api-key'
};
```

## 🎨 Design System

### Color Palette
- **Primary**: #667eea (Modern Blue)
- **Secondary**: #2c3e50 (Dark Blue)
- **Success**: #28a745 (Green)
- **Warning**: #ffc107 (Yellow)
- **Danger**: #e74c3c (Red)
- **Info**: #17a2b8 (Cyan)

### Typography
- **Primary Font**: 'Inter', sans-serif
- **Secondary Font**: 'Lato', sans-serif
- **Code Font**: 'Fira Code', monospace

### Components
- **Buttons**: Modern, rounded with hover effects
- **Cards**: Clean, shadowed containers
- **Forms**: Floating labels with validation
- **Navigation**: Responsive, accessible menus
- **Modals**: Smooth animations and overlays

## 🔐 Security Features

### Authentication & Authorization
- JWT token-based authentication
- Role-based access control (Patient, Provider, Admin)
- Secure password hashing with BCrypt
- Token refresh mechanism
- Session management

### Data Protection
- HTTPS enforcement
- SQL injection prevention
- XSS protection
- CSRF token validation
- Input sanitization

### HIPAA Compliance
- Data encryption at rest and in transit
- Audit logging for all data access
- Secure file uploads
- Patient data anonymization options
- Access control and authentication

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/logout` - User logout
- `POST /api/auth/refresh` - Token refresh

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Appointments
- `GET /api/appointments` - Get appointments
- `POST /api/appointments` - Create appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Cancel appointment

### Subscriptions
- `GET /api/subscriptions` - Get subscriptions
- `POST /api/subscriptions` - Create subscription
- `PUT /api/subscriptions/{id}` - Update subscription
- `DELETE /api/subscriptions/{id}` - Cancel subscription

### Chat
- `GET /api/chat/conversations` - Get conversations
- `GET /api/chat/messages/{conversationId}` - Get messages
- `POST /api/chat/messages` - Send message

## 🧪 Testing

### Backend Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/SmartTelehealth.Tests/

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests
```bash
# Run unit tests
npm test

# Run e2e tests
npm run e2e

# Run with coverage
npm run test:coverage
```

## 🚀 Deployment

### Backend Deployment (Azure)
```bash
# Publish to Azure
dotnet publish -c Release -o ./publish
az webapp deployment source config-zip --resource-group <rg> --name <app-name> --src ./publish.zip
```

### Frontend Deployment
```bash
# Build for production
npm run build

# Deploy to Azure Static Web Apps
az staticwebapp create --name <app-name> --resource-group <rg> --source .
```

## 📈 Monitoring & Analytics

### Application Insights
- Performance monitoring
- Error tracking
- User analytics
- Custom telemetry

### Health Checks
- Database connectivity
- External service status
- System resources
- Custom health indicators

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- 📧 Email: support@telehealth.com
- 💬 Discord: [TeleHealth Community](https://discord.gg/telehealth)
- 📖 Documentation: [docs.telehealth.com](https://docs.telehealth.com)
- 🐛 Issues: [GitHub Issues](https://github.com/telehealth/issues)

## 🙏 Acknowledgments

- **Design Inspiration**: Hims.com, Calendly, Zoom
- **Icons**: Font Awesome, Material Icons
- **Charts**: Chart.js, D3.js
- **Video**: OpenTok, Twilio Video
- **Payments**: Stripe, PayPal

---

**Built with ❤️ for better healthcare access** 