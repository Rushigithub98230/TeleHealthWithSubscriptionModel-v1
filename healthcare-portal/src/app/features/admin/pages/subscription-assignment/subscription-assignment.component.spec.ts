import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubscriptionAssignmentComponent } from './subscription-assignment.component';

describe('SubscriptionAssignmentComponent', () => {
  let component: SubscriptionAssignmentComponent;
  let fixture: ComponentFixture<SubscriptionAssignmentComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubscriptionAssignmentComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubscriptionAssignmentComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
