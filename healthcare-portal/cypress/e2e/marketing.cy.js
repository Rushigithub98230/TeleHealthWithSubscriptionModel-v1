describe('Marketing Home Page', () => {
  it('should display all plans, categories, and reviews', () => {
    cy.visit('/');
    cy.contains('All Plans');
    cy.get('.plan-card').should('have.length.greaterThan', 0);
    cy.contains('Categories');
    cy.get('.category-card').should('have.length.greaterThan', 0);
    cy.contains('User Reviews');
    cy.get('.review-card').should('have.length.greaterThan', 0);
  });
}); 