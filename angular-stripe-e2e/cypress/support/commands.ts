/// <reference types="cypress" />
// ***********************************************
// This example commands.ts shows you how to
// create various custom commands and overwrite
// existing commands.
//
// For more comprehensive examples of custom
// commands please read more here:
// https://on.cypress.io/custom-commands
// ***********************************************
//
//
// -- This is a parent command --
// Cypress.Commands.add('login', (email, password) => { ... })
//
//
// -- This is a child command --
// Cypress.Commands.add('drag', { prevSubject: 'element'}, (subject, options) => { ... })
//
//
// -- This is a dual command --
// Cypress.Commands.add('dismiss', { prevSubject: 'optional'}, (subject, options) => { ... })
//
//
// -- This will overwrite an existing command --
// Cypress.Commands.overwrite('visit', (originalFn, url, options) => { ... })
//
// declare global {
//   namespace Cypress {
//     interface Chainable {
//       login(email: string, password: string): Chainable<void>
//       drag(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       dismiss(subject: string, options?: Partial<TypeOptions>): Chainable<Element>
//       visit(originalFn: CommandOriginalFn, url: string, options: Partial<VisitOptions>): Chainable<Element>
//     }
//   }
// }

// Custom command to get the body of the first iframe (for Stripe Elements)
Cypress.Commands.add('getStripeIframeBody', () => {
  // Wait for the iframe to be loaded
  return cy
    .get('iframe')
    .its('0.contentDocument.body').should('not.be.empty')
    .then(cy.wrap);
});

// Robust custom command to get the body of the Stripe card number iframe
Cypress.Commands.add('getStripeCardIframeBody', () => {
  // Find the iframe with name attribute containing 'privateStripeFrame' (Stripe Elements)
  return cy
    .get('iframe[name^="__privateStripeFrame"]', { timeout: 10000 })
    .first()
    .its('0.contentDocument.body').should('not.be.empty')
    .then(cy.wrap);
});