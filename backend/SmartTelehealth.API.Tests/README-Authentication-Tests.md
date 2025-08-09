# Authentication System Test Suite

## Overview

This test suite provides comprehensive coverage for the authentication system, ensuring security, reliability, and proper error handling. The tests cover all authentication scenarios including login, registration, password recovery, and security features.

## Test Categories

### 1. Login Tests
- **Valid Credentials**: Tests successful login with correct email and password
- **Invalid Credentials**: Tests failed login attempts with wrong credentials
- **Empty Fields**: Tests validation for empty email and password fields
- **SQL Injection Attempts**: Tests security against SQL injection attacks

### 2. Registration Tests
- **Valid Registration**: Tests successful user registration with valid data
- **Existing Email**: Tests duplicate email validation
- **Weak Password**: Tests password strength requirements
- **Invalid Role**: Tests role validation (only Client, Admin, Provider allowed)
- **Valid Roles**: Tests registration with each valid role
- **XSS Protection**: Tests protection against XSS attacks

### 3. Password Recovery Tests
- **Forgot Password**: Tests password reset request functionality
- **Non-existent Email**: Tests security (doesn't reveal if email exists)
- **Reset Password**: Tests password reset with valid token
- **Weak Reset Password**: Tests password strength validation during reset

### 4. Password Change Tests
- **Valid Password Change**: Tests authenticated password change
- **Weak New Password**: Tests password strength validation during change

### 5. Logout Tests
- **Authenticated Logout**: Tests successful logout for authenticated users

### 6. Token Refresh Tests
- **Valid Token Refresh**: Tests JWT token refresh functionality

### 7. Security Tests
- **SQL Injection Protection**: Tests against SQL injection attempts
- **XSS Protection**: Tests against XSS attack attempts

### 8. Password Strength Tests
- **Strong Passwords**: Tests various strong password combinations
- **Weak Passwords**: Tests rejection of weak passwords
- **Edge Cases**: Tests null, empty, and invalid password inputs

### 9. Rate Limiting Tests
- **Multiple Rapid Attempts**: Tests rate limiting for rapid login attempts

### 10. Error Handling Tests
- **Service Exceptions**: Tests proper error handling when services fail
- **Database Errors**: Tests graceful handling of database connection failures

## Running the Tests

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Test runner (built into Visual Studio or use `dotnet test`)

### Command Line
```bash
# Navigate to the test project directory
cd backend/SmartTelehealth.API.Tests

# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run specific test category
dotnet test --filter "Category=Login"

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Visual Studio
1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Run all tests or specific test categories
4. View detailed results and coverage

## Test Data

### Valid Test Users
```json
{
  "Client": {
    "email": "client@example.com",
    "password": "ClientPass123!",
    "role": "Client"
  },
  "Admin": {
    "email": "admin@example.com", 
    "password": "AdminPass123!",
    "role": "Admin"
  },
  "Provider": {
    "email": "provider@example.com",
    "password": "ProviderPass123!",
    "role": "Provider"
  }
}
```

### Strong Password Examples
- `StrongPass123!` ✅
- `Abcdef1!` ✅
- `MySecureP@ss1` ✅

### Weak Password Examples
- `weak` ❌ (too short)
- `12345678` ❌ (no letters)
- `abcdefgh` ❌ (no uppercase, numbers, or special chars)
- `ABCDEFGH` ❌ (no lowercase, numbers, or special chars)

## Security Features Tested

### 1. Password Strength Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one number
- At least one special character

### 2. Role Validation
- Only allows: Client, Admin, Provider
- Rejects invalid roles with clear error messages

### 3. Input Validation
- Email format validation
- Required field validation
- Password confirmation matching

### 4. Security Measures
- SQL injection protection
- XSS attack protection
- Rate limiting (framework level)
- Secure error messages (don't reveal user existence)

### 5. JWT Token Security
- Proper token generation with claims
- Token refresh functionality
- Secure token storage and transmission

## Expected Test Results

### All Tests Should Pass
- ✅ Login Tests: 4/4
- ✅ Registration Tests: 6/6  
- ✅ Password Recovery Tests: 4/4
- ✅ Password Change Tests: 2/2
- ✅ Logout Tests: 1/1
- ✅ Token Refresh Tests: 1/1
- ✅ Security Tests: 2/2
- ✅ Password Strength Tests: 12/12
- ✅ Rate Limiting Tests: 1/1
- ✅ Error Handling Tests: 2/2

**Total: 35/35 tests passing**

## Coverage Goals

- **Line Coverage**: >95%
- **Branch Coverage**: >90%
- **Function Coverage**: 100%

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run Authentication Tests
  run: |
    cd backend/SmartTelehealth.API.Tests
    dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## Troubleshooting

### Common Issues

1. **Test Project Not Building**
   - Ensure all project references are correct
   - Check that all required packages are installed

2. **Mock Setup Issues**
   - Verify that all service interfaces are properly mocked
   - Check that configuration mocks are set up correctly

3. **Authentication Context Issues**
   - Ensure user claims are properly set up for authenticated tests
   - Verify JWT configuration is mocked correctly

### Debug Mode
```bash
# Run tests in debug mode
dotnet test --verbosity detailed --logger "console;verbosity=detailed"
```

## Maintenance

### Adding New Tests
1. Follow the existing naming convention: `MethodName_Scenario_ExpectedResult`
2. Use appropriate test categories with `#region` blocks
3. Include both positive and negative test cases
4. Add security tests for any new authentication features

### Updating Tests
1. Update tests when authentication logic changes
2. Ensure all edge cases are covered
3. Maintain security test coverage
4. Update documentation when adding new features

## Security Considerations

### Test Data Security
- Never use real credentials in tests
- Use mock data for all authentication scenarios
- Ensure test data doesn't contain sensitive information

### Test Isolation
- Each test should be independent
- Use fresh mocks for each test
- Clean up any test data after tests complete

### Security Testing
- Always test both positive and negative security scenarios
- Include attack vector testing (SQL injection, XSS, etc.)
- Test rate limiting and brute force protection
