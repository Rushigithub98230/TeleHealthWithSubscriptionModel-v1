# Smart TeleHealth Backend API

A comprehensive .NET 8 backend API for a telehealth platform with subscription management, billing, payment processing, and healthcare services integration.

## 🚀 Features

### Subscription Management
- **Subscription Plans**: Create, manage, and configure subscription plans
- **User Subscriptions**: Handle user subscription lifecycle (create, pause, resume, cancel)
- **Billing Automation**: Automated billing cycles and invoice generation
- **Payment Processing**: Stripe integration for secure payment processing
- **Analytics**: Revenue analytics, usage statistics, growth metrics

### Healthcare Services
- **Provider Management**: Healthcare provider onboarding and management
- **Appointment System**: Appointment scheduling and management
- **Chat & Messaging**: Real-time messaging between patients and providers
- **Video Calling**: Integrated video consultation capabilities
- **Document Management**: Secure document storage and retrieval

### Authentication & Security
- **JWT Authentication**: Secure token-based authentication
- **Role-based Access**: Patient, Provider, Admin, and SuperAdmin roles
- **Multi-factor Authentication**: Enhanced security features
- **Audit Logging**: Comprehensive audit trail for all operations

## 🛠️ Technology Stack

- **Framework**: .NET 8 Web API
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT with custom identity system
- **Payment Processing**: Stripe.NET
- **Real-time Communication**: SignalR for chat and video calls
- **Documentation**: Swagger/OpenAPI
- **Testing**: xUnit with comprehensive test coverage
- **Architecture**: Clean Architecture with CQRS pattern

## 📋 Prerequisites

- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022 or VS Code
- Stripe Account (for payment processing)

## 🚀 Quick Start

### 1. Clone and Setup
```bash
git clone [repository-url]
cd TeleHealthWithSubscriptionModel/backend
```

### 2. Configure Database
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartTelehealth;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Run Database Migrations
```bash
cd SmartTelehealth.API
dotnet ef database update
```

### 4. Configure Stripe
Update `appsettings.json`:
```json
{
  "Stripe": {
    "SecretKey": "sk_test_your_stripe_secret_key",
    "PublishableKey": "pk_test_your_stripe_publishable_key",
    "WebhookSecret": "whsec_your_webhook_secret"
  }
}
```

### 5. Start the API
```bash
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger documentation at `https://localhost:5001/swagger`

## 🔐 Default Admin Credentials

For testing purposes, use these credentials:
- **Email**: admin@test.com
- **Password**: Admin123!

## 📊 API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - User logout

### Subscription Management
- `GET /api/subscription-management/subscriptions` - Get all subscriptions
- `POST /api/subscriptions` - Create subscription
- `POST /api/subscriptions/{id}/cancel` - Cancel subscription
- `POST /api/subscriptions/{id}/pause` - Pause subscription
- `POST /api/subscriptions/{id}/resume` - Resume subscription

### Plan Management
- `GET /api/subscription-management/plans` - Get all plans
- `POST /api/subscription-management/plans` - Create plan
- `PUT /api/subscription-management/plans/{id}` - Update plan
- `DELETE /api/subscription-management/plans/{id}` - Delete plan

### Billing & Payments
- `GET /api/billing/records` - Get billing records
- `POST /api/billing/{id}/process-payment` - Process payment
- `POST /api/payments/retry-payment/{id}` - Retry failed payment
- `POST /api/stripe/webhook` - Stripe webhook handler

### Provider Management
- `GET /api/providers` - Get all providers
- `POST /api/providers` - Create provider
- `PUT /api/providers/{id}` - Update provider
- `DELETE /api/providers/{id}` - Delete provider

### Analytics
- `GET /api/subscription-analytics/revenue` - Revenue analytics
- `GET /api/subscriptions/{id}/usage` - Usage statistics

### Appointments
- `GET /api/appointments` - Get appointments
- `POST /api/appointments` - Create appointment
- `PUT /api/appointments/{id}` - Update appointment
- `DELETE /api/appointments/{id}` - Delete appointment

### Users & Patients
- `GET /api/users` - Get all users
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

## 🏗️ Project Structure

```
backend/
├── SmartTelehealth.API/          # Web API Layer
│   ├── Controllers/              # API Controllers
│   ├── Middleware/               # Custom Middleware
│   ├── Hubs/                     # SignalR Hubs
│   └── Program.cs                # Application Entry Point
├── SmartTelehealth.Application/  # Application Layer
│   ├── Services/                 # Business Logic Services
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Interfaces/               # Service Interfaces
│   └── Mapping/                  # AutoMapper Profiles
├── SmartTelehealth.Core/         # Domain Layer
│   ├── Entities/                 # Domain Entities
│   └── Interfaces/               # Repository Interfaces
├── SmartTelehealth.Infrastructure/ # Infrastructure Layer
│   ├── Data/                     # Database Context & Migrations
│   ├── Repositories/             # Repository Implementations
│   └── Services/                 # External Service Implementations
└── SmartTelehealth.API.Tests/    # Test Projects
    ├── Integration Tests/
    ├── Unit Tests/
    └── Test Data/
```

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Role-based Authorization**: Granular access control based on user roles
- **Input Validation**: Comprehensive input validation and sanitization
- **Rate Limiting**: API rate limiting to prevent abuse
- **Audit Logging**: Complete audit trail for all operations
- **HTTPS Enforcement**: SSL/TLS encryption for all communications
- **CORS Configuration**: Cross-Origin Resource Sharing configuration

## 🚀 Deployment

### Docker Deployment
```bash
# Build Docker image
docker build -t smarttelehealth-api .

# Run container
docker run -p 5000:80 smarttelehealth-api
```

### Azure Deployment
```bash
# Publish to Azure
dotnet publish -c Release
```

### Environment Configuration
Update `appsettings.Production.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your production connection string"
  },
  "Stripe": {
    "SecretKey": "sk_live_your_production_key",
    "PublishableKey": "pk_live_your_production_key",
    "WebhookSecret": "whsec_your_production_webhook_secret"
  }
}
```

## 🧪 Testing

### Run Unit Tests
```bash
cd SmartTelehealth.API.Tests
dotnet test
```

### Run Integration Tests
```bash
cd SmartTelehealth.API.Tests
dotnet test --filter Category=Integration
```

### Test Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📝 Development Notes

### Clean Architecture Layers
- **API Layer**: Controllers, middleware, and API-specific logic
- **Application Layer**: Business logic, services, and DTOs
- **Domain Layer**: Core entities and business rules
- **Infrastructure Layer**: Data access, external services, and implementations

### Key Design Patterns
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Loose coupling and testability
- **CQRS**: Command Query Responsibility Segregation
- **Mediator Pattern**: Decoupled request handling

### Database Design
- **Entity Framework Core**: ORM for data access
- **Code First Migrations**: Database schema management
- **Seed Data**: Initial data population
- **Relationship Management**: Complex entity relationships

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
