# 📋 Questionnaire System Angular Frontend (Production-Ready)
## 1. Models & Interfaces (`questionnaire.models.ts`)

```typescript
export enum QuestionType {
  TEXT = 'text',
  TEXTAREA = 'textarea',
  RADIO = 'radio',
  CHECKBOX = 'checkbox',
  DROPDOWN = 'dropdown',
  RANGE = 'range'
}

export interface QuestionOptionDto {
  id?: string;
  text: string;
  value: string;
  order: number;
  mediaUrl?: string;
}

export interface QuestionDto {
  id?: string;
  text: string;
  type: QuestionType;
  isRequired: boolean;
  order: number;
  helpText?: string;
  mediaUrl?: string;
  minValue?: number;
  maxValue?: number;
  stepValue?: number;
  options: QuestionOptionDto[];
}

export interface QuestionnaireTemplateDto {
  id?: string;
  name: string;
  description: string;
  categoryId: string;
  isActive: boolean;
  version?: number;
  questions: QuestionDto[];
}

export interface UserAnswerDto {
  questionId: string;
  answerText?: string;
  numericValue?: number;
  selectedOptionIds: string[];
}

export interface CreateUserResponseDto {
  userId: string;
  categoryId: string;
  templateId: string;
  status: string;
  answers: UserAnswerDto[];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  statusCode: number;
  data: T;
  errors?: string[];
}
```

---

## 2. Service (`questionnaire.service.ts`)

```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  QuestionnaireTemplateDto,
  CreateUserResponseDto,
  ApiResponse
} from './questionnaire.models';

@Injectable({ providedIn: 'root' })
export class QuestionnaireService {
  private apiUrl = '/api/questionnaire';

  constructor(private http: HttpClient) {}

  createTemplate(formData: FormData): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/templates`, formData);
  }

  updateTemplate(id: string, formData: FormData): Observable<ApiResponse<object>> {
    return this.http.put<ApiResponse<object>>(`${this.apiUrl}/templates/${id}`, formData);
  }

  getTemplatesByCategory(categoryId: string): Observable<QuestionnaireTemplateDto[]> {
    return this.http.get<QuestionnaireTemplateDto[]>(`${this.apiUrl}/templates/by-category/${categoryId}`);
  }

  getTemplateById(id: string): Observable<QuestionnaireTemplateDto> {
    return this.http.get<QuestionnaireTemplateDto>(`${this.apiUrl}/templates/${id}`);
  }

  submitUserResponse(response: CreateUserResponseDto): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/responses`, response);
  }
}
```

---

## 3. Module Setup (`questionnaire.module.ts`)

```typescript
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatStepperModule } from '@angular/material/stepper';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { QuestionCreateComponent } from './question-create.component';
import { QuestionUpdateComponent } from './question-update.component';
import { QuestionnaireStepperComponent } from './questionnaire-stepper.component';

@NgModule({
  declarations: [
    QuestionCreateComponent,
    QuestionUpdateComponent,
    QuestionnaireStepperComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatRadioModule,
    MatButtonModule,
    MatStepperModule,
    MatCardModule,
    MatIconModule
  ],
  exports: [
    QuestionCreateComponent,
    QuestionUpdateComponent,
    QuestionnaireStepperComponent
  ]
})
export class QuestionnaireModule {}
```

---

## 4. Create/Update Component (`question-create.component.ts` & `question-update.component.ts`)

