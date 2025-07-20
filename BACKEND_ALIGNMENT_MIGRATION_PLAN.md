# Backend Alignment & Migration Plan

This document outlines the step-by-step plan to align the backend with the new, enhanced subscription management schema for the healthcare application.

---

## 1. Database Schema Migration
- **Update Entities:** Refactor all relevant entity classes to match the new schema (master tables, FKs, new fields).
- **Update DbContext:** Add new DbSets and configure relationships for master tables and new FKs.
- **Generate Migration:** Use EF Core to create a migration for the new schema.
- **Apply Migration:** Update the database to the new schema.

## 2. DTOs & Mapping Profiles
- **Refactor DTOs:** Update all DTOs (plan, subscription, privilege, etc.) to use new fields (Price, BillingCycleId, CurrencyId, etc.).
- **Update AutoMapper Profiles:** Ensure all mappings reflect the new schema.

## 3. Repositories
- **Update Queries:** Refactor repository methods to use new FKs and master tables.
- **Remove Old Logic:** Eliminate all references to removed fields and enums.

## 4. Services
- **Refactor Business Logic:** Update all service logic for plan creation, privilege assignment, usage tracking, billing, and payment to use the new schema.
- **Align with New DTOs:** Ensure all service methods use updated DTOs and entities.

## 5. Controllers
- **Update Endpoints:** Refactor all API endpoints to use new DTOs and business logic.
- **Align with New Schema:** Ensure all controller actions are compatible with the new backend structure.

## 6. Tests
- **Update Test Data:** Refactor test data and integration tests to use the new schema.
- **Validate All Flows:** Ensure all subscription scenarios (creation, usage, billing, add-ons, upgrades, reporting, etc.) are tested.

## 7. Build & Validate
- **Rebuild Solution:** Ensure the codebase compiles without errors.
- **Run Tests:** Validate all tests pass.
- **Manual Testing:** Test all critical subscription flows in the app.

---

## Execution Order
1. Entities & DbContext
2. Migration (EF Core)
3. DTOs & Mapping Profiles
4. Repositories
5. Services
6. Controllers
7. Tests
8. Build & Validate 