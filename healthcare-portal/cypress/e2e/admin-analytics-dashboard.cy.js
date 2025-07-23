// Cypress E2E test for admin analytics dashboard

describe('Admin Analytics Dashboard', () => {
  before(() => {
    // Replace with actual login flow if needed
    cy.visit('/admin/login');
    cy.get('input[name=email]').type('admin@example.com');
    cy.get('input[name=password]').type('adminpassword');
    cy.get('button[type=submit]').click();
    cy.url().should('include', '/admin/dashboard');
  });

  it('should display key metrics in cards', () => {
    cy.visit('/admin/analytics');
    cy.get('.analytics-dashboard').should('exist');
    cy.get('.dashboard-cards mat-card').should('have.length.at.least', 1);
    cy.get('.dashboard-cards').should('contain', 'Total Subscriptions');
    cy.get('.dashboard-cards').should('contain', 'Total Revenue');
  });

  it('should render revenue, user growth, and plan distribution charts', () => {
    cy.get('.dashboard-charts canvas[aria-label="Revenue Trend Chart"]').should('exist');
    cy.get('.dashboard-charts canvas[aria-label="User Growth Chart"]').should('exist');
    cy.get('.dashboard-charts canvas[aria-label="Plan Distribution Chart"]').should('exist');
  });

  it('should export analytics as CSV', () => {
    cy.intercept('GET', '/api/Analytics/reports/subscriptions*').as('exportCsv');
    cy.get('button[aria-label="Export analytics as CSV"]').click();
    cy.wait('@exportCsv').its('response.statusCode').should('eq', 200);
  });

  it('should export analytics as PDF', () => {
    cy.intercept('GET', '/api/Analytics/reports/subscriptions*').as('exportPdf');
    cy.get('button[aria-label="Export analytics as PDF"]').click();
    cy.wait('@exportPdf').its('response.statusCode').should('eq', 200);
  });
}); 