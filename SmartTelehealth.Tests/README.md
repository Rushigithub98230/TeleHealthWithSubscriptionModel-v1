# SmartTelehealth Subscription Management Test Suite

## Overview

This comprehensive test suite validates the entire subscription management workflow for the SmartTelehealth platform. It covers all aspects of subscription lifecycle management, payment processing, Stripe integration, privilege usage tracking, and administrative operations.

## Test Coverage

### 🎯 Core Subscription Workflow Tests
- **Subscription Creation**: New subscription creation with various plans and billing cycles
- **Trial Management**: Trial period functionality and conversion to paid subscriptions
- **User Purchase Flow**: Complete user subscription purchase workflow
- **Lifecycle Management**: Pause, resume, cancel, and reactivate operations

### 💳 Payment Processing Tests
- **Payment Processing**: End-to-end payment processing workflows
- **Payment Failures**: Handling of failed payments and retry mechanisms
- **Payment Method Management**: Adding, removing, and setting default payment methods
- **Billing Records**: Creation and management of billing records

### 🔗 Stripe Integration Tests
- **Webhook Handling**: All major Stripe webhook events (subscription created, updated, deleted, payment succeeded/failed)
- **Customer Management**: Stripe customer creation and retrieval
- **Subscription Management**: Stripe subscription operations
- **Payment Method Operations**: Stripe payment method CRUD operations

### 🎁 Privilege Usage Tests
- **Usage Tracking**: Monitoring privilege consumption
- **Limit Enforcement**: Preventing usage beyond subscription limits
- **Usage Analytics**: Tracking and reporting privilege usage

### 👨‍💼 Admin Management Tests
- **Subscription Overview**: Admin view of all user subscriptions
- **Subscription Extension**: Admin ability to extend subscription periods
- **Bulk Operations**: Managing multiple subscriptions simultaneously
- **Analytics**: Revenue and usage analytics for administrators

### ⚠️ Error Handling & Edge Cases
- **Invalid Operations**: Testing invalid status transitions
- **Access Control**: Unauthorized access attempts
- **Service Failures**: Handling of external service failures
- **Data Validation**: Input validation and error responses

## Test Structure

```
SmartTelehealth.Tests/
├── ComprehensiveSubscriptionTestSuite.cs    # Main test suite covering all workflows
├── StripeIntegrationTests.cs               # Stripe-specific integration tests
├── SmartTelehealth.Tests.csproj            # Test project file
├── run-comprehensive-tests.ps1             # PowerShell test runner
└── README.md                               # This documentation
```

## Prerequisites

### Required Software
- **.NET 8.0 SDK** or later
- **PowerShell 7.0** or later (for test runner script)
- **Visual Studio 2022** or **VS Code** (optional, for debugging)

### Required Dependencies
The test project automatically references:
- `SmartTelehealth.API`
- `SmartTelehealth.Application`
- `SmartTelehealth.Core`
- `SmartTelehealth.Infrastructure`

### Test Dependencies
- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities

## Running Tests

### Option 1: PowerShell Test Runner (Recommended)

The PowerShell test runner provides a comprehensive testing experience with detailed reporting:

```powershell
# Run all tests
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1

# Run with specific configuration
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1 -Configuration Release

# Run with detailed output
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1 -Verbosity detailed

# Run only Stripe-related tests
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1 -Filter "Stripe"

# Run with code coverage
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1 -Coverage

# Show help
.\SmartTelehealth.Tests\run-comprehensive-tests.ps1 -Help
```

### Option 2: .NET CLI

```bash
# Navigate to test project
cd SmartTelehealth.Tests

# Run all tests
dotnet test

# Run with specific configuration
dotnet test --configuration Release

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~ComprehensiveSubscriptionTestSuite"

# Run with code coverage
dotnet test --collect "XPlat Code Coverage"
```

### Option 3: Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the test project in Solution Explorer
3. Select "Run Tests" or "Debug Tests"

## Test Categories

### 1. Subscription Creation and Purchase Tests
- `Test_Complete_Subscription_Creation_Workflow`
- `Test_Trial_Subscription_Creation`
- `Test_User_Purchase_Subscription_Flow`

### 2. Subscription Lifecycle Management Tests
- `Test_Subscription_Pause_Workflow`
- `Test_Subscription_Resume_Workflow`
- `Test_Subscription_Cancellation_Workflow`
- `Test_Subscription_Upgrade_Workflow`
- `Test_Subscription_Reactivation_Workflow`

### 3. Privilege Usage and Management Tests
- `Test_Privilege_Usage_Tracking`
- `Test_Privilege_Usage_Consumption`
- `Test_Privilege_Limit_Exceeded`

### 4. Payment Processing Tests
- `Test_Payment_Processing_End_To_End`
- `Test_Payment_Failure_Scenario`
- `Test_Payment_Method_Management`

### 5. Billing and Invoice Tests
- `Test_Billing_Record_Creation`
- `Test_Billing_Analytics`

### 6. Admin Management Tests
- `Test_Admin_Get_All_Subscriptions`
- `Test_Admin_Extend_Subscription`

### 7. Error Handling and Edge Cases
- `Test_Invalid_Subscription_Status_Transition`
- `Test_Subscription_Not_Found`
- `Test_Unauthorized_Access`

### 8. Integration and Workflow Tests
- `Test_Complete_Subscription_Lifecycle_End_To_End`
- `Test_Subscription_With_Privilege_Usage_Workflow`