```typescript
import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { QuestionnaireService } from './questionnaire.service';
import { QuestionType, QuestionnaireTemplateDto, ApiResponse } from './questionnaire.models';

@Component({
  selector: 'app-question-create',
  templateUrl: './question-create.component.html'
})
export class QuestionCreateComponent {
  @Output() saved = new EventEmitter<void>();
  form: FormGroup;
  questionTypes = Object.values(QuestionType);
  fileMap = new Map<string, File>();

  constructor(private fb: FormBuilder, private service: QuestionnaireService) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      isActive: [true],
      questions: this.fb.array([])
    });
  }

  get questions(): FormArray {
    return this.form.get('questions') as FormArray;
  }

  addQuestion(): void {
    this.questions.push(this.fb.group({
      text: ['', Validators.required],
      type: [QuestionType.TEXT, Validators.required],
      isRequired: [true],
      order: [this.questions.length],
      helpText: [''],
      mediaUrl: [''],
      minValue: [null],
      maxValue: [null],
      stepValue: [1],
      options: this.fb.array([])
    }));
  }

  removeQuestion(i: number): void {
    this.questions.removeAt(i);
  }

  getOptions(qIndex: number): FormArray {
    return this.questions.at(qIndex).get('options') as FormArray;
  }

  addOption(qIndex: number): void {
    this.getOptions(qIndex).push(this.fb.group({
      text: ['', Validators.required],
      value: ['', Validators.required],
      order: [this.getOptions(qIndex).length],
      mediaUrl: ['']
    }));
  }

  removeOption(qIndex: number, oIndex: number): void {
    this.getOptions(qIndex).removeAt(oIndex);
  }

  isMultipleChoice(type: QuestionType): boolean {
    return [QuestionType.RADIO, QuestionType.CHECKBOX, QuestionType.DROPDOWN].includes(type);
  }

  isRange(type: QuestionType): boolean {
    return type === QuestionType.RANGE;
  }

  onFileChange(event: any, qIdx: number, oIdx?: number): void {
    const file = event.target.files[0];
    if (!file) return;
    const key = oIdx !== undefined ? `option_${qIdx}_${oIdx}_media` : `question_${qIdx}_media`;
    this.fileMap.set(key, file);
  }

  buildFormData(): FormData {
    const formData = new FormData();
    const template = this.form.value;
    formData.append('templateJson', JSON.stringify(template));
    this.fileMap.forEach((file, key) => {
      formData.append(key, file);
    });
    return formData;
  }

  onSubmit(): void {
    if (this.form.valid) {
      const formData = this.buildFormData();
      this.service.createTemplate(formData).subscribe((res: ApiResponse<string>) => {
        if (res.success) {
          this.saved.emit();
          this.form.reset();
          this.questions.clear();
          this.fileMap.clear();
        } else {
          alert(res.message);
        }
      });
    }
  }
}

// For update, use the same logic but call updateTemplate(id, formData)
@Component({
  selector: 'app-question-update',
  templateUrl: './question-create.component.html'
})
export class QuestionUpdateComponent implements OnInit {
  @Input() template: QuestionnaireTemplateDto;
  @Output() updated = new EventEmitter<void>();
  form: FormGroup;
  questionTypes = Object.values(QuestionType);
  fileMap = new Map<string, File>();

  constructor(private fb: FormBuilder, private service: QuestionnaireService) {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      categoryId: ['', Validators.required],
      isActive: [true],
      questions: this.fb.array([])
    });
  }

  ngOnInit(): void {
    if (this.template) {
      this.form.patchValue({
        name: this.template.name,
        description: this.template.description,
        categoryId: this.template.categoryId,
        isActive: this.template.isActive
      });
      this.questions.clear();
      this.template.questions.forEach((q, qIdx) => {
        const qGroup = this.fb.group({
          text: [q.text, Validators.required],
          type: [q.type, Validators.required],
          isRequired: [q.isRequired],
          order: [q.order],
          helpText: [q.helpText],
          mediaUrl: [q.mediaUrl],
          minValue: [q.minValue],
          maxValue: [q.maxValue],
          stepValue: [q.stepValue],
          options: this.fb.array([])
        });
        q.options.forEach((o, oIdx) => {
          (qGroup.get('options') as FormArray).push(this.fb.group({
            text: [o.text, Validators.required],
            value: [o.value, Validators.required],
            order: [o.order],
            mediaUrl: [o.mediaUrl]
          }));
        });
        this.questions.push(qGroup);
      });
    }
  }

  get questions(): FormArray {
    return this.form.get('questions') as FormArray;
  }

  getOptions(qIndex: number): FormArray {
    return this.questions.at(qIndex).get('options') as FormArray;
  }

  onFileChange(event: any, qIdx: number, oIdx?: number): void {
    const file = event.target.files[0];
    if (!file) return;
    const key = oIdx !== undefined ? `option_${qIdx}_${oIdx}_media` : `question_${qIdx}_media`;
    this.fileMap.set(key, file);
  }

  buildFormData(): FormData {
    const formData = new FormData();
    const template = this.form.value;
    formData.append('templateJson', JSON.stringify(template));
    this.fileMap.forEach((file, key) => {
      formData.append(key, file);
    });
    return formData;
  }

  onSubmit(): void {
    if (this.form.valid && this.template.id) {
      const formData = this.buildFormData();
      this.service.updateTemplate(this.template.id, formData).subscribe((res: ApiResponse<object>) => {
        if (res.success) {
          this.updated.emit();
        } else {
          alert(res.message);
        }
      });
    }
  }
}
```

### `question-create.component.html` (used for both create and update)

