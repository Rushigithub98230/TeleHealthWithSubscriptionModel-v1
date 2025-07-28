import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../../environments/environment';

interface TestResult {
  testName: string;
  status: 'pending' | 'success' | 'error';
  message: string;
  data?: any;
  timestamp: Date;
}

interface StripeTestData {
  customerId: string;
  paymentMethodId: string;
  productId: string;
  priceId: string;
  subscriptionId: string;
  webhookSecret: string;
}

@Component({
  selector: 'app-subscription-testing',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  template: `
    <div class="container mx-auto p-6">
      <div class="bg-white rounded-lg shadow-lg p-6">
        <h1 class="text-3xl font-bold text-gray-800 mb-6">Subscription Management Testing Dashboard</h1>
        
        <!-- Test Configuration -->
        <div class="mb-8 p-4 bg-blue-50 rounded-lg">
          <h2 class="text-xl font-semibold mb-4">Test Configuration</h2>
          <form [formGroup]="configForm" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-gray-700">API Base URL</label>
              <input type="text" formControlName="apiBaseUrl" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Stripe Customer ID</label>
              <input type="text" formControlName="customerId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Payment Method ID</label>
              <input type="text" formControlName="paymentMethodId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Product ID</label>
              <input type="text" formControlName="productId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Price ID</label>
              <input type="text" formControlName="priceId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">User ID</label>
              <input type="text" formControlName="userId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Plan ID</label>
              <input type="text" formControlName="planId" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
            <div>
              <label class="block text-sm font-medium text-gray-700">Webhook Secret</label>
              <input type="text" formControlName="webhookSecret" 
                     class="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500">
            </div>
          </form>
        </div>

        <!-- Test Categories -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          
          <!-- Stripe Service Tests -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h3 class="text-lg font-semibold mb-4 text-gray-800">Stripe Service Tests</h3>
            <div class="space-y-2">
              <button (click)="testCreateCustomer()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Create Customer
              </button>
              <button (click)="testCreateProduct()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Create Product
              </button>
              <button (click)="testCreatePrice()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Create Price
              </button>
              <button (click)="testCreateSubscription()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Create Subscription
              </button>
              <button (click)="testGetSubscription()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Get Subscription
              </button>
              <button (click)="testCancelSubscription()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Cancel Subscription
              </button>
              <button (click)="testPauseSubscription()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Pause Subscription
              </button>
              <button (click)="testResumeSubscription()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Resume Subscription
              </button>
              <button (click)="testProcessPayment()" 
                      class="w-full bg-blue-500 text-white px-4 py-2 rounded hover:bg-blue-600">
                Test Process Payment
              </button>
            </div>
          </div>

          <!-- Subscription Service Tests -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h3 class="text-lg font-semibold mb-4 text-gray-800">Subscription Service Tests</h3>
            <div class="space-y-2">
              <button (click)="testCreateSubscriptionService()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Create Subscription (Service)
              </button>
              <button (click)="testGetUserSubscriptions()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Get User Subscriptions
              </button>
              <button (click)="testCancelSubscriptionService()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Cancel Subscription (Service)
              </button>
              <button (click)="testPauseSubscriptionService()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Pause Subscription (Service)
              </button>
              <button (click)="testResumeSubscriptionService()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Resume Subscription (Service)
              </button>
              <button (click)="testGetUsageStatistics()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Get Usage Statistics
              </button>
              <button (click)="testProcessPaymentService()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Process Payment (Service)
              </button>
              <button (click)="testGetBillingHistory()" 
                      class="w-full bg-green-500 text-white px-4 py-2 rounded hover:bg-green-600">
                Test Get Billing History
              </button>
            </div>
          </div>

          <!-- Plan Management Tests -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h3 class="text-lg font-semibold mb-4 text-gray-800">Plan Management Tests</h3>
            <div class="space-y-2">
              <button (click)="testCreatePlan()" 
                      class="w-full bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600">
                Test Create Plan
              </button>
              <button (click)="testGetAllPlans()" 
                      class="w-full bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600">
                Test Get All Plans
              </button>
              <button (click)="testGetActivePlans()" 
                      class="w-full bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600">
                Test Get Active Plans
              </button>
              <button (click)="testUpdatePlan()" 
                      class="w-full bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600">
                Test Update Plan
              </button>
              <button (click)="testDeletePlan()" 
                      class="w-full bg-purple-500 text-white px-4 py-2 rounded hover:bg-purple-600">
                Test Delete Plan
              </button>
            </div>
          </div>

          <!-- Webhook & Integration Tests -->
          <div class="bg-gray-50 rounded-lg p-4">
            <h3 class="text-lg font-semibold mb-4 text-gray-800">Webhook & Integration Tests</h3>
            <div class="space-y-2">
              <button (click)="testWebhookPaymentSucceeded()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Webhook: Payment Succeeded
              </button>
              <button (click)="testWebhookPaymentFailed()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Webhook: Payment Failed
              </button>
              <button (click)="testWebhookSubscriptionCreated()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Webhook: Subscription Created
              </button>
              <button (click)="testWebhookSubscriptionCancelled()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Webhook: Subscription Cancelled
              </button>
              <button (click)="testCheckoutSession()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Create Checkout Session
              </button>
              <button (click)="testGetPaymentMethods()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Get Payment Methods
              </button>
              <button (click)="testCreateTestPaymentMethod()" 
                      class="w-full bg-orange-500 text-white px-4 py-2 rounded hover:bg-orange-600">
                Test Create Test Payment Method
              </button>
            </div>
          </div>

        </div>

        <!-- Test Results -->
        <div class="mt-8">
          <h2 class="text-xl font-semibold mb-4">Test Results</h2>
          <div class="space-y-2 max-h-96 overflow-y-auto">
            <div *ngFor="let result of testResults" 
                 [class]="getResultClass(result.status)"
                 class="p-3 rounded border">
              <div class="flex justify-between items-start">
                <div>
                  <strong>{{ result.testName }}</strong>
                  <p class="text-sm mt-1">{{ result.message }}</p>
                  <p class="text-xs text-gray-500">{{ result.timestamp | date:'medium' }}</p>
                </div>
                <span [class]="getStatusClass(result.status)" 
                      class="px-2 py-1 rounded text-xs font-medium">
                  {{ result.status.toUpperCase() }}
                </span>
              </div>
              <pre *ngIf="result.data" class="mt-2 text-xs bg-gray-100 p-2 rounded overflow-x-auto">
                {{ result.data | json }}
              </pre>
            </div>
          </div>
        </div>

        <!-- Clear Results -->
        <div class="mt-4">
          <button (click)="clearResults()" 
                  class="bg-red-500 text-white px-4 py-2 rounded hover:bg-red-600">
            Clear All Results
          </button>
        </div>

      </div>
    </div>
  `,
  styles: [`
    .success { @apply bg-green-50 border-green-200; }
    .error { @apply bg-red-50 border-red-200; }
    .pending { @apply bg-yellow-50 border-yellow-200; }
    
    .status-success { @apply bg-green-100 text-green-800; }
    .status-error { @apply bg-red-100 text-red-800; }
    .status-pending { @apply bg-yellow-100 text-yellow-800; }
  `]
})
export class SubscriptionTestingComponent implements OnInit {
  configForm: FormGroup;
  testResults: TestResult[] = [];
  currentSubscriptionId: string = '';
  currentCustomerId: string = '';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient
  ) {
    this.configForm = this.fb.group({
      apiBaseUrl: [environment.apiUrl || 'https://localhost:7001', Validators.required],
      customerId: ['', Validators.required],
      paymentMethodId: ['', Validators.required],
      productId: ['', Validators.required],
      priceId: ['', Validators.required],
      userId: ['', Validators.required],
      planId: ['', Validators.required],
      webhookSecret: ['', Validators.required]
    });
  }

  ngOnInit() {
    // Load saved configuration
    const savedConfig = localStorage.getItem('subscription-test-config');
    if (savedConfig) {
      this.configForm.patchValue(JSON.parse(savedConfig));
    }

    // Auto-save configuration changes
    this.configForm.valueChanges.subscribe(value => {
      localStorage.setItem('subscription-test-config', JSON.stringify(value));
    });
  }

  // Helper methods
  private addResult(testName: string, status: 'pending' | 'success' | 'error', message: string, data?: any) {
    this.testResults.unshift({
      testName,
      status,
      message,
      data,
      timestamp: new Date()
    });
  }

  public getResultClass(status: string): string {
    switch (status) {
      case 'success': return 'success';
      case 'error': return 'error';
      case 'pending': return 'pending';
      default: return '';
    }
  }

  public getStatusClass(status: string): string {
    switch (status) {
      case 'success': return 'status-success';
      case 'error': return 'status-error';
      case 'pending': return 'status-pending';
      default: return '';
    }
  }

  clearResults() {
    this.testResults = [];
  }

  // Stripe Service Tests
  async testCreateCustomer() {
    this.addResult('Create Customer', 'pending', 'Creating Stripe customer...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/create-customer`, {
        email: 'test@example.com',
        name: 'Test Customer'
      }).toPromise();
      
      this.addResult('Create Customer', 'success', 'Customer created successfully', response);
      this.currentCustomerId = (response as any).customerId;
      this.configForm.patchValue({ customerId: this.currentCustomerId });
    } catch (error: any) {
      this.addResult('Create Customer', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCreateProduct() {
    this.addResult('Create Product', 'pending', 'Creating Stripe product...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/create-product`, {
        name: 'Test Healthcare Plan',
        description: 'Comprehensive healthcare subscription plan'
      }).toPromise();
      
      this.addResult('Create Product', 'success', 'Product created successfully', response);
      this.configForm.patchValue({ productId: (response as any).productId });
    } catch (error: any) {
      this.addResult('Create Product', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCreatePrice() {
    this.addResult('Create Price', 'pending', 'Creating Stripe price...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/create-price`, {
        productId: this.configForm.value.productId,
        amount: 29.99,
        currency: 'usd',
        interval: 'month',
        intervalCount: 1
      }).toPromise();
      
      this.addResult('Create Price', 'success', 'Price created successfully', response);
      this.configForm.patchValue({ priceId: (response as any).priceId });
    } catch (error: any) {
      this.addResult('Create Price', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCreateSubscription() {
    this.addResult('Create Subscription', 'pending', 'Creating Stripe subscription...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/create-subscription`, {
        customerId: this.configForm.value.customerId,
        priceId: this.configForm.value.priceId,
        paymentMethodId: this.configForm.value.paymentMethodId
      }).toPromise();
      
      this.addResult('Create Subscription', 'success', 'Subscription created successfully', response);
      this.currentSubscriptionId = (response as any).subscriptionId;
    } catch (error: any) {
      this.addResult('Create Subscription', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetSubscription() {
    this.addResult('Get Subscription', 'pending', 'Getting subscription details...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/stripe/subscription/${this.currentSubscriptionId}`).toPromise();
      this.addResult('Get Subscription', 'success', 'Subscription retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get Subscription', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCancelSubscription() {
    this.addResult('Cancel Subscription', 'pending', 'Cancelling subscription...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/cancel-subscription`, {
        subscriptionId: this.currentSubscriptionId
      }).toPromise();
      
      this.addResult('Cancel Subscription', 'success', 'Subscription cancelled successfully', response);
    } catch (error: any) {
      this.addResult('Cancel Subscription', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testPauseSubscription() {
    this.addResult('Pause Subscription', 'pending', 'Pausing subscription...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/pause-subscription`, {
        subscriptionId: this.currentSubscriptionId
      }).toPromise();
      
      this.addResult('Pause Subscription', 'success', 'Subscription paused successfully', response);
    } catch (error: any) {
      this.addResult('Pause Subscription', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testResumeSubscription() {
    this.addResult('Resume Subscription', 'pending', 'Resuming subscription...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/resume-subscription`, {
        subscriptionId: this.currentSubscriptionId
      }).toPromise();
      
      this.addResult('Resume Subscription', 'success', 'Subscription resumed successfully', response);
    } catch (error: any) {
      this.addResult('Resume Subscription', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testProcessPayment() {
    this.addResult('Process Payment', 'pending', 'Processing payment...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/process-payment`, {
        paymentMethodId: this.configForm.value.paymentMethodId,
        amount: 29.99,
        currency: 'usd'
      }).toPromise();
      
      this.addResult('Process Payment', 'success', 'Payment processed successfully', response);
    } catch (error: any) {
      this.addResult('Process Payment', 'error', `Failed: ${error.message}`, error);
    }
  }

  // Subscription Service Tests
  async testCreateSubscriptionService() {
    this.addResult('Create Subscription (Service)', 'pending', 'Creating subscription via service...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/subscriptions`, {
        userId: this.configForm.value.userId,
        planId: this.configForm.value.planId,
        paymentMethodId: this.configForm.value.paymentMethodId
      }).toPromise();
      
      this.addResult('Create Subscription (Service)', 'success', 'Subscription created via service successfully', response);
    } catch (error: any) {
      this.addResult('Create Subscription (Service)', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetUserSubscriptions() {
    this.addResult('Get User Subscriptions', 'pending', 'Getting user subscriptions...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/subscriptions/user/${this.configForm.value.userId}`).toPromise();
      this.addResult('Get User Subscriptions', 'success', 'User subscriptions retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get User Subscriptions', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCancelSubscriptionService() {
    this.addResult('Cancel Subscription (Service)', 'pending', 'Cancelling subscription via service...');
    
    try {
      const response = await this.http.delete(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}?reason=Testing`).toPromise();
      this.addResult('Cancel Subscription (Service)', 'success', 'Subscription cancelled via service successfully', response);
    } catch (error: any) {
      this.addResult('Cancel Subscription (Service)', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testPauseSubscriptionService() {
    this.addResult('Pause Subscription (Service)', 'pending', 'Pausing subscription via service...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}/pause`, {}).toPromise();
      this.addResult('Pause Subscription (Service)', 'success', 'Subscription paused via service successfully', response);
    } catch (error: any) {
      this.addResult('Pause Subscription (Service)', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testResumeSubscriptionService() {
    this.addResult('Resume Subscription (Service)', 'pending', 'Resuming subscription via service...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}/resume`, {}).toPromise();
      this.addResult('Resume Subscription (Service)', 'success', 'Subscription resumed via service successfully', response);
    } catch (error: any) {
      this.addResult('Resume Subscription (Service)', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetUsageStatistics() {
    this.addResult('Get Usage Statistics', 'pending', 'Getting usage statistics...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}/usage-statistics`).toPromise();
      this.addResult('Get Usage Statistics', 'success', 'Usage statistics retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get Usage Statistics', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testProcessPaymentService() {
    this.addResult('Process Payment (Service)', 'pending', 'Processing payment via service...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}/process-payment`, {
        paymentMethodId: this.configForm.value.paymentMethodId,
        amount: 29.99,
        currency: 'usd'
      }).toPromise();
      
      this.addResult('Process Payment (Service)', 'success', 'Payment processed via service successfully', response);
    } catch (error: any) {
      this.addResult('Process Payment (Service)', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetBillingHistory() {
    this.addResult('Get Billing History', 'pending', 'Getting billing history...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/subscriptions/${this.currentSubscriptionId}/billing-history`).toPromise();
      this.addResult('Get Billing History', 'success', 'Billing history retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get Billing History', 'error', `Failed: ${error.message}`, error);
    }
  }

  // Plan Management Tests
  async testCreatePlan() {
    this.addResult('Create Plan', 'pending', 'Creating subscription plan...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/subscription-plans`, {
        name: 'Test Healthcare Plan',
        description: 'Comprehensive healthcare subscription plan',
        price: 29.99,
        billingCycleId: '00000000-0000-0000-0000-000000000001',
        currencyId: '00000000-0000-0000-0000-000000000001',
        isTrialAllowed: true,
        trialDurationInDays: 7,
        isActive: true
      }).toPromise();
      
      this.addResult('Create Plan', 'success', 'Plan created successfully', response);
    } catch (error: any) {
      this.addResult('Create Plan', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetAllPlans() {
    this.addResult('Get All Plans', 'pending', 'Getting all subscription plans...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/subscription-plans`).toPromise();
      this.addResult('Get All Plans', 'success', 'All plans retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get All Plans', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetActivePlans() {
    this.addResult('Get Active Plans', 'pending', 'Getting active subscription plans...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/subscription-plans/active`).toPromise();
      this.addResult('Get Active Plans', 'success', 'Active plans retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get Active Plans', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testUpdatePlan() {
    this.addResult('Update Plan', 'pending', 'Updating subscription plan...');
    
    try {
      const response = await this.http.put(`${this.configForm.value.apiBaseUrl}/api/subscription-plans/${this.configForm.value.planId}`, {
        name: 'Updated Healthcare Plan',
        description: 'Updated comprehensive healthcare subscription plan',
        price: 39.99,
        isActive: true
      }).toPromise();
      
      this.addResult('Update Plan', 'success', 'Plan updated successfully', response);
    } catch (error: any) {
      this.addResult('Update Plan', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testDeletePlan() {
    this.addResult('Delete Plan', 'pending', 'Deleting subscription plan...');
    
    try {
      const response = await this.http.delete(`${this.configForm.value.apiBaseUrl}/api/subscription-plans/${this.configForm.value.planId}`).toPromise();
      this.addResult('Delete Plan', 'success', 'Plan deleted successfully', response);
    } catch (error: any) {
      this.addResult('Delete Plan', 'error', `Failed: ${error.message}`, error);
    }
  }

  // Webhook & Integration Tests
  async testWebhookPaymentSucceeded() {
    this.addResult('Webhook: Payment Succeeded', 'pending', 'Testing payment succeeded webhook...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/webhook`, {
        type: 'invoice.payment_succeeded',
        data: {
          object: {
            id: 'in_test',
            customer: this.configForm.value.customerId,
            subscription: this.currentSubscriptionId,
            amount_paid: 2999,
            currency: 'usd'
          }
        }
      }).toPromise();
      
      this.addResult('Webhook: Payment Succeeded', 'success', 'Payment succeeded webhook processed successfully', response);
    } catch (error: any) {
      this.addResult('Webhook: Payment Succeeded', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testWebhookPaymentFailed() {
    this.addResult('Webhook: Payment Failed', 'pending', 'Testing payment failed webhook...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/webhook`, {
        type: 'invoice.payment_failed',
        data: {
          object: {
            id: 'in_test',
            customer: this.configForm.value.customerId,
            subscription: this.currentSubscriptionId,
            amount_due: 2999,
            currency: 'usd',
            next_payment_attempt: new Date(Date.now() + 86400000).toISOString()
          }
        }
      }).toPromise();
      
      this.addResult('Webhook: Payment Failed', 'success', 'Payment failed webhook processed successfully', response);
    } catch (error: any) {
      this.addResult('Webhook: Payment Failed', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testWebhookSubscriptionCreated() {
    this.addResult('Webhook: Subscription Created', 'pending', 'Testing subscription created webhook...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/webhook`, {
        type: 'customer.subscription.created',
        data: {
          object: {
            id: this.currentSubscriptionId,
            customer: this.configForm.value.customerId,
            status: 'active',
            current_period_start: new Date().toISOString(),
            current_period_end: new Date(Date.now() + 2592000000).toISOString()
          }
        }
      }).toPromise();
      
      this.addResult('Webhook: Subscription Created', 'success', 'Subscription created webhook processed successfully', response);
    } catch (error: any) {
      this.addResult('Webhook: Subscription Created', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testWebhookSubscriptionCancelled() {
    this.addResult('Webhook: Subscription Cancelled', 'pending', 'Testing subscription cancelled webhook...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/webhook`, {
        type: 'customer.subscription.deleted',
        data: {
          object: {
            id: this.currentSubscriptionId,
            customer: this.configForm.value.customerId,
            status: 'canceled',
            canceled_at: new Date().toISOString()
          }
        }
      }).toPromise();
      
      this.addResult('Webhook: Subscription Cancelled', 'success', 'Subscription cancelled webhook processed successfully', response);
    } catch (error: any) {
      this.addResult('Webhook: Subscription Cancelled', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCheckoutSession() {
    this.addResult('Create Checkout Session', 'pending', 'Creating checkout session...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/create-checkout-session`, {
        priceId: this.configForm.value.priceId,
        successUrl: 'https://localhost:4200/success',
        cancelUrl: 'https://localhost:4200/cancel'
      }).toPromise();
      
      this.addResult('Create Checkout Session', 'success', 'Checkout session created successfully', response);
    } catch (error: any) {
      this.addResult('Create Checkout Session', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testGetPaymentMethods() {
    this.addResult('Get Payment Methods', 'pending', 'Getting customer payment methods...');
    
    try {
      const response = await this.http.get(`${this.configForm.value.apiBaseUrl}/api/stripe/customer/${this.configForm.value.customerId}/payment-methods`).toPromise();
      this.addResult('Get Payment Methods', 'success', 'Payment methods retrieved successfully', response);
    } catch (error: any) {
      this.addResult('Get Payment Methods', 'error', `Failed: ${error.message}`, error);
    }
  }

  async testCreateTestPaymentMethod() {
    this.addResult('Create Test Payment Method', 'pending', 'Creating test payment method...');
    
    try {
      const response = await this.http.post(`${this.configForm.value.apiBaseUrl}/api/stripe/customer/${this.configForm.value.customerId}/create-test-payment-method`, {}).toPromise();
      const paymentMethodId = (response as any).paymentMethodId;
      
      // Auto-fill the payment method ID in the form
      this.configForm.patchValue({ paymentMethodId });
      
      this.addResult('Create Test Payment Method', 'success', `Test payment method created: ${paymentMethodId}`, response);
    } catch (error: any) {
      this.addResult('Create Test Payment Method', 'error', `Failed: ${error.message}`, error);
    }
  }
} 