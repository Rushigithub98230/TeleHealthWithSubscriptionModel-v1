# 🧪 Subscription Testing Dashboard Guide

## Overview

The Subscription Testing Dashboard is a comprehensive frontend component that allows you to manually test all subscription functionality with real backend integration. This ensures 100% confidence that the implementation works without any logical errors or bugs.

## 🚀 Getting Started

### 1. Start Both Applications

```bash
# Terminal 1: Start Backend
cd backend
dotnet run --project SmartTelehealth.API

# Terminal 2: Start Frontend
cd healthcare-portal
ng serve
```

### 2. Access the Testing Dashboard

Navigate to: `http://localhost:4200/subscription-testing`

## 📋 Prerequisites

Before testing, you need to gather the following test data:

### Required Test Data

1. **Stripe Customer ID**: `cus_xxxxxxxxxxxxx`
2. **Payment Method ID**: `pm_xxxxxxxxxxxxx`
3. **Product ID**: `prod_xxxxxxxxxxxxx`
4. **Price ID**: `price_xxxxxxxxxxxxx`
5. **User ID**: `00000000-0000-0000-0000-000000000001`
6. **Plan ID**: `00000000-0000-0000-0000-000000000002`
7. **Webhook Secret**: `whsec_xxxxxxxxxxxxx`

### How to Get Test Data

#### 1. Stripe Test Data
```bash
# Create test customer
curl -X POST https://api.stripe.com/v1/customers \
  -H "Authorization: Bearer sk_test_..." \
  -d "email=test@example.com" \
  -d "name=Test Customer"

# Create test payment method
curl -X POST https://api.stripe.com/v1/payment_methods \
  -H "Authorization: Bearer sk_test_..." \
  -d "type=card" \
  -d "card[number]=4242424242424242" \
  -d "card[exp_month]=12" \
  -d "card[exp_year]=2024" \
  -d "card[cvc]=123"

# Create test product
curl -X POST https://api.stripe.com/v1/products \
  -H "Authorization: Bearer sk_test_..." \
  -d "name=Test Healthcare Plan" \
  -d "description=Comprehensive healthcare subscription plan"

# Create test price
curl -X POST https://api.stripe.com/v1/prices \
  -H "Authorization: Bearer sk_test_..." \
  -d "product=prod_xxxxxxxxxxxxx" \
  -d "unit_amount=2999" \
  -d "currency=usd" \
  -d "recurring[interval]=month"
```

#### 2. Database Test Data
```sql
-- Insert test user
INSERT INTO AspNetUsers (Id, UserName, Email, FirstName, LastName, IsActive, CreatedAt)
VALUES ('00000000-0000-0000-0000-000000000001', 'testuser', 'test@example.com', 'Test', 'User', 1, GETUTCDATE());

-- Insert test plan
INSERT INTO SubscriptionPlans (Id, Name, Description, Price, IsActive, BillingCycleId, CurrencyId, CreatedAt)
VALUES ('00000000-0000-0000-0000-000000000002', 'Test Plan', 'Test subscription plan', 29.99, 1, 
        '00000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000004', GETUTCDATE());
```

## 🧪 Testing Scenarios

### 1. Stripe Service Tests

#### Test Create Customer
- **Purpose**: Validates Stripe customer creation
- **Expected**: Customer ID returned
- **Use Case**: New user registration

#### Test Create Product
- **Purpose**: Validates Stripe product creation
- **Expected**: Product ID returned
- **Use Case**: Admin creating new subscription plans

#### Test Create Price
- **Purpose**: Validates Stripe price creation
- **Expected**: Price ID returned
- **Use Case**: Setting up pricing for plans

#### Test Create Subscription
- **Purpose**: Validates Stripe subscription creation
- **Expected**: Subscription ID returned
- **Use Case**: User subscribing to a plan

#### Test Get Subscription
- **Purpose**: Validates subscription retrieval
- **Expected**: Subscription details returned
- **Use Case**: Checking subscription status

#### Test Cancel Subscription
- **Purpose**: Validates subscription cancellation
- **Expected**: Success response
- **Use Case**: User cancelling subscription

#### Test Pause/Resume Subscription
- **Purpose**: Validates subscription pausing and resuming
- **Expected**: Success response
- **Use Case**: Temporary subscription suspension

#### Test Process Payment
- **Purpose**: Validates one-time payment processing
- **Expected**: Payment success response
- **Use Case**: Manual payment processing

### 2. Subscription Service Tests

#### Test Create Subscription (Service)
- **Purpose**: Validates complete subscription creation flow
- **Expected**: Database and Stripe subscription created
- **Use Case**: Full subscription lifecycle

#### Test Get User Subscriptions
- **Purpose**: Validates user subscription retrieval
- **Expected**: List of user subscriptions
- **Use Case**: User dashboard

#### Test Get Usage Statistics
- **Purpose**: Validates usage tracking
- **Expected**: Usage statistics returned
- **Use Case**: Usage monitoring

#### Test Process Payment (Service)
- **Purpose**: Validates payment processing with service layer
- **Expected**: Payment processed and recorded
- **Use Case**: Manual payment processing

#### Test Get Billing History
- **Purpose**: Validates billing record retrieval
- **Expected**: Billing history returned
- **Use Case**: Billing management

### 3. Plan Management Tests

#### Test Create Plan
- **Purpose**: Validates subscription plan creation
- **Expected**: Plan created with Stripe integration
- **Use Case**: Admin creating new plans

#### Test Get All Plans
- **Purpose**: Validates plan retrieval
- **Expected**: List of all plans
- **Use Case**: Plan listing

