# Subscription & Workflow Management Extraction Guide

This guide provides a comprehensive overview of how to extract the core subscription functionality, Stripe integration, billing, payment processing, and workflow management from the SmartTelehealth codebase into a standalone project.

## 🎯 What This Guide Covers

This extraction focuses on **subscription management only**, excluding:
- ❌ Video calling services (OpenTok, VideoCall)
- ❌ Chat/messaging services
- ❌ Health assessment/questionnaire services
- ❌ Appointment management
- ❌ Provider onboarding
- ❌ Medication delivery
- ❌ File storage services

## 🏗️ Core Architecture Overview

The subscription system follows a clean architecture pattern with these layers:
- **Core**: Entities, interfaces, and domain logic
- **Application**: Business logic, DTOs, and service orchestration
- **Infrastructure**: External integrations (Stripe), data access, and implementations
- **API**: Controllers and HTTP endpoints

## 📁 Required Components for Extraction

### 1. Core Entities (SmartTelehealth.Core/Entities/)

#### Essential Subscription Entities:
```
✅ Subscription.cs                    - Main subscription entity
✅ SubscriptionPlan.cs               - Subscription plan definitions
✅ SubscriptionPlanPrivilege.cs     - Plan feature mappings
✅ SubscriptionPayment.cs            - Payment tracking
✅ SubscriptionStatusHistory.cs      - Status change audit trail
✅ UserSubscriptionPrivilegeUsage.cs - Feature usage tracking
✅ BillingRecord.cs                  - Billing and payment records
✅ BillingAdjustment.cs             - Billing modifications
✅ PaymentRefund.cs                  - Refund tracking
✅ Category.cs                       - Service categories
✅ CategoryFeeRange.cs               - Category-based pricing
```

#### Supporting Entities:
```
✅ BaseEntity.cs                     - Base entity with audit fields
✅ User.cs                          - User management (minimal)
✅ Role.cs                          - Role-based access control
✅ AuditLog.cs                      - Audit trail
✅ MasterTables.cs                  - Master data (billing cycles, currencies)
```

#### Entities to EXCLUDE:
```
❌ Consultation.cs                   - Not subscription-related
❌ Appointment.cs                    - Not subscription-related
❌ VideoCall.cs                      - Video calling (exclude)
❌ ChatRoom.cs                       - Chat services (exclude)
❌ Message.cs                        - Messaging (exclude)
❌ Provider.cs                       - Provider management (exclude)
❌ HealthAssessment.cs               - Health services (exclude)
❌ QuestionnaireTemplate.cs          - Health services (exclude)
❌ MedicationDelivery.cs             - Medication (exclude)
❌ Prescription.cs                   - Medication (exclude)
```

### 2. Application Services (SmartTelehealth.Application/Services/)

#### Core Subscription Services:
```
✅ SubscriptionService.cs            - Main subscription business logic
✅ SubscriptionLifecycleService.cs   - Subscription state management
✅ SubscriptionAutomationService.cs  - Automated workflows
✅ BillingService.cs                 - Billing and payment processing
✅ AutomatedBillingService.cs        - Recurring billing automation
✅ CategoryService.cs                - Category management
✅ PrivilegeService.cs               - Feature privilege management
✅ AnalyticsService.cs               - Subscription analytics (subset)
```

#### Services to EXCLUDE:
```
❌ VideoCallService.cs               - Video calling (exclude)
❌ VideoCallSubscriptionService.cs   - Video-specific (exclude)
❌ ChatService.cs                    - Chat services (exclude)
❌ ChatRoomService.cs                - Chat management (exclude)
❌ MessagingService.cs               - Messaging (exclude)
❌ AppointmentService.cs             - Appointments (exclude)
❌ ConsultationService.cs            - Consultations (exclude)
❌ HealthAssessmentService.cs        - Health services (exclude)
❌ QuestionnaireService.cs           - Health services (exclude)
❌ ProviderService.cs                - Provider management (exclude)
❌ HomeMedService.cs                 - Home medication (exclude)
❌ NotificationService.cs             - General notifications (exclude)
```

### 3. Application Interfaces (SmartTelehealth.Application/Interfaces/)

#### Required Interfaces:
```
✅ ISubscriptionService.cs           - Subscription operations
✅ ISubscriptionLifecycleService.cs  - Lifecycle management
✅ ISubscriptionAutomationService.cs - Automation workflows
✅ IBillingService.cs                - Billing operations
✅ IAutomatedBillingService.cs       - Automated billing
✅ ICategoryService.cs                - Category management
✅ IPrivilegeService.cs              - Privilege management
✅ IAnalyticsService.cs              - Analytics (subset)
✅ IStripeService.cs                 - Stripe integration
✅ IAuditService.cs                  - Audit logging
✅ IUserService.cs                   - User management (minimal)
```

