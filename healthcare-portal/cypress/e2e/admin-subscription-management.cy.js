// Cypress E2E test for admin subscription management

describe('Admin Subscription Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list subscriptions and search', () => {
    cy.visit('/admin/subscription-management');
    cy.get('.subscription-management-table-responsive').should('exist');
    cy.get('input[placeholder*="Search"]').type('active');
    cy.get('.subscriptions-table tbody tr').should('exist');
  });

  it('should open details modal for a subscription', () => {
    cy.get('.subscriptions-table tbody tr').first().within(() => {
      cy.contains('Details').click();
    });
    cy.get('.modal').should('be.visible');
    cy.get('.modal').contains('Subscription Details');
    cy.get('.modal button').contains('Close').click();
  });

  it('should cancel a subscription and show toast', () => {
    cy.get('.subscriptions-table tbody tr').first().within(() => {
      cy.contains('Cancel').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'prompt').returns('Test cancellation reason');
      cy.stub(win, 'confirm').returns(true);
    });
    // After action, toast should appear
    cy.get('.toast.success').should('be.visible').and('contain', 'cancelled');
  });

  it('should bulk cancel subscriptions and show summary toast', () => {
    cy.visit('/admin/subscription-management');
    cy.get('.subscriptions-table tbody tr').eq(0).find('input[type=checkbox]').check({ force: true });
    cy.get('.subscriptions-table tbody tr').eq(1).find('input[type=checkbox]').check({ force: true });
    cy.get('.bulk-actions-bar').should('exist');
    cy.get('.bulk-actions-bar button').contains('Bulk Cancel').click();
    cy.on('window:confirm', () => true);
    cy.get('.toast').should('be.visible').and('contain', 'Bulk cancel complete');
    // Optionally, check that the subscriptions are updated/removed
  });

  it('should filter subscriptions by status, planId, and userId', () => {
    cy.visit('/admin/subscription-management');
    cy.get('.filter-panel').should('exist');
    // Filter by status
    cy.get('.filter-panel mat-select[formcontrolname="status"]').click();
    cy.get('mat-option').contains('Active').click();
    cy.get('.subscriptions-table tbody tr').each($row => {
      cy.wrap($row).contains('Active');
    });
    // Filter by planId
    cy.get('.filter-panel input[formcontrolname="planId"]').type('plan1');
    cy.get('.subscriptions-table tbody tr').each($row => {
      cy.wrap($row).contains('plan1');
    });
    // Filter by userId
    cy.get('.filter-panel input[formcontrolname="userId"]').type('user1');
    cy.get('.subscriptions-table tbody tr').each($row => {
      cy.wrap($row).contains('user1');
    });
    // Clear all filters
    cy.get('.filter-panel button').contains('Clear All Filters').click();
    cy.get('.subscriptions-table tbody tr').should('have.length.greaterThan', 0);
  });
}); 