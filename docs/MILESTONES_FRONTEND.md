# Frontend Development Milestones

## Milestone 1: Initial User Journey Flow and Frontend Enhancements to Support the User Experience
- UI to display available plans (name, pricing, duration, features)
- Design the user flow for plan selection followed by a pre-check questionnaire submission on frontend side
- Implementation of flow for allowing patients to book their first appointment with a one-time charge (pay after the consultation). After that, they must subscribe to continue booking further appointments

## Milestone 2: Core Infrastructure & Initial User Flow and SuperAdmin Administration
- Subscription DB Schema: Tables for plans, subscriptions, billing, refund
- Plan Configuration Panel: Admin UI for plan setup
- Category-based Subscriptions: Users pick verticals (Hair Loss, Sexual Health, etc.)
- Category-based Subscriptions questionnaire
- Complete the initial subscription flow for Superadmin, including the development of all related screens
- Treatment Plan Creation: Store dosage, frequency, duration
- Finalize the Admin UI and implement all administrative operations for managing subscriptions

## Milestone 3: Frontend & Provider Portal Enhancements
- New Screens and Design Modifications
- Multi-Step Subscription Wizard: questionnaire Select Intake Checkout
- Category-Driven Home Page Design improvement: Visual cards
- Pause/Resume/Cancel subscription by patient
- Initial patient portal Modification to incorporate subscription module
- Patient Subscription Management and screens for Subscription Summary: View plans, shipments, consults
- Pause/Resume/Cancel subscription by superadmin
- Provider Action Center: Tasks for intakes, refills, messages
- Provider Dashboard Filters: Filter by category, status / Filter patients by subscription
- Notification Center: Alerts for billing, shipments, messages
- Mobile-First & Accessibility: Responsive
- Chat by Subscription: Separate threads per category/subscription

## Milestone 4: Billing, Medication & Core User Management
- Billing Engine: Recurring and upfront payments, bundle shipping
- Payment Gateways: Stripe
- HomeMed Pharmacy API: Fulfill prescriptions and manage refills
- Auto Medication Dispatch: Trigger HomeMed Pharmacy API
- Inframindica Symptom Checker: Pre-intake triage
- Admin Subscription Dashboard: Overview of plans, payments, shipments
- Invoice & Billing History: Downloadable PDFs and history view
- Delivery Rescheduling: Modify next shipment date or skip
- HomeMed eRx Integration: Send signed prescriptions electronically via the HomeMed Pharmacy API
- HomeMed Pharmacy Shipment Tracking

## Milestone 5: Advanced Integrations & UX Refinements
- Video Consult Tool: OpenTok changes
- Super Admin Controls: Global oversight and reports
- Audit & Compliance Logs: Immutable event records
- Symptom-Checker Entry: Reuse data for follow-ups
- Upsell/Upgrade Prompts: Show related subscription offers dynamically if any
- One-Time Consultation Option: Fee if no subscription purchased
- Pre-subscription Symptom Checker: Embedded Inframindica widget
- Delivery Tracker UI: Real-time shipment status and ETA (Estimated Time of Arrival)
- CCDA Document Export (FHIR/HL7/XML)

## Backend Support Status Analysis

### ✅ Backend Ready for Frontend Development
- **Subscription Management**: Complete API endpoints
- **Billing System**: Full Stripe integration
- **User Authentication**: JWT implementation
- **Category Management**: Complete CRUD operations
- **Provider Management**: Available
- **Consultation System**: Basic implementation
- **Notification System**: Email/SMS ready
- **File Storage**: Azure Blob integration
- **Audit Logging**: Available
- **Analytics**: Basic reporting

### ⚠️ Backend Needs Enhancement for Frontend Features
- **Video Consultation**: Basic implementation, needs OpenTok integration
- **Symptom Checker**: Not implemented (Inframindica integration needed)
- **Pharmacy Integration**: Not implemented (HomeMed API needed)
- **Delivery Tracking**: Basic implementation, needs external API
- **PDF Generation**: Not implemented for invoices
- **Advanced Analytics**: Basic implementation, needs enhancement 