describe('Admin Login and Dashboard', () => {
  it('should login and show dashboard', () => {
    cy.visit('/admin/login');
    cy.get('input[name=username]').type('admin');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
    cy.contains('Admin Dashboard');
  });
}); 