#### Interfaces to EXCLUDE:
```
❌ IVideoCallService.cs              - Video calling (exclude)
❌ IChatService.cs                   - Chat services (exclude)
❌ IMessagingService.cs              - Messaging (exclude)
❌ IAppointmentService.cs            - Appointments (exclude)
❌ IConsultationService.cs           - Consultations (exclude)
❌ IHealthAssessmentService.cs       - Health services (exclude)
❌ IQuestionnaireService.cs          - Health services (exclude)
❌ IProviderService.cs               - Provider management (exclude)
❌ IHomeMedService.cs                - Home medication (exclude)
❌ INotificationService.cs           - General notifications (exclude)
```

### 4. Application DTOs (SmartTelehealth.Application/DTOs/)

#### Required DTOs:
```
✅ JsonModel.cs                      - Standard response wrapper
✅ SubscriptionDtos.cs               - Subscription-related DTOs
✅ BillingDtos.cs                    - Billing-related DTOs
✅ AnalyticsDtos.cs                  - Analytics DTOs (subset)
✅ SubscriptionDashboardDto.cs       - Dashboard data
✅ CategoryDtos.cs                   - Category management
✅ UserDtos.cs                       - User management (minimal)
```

#### DTOs to EXCLUDE:
```
❌ AppointmentDtos.cs                - Appointment-related (exclude)
❌ ConsultationDtos.cs               - Consultation-related (exclude)
❌ VideoCallDtos.cs                  - Video calling (exclude)
❌ ChatDtos.cs                       - Chat services (exclude)
❌ HealthAssessmentDtos.cs           - Health services (exclude)
❌ ProviderDtos.cs                   - Provider management (exclude)
❌ HomeMedDtos.cs                    - Home medication (exclude)
```

### 5. Infrastructure Services (SmartTelehealth.Infrastructure/Services/)

#### Required Services:
```
✅ StripeService.cs                  - Stripe payment integration
✅ PaymentSecurityService.cs         - Payment security
✅ BillingService.cs                 - Billing implementation
✅ AutomatedBillingService.cs        - Automated billing implementation
```

#### Services to EXCLUDE:
```
❌ OpenTokService.cs                 - Video calling (exclude)
❌ ChatStorageService.cs             - Chat storage (exclude)
❌ VideoCallService.cs               - Video calling (exclude)
❌ DocumentService.cs                - Document management (exclude)
❌ FileStorageService.cs             - File storage (exclude)
❌ NotificationService.cs             - General notifications (exclude)
❌ JwtService.cs                     - Authentication (exclude)
❌ AuthService.cs                    - Authentication (exclude)
```

### 6. Infrastructure Repositories (SmartTelehealth.Infrastructure/Repositories/)

#### Required Repositories:
```
✅ SubscriptionRepository.cs          - Subscription data access
✅ SubscriptionPlanRepository.cs     - Plan data access
✅ SubscriptionPaymentRepository.cs  - Payment data access
✅ SubscriptionStatusHistoryRepository.cs - Status history
✅ SubscriptionPlanPrivilegeRepository.cs - Plan privileges
✅ UserSubscriptionPrivilegeUsageRepository.cs - Usage tracking
✅ BillingRepository.cs              - Billing data access
✅ BillingAdjustmentRepository.cs    - Billing adjustments
✅ CategoryRepository.cs             - Category data access
✅ PrivilegeRepository.cs            - Privilege data access
✅ UserRepository.cs                 - User data access (minimal)
✅ AuditLogRepository.cs             - Audit logging
✅ GenericRepository.cs              - Base repository
```

#### Repositories to EXCLUDE:
```
❌ VideoCallRepository.cs            - Video calling (exclude)
❌ ChatRoomRepository.cs             - Chat services (exclude)
❌ MessageRepository.cs              - Messaging (exclude)
❌ AppointmentRepository.cs          - Appointments (exclude)
❌ ConsultationRepository.cs         - Consultations (exclude)
❌ HealthAssessmentRepository.cs     - Health services (exclude)
❌ QuestionnaireRepository.cs        - Health services (exclude)
❌ ProviderRepository.cs             - Provider management (exclude)
❌ HomeMedRepository.cs              - Home medication (exclude)
❌ NotificationRepository.cs         - General notifications (exclude)
```

### 7. API Controllers (SmartTelehealth.API/Controllers/)

#### Required Controllers:
```
✅ SubscriptionsController.cs         - Main subscription endpoints
✅ UserSubscriptionsController.cs    - User subscription management
✅ SubscriptionPlansController.cs    - Plan management
✅ SubscriptionManagementController.cs - Admin subscription management
✅ SubscriptionAnalyticsController.cs - Subscription analytics
✅ SubscriptionAutomationController.cs - Automation workflows
✅ BillingController.cs              - Billing endpoints
✅ PaymentController.cs              - Payment processing
✅ StripeController.cs               - Stripe integration
✅ StripeWebhookController.cs        - Stripe webhooks
✅ CategoriesController.cs           - Category management
✅ PrivilegesController.cs           - Privilege management
✅ AnalyticsController.cs            - Analytics (subset)
```

