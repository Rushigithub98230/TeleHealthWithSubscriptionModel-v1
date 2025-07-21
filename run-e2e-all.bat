@echo off
REM Start the .NET backend
start "Backend" cmd /k "cd src\SmartTelehealth.API && dotnet run --urls=http://localhost:5000"

REM Start the Angular frontend
start "Frontend" cmd /k "cd angular-stripe-e2e && ng serve --port 4201"

REM Wait for user to confirm both are running
echo.
echo Please wait for both backend (http://localhost:5000) and frontend (http://localhost:4201) to be fully started.
echo When both are running, press any key to continue to Cypress E2E test...
pause

REM Run Cypress E2E test
cd angular-stripe-e2e
npx cypress run --spec cypress/e2e/stripe-subscription.cy.js

REM End
cd ..
echo.
echo All done! Check your Stripe dashboard for test subscriptions.
pause 