### 9. Stripe Integration Tests
- `Test_Stripe_Subscription_Created_Webhook`
- `Test_Stripe_Subscription_Updated_Webhook`
- `Test_Stripe_Subscription_Deleted_Webhook`
- `Test_Stripe_Payment_Succeeded_Webhook`
- `Test_Stripe_Payment_Failed_Webhook`
- `Test_Stripe_Payment_Processing_Success`
- `Test_Stripe_Payment_Processing_Failure`
- `Test_Create_Stripe_Customer`
- `Test_Get_Stripe_Customer`
- `Test_Create_Stripe_Subscription`
- `Test_Cancel_Stripe_Subscription`
- `Test_Update_Stripe_Subscription`
- `Test_Add_Payment_Method`
- `Test_Remove_Payment_Method`
- `Test_Set_Default_Payment_Method`
- `Test_Invalid_Webhook_Event`
- `Test_Stripe_Service_Unavailable`

## Test Data and Mocking

### Test Data Setup
The test suite creates comprehensive test data including:
- **Test Users**: Regular users and administrators with different roles
- **Subscription Plans**: Basic and premium plans with various features
- **Billing Cycles**: Monthly and annual billing options
- **Privileges**: Consultation and messaging privileges with usage limits
- **Currencies**: USD currency configuration

### Mock Services
All external dependencies are mocked using Moq:
- `ISubscriptionService`
- `IBillingService`
- `IStripeService`
- `IPrivilegeService`
- `IAnalyticsService`
- `INotificationService`
- `IAuditService`
- `IUserService`
- And more...

### Controller Context Setup
Each test properly sets up controller context with:
- Authenticated user claims
- Proper role assignments
- HTTP context simulation

## Test Results and Reporting

### Test Output
- **Console Output**: Real-time test execution progress
- **TRX Files**: Visual Studio compatible test results
- **HTML Reports**: Detailed test execution reports
- **Coverage Reports**: Code coverage analysis (when enabled)

### Test Results Location
```
TestResults/
├── TestResults.trx           # Visual Studio test results
├── TestReport.html           # HTML test report
└── Coverage/                 # Code coverage reports (if enabled)
```

## Troubleshooting

### Common Issues

#### Build Failures
```bash
# Clean and restore
dotnet clean
dotnet restore

# Rebuild
dotnet build
```

#### Test Execution Failures
```bash
# Check test project builds
dotnet build SmartTelehealth.Tests

# Run with detailed output
dotnet test --verbosity detailed
```

#### Missing Dependencies
```bash
# Restore packages
dotnet restore

# Check package references
dotnet list package
```

### Debugging Tests

#### Visual Studio
1. Set breakpoints in test methods
2. Right-click test → "Debug Test"
3. Use Test Explorer for step-by-step execution

#### VS Code
1. Install C# extension
2. Set breakpoints in test files
3. Use F5 to debug tests

#### Command Line
```bash
# Run specific test with debug output
dotnet test --filter "TestName" --verbosity diagnostic
```

## Extending the Test Suite

### Adding New Tests

1. **Create Test Method**:
```csharp
[Fact]
public async Task Test_New_Feature()
{
    // Arrange
    // Setup test data and mocks
    
    // Act
    // Execute the functionality being tested
    
    // Assert
    // Verify expected outcomes
}
```

2. **Add to Appropriate Test Class**:
   - Use existing test classes for related functionality
   - Create new test classes for new feature areas

3. **Update Test Runner**:
   - Add new test categories to PowerShell script
   - Update test filtering options

### Adding New Test Data

1. **Extend Test Data Setup**:
```csharp
private void InitializeTestData()
{
    // Add new test entities
    _newEntity = new NewEntity
    {
        // Configure properties
    };
}
```

2. **Update Mock Setup**:
```csharp
_mockNewService.Setup(x => x.MethodAsync(It.IsAny<Parameter>()))
    .ReturnsAsync(expectedResult);
```

## Best Practices

### Test Design
- **Arrange-Act-Assert**: Follow the AAA pattern for test structure
- **Single Responsibility**: Each test should verify one specific behavior
- **Descriptive Names**: Use clear, descriptive test method names
- **Proper Assertions**: Use FluentAssertions for readable assertions

### Mocking
- **Minimal Mocking**: Only mock external dependencies
- **Realistic Data**: Use realistic test data that represents production scenarios
- **Proper Setup**: Ensure mocks are properly configured for each test

### Test Organization
- **Logical Grouping**: Group related tests in regions
- **Consistent Naming**: Use consistent naming conventions
- **Documentation**: Add comments for complex test scenarios

## Contributing

### Code Style
- Follow existing code formatting and naming conventions
- Use regions to organize test code logically
- Add XML documentation for complex test methods

### Test Coverage
- Aim for high test coverage of critical business logic
- Focus on edge cases and error scenarios
- Ensure all public APIs are tested

### Review Process
- All new tests should be reviewed by team members
- Ensure tests are maintainable and readable
- Verify that tests provide value and catch real issues

## Support and Maintenance

### Regular Maintenance
- **Update Dependencies**: Keep test dependencies up to date
- **Review Test Results**: Regularly review test execution results
- **Refactor Tests**: Improve test code quality over time

### Performance Considerations
- **Test Execution Time**: Monitor and optimize test execution time
- **Resource Usage**: Ensure tests don't consume excessive resources
- **Parallel Execution**: Consider parallel test execution for large test suites

## License

This test suite is part of the SmartTelehealth project and follows the same licensing terms.

---

For questions or issues with the test suite, please contact the development team or create an issue in the project repository.
