// Cypress E2E test for admin notifications management

describe('Admin Notifications Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list notifications', () => {
    cy.visit('/admin/notifications-management');
    cy.get('.notifications-management').should('exist');
    cy.get('app-global-table').should('exist');
    cy.get('app-global-table table tbody tr').should('exist');
  });

  it('should filter notifications by user', () => {
    cy.get('input[formcontrolname=userId]').clear().type('u1');
    cy.get('app-global-table').should('contain', 'u1');
  });

  it('should filter notifications by title', () => {
    cy.get('input[formcontrolname=title]').clear().type('Payment');
    cy.get('app-global-table').should('contain', 'Payment');
  });

  it('should paginate notifications', () => {
    cy.get('mat-paginator').should('exist');
    cy.get('mat-paginator button[aria-label="Next page"]').click();
    cy.get('app-global-table').should('exist');
  });

  it('should open details modal for a notification', () => {
    cy.get('app-global-table table tbody tr').first().click();
    cy.get('.modal[role=dialog]').should('be.visible');
    cy.get('.modal-content').should('contain', 'Title');
    cy.get('.close-btn').click();
    cy.get('.modal[role=dialog]').should('not.exist');
  });
}); 