```html
<form [formGroup]="form" (ngSubmit)="onSubmit()">
  <mat-card>
    <mat-form-field appearance="outline">
      <mat-label>Template Name</mat-label>
      <input matInput formControlName="name" required />
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Description</mat-label>
      <textarea matInput formControlName="description"></textarea>
    </mat-form-field>
    <mat-form-field appearance="outline">
      <mat-label>Category</mat-label>
      <input matInput formControlName="categoryId" required />
    </mat-form-field>
    <mat-checkbox formControlName="isActive">Active</mat-checkbox>
    <div formArrayName="questions">
      <div *ngFor="let q of questions.controls; let i = index" [formGroupName]="i" class="question-block">
        <mat-divider></mat-divider>
        <mat-form-field appearance="outline">
          <mat-label>Question Text</mat-label>
          <input matInput formControlName="text" required />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Type</mat-label>
          <mat-select formControlName="type">
            <mat-option *ngFor="let t of questionTypes" [value]="t">{{ t }}</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-checkbox formControlName="isRequired">Required</mat-checkbox>
        <mat-form-field appearance="outline">
          <mat-label>Order</mat-label>
          <input matInput type="number" formControlName="order" />
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Help Text</mat-label>
          <input matInput formControlName="helpText" />
        </mat-form-field>
        <div>
          <label>Question Image:</label>
          <input type="file" (change)="onFileChange($event, i)" />
          <span *ngIf="q.get('mediaUrl').value">Current: {{ q.get('mediaUrl').value }}</span>
        </div>
        <div *ngIf="isRange(q.get('type').value)">
          <mat-form-field appearance="outline">
            <mat-label>Min Value</mat-label>
            <input matInput type="number" formControlName="minValue" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Max Value</mat-label>
            <input matInput type="number" formControlName="maxValue" />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Step Value</mat-label>
            <input matInput type="number" formControlName="stepValue" />
          </mat-form-field>
        </div>
        <div *ngIf="isMultipleChoice(q.get('type').value)" formArrayName="options">
          <div *ngFor="let o of getOptions(i).controls; let j = index" [formGroupName]="j">
            <mat-form-field appearance="outline">
              <mat-label>Option Text</mat-label>
              <input matInput formControlName="text" required />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Option Value</mat-label>
              <input matInput formControlName="value" required />
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>Order</mat-label>
              <input matInput type="number" formControlName="order" />
            </mat-form-field>
            <div>
              <label>Option Image:</label>
              <input type="file" (change)="onFileChange($event, i, j)" />
              <span *ngIf="o.get('mediaUrl').value">Current: {{ o.get('mediaUrl').value }}</span>
            </div>
            <button mat-icon-button color="warn" (click)="removeOption(i, j)"><mat-icon>delete</mat-icon></button>
          </div>
          <button mat-button color="primary" (click)="addOption(i)" type="button">Add Option</button>
        </div>
        <button mat-icon-button color="warn" (click)="removeQuestion(i)"><mat-icon>delete</mat-icon></button>
      </div>
    </div>
    <button mat-button color="primary" (click)="addQuestion()" type="button">Add Question</button>
    <button mat-raised-button color="accent" type="submit" [disabled]="!form.valid">Save Template</button>
  </mat-card>
</form>
```

---

## 5. Stepper Component (`questionnaire-stepper.component.ts`)

```typescript
import { Component, Input, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { QuestionnaireService } from './questionnaire.service';
import { QuestionnaireTemplateDto, QuestionType, CreateUserResponseDto, ApiResponse } from './questionnaire.models';

@Component({
  selector: 'app-questionnaire-stepper',
  templateUrl: './questionnaire-stepper.component.html'
})
export class QuestionnaireStepperComponent implements OnInit {
  @Input() categoryId: string;
  @Input() userId: string;
  templates: QuestionnaireTemplateDto[] = [];
  selectedTemplate: QuestionnaireTemplateDto;
  stepperForm: FormGroup;
  questionType = QuestionType;
  submitted = false;

  constructor(private fb: FormBuilder, private service: QuestionnaireService) {
    this.stepperForm = this.fb.group({
      answers: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.service.getTemplatesByCategory(this.categoryId).subscribe(templates => {
      this.templates = templates;
      if (templates.length) {
        this.selectTemplate(templates[0]);
      }
    });
  }

  selectTemplate(template: QuestionnaireTemplateDto): void {
    this.selectedTemplate = template;
    const answersArray = this.stepperForm.get('answers') as FormArray;
    answersArray.clear();
    template.questions.forEach(q => {
      answersArray.push(this.createAnswerFormGroup(q));
    });
  }

  createAnswerFormGroup(q: any): FormGroup {
    switch (q.type) {
      case QuestionType.TEXT:
      case QuestionType.TEXTAREA:
        return this.fb.group({
          questionId: [q.id],
          answerText: ['', q.isRequired ? Validators.required : []],
          numericValue: [null],
          selectedOptionIds: [[]]
        });
      case QuestionType.RADIO:
      case QuestionType.DROPDOWN:
        return this.fb.group({
          questionId: [q.id],
          answerText: [null],
          numericValue: [null],
          selectedOptionIds: [[], q.isRequired ? Validators.required : []]
        });
      case QuestionType.CHECKBOX:
        return this.fb.group({
          questionId: [q.id],
          answerText: [null],
          numericValue: [null],
          selectedOptionIds: [[], q.isRequired ? Validators.required : []]
        });
      case QuestionType.RANGE:
        return this.fb.group({
          questionId: [q.id],
          answerText: [null],
          numericValue: [null, q.isRequired ? Validators.required : []],
          selectedOptionIds: [[]]
        });
      default:
        return this.fb.group({
          questionId: [q.id],
          answerText: [''],
          numericValue: [null],
          selectedOptionIds: [[]]
        });
    }
  }

  get answersArray(): FormArray {
    return this.stepperForm.get('answers') as FormArray;
  }

  onCheckboxChange(event: any, answerIndex: number, optionId: string): void {
    const selected = this.answersArray.at(answerIndex).get('selectedOptionIds').value as string[];
    if (event.checked) {
      selected.push(optionId);
    } else {
      const idx = selected.indexOf(optionId);
      if (idx > -1) selected.splice(idx, 1);
    }
    this.answersArray.at(answerIndex).get('selectedOptionIds').setValue(selected);
  }

  onSubmit(): void {
    if (this.stepperForm.valid && this.selectedTemplate) {
      const response: CreateUserResponseDto = {
        userId: this.userId,
        categoryId: this.categoryId,
        templateId: this.selectedTemplate.id,
        status: 'completed',
        answers: this.stepperForm.value.answers
      };
      this.service.submitUserResponse(response).subscribe((res: ApiResponse<string>) => {
        if (res.success) {
          this.submitted = true;
        } else {
          alert(res.message);
        }
      });
    }
  }
}
```

