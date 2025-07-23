describe('Patient Login', () => {
  it('should allow a patient to log in and see their dashboard', () => {
    cy.visit('/patient/login');
    cy.get('input[name="username"]').type('testpatient');
    cy.get('input[name="password"]').type('password123');
    cy.get('button[type="submit"]').click();
    cy.contains('Welcome, Patient!');
  });
}); 