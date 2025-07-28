#!/bin/bash

echo "🧪 Setting up Subscription Testing Environment"
echo "=============================================="

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET is not installed. Please install .NET 8.0 first."
    exit 1
fi

# Check if Node.js is installed
if ! command -v node &> /dev/null; then
    echo "❌ Node.js is not installed. Please install Node.js first."
    exit 1
fi

# Check if Angular CLI is installed
if ! command -v ng &> /dev/null; then
    echo "❌ Angular CLI is not installed. Installing..."
    npm install -g @angular/cli
fi

echo "✅ Prerequisites check passed"

# Create test data script
echo "📝 Creating test data setup script..."
cat > setup-test-data.sql << 'EOF'
-- Test Data Setup Script
-- Run this in your SQL Server database

-- Insert test user
IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = '00000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO AspNetUsers (Id, UserName, Email, FirstName, LastName, IsActive, CreatedAt, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
    VALUES ('00000000-0000-0000-0000-000000000001', 'testuser', 'test@example.com', 'Test', 'User', 1, GETUTCDATE(), 1, 0, 0, 0, 0);
END

-- Insert test billing cycle
IF NOT EXISTS (SELECT 1 FROM MasterBillingCycles WHERE Id = '00000000-0000-0000-0000-000000000003')
BEGIN
    INSERT INTO MasterBillingCycles (Id, Name, DurationInMonths, IsActive, CreatedAt)
    VALUES ('00000000-0000-0000-0000-000000000003', 'month', 1, 1, GETUTCDATE());
END

-- Insert test currency
IF NOT EXISTS (SELECT 1 FROM MasterCurrencies WHERE Id = '00000000-0000-0000-0000-000000000004')
BEGIN
    INSERT INTO MasterCurrencies (Id, Name, Code, Symbol, IsActive, CreatedAt)
    VALUES ('00000000-0000-0000-0000-000000000004', 'US Dollar', 'USD', '$', 1, GETUTCDATE());
END

-- Insert test plan
IF NOT EXISTS (SELECT 1 FROM SubscriptionPlans WHERE Id = '00000000-0000-0000-0000-000000000002')
BEGIN
    INSERT INTO SubscriptionPlans (Id, Name, Description, Price, IsActive, BillingCycleId, CurrencyId, CreatedAt, IsTrialAllowed, TrialDurationInDays)
    VALUES ('00000000-0000-0000-0000-000000000002', 'Test Healthcare Plan', 'Comprehensive healthcare subscription plan for testing', 29.99, 1, 
            '00000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000004', GETUTCDATE(), 1, 7);
END

PRINT '✅ Test data setup completed successfully!'
EOF

echo "✅ Test data script created: setup-test-data.sql"

# Create Stripe test data script
echo "📝 Creating Stripe test data script..."
cat > setup-stripe-test-data.sh << 'EOF'
#!/bin/bash

# Stripe Test Data Setup Script
# Replace YOUR_STRIPE_SECRET_KEY with your actual Stripe secret key

STRIPE_SECRET_KEY="YOUR_STRIPE_SECRET_KEY"

echo "🧪 Setting up Stripe test data..."

# Create test customer
echo "Creating test customer..."
CUSTOMER_RESPONSE=$(curl -s -X POST https://api.stripe.com/v1/customers \
  -H "Authorization: Bearer $STRIPE_SECRET_KEY" \
  -d "email=test@example.com" \
  -d "name=Test Customer")

