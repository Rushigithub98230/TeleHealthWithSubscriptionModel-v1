#!/bin/bash
# Sample deployment script for TeleHealth Admin Portal (Angular)
# Usage: ./deploy-admin-portal.sh

set -e

# 1. Install dependencies
npm install

# 2. Run unit tests
ng test --watch=false --browsers=ChromeHeadless

# 3. Run E2E tests
npx cypress run

# 4. Build for production
ng build --configuration production

# 5. Deploy to static host (replace with your actual deployment command)
# Example: AWS S3
# aws s3 sync dist/healthcare-portal s3://your-bucket-name --delete

# Example: Azure Static Web Apps
# az storage blob upload-batch -d '$web' -s dist/healthcare-portal --account-name <your-storage-account>

# Example: Vercel (if using Vercel CLI)
# vercel --prod

echo "Deployment complete!" 