import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionPlansManagementComponent } from './subscription-plans-management.component';

describe('SubscriptionPlansManagementComponent', () => {
  let component: SubscriptionPlansManagementComponent;
  let fixture: ComponentFixture<SubscriptionPlansManagementComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubscriptionPlansManagementComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubscriptionPlansManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