CUSTOMER_ID=$(echo $CUSTOMER_RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
echo "✅ Customer created: $CUSTOMER_ID"

# Create test payment method
echo "Creating test payment method..."
PAYMENT_METHOD_RESPONSE=$(curl -s -X POST https://api.stripe.com/v1/payment_methods \
  -H "Authorization: Bearer $STRIPE_SECRET_KEY" \
  -d "type=card" \
  -d "card[number]=4242424242424242" \
  -d "card[exp_month]=12" \
  -d "card[exp_year]=2024" \
  -d "card[cvc]=123")

PAYMENT_METHOD_ID=$(echo $PAYMENT_METHOD_RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
echo "✅ Payment method created: $PAYMENT_METHOD_ID"

# Attach payment method to customer
echo "Attaching payment method to customer..."
curl -s -X POST https://api.stripe.com/v1/payment_methods/$PAYMENT_METHOD_ID/attach \
  -H "Authorization: Bearer $STRIPE_SECRET_KEY" \
  -d "customer=$CUSTOMER_ID"

# Create test product
echo "Creating test product..."
PRODUCT_RESPONSE=$(curl -s -X POST https://api.stripe.com/v1/products \
  -H "Authorization: Bearer $STRIPE_SECRET_KEY" \
  -d "name=Test Healthcare Plan" \
  -d "description=Comprehensive healthcare subscription plan")

PRODUCT_ID=$(echo $PRODUCT_RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
echo "✅ Product created: $PRODUCT_ID"

# Create test price
echo "Creating test price..."
PRICE_RESPONSE=$(curl -s -X POST https://api.stripe.com/v1/prices \
  -H "Authorization: Bearer $STRIPE_SECRET_KEY" \
  -d "product=$PRODUCT_ID" \
  -d "unit_amount=2999" \
  -d "currency=usd" \
  -d "recurring[interval]=month")

PRICE_ID=$(echo $PRICE_RESPONSE | grep -o '"id":"[^"]*"' | cut -d'"' -f4)
echo "✅ Price created: $PRICE_ID"

echo ""
echo "🎉 Stripe test data setup completed!"
echo ""
echo "📋 Test Data Summary:"
echo "Customer ID: $CUSTOMER_ID"
echo "Payment Method ID: $PAYMENT_METHOD_ID"
echo "Product ID: $PRODUCT_ID"
echo "Price ID: $PRICE_ID"
echo ""
echo "💡 Copy these IDs to your testing dashboard configuration"
EOF

chmod +x setup-stripe-test-data.sh
echo "✅ Stripe test data script created: setup-stripe-test-data.sh"

# Create environment setup script
echo "📝 Creating environment setup script..."
cat > start-testing-environment.sh << 'EOF'
#!/bin/bash

echo "🚀 Starting Subscription Testing Environment"
echo "=========================================="

# Function to check if a port is in use
check_port() {
    if lsof -Pi :$1 -sTCP:LISTEN -t >/dev/null ; then
        echo "❌ Port $1 is already in use"
        return 1
    else
        echo "✅ Port $1 is available"
        return 0
    fi
}

# Check ports
echo "🔍 Checking port availability..."
check_port 7001 || exit 1
check_port 4200 || exit 1

echo ""
echo "📦 Starting backend..."
cd backend
dotnet run --project SmartTelehealth.API &
BACKEND_PID=$!

echo "📦 Starting frontend..."
cd ../healthcare-portal
ng serve &
FRONTEND_PID=$!

echo ""
echo "⏳ Waiting for services to start..."
sleep 10

echo ""
echo "🎉 Services started successfully!"
echo ""
echo "📋 Access URLs:"
echo "Frontend: http://localhost:4200"
echo "Backend: https://localhost:7001"
echo "Testing Dashboard: http://localhost:4200/subscription-testing"
echo ""
echo "📝 Next steps:"
echo "1. Run setup-test-data.sql in your database"
echo "2. Update setup-stripe-test-data.sh with your Stripe secret key"
echo "3. Run ./setup-stripe-test-data.sh to create Stripe test data"
echo "4. Navigate to the testing dashboard and configure test data"
echo ""
echo "🛑 To stop services, press Ctrl+C"

# Wait for user to stop
wait

# Cleanup
echo "🧹 Cleaning up..."
kill $BACKEND_PID 2>/dev/null
kill $FRONTEND_PID 2>/dev/null
echo "✅ Services stopped"
EOF

chmod +x start-testing-environment.sh
echo "✅ Environment setup script created: start-testing-environment.sh"

echo ""
echo "🎉 Setup completed successfully!"
echo ""
echo "📋 Next steps:"
echo "1. Run setup-test-data.sql in your database"
echo "2. Update setup-stripe-test-data.sh with your Stripe secret key"
echo "3. Run ./setup-stripe-test-data.sh to create Stripe test data"
echo "4. Run ./start-testing-environment.sh to start both services"
echo "5. Navigate to http://localhost:4200/subscription-testing"
echo ""
echo "📖 For detailed instructions, see SUBSCRIPTION_TESTING_GUIDE.md" 