#### Controllers to EXCLUDE:
```
❌ VideoCallController.cs            - Video calling (exclude)
❌ ChatController.cs                 - Chat services (exclude)
❌ ChatRoomController.cs             - Chat management (exclude)
❌ MessageController.cs              - Messaging (exclude)
❌ AppointmentController.cs          - Appointments (exclude)
❌ ConsultationController.cs         - Consultations (exclude)
❌ HealthAssessmentsController.cs    - Health services (exclude)
❌ QuestionnaireController.cs        - Health services (exclude)
❌ ProviderController.cs             - Provider management (exclude)
❌ HomeMedController.cs              - Home medication (exclude)
❌ NotificationController.cs         - General notifications (exclude)
❌ AuthController.cs                 - Authentication (exclude)
❌ AdminController.cs                - General admin (exclude)
```

### 8. Infrastructure Data (SmartTelehealth.Infrastructure/Data/)

#### Required Components:
```
✅ DbContext.cs                      - Entity Framework context
✅ Migrations/                       - Database migrations
✅ DependencyInjection.cs            - Service registration
```

### 9. Project Files

#### Required Project Files:
```
✅ SmartTelehealth.Core.csproj       - Core project
✅ SmartTelehealth.Application.csproj - Application project
✅ SmartTelehealth.Infrastructure.csproj - Infrastructure project
✅ SmartTelehealth.API.csproj        - API project
✅ SmartTelehealth.sln               - Solution file
```

## 🔧 Configuration Requirements

### 1. Stripe Configuration
```json
{
  "StripeSettings": {
    "SecretKey": "sk_test_...",
    "PublishableKey": "pk_test_...",
    "WebhookSecret": "whsec_...",
    "WebhookRetryAttempts": 3,
    "WebhookRetryDelaySeconds": 5
  }
}
```

### 2. Database Configuration
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=...;Trusted_Connection=true;"
  }
}
```

### 3. Billing Configuration
```json
{
  "BillingSettings": {
    "DefaultCurrency": "USD",
    "TaxRate": 0.08,
    "GracePeriodDays": 7,
    "AutoRetryAttempts": 3
  }
}
```

## 📊 Database Schema Requirements

### Core Tables:
- `Subscriptions` - Main subscription records
- `SubscriptionPlans` - Available plans
- `SubscriptionPlanPrivileges` - Plan features
- `SubscriptionPayments` - Payment history
- `SubscriptionStatusHistory` - Status changes
- `BillingRecords` - Billing and invoices
- `BillingAdjustments` - Billing modifications
- `Categories` - Service categories
- `Privileges` - Available features
- `Users` - User accounts (minimal)
- `AuditLogs` - Audit trail

### Master Tables:
- `MasterBillingCycles` - Billing frequency options
- `MasterCurrencies` - Supported currencies
- `MasterSubscriptionStatuses` - Status definitions

## 🚀 Implementation Steps

### Phase 1: Core Extraction
1. Create new solution structure
2. Copy required Core entities
3. Copy required Application interfaces and DTOs
4. Copy required Infrastructure repositories and services
5. Copy required API controllers

### Phase 2: Dependency Resolution
1. Remove references to excluded services
2. Update service registrations
3. Fix dependency injection
4. Update database context

### Phase 3: Testing & Validation
1. Build solution
2. Run database migrations
3. Test core subscription flows
4. Validate Stripe integration
5. Test billing automation

### Phase 4: Cleanup
1. Remove unused using statements
2. Clean up unused DTOs
3. Remove unused entity properties
4. Optimize database queries

## ⚠️ Important Considerations

### 1. Dependencies
- Ensure all required NuGet packages are included
- Stripe.net for payment processing
- Entity Framework Core for data access
- AutoMapper for object mapping

### 2. Data Migration
- Export existing subscription data
- Create new database schema
- Import subscription data
- Validate data integrity

### 3. External Integrations
- Update Stripe webhook endpoints
- Configure new domain for webhooks
- Update Stripe product/price IDs if needed

### 4. Security
- Implement proper authentication
- Secure API endpoints
- Validate webhook signatures
- Encrypt sensitive data

## 📈 Expected Outcomes

After extraction, you'll have a standalone subscription management system with:

✅ **Complete subscription lifecycle management**
✅ **Stripe payment integration**
✅ **Automated billing and recurring payments**
✅ **Subscription analytics and reporting**
✅ **Role-based access control**
✅ **Audit logging and compliance**
✅ **Multi-currency support**
✅ **Flexible billing cycles**
✅ **Feature-based privilege system**
✅ **Webhook handling for real-time updates**

## 🆘 Troubleshooting

### Common Issues:
1. **Missing Dependencies**: Ensure all required services are registered
2. **Database Context**: Update DbContext to only include required entities
3. **Service References**: Remove calls to excluded services
4. **Configuration**: Update appsettings.json with required settings
5. **Migrations**: Create new migrations for the extracted schema

### Support:
- Check build errors for missing references
- Validate database schema matches entity definitions
- Test Stripe webhook endpoints
- Verify billing automation workflows

---

**Note**: This extraction guide assumes you want to maintain the same architecture patterns and code quality. Adjust the approach based on your specific requirements and target platform.
