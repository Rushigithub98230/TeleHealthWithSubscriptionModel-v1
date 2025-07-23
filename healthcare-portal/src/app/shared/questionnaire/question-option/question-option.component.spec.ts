import { ComponentFixture, TestBed } from '@angular/core/testing';

import { QuestionOptionComponent } from './question-option.component';

describe('QuestionOptionComponent', () => {
  let component: QuestionOptionComponent;
  let fixture: ComponentFixture<QuestionOptionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [QuestionOptionComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(QuestionOptionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
