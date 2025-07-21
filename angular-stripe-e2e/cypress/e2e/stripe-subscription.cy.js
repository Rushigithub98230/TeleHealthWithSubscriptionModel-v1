describe('Stripe Subscription E2E', () => {
  it('should redirect to Stripe Checkout', () => {
    cy.visit('http://localhost:4201');
    cy.contains('Subscribe').click();
    // Wait for redirect to Stripe Checkout
    cy.url({ timeout: 20000 }).should('include', 'checkout.stripe.com');
  });
}); 