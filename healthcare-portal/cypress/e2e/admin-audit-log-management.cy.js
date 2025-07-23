// Cypress E2E test for admin audit log management

describe('Admin Audit Log Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list audit logs', () => {
    cy.visit('/admin/audit-log-management');
    cy.get('.audit-log-management').should('exist');
    cy.get('app-global-table').should('exist');
    cy.get('app-global-table table tbody tr').should('exist');
  });

  it('should filter logs by user', () => {
    cy.get('input[formcontrolname=userId]').clear().type('u1');
    cy.get('app-global-table').should('contain', 'u1');
  });

  it('should filter logs by action', () => {
    cy.get('input[formcontrolname=action]').clear().type('Login');
    cy.get('app-global-table').should('contain', 'Login');
  });

  it('should filter logs by entity type', () => {
    cy.get('input[formcontrolname=entityType]').clear().type('User');
    cy.get('app-global-table').should('contain', 'User');
  });

  it('should filter logs by date range', () => {
    cy.get('input[formcontrolname=startDate]').type('2024-07-20');
    cy.get('input[formcontrolname=endDate]').type('2024-07-20');
    cy.get('app-global-table').should('exist');
  });

  it('should paginate audit logs', () => {
    cy.get('mat-paginator').should('exist');
    cy.get('mat-paginator button[aria-label="Next page"]').click();
    cy.get('app-global-table').should('exist');
  });

  it('should open details modal for a log entry', () => {
    cy.get('app-global-table table tbody tr').first().click();
    cy.get('.modal[role=dialog]').should('be.visible');
    cy.get('.modal-content').should('contain', 'Audit Log Details');
    cy.get('.close-btn').click();
    cy.get('.modal[role=dialog]').should('not.exist');
  });
}); 