### `questionnaire-stepper.component.html`

```html
<mat-card *ngIf="selectedTemplate">
  <h2>{{ selectedTemplate.name }}</h2>
  <mat-horizontal-stepper [linear]="true" #stepper>
    <mat-step *ngFor="let q of selectedTemplate.questions; let i = index" [stepControl]="answersArray.at(i)">
      <form [formGroup]="answersArray.at(i)">
        <ng-container [ngSwitch]="q.type">
          <mat-form-field *ngSwitchCase="questionType.TEXT" appearance="outline">
            <mat-label>{{ q.text }}</mat-label>
            <input matInput formControlName="answerText" [required]="q.isRequired" />
          </mat-form-field>
          <mat-form-field *ngSwitchCase="questionType.TEXTAREA" appearance="outline">
            <mat-label>{{ q.text }}</mat-label>
            <textarea matInput formControlName="answerText" [required]="q.isRequired"></textarea>
          </mat-form-field>
          <mat-radio-group *ngSwitchCase="questionType.RADIO" formControlName="selectedOptionIds">
            <mat-label>{{ q.text }}</mat-label>
            <mat-radio-button *ngFor="let opt of q.options" [value]="[opt.id]">{{ opt.text }}</mat-radio-button>
          </mat-radio-group>
          <mat-checkbox-group *ngSwitchCase="questionType.CHECKBOX">
            <mat-label>{{ q.text }}</mat-label>
            <mat-checkbox *ngFor="let opt of q.options" [checked]="answersArray.at(i).get('selectedOptionIds').value.includes(opt.id)"
              (change)="onCheckboxChange($event, i, opt.id)">{{ opt.text }}</mat-checkbox>
          </mat-checkbox-group>
          <mat-form-field *ngSwitchCase="questionType.DROPDOWN" appearance="outline">
            <mat-label>{{ q.text }}</mat-label>
            <mat-select formControlName="selectedOptionIds">
              <mat-option *ngFor="let opt of q.options" [value]="[opt.id]">{{ opt.text }}</mat-option>
            </mat-select>
          </mat-form-field>
          <mat-form-field *ngSwitchCase="questionType.RANGE" appearance="outline">
            <mat-label>{{ q.text }}</mat-label>
            <input matInput type="number" formControlName="numericValue" [min]="q.minValue" [max]="q.maxValue" [step]="q.stepValue" />
          </mat-form-field>
        </ng-container>
        <div *ngIf="answersArray.at(i).invalid && (answersArray.at(i).dirty || answersArray.at(i).touched)">
          <mat-error>This question is required.</mat-error>
        </div>
        <div>
          <button mat-button matStepperPrevious *ngIf="i > 0">Back</button>
          <button mat-button matStepperNext *ngIf="i < selectedTemplate.questions.length - 1" [disabled]="answersArray.at(i).invalid">Next</button>
          <button mat-raised-button color="accent" *ngIf="i === selectedTemplate.questions.length - 1" (click)="onSubmit()" [disabled]="stepperForm.invalid">Submit</button>
        </div>
      </form>
    </mat-step>
  </mat-horizontal-stepper>
  <div *ngIf="submitted">
    <p>Thank you! Your answers have been submitted.</p>
  </div>
</mat-card>
```

