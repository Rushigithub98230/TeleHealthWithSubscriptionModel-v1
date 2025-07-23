// Cypress E2E test for admin billing management

describe('Admin Billing Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list pending and overdue billing records', () => {
    cy.visit('/admin/billing-management');
    cy.get('.billing-management-table-responsive').should('exist');
    cy.get('.billing-table tbody tr').should('exist');
  });

  it('should open details modal for a billing record', () => {
    cy.get('.billing-table tbody tr').first().within(() => {
      cy.contains('Details').click();
    });
    cy.get('.modal').should('be.visible');
    cy.get('.modal').contains('Billing Record Details');
    cy.get('.modal button').contains('Close').click();
  });

  it('should process a refund and show toast', () => {
    cy.get('.billing-table tbody tr').first().within(() => {
      cy.contains('Details').click();
    });
    cy.get('.modal input[type=number]').type('10');
    cy.window().then(win => {
      cy.stub(win, 'confirm').returns(true);
    });
    cy.get('.modal button').contains('Process Refund').click();
    cy.get('.toast.success').should('be.visible').and('contain', 'Refund');
  });
}); 