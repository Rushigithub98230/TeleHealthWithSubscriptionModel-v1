# Smart Telehealth Subscription Workflow (2024 Update)

This document outlines the complete, up-to-date subscription workflow for the Smart Telehealth platform, reflecting the latest changes in privilege management, billing, extensibility, and business logic.

---

## 1. Overview

The Smart Telehealth platform operates on a subscription-based model where users subscribe to healthcare categories (e.g., Primary Care, Mental Health) and select plans that define their access to consultations, messaging, medication delivery, and more. The system supports recurring billing, plan upgrades/downgrades, privilege management, and deep integration with providers and payment gateways.

---

## 2. Database Tables & Relationships

### **Core Subscription Tables**

#### **Categories** 📋
```sql
Categories (
    Id (PK, Guid),
    Name (nvarchar(100), Required),
    Description (nvarchar(500)),
    IsActive (bit, Default: true),
    BasePrice (decimal(18,2)),
    ConsultationFee (decimal(18,2)),
    OneTimeConsultationFee (decimal(18,2)),
    RequiresHealthAssessment (bit, Default: true),
    AllowsMedicationDelivery (bit, Default: true),
    AllowsFollowUpMessaging (bit, Default: true),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **SubscriptionPlans** 📋
```sql
SubscriptionPlans (
    Id (PK, Guid),
    CategoryId (FK → Categories.Id),
    Name (nvarchar(100), Required),
    Description (nvarchar(500)),
    MonthlyPrice (decimal(18,2)),
    QuarterlyPrice (decimal(18,2)),
    AnnualPrice (decimal(18,2)),
    IsActive (bit, Default: true),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **Subscriptions** 📋
```sql
Subscriptions (
    Id (PK, Guid),
    UserId (FK → Users.Id),
    SubscriptionPlanId (FK → SubscriptionPlans.Id),
    ProviderId (FK → Providers.Id, Nullable),
    Status (nvarchar(50), Enum: Active, Paused, Cancelled, Expired),
    BillingFrequency (nvarchar(50), Enum: Monthly, Quarterly, Annual),
    CurrentPrice (decimal(18,2)),
    AutoRenew (bit, Default: true),
    StartDate (datetime2),
    EndDate (datetime2),
    NextBillingDate (datetime2),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

### **Privilege Management Tables**

#### **Privileges** 📋
```sql
Privileges (
    Id (PK, Guid),
    Name (nvarchar(100), Required),
    Description (nvarchar(500)),
    DataType (nvarchar(50), Default: "int"),
    DefaultValue (nvarchar(100)),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **SubscriptionPlanPrivileges** 📋
```sql
SubscriptionPlanPrivileges (
    Id (PK, Guid),
    SubscriptionPlanId (FK → SubscriptionPlans.Id),
    PrivilegeId (FK → Privileges.Id),
    Value (nvarchar(100), Required),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **UserSubscriptionPrivilegeUsages** 📋
```sql
UserSubscriptionPrivilegeUsages (
    Id (PK, Guid),
    SubscriptionId (FK → Subscriptions.Id),
    PrivilegeId (FK → Privileges.Id),
    UsedValue (nvarchar(100), Default: "0"),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

### **Billing & Payment Tables**

#### **BillingRecords** 📋
```sql
BillingRecords (
    Id (PK, Guid),
    UserId (FK → Users.Id),
    SubscriptionId (FK → Subscriptions.Id, Nullable),
    Amount (decimal(18,2)),
    Description (nvarchar(500)),
    DueDate (datetime2),
    PaidDate (datetime2, Nullable),
    Status (nvarchar(50), Enum: Pending, Paid, Overdue, Cancelled),
    PaymentMethod (nvarchar(100)),
    StripePaymentIntentId (nvarchar(255)),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **SubscriptionPayments** 📋
```sql
SubscriptionPayments (
    Id (PK, Guid),
    SubscriptionId (FK → Subscriptions.Id),
    Amount (decimal(18,2)),
    PaymentDate (datetime2),
    PaymentMethod (nvarchar(100)),
    StripePaymentIntentId (nvarchar(255)),
    Status (nvarchar(50), Enum: Pending, Completed, Failed, Refunded),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

### **Supporting Tables**

#### **Providers** 📋
```sql
Providers (
    Id (PK, Guid),
    UserId (FK → Users.Id),
    LicenseNumber (nvarchar(100)),
    Specialization (nvarchar(200)),
    IsActive (bit, Default: true),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **ProviderCategories** 📋
```sql
ProviderCategories (
    Id (PK, Guid),
    ProviderId (FK → Providers.Id),
    CategoryId (FK → Categories.Id),
    IsActive (bit, Default: true),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

#### **Consultations** 📋
```sql
Consultations (
    Id (PK, Guid),
    UserId (FK → Users.Id),
    ProviderId (FK → Providers.Id),
    CategoryId (FK → Categories.Id),
    SubscriptionId (FK → Subscriptions.Id, Nullable),
    HealthAssessmentId (FK → HealthAssessments.Id, Nullable),
    Status (nvarchar(50), Enum: Scheduled, InProgress, Completed, Cancelled),
    Type (nvarchar(50), Enum: Initial, FollowUp, Emergency),
    Fee (decimal(18,2)),
    ScheduledAt (datetime2),
    StartedAt (datetime2, Nullable),
    EndedAt (datetime2, Nullable),
    RequiresFollowUp (bit, Default: false),
    CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
)
```

---

## 3. Table Relationships & Foreign Keys

### **Primary Relationships:**
1. **Categories** → **SubscriptionPlans** (1:Many)
2. **SubscriptionPlans** → **Subscriptions** (1:Many)
3. **Users** → **Subscriptions** (1:Many)
4. **Providers** → **Subscriptions** (1:Many, Optional)
5. **Categories** → **Consultations** (1:Many)
6. **Subscriptions** → **Consultations** (1:Many, Optional)

### **Privilege Relationships:**
1. **Privileges** → **SubscriptionPlanPrivileges** (1:Many)
2. **SubscriptionPlans** → **SubscriptionPlanPrivileges** (1:Many)
3. **Subscriptions** → **UserSubscriptionPrivilegeUsages** (1:Many)
4. **Privileges** → **UserSubscriptionPrivilegeUsages** (1:Many)

### **Billing Relationships:**
1. **Users** → **BillingRecords** (1:Many)
2. **Subscriptions** → **BillingRecords** (1:Many, Optional)
3. **Subscriptions** → **SubscriptionPayments** (1:Many)

### **Provider Relationships:**
1. **Users** → **Providers** (1:1)
2. **Providers** → **ProviderCategories** (1:Many)
3. **Categories** → **ProviderCategories** (1:Many)

---

## 4. Entity Framework Configuration

The subscription model uses Entity Framework Core with the following key configurations:

### **DbContext Configuration:**
```csharp
// Core subscription tables
public DbSet<Category> Categories { get; set; }
public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
public DbSet<Subscription> Subscriptions { get; set; }

// Privilege management
public DbSet<Privilege> Privileges { get; set; }
public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
public DbSet<UserSubscriptionPrivilegeUsage> UserSubscriptionPrivilegeUsages { get; set; }

// Billing & payments
public DbSet<BillingRecord> BillingRecords { get; set; }
public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
```

### **Key Entity Configurations:**
```csharp
// Categories
entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
entity.Property(e => e.BasePrice).HasPrecision(18, 2);
entity.Property(e => e.IsActive).HasDefaultValue(true);

// SubscriptionPlans
entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
entity.HasOne(e => e.Category)
    .WithMany(e => e.SubscriptionPlans)
    .HasForeignKey(e => e.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);

// Subscriptions
entity.Property(e => e.Status).HasConversion<string>();
entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
entity.HasOne(e => e.User)
    .WithMany(e => e.Subscriptions)
    .HasForeignKey(e => e.UserId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## 5. Workflow & Business Logic

### **Subscription Creation Flow:**
1. **User Selection**: User selects a healthcare category (e.g., Primary Care)
2. **Plan Display**: System shows available subscription plans for the category
3. **Plan Selection**: User chooses a plan with specific privileges and pricing
4. **Subscription Creation**: System creates a subscription record
5. **Privilege Initialization**: System creates usage tracking records for each privilege
6. **Billing Setup**: System creates initial billing record
7. **Payment Processing**: System processes payment via Stripe integration

### **Privilege Usage Flow:**
1. **Feature Access**: User attempts to use a feature (e.g., schedule consultation)
2. **Usage Check**: System checks current privilege usage against limits
3. **Limit Validation**: System validates against subscription plan privileges
4. **Action Execution**: If within limits, allows action and updates usage
5. **Limit Enforcement**: If over limit, blocks action or charges additional fees

### **Billing & Renewal Flow:**
1. **Billing Trigger**: NextBillingDate triggers automated billing process
2. **Record Creation**: System creates new billing record for the subscription
3. **Payment Processing**: System processes payment via Stripe
4. **Payment Recording**: System creates subscription payment record
5. **Date Updates**: System updates NextBillingDate for next billing cycle

---

## 6. Privilege Management System

### **Privilege Types:**
- **Integer Privileges**: Count-based limits (e.g., "Consultations": 5)
- **Boolean Privileges**: Feature toggles (e.g., "MedicationDelivery": true)
- **String Privileges**: Text-based values (e.g., "SupportLevel": "Premium")

### **Usage Tracking:**
- **Real-time Monitoring**: Tracks usage as users consume features
- **Limit Enforcement**: Prevents over-usage based on subscription plan
- **Rollover Logic**: Handles unused privileges across billing cycles
- **Upgrade Handling**: Manages privilege changes during plan upgrades

### **Extensibility:**
- **Dynamic Privileges**: New privileges can be added without code changes
- **Plan Flexibility**: Plans can have different privilege combinations
- **Value Customization**: Each plan can set different values for the same privilege

---

## 7. Billing & Payment Integration

### **Stripe Integration:**
- **Customer Management**: Creates and manages Stripe customers
- **Payment Processing**: Handles recurring and one-time payments
- **Webhook Handling**: Processes payment confirmations and failures
- **Refund Management**: Handles partial and full refunds

### **Billing Features:**
- **Recurring Billing**: Automated monthly/quarterly/annual billing
- **Proration**: Handles plan changes mid-cycle
- **Late Payment**: Manages overdue payments and late fees
- **Payment Methods**: Supports multiple payment methods per user

---

## 8. Provider Integration

### **Provider Assignment:**
- **Automatic Assignment**: System assigns providers based on availability
- **Manual Assignment**: Users can choose specific providers
- **Category Matching**: Providers are matched to subscription categories
- **Availability Tracking**: Monitors provider availability and capacity

### **Provider Management:**
- **License Verification**: Validates provider licenses and credentials
- **Specialization Matching**: Matches providers to user needs
- **Performance Tracking**: Monitors provider performance and ratings
- **Capacity Management**: Tracks provider workload and availability

---

## 9. Extensibility & Customization

### **Adding New Categories:**
1. Insert record into Categories table
2. Create SubscriptionPlans for the category
3. Assign Privileges to the plans
4. Update UI to display new category

### **Adding New Privileges:**
1. Insert record into Privileges table
2. Assign to existing plans via SubscriptionPlanPrivileges
3. Update business logic to check privilege usage
4. Add UI elements for privilege display

### **Custom Billing Frequencies:**
1. Update Subscriptions.BillingFrequency enum
2. Update billing logic for new frequency
3. Update UI to show new options
4. Test billing cycle calculations

---

## 10. Security & Compliance

### **Data Protection:**
- **Encryption**: Sensitive data encrypted at rest and in transit
- **Access Control**: Role-based access to subscription data
- **Audit Logging**: Complete audit trail for all changes
- **GDPR Compliance**: User data handling and deletion

### **Payment Security:**
- **PCI Compliance**: Secure payment processing via Stripe
- **Tokenization**: Payment methods stored as tokens
- **Fraud Detection**: Automated fraud detection and prevention
- **Refund Security**: Secure refund processing and validation

---

## 11. Performance & Scalability

### **Database Optimization:**
- **Indexing**: Strategic indexes on frequently queried columns
- **Partitioning**: Large tables partitioned for performance
- **Caching**: Redis caching for static data
- **Query Optimization**: Optimized queries for common operations

### **Scalability Features:**
- **Horizontal Scaling**: Read replicas for database scaling
- **Microservices**: Modular architecture for service scaling
- **Load Balancing**: Distributed load across multiple instances
- **Monitoring**: Real-time performance monitoring and alerting

---

This comprehensive subscription model provides a robust, scalable foundation for healthcare service delivery while maintaining flexibility for future enhancements and customizations. 