#### Test Update Plan
- **Purpose**: Validates plan updates
- **Expected**: Plan updated successfully
- **Use Case**: Plan modifications

#### Test Delete Plan
- **Purpose**: Validates plan deletion
- **Expected**: Plan deactivated
- **Use Case**: Plan removal

### 4. Webhook & Integration Tests

#### Test Webhook: Payment Succeeded
- **Purpose**: Validates payment success webhook processing
- **Expected**: Database updated, notifications sent
- **Use Case**: Automatic payment processing

#### Test Webhook: Payment Failed
- **Purpose**: Validates payment failure webhook processing
- **Expected**: Failed payment handling
- **Use Case**: Payment failure recovery

#### Test Webhook: Subscription Created
- **Purpose**: Validates subscription creation webhook
- **Expected**: Database synchronization
- **Use Case**: Real-time updates

#### Test Webhook: Subscription Cancelled
- **Purpose**: Validates subscription cancellation webhook
- **Expected**: Database synchronization
- **Use Case**: Real-time updates

#### Test Create Checkout Session
- **Purpose**: Validates Stripe checkout session creation
- **Expected**: Checkout URL returned
- **Use Case**: Payment flow

#### Test Get Payment Methods
- **Purpose**: Validates customer payment method retrieval
- **Expected**: List of payment methods
- **Use Case**: Payment method management

## 🔍 Testing Process

### Step 1: Configure Test Data
1. Fill in all required fields in the "Test Configuration" section
2. Ensure all IDs are valid and accessible
3. Save configuration (auto-saves to localStorage)

### Step 2: Run Stripe Service Tests
1. Start with "Test Create Customer"
2. Use returned customer ID for subsequent tests
3. Create product and price
4. Test subscription creation
5. Test subscription management operations

### Step 3: Run Subscription Service Tests
1. Test complete subscription creation flow
2. Test user subscription retrieval
3. Test usage statistics
4. Test payment processing
5. Test billing history

### Step 4: Run Plan Management Tests
1. Test plan creation
2. Test plan retrieval
3. Test plan updates
4. Test plan deletion

### Step 5: Run Webhook Tests
1. Test payment success webhook
2. Test payment failure webhook
3. Test subscription webhooks
4. Test checkout session creation

### Step 6: Verify Results
1. Check all test results in the "Test Results" section
2. Verify success responses
3. Check error messages for failed tests
4. Review response data for accuracy

## 📊 Expected Results

### Success Indicators
- ✅ All tests return "SUCCESS" status
- ✅ Response data matches expected format
- ✅ Database and Stripe state are synchronized
- ✅ No error messages in test results

### Common Issues & Solutions

#### Issue: "Customer not found"
**Solution**: Ensure customer ID is valid and exists in Stripe

#### Issue: "Payment method not found"
**Solution**: Ensure payment method ID is valid and attached to customer

#### Issue: "Product not found"
**Solution**: Ensure product ID is valid and exists in Stripe

#### Issue: "Price not found"
**Solution**: Ensure price ID is valid and exists in Stripe

#### Issue: "User not found"
**Solution**: Ensure user ID exists in database

#### Issue: "Plan not found"
**Solution**: Ensure plan ID exists in database

#### Issue: "Webhook signature verification failed"
**Solution**: Ensure webhook secret is correct

## 🚨 Error Handling

### Network Errors
- Check if backend is running on correct port
- Verify API base URL configuration
- Check CORS settings

### Authentication Errors
- Verify API keys are configured
- Check authentication headers
- Ensure proper authorization

### Database Errors
- Check database connection
- Verify entity relationships
- Check foreign key constraints

### Stripe Errors
- Verify Stripe API keys
- Check Stripe account status
- Validate test data format

## 📈 Monitoring & Debugging

### Test Results Analysis
- Review all test results for patterns
- Check response times for performance issues
- Analyze error messages for root causes

### Database Verification
```sql
-- Check subscription records
SELECT * FROM Subscriptions WHERE UserId = '00000000-0000-0000-0000-000000000001';

-- Check billing records
SELECT * FROM BillingRecords WHERE UserId = '00000000-0000-0000-0000-000000000001';

-- Check audit logs
SELECT * FROM AuditLogs WHERE UserId = '00000000-0000-0000-0000-000000000001';
```

### Stripe Dashboard Verification
- Check customers in Stripe dashboard
- Verify subscriptions exist
- Confirm payment methods are attached
- Review webhook events

## 🎯 Success Criteria

A successful test run should demonstrate:

1. **Complete Stripe Integration**: All Stripe operations work correctly
2. **Database Synchronization**: Database and Stripe state are consistent
3. **Error Handling**: Proper error handling and user feedback
4. **Webhook Processing**: Real-time event processing works
5. **Payment Processing**: Payment flows work end-to-end
6. **Plan Management**: Complete plan lifecycle management
7. **Usage Tracking**: Usage statistics are accurate
8. **Audit Logging**: All operations are properly logged

## 🔄 Continuous Testing

### Automated Testing
- Run tests after each deployment
- Test in staging environment before production
- Monitor test results over time

### Manual Testing
- Test new features before deployment
- Verify bug fixes work correctly
- Test edge cases and error scenarios

## 📝 Test Documentation

### Test Logs
- All test results are logged with timestamps
- Response data is captured for analysis
- Error messages are preserved for debugging

### Configuration Management
- Test configuration is auto-saved
- Configuration persists between sessions
- Easy to switch between test environments

## 🎉 Conclusion

The Subscription Testing Dashboard provides comprehensive validation of all subscription functionality. By following this guide and running all test scenarios, you can ensure 100% confidence that your subscription management system works correctly in production.

**Remember**: Always test in a safe environment with test data before running in production! 