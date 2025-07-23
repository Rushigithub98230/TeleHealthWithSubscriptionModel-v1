// Cypress E2E test for admin plan management

describe('Admin Plan Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list plans', () => {
    cy.visit('/admin/subscription-plans-management');
    cy.get('.plans-management-table-responsive').should('exist');
    cy.get('.plans-table tbody tr').should('exist');
  });

  it('should add a plan and show toast', () => {
    cy.contains('Add Plan').click();
    cy.window().then(win => {
      cy.stub(win, 'prompt')
        .onFirstCall().returns('Test Plan')
        .onSecondCall().returns('99.99')
        .onThirdCall().returns('1 Month')
        .onCall(3).returns('Active');
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'added');
  });

  it('should edit a plan and show toast', () => {
    cy.get('.plans-table tbody tr').first().within(() => {
      cy.contains('Edit').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'prompt')
        .onFirstCall().returns('Edited Plan')
        .onSecondCall().returns('199.99')
        .onThirdCall().returns('3 Months')
        .onCall(3).returns('Inactive');
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'updated');
  });

  it('should delete a plan and show toast', () => {
    cy.get('.plans-table tbody tr').first().within(() => {
      cy.contains('Delete').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'confirm').returns(true);
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'deleted');
  });
}); 