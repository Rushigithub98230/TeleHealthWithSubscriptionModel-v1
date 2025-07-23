import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AssignPlanDialogComponent } from './assign-plan-dialog.component';

describe('AssignPlanDialogComponent', () => {
  let component: AssignPlanDialogComponent;
  let fixture: ComponentFixture<AssignPlanDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AssignPlanDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AssignPlanDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
