import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CancelSubscriptionDialogComponent } from './cancel-subscription-dialog.component';

describe('CancelSubscriptionDialogComponent', () => {
  let component: CancelSubscriptionDialogComponent;
  let fixture: ComponentFixture<CancelSubscriptionDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CancelSubscriptionDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CancelSubscriptionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
