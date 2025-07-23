import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuestionnaireEditDialogComponent } from './questionnaire-edit-dialog.component';

describe('QuestionnaireEditDialogComponent', () => {
  let component: QuestionnaireEditDialogComponent;
  let fixture: ComponentFixture<QuestionnaireEditDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuestionnaireEditDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuestionnaireEditDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
