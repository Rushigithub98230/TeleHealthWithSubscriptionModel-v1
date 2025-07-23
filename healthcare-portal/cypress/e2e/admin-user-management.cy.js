// Cypress E2E test for admin user management

describe('Admin User Management', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should list users', () => {
    cy.visit('/admin/user-management');
    cy.get('.user-management-table-responsive').should('exist');
    cy.get('.user-table tbody tr').should('exist');
  });

  it('should add a user and show toast', () => {
    cy.contains('Add User').click();
    cy.window().then(win => {
      cy.stub(win, 'prompt')
        .onFirstCall().returns('Test User')
        .onSecondCall().returns('testuser@example.com')
        .onThirdCall().returns('Admin')
        .onCall(3).returns('Active');
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'added');
  });

  it('should edit a user and show toast', () => {
    cy.get('.user-table tbody tr').first().within(() => {
      cy.contains('Edit').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'prompt')
        .onFirstCall().returns('Edited User')
        .onSecondCall().returns('editeduser@example.com')
        .onThirdCall().returns('Provider')
        .onCall(3).returns('Inactive');
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'updated');
  });

  it('should assign a role and show toast', () => {
    cy.get('.user-table tbody tr').first().within(() => {
      cy.contains('Assign Role').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'prompt').returns('Provider');
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'Role assigned');
  });

  it('should delete a user and show toast', () => {
    cy.get('.user-table tbody tr').first().within(() => {
      cy.contains('Delete').click();
    });
    cy.window().then(win => {
      cy.stub(win, 'confirm').returns(true);
    });
    cy.get('.toast.success').should('be.visible').and('contain', 'deleted');
  });

  it('should filter users by role', () => {
    cy.visit('/admin/user-management');
    cy.get('select[formcontrolname=role]').select('Admin');
    cy.get('.user-management-table-responsive').should('contain', 'Admin');
  });

  it('should filter users by status', () => {
    cy.get('select[formcontrolname=status]').select('Active');
    cy.get('.user-management-table-responsive').should('contain', 'Active');
  });

  it('should filter users by search term', () => {
    cy.get('input[formcontrolname=search]').clear().type('alice');
    cy.get('.user-management-table-responsive').should('contain', 'alice');
  });

  it('should perform bulk delete on selected users', () => {
    cy.get('app-global-table input[type=checkbox]').eq(1).check();
    cy.get('app-global-table input[type=checkbox]').eq(2).check();
    cy.get('select').contains('Bulk Actions').parent().select('delete');
    cy.window().then(win => {
      cy.stub(win, 'confirm').returns(true);
    });
    cy.contains('Apply').click();
    cy.get('.toast.success').should('be.visible').and('contain', 'deleted');
  });

  it('should perform bulk activate on selected users', () => {
    cy.get('app-global-table input[type=checkbox]').eq(1).check();
    cy.get('app-global-table input[type=checkbox]').eq(2).check();
    cy.get('select').contains('Bulk Actions').parent().select('activate');
    cy.contains('Apply').click();
    cy.get('.toast.success').should('be.visible').and('contain', 'updated');
  });

  it('should perform bulk assign role on selected users', () => {
    cy.get('app-global-table input[type=checkbox]').eq(1).check();
    cy.get('app-global-table input[type=checkbox]').eq(2).check();
    cy.get('select').contains('Bulk Actions').parent().select('assignRole');
    cy.window().then(win => {
      cy.stub(win, 'prompt').returns('Provider');
    });
    cy.contains('Apply').click();
    cy.get('.toast.success').should('be.visible').and('contain', 'Role assigned');
  });
}); 