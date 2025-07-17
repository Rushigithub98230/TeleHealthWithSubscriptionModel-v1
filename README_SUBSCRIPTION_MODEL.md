# TeleHealth Subscription Model – Developer Documentation

## Overview

The TeleHealth platform uses a robust, flexible subscription model to deliver healthcare services. Users subscribe to healthcare categories (e.g., Primary Care, Mental Health) and select plans that define their access to consultations, messaging, medication delivery, and more. The system supports recurring billing, plan upgrades/downgrades, privilege management, and deep integration with providers and payment gateways.

---

## 1. Key Concepts & Entities

### a. Category
- Represents a healthcare vertical (e.g., Primary Care, Mental Health).
- Each category has its own plans, features, and pricing.

### b. SubscriptionPlan
- Defines the features, pricing, and limits for a subscription.
- Belongs to a specific Category.
- Contains privileges that define what users can access.

### c. Subscription
- A user's active subscription to a specific plan.
- Tracks billing, status, and usage.

### d. Privilege
- Defines a specific feature or capability (e.g., "Consultations", "Medication Delivery").
- Has a data type (int, bool, string) and default value.
- Can be assigned to subscription plans with specific values.

### e. UserSubscriptionPrivilegeUsage
- Tracks how much of each privilege a user has consumed.
- Used for enforcing limits and tracking usage.

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

## 4. Database Constraints & Indexes

### **Key Constraints:**
- **Categories.IsActive**: Ensures only active categories are used
- **SubscriptionPlans.IsActive**: Ensures only active plans are available
- **Subscriptions.Status**: Enum constraint for valid status values
- **BillingRecords.Status**: Enum constraint for valid payment status
- **Privileges.DataType**: Constraint for valid data types (int, bool, string)

### **Recommended Indexes:**
```sql
-- Performance indexes for common queries
CREATE INDEX IX_Subscriptions_UserId_Status ON Subscriptions(UserId, Status);
CREATE INDEX IX_Subscriptions_NextBillingDate ON Subscriptions(NextBillingDate);
CREATE INDEX IX_BillingRecords_UserId_Status ON BillingRecords(UserId, Status);
CREATE INDEX IX_Consultations_UserId_Status ON Consultations(UserId, Status);
CREATE INDEX IX_UserSubscriptionPrivilegeUsages_SubscriptionId ON UserSubscriptionPrivilegeUsages(SubscriptionId);
```

---

## 5. Data Flow & Workflow

### **Subscription Creation Flow:**
1. User selects a **Category** (e.g., Primary Care)
2. System shows available **SubscriptionPlans** for that category
3. User selects a plan and creates a **Subscription**
4. System creates **UserSubscriptionPrivilegeUsage** records for each privilege
5. System creates initial **BillingRecord** for the subscription
6. System processes payment via **SubscriptionPayments**

### **Privilege Usage Flow:**
1. User attempts to use a feature (e.g., schedule consultation)
2. System checks **UserSubscriptionPrivilegeUsage** for current usage
3. System checks **SubscriptionPlanPrivileges** for the limit
4. If within limits, allows the action and updates usage
5. If over limit, blocks the action or charges additional fees

### **Billing Flow:**
1. **Subscriptions.NextBillingDate** triggers billing process
2. System creates new **BillingRecord** for the subscription
3. System processes payment via Stripe
4. System creates **SubscriptionPayment** record
5. System updates **Subscriptions.NextBillingDate**

---

## 6. Extensibility & Customization

### **Adding New Categories:**
1. Insert record into **Categories** table
2. Create **SubscriptionPlans** for the category
3. Assign **Privileges** to the plans via **SubscriptionPlanPrivileges**

### **Adding New Privileges:**
1. Insert record into **Privileges** table
2. Assign to existing plans via **SubscriptionPlanPrivileges**
3. Update business logic to check privilege usage

### **Adding New Billing Frequencies:**
1. Update **Subscriptions.BillingFrequency** enum
2. Update billing logic to handle new frequency
3. Update UI to show new options

---

## 7. Security & Data Integrity

### **Data Validation:**
- All monetary values use `decimal(18,2)` for precision
- Enum constraints ensure valid status values
- Foreign key constraints maintain referential integrity
- Required fields prevent null values

### **Audit Trail:**
- All entities inherit from `BaseEntity` with audit fields
- **AuditLogs** table tracks all changes
- **CreatedBy** and **UpdatedBy** track user actions

### **Soft Deletes:**
- Most entities support soft deletion via `IsDeleted` flag
- Maintains data integrity while allowing recovery

---

## 8. Performance Considerations

### **Query Optimization:**
- Indexes on frequently queried columns
- Eager loading for related entities
- Pagination for large result sets
- Caching for static data (categories, privileges)

### **Scalability:**
- Horizontal scaling via read replicas
- Partitioning for large tables (billing records)
- Archiving old data to separate tables

---

This database structure provides a robust, scalable foundation for the subscription model while maintaining flexibility for future enhancements and customizations. 