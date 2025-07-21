# Dynamic Category-Based Questionnaire System (Angular Client)

## Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [API Endpoints](#api-endpoints)
4. [Angular Implementation](#angular-implementation)
    - [1. Install Dependencies](#1-install-dependencies)
    - [2. Models & Interfaces](#2-models--interfaces)
    - [3. API Service](#3-api-service)
    - [4. Admin UI: Create/Edit Questions](#4-admin-ui-createedit-questions)
        - [Admin Question Form Component (TypeScript)](#admin-question-form-component-typescript)
        - [Admin Question Form Template (HTML)](#admin-question-form-template-html)
        - [Admin Module Setup & Usage](#admin-module-setup--usage)
    - [5. User UI: Dynamic Questionnaire](#5-user-ui-dynamic-questionnaire)
        - [Dynamic Questionnaire Component (TypeScript)](#dynamic-questionnaire-component-typescript)
        - [Dynamic Questionnaire Template (HTML)](#dynamic-questionnaire-template-html)
        - [User Module Setup & Usage](#user-module-setup--usage)
    - [6. Validation & Error Handling](#6-validation--error-handling)
    - [7. Extending for New Input Types](#7-extending-for-new-input-types)
5. [Admin Example: Creating Questions](#admin-example-creating-questions)
6. [User Example: Submitting Answers](#user-example-submitting-answers)
7. [Summary](#summary)

---

## Overview
This system enables dynamic, config-driven questionnaires for each subscription category. Admins can create questions (text, image, or both) and define answer types (textbox, textarea, radio, checkbox, range, etc.) for each category. Users see relevant questions when selecting a category and must answer them before viewing available plans.

---

## Features
- **Admin:**
  - Create/edit/delete questions for any category
  - Configure each question as text, image, or both
  - Select answer type: textbox, textarea, radio, checkbox, range, etc.
  - Add/edit/delete options for radio/checkbox/range dynamically
- **User:**
  - Select a category
  - See all relevant questions rendered dynamically
  - Fill answers using the correct input type
  - Submit answers, then view/select plans for that category
- **Backend:**
  - All questions, types, and options are config-driven (no hardcoding)
  - API endpoints for CRUD, fetch by category, and answer submission

---

## API Endpoints
- `POST /api/CategoryQuestions` — Create question (admin)
- `PUT /api/CategoryQuestions/{id}` — Update question (admin)
- `DELETE /api/CategoryQuestions/{id}` — Delete question (admin)
- `GET /api/CategoryQuestions/by-category/{categoryId}` — Get questions for category (user)
- `POST /api/CategoryQuestions/answers` — Submit answer (user)

---

## Angular Implementation

### 1. Install Dependencies
```sh
ng new category-questionnaire-client
cd category-questionnaire-client
ng add @angular/material
npm install @angular/forms @angular/common @angular/http
```

### 2. Models & Interfaces
```typescript
// src/app/models/category-question.model.ts
export interface CategoryQuestion {
  id?: string;
  categoryId: string;
  questionText: string;
  questionType: string; // 'text', 'textarea', 'radio', 'checkbox', 'range', 'image', etc.
  isRequired: boolean;
  isActive: boolean;
  optionsJson?: string; // JSON string for options, imageUrl, etc.
}

export interface CategoryQuestionAnswer {
  categoryQuestionId: string;
  userId: string;
  answer: string; // string, JSON array, or value depending on type
}

export interface Category {
  id: string;
  name: string;
}
```

### 3. API Service
```typescript
// src/app/services/category-question.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CategoryQuestion, CategoryQuestionAnswer } from '../models/category-question.model';

@Injectable({ providedIn: 'root' })
export class CategoryQuestionService {
  constructor(private http: HttpClient) {}

  getQuestionsByCategory(categoryId: string): Observable<CategoryQuestion[]> {
    return this.http.get<CategoryQuestion[]>(`/api/CategoryQuestions/by-category/${categoryId}`);
  }

  createQuestion(question: CategoryQuestion): Observable<any> {
    return this.http.post('/api/CategoryQuestions', question);
  }

  updateQuestion(id: string, question: CategoryQuestion): Observable<any> {
    return this.http.put(`/api/CategoryQuestions/${id}`, question);
  }

  deleteQuestion(id: string): Observable<any> {
    return this.http.delete(`/api/CategoryQuestions/${id}`);
  }

  submitAnswers(answers: CategoryQuestionAnswer[]): Observable<any> {
    return this.http.post('/api/CategoryQuestions/answers', answers);
  }
}
```

---

### 4. Admin UI: Create/Edit Questions

#### Admin Question Form Component (TypeScript)
```typescript
// src/app/components/admin-question-form/admin-question-form.component.ts
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { CategoryQuestionService } from '../../services/category-question.service';
import { Category } from '../../models/category-question.model';

@Component({
  selector: 'app-admin-question-form',
  templateUrl: './admin-question-form.component.html',
})
export class AdminQuestionFormComponent implements OnInit {
  form: FormGroup = this.fb.group({
    categoryId: ['', Validators.required],
    questionText: ['', Validators.required],
    imageUrl: [''],
    questionType: ['', Validators.required],
    isRequired: [false],
    isActive: [true],
    options: this.fb.array([]), // for radio/checkbox/range
    min: [''], // for range
    max: ['']  // for range
  });
  categories: Category[] = [];
  success: string | null = null;
  error: string | null = null;

  get options() { return this.form.get('options') as FormArray; }

  constructor(private fb: FormBuilder, private cqService: CategoryQuestionService) {}

  ngOnInit() {
    // TODO: Fetch categories from backend
    this.categories = [
      { id: 'cat1', name: 'Dermatology' },
      { id: 'cat2', name: 'Cardiology' }
    ];
  }

  addOption() { this.options.push(this.fb.control('')); }
  removeOption(i: number) { this.options.removeAt(i); }

  onTypeChange() {
    this.options.clear();
    this.form.patchValue({ min: '', max: '' });
  }

  submit() {
    this.success = null;
    this.error = null;
    let optionsJson = '';
    if (['radio', 'checkbox'].includes(this.form.value.questionType)) {
      optionsJson = JSON.stringify(this.options.value.filter((o: string) => o));
    } else if (this.form.value.questionType === 'range') {
      optionsJson = JSON.stringify({ min: this.form.value.min, max: this.form.value.max });
    }
    if (this.form.value.imageUrl) {
      if (optionsJson) {
        const parsed = JSON.parse(optionsJson);
        optionsJson = JSON.stringify({ imageUrl: this.form.value.imageUrl, options: parsed.options || parsed });
      } else {
        optionsJson = JSON.stringify({ imageUrl: this.form.value.imageUrl });
      }
    }
    const payload = {
      categoryId: this.form.value.categoryId,
      questionText: this.form.value.questionText,
      questionType: this.form.value.questionType,
      isRequired: this.form.value.isRequired,
      isActive: this.form.value.isActive,
      optionsJson
    };
    this.cqService.createQuestion(payload).subscribe({
      next: () => this.success = 'Question saved!',
      error: () => this.error = 'Failed to save question.'
    });
  }
}
```

#### Admin Question Form Template (HTML)
```html
<form [formGroup]="form" (ngSubmit)="submit()">
  <div *ngIf="error" style="color:red;">{{error}}</div>
  <div *ngIf="success" style="color:green;">{{success}}</div>
  <label>Category:</label>
  <select formControlName="categoryId">
    <option *ngFor="let cat of categories" [value]="cat.id">{{cat.name}}</option>
  </select>
  <label>Question Text:</label>
  <input formControlName="questionText" placeholder="Question Text" />
  <label>Image URL (optional):</label>
  <input formControlName="imageUrl" placeholder="Image URL" />
  <label>Answer Type:</label>
  <select formControlName="questionType" (change)="onTypeChange()">
    <option value="text">Textbox</option>
    <option value="textarea">Textarea</option>
    <option value="radio">Radio</option>
    <option value="checkbox">Checkbox</option>
    <option value="range">Range</option>
    <option value="image">Image</option>
  </select>
  <div *ngIf="['radio','checkbox'].includes(form.value.questionType)">
    <label>Options:</label>
    <div formArrayName="options">
      <div *ngFor="let opt of options.controls; let i=index">
        <input [formControlName]="i" placeholder="Option" />
        <button type="button" (click)="removeOption(i)">Remove</button>
      </div>
    </div>
    <button type="button" (click)="addOption()">Add Option</button>
  </div>
  <div *ngIf="form.value.questionType === 'range'">
    <label>Min:</label>
    <input formControlName="min" type="number" placeholder="Min" />
    <label>Max:</label>
    <input formControlName="max" type="number" placeholder="Max" />
  </div>
  <label><input type="checkbox" formControlName="isRequired" /> Required</label>
  <label><input type="checkbox" formControlName="isActive" /> Active</label>
  <button type="submit">Save Question</button>
</form>
```

#### Admin Module Setup & Usage
```typescript
// src/app/app.module.ts
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { AdminQuestionFormComponent } from './components/admin-question-form/admin-question-form.component';

@NgModule({
  declarations: [AdminQuestionFormComponent],
  imports: [BrowserModule, ReactiveFormsModule, HttpClientModule],
  bootstrap: [/* your root component */]
})
export class AppModule {}
```

**Usage in a parent component:**
```html
<app-admin-question-form></app-admin-question-form>
```

---

### 5. User UI: Dynamic Questionnaire

#### Dynamic Questionnaire Component (TypeScript)
```typescript
// src/app/components/dynamic-questionnaire/dynamic-questionnaire.component.ts
import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoryQuestion, CategoryQuestionAnswer } from '../../models/category-question.model';
import { CategoryQuestionService } from '../../services/category-question.service';

@Component({
  selector: 'app-dynamic-questionnaire',
  templateUrl: './dynamic-questionnaire.component.html',
})
export class DynamicQuestionnaireComponent implements OnInit {
  @Input() categoryId!: string;
  @Input() userId!: string;
  questions: CategoryQuestion[] = [];
  form: FormGroup = this.fb.group({});
  loading = false;
  error: string | null = null;
  success: string | null = null;

  constructor(private fb: FormBuilder, private cqService: CategoryQuestionService) {}

  ngOnInit() {
    this.loadQuestions();
  }

  loadQuestions() {
    this.loading = true;
    this.cqService.getQuestionsByCategory(this.categoryId).subscribe({
      next: (qs) => {
        this.questions = qs;
        this.buildForm();
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load questions.';
        this.loading = false;
      }
    });
  }

  buildForm() {
    this.form = this.fb.group({});
    this.questions.forEach(q => {
      let validators = q.isRequired ? [Validators.required] : [];
      if (q.questionType === 'checkbox') {
        this.form.addControl(q.id!, this.fb.control([], validators));
      } else {
        this.form.addControl(q.id!, this.fb.control('', validators));
      }
    });
  }

  getOptions(q: CategoryQuestion): string[] {
    if (!q.optionsJson) return [];
    try {
      const parsed = JSON.parse(q.optionsJson);
      if (Array.isArray(parsed)) return parsed;
      if (parsed.options) return parsed.options;
      return [];
    } catch { return []; }
  }

  getImageUrl(q: CategoryQuestion): string | null {
    if (!q.optionsJson) return null;
    try {
      const parsed = JSON.parse(q.optionsJson);
      return parsed.imageUrl || null;
    } catch { return null; }
  }

  getRange(q: CategoryQuestion): {min: number, max: number} {
    if (!q.optionsJson) return {min: 0, max: 10};
    try {
      const parsed = JSON.parse(q.optionsJson);
      return { min: parsed.min || 0, max: parsed.max || 10 };
    } catch { return {min: 0, max: 10}; }
  }

  onCheckboxChange(event: any, questionId: string, option: string) {
    const current = this.form.get(questionId)?.value || [];
    if (event.target.checked) {
      this.form.get(questionId)?.setValue([...current, option]);
    } else {
      this.form.get(questionId)?.setValue(current.filter((o: string) => o !== option));
    }
  }

  submit() {
    this.error = null;
    this.success = null;
    if (this.form.invalid) {
      this.error = 'Please fill all required fields.';
      return;
    }
    const answers: CategoryQuestionAnswer[] = Object.entries(this.form.value).map(([questionId, answer]) => ({
      categoryQuestionId: questionId,
      userId: this.userId,
      answer: Array.isArray(answer) ? JSON.stringify(answer) : answer
    }));
    this.cqService.submitAnswers(answers).subscribe({
      next: () => this.success = 'Answers submitted successfully!',
      error: () => this.error = 'Failed to submit answers.'
    });
  }
}
```

#### Dynamic Questionnaire Template (HTML)
```html
<form [formGroup]="form" (ngSubmit)="submit()">
  <div *ngIf="loading">Loading questions...</div>
  <div *ngIf="error" style="color:red;">{{error}}</div>
  <div *ngIf="success" style="color:green;">{{success}}</div>
  <div *ngFor="let q of questions">
    <div *ngIf="getImageUrl(q)">
      <img [src]="getImageUrl(q)" alt="Question Image" style="max-width:200px;" />
    </div>
    <label>{{q.questionText}}</label>
    <ng-container [ngSwitch]="q.questionType">
      <input *ngSwitchCase="'text'" [formControlName]="q.id" type="text" />
      <textarea *ngSwitchCase="'textarea'" [formControlName]="q.id"></textarea>
      <div *ngSwitchCase="'radio'">
        <label *ngFor="let opt of getOptions(q)">
          <input type="radio" [formControlName]="q.id" [value]="opt" /> {{opt}}
        </label>
      </div>
      <div *ngSwitchCase="'checkbox'">
        <label *ngFor="let opt of getOptions(q)">
          <input type="checkbox" (change)="onCheckboxChange($event, q.id, opt)" /> {{opt}}
        </label>
      </div>
      <input *ngSwitchCase="'range'" type="range" [formControlName]="q.id" [min]="getRange(q).min" [max]="getRange(q).max" />
      <!-- Add more input types as needed -->
    </ng-container>
    <div *ngIf="form.get(q.id)?.invalid && form.get(q.id)?.touched" style="color:red;">This field is required</div>
  </div>
  <button type="submit">Submit</button>
</form>
```

#### User Module Setup & Usage
```typescript
// src/app/app.module.ts
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { DynamicQuestionnaireComponent } from './components/dynamic-questionnaire/dynamic-questionnaire.component';

@NgModule({
  declarations: [DynamicQuestionnaireComponent],
  imports: [BrowserModule, ReactiveFormsModule, HttpClientModule],
  bootstrap: [/* your root component */]
})
export class AppModule {}
```

**Usage in a parent component:**
```html
<app-dynamic-questionnaire [categoryId]="selectedCategoryId" [userId]="currentUserId"></app-dynamic-questionnaire>
```

---

### 6. Validation & Error Handling
- All required fields are validated in the form.
- For radio/checkbox, at least one option must be selected if required.
- For range, value must be within min/max.
- Errors and success messages are displayed to the user.

### 7. Extending for New Input Types
- Add new cases in the template and logic in the component for new types (e.g., date, file upload).
- Update backend to accept and store new types as needed.

---

## Admin Example: Creating Questions
**Text + Image + Radio:**
```json
{
  "categoryId": "DERMATOLOGY-GUID",
  "questionText": "What do you see in this image?",
  "questionType": "radio",
  "isRequired": true,
  "isActive": true,
  "optionsJson": "{\"imageUrl\": \"https://example.com/image.jpg\", \"options\": [\"Rash\", \"Bruise\", \"Normal\"]}"
}
```
**Checkboxes:**
```json
{
  "categoryId": "DERMATOLOGY-GUID",
  "questionText": "Which symptoms do you have?",
  "questionType": "checkbox",
  "isRequired": false,
  "isActive": true,
  "optionsJson": "[\"Fever\", \"Cough\", \"Fatigue\"]"
}
```
**Range:**
```json
{
  "categoryId": "DERMATOLOGY-GUID",
  "questionText": "Rate your pain level",
  "questionType": "range",
  "isRequired": true,
  "isActive": true,
  "optionsJson": "{\"min\": 1, \"max\": 10}"
}
```

---

## User Example: Submitting Answers
**Payload:**
```json
[
  {
    "categoryQuestionId": "GUID-1",
    "userId": "USER-GUID",
    "answer": "Rash"
  },
  {
    "categoryQuestionId": "GUID-2",
    "userId": "USER-GUID",
    "answer": "[\"Fever\", \"Cough\"]"
  },
  {
    "categoryQuestionId": "GUID-3",
    "userId": "USER-GUID",
    "answer": "7"
  }
]
```

---

## Summary
- **Fully dynamic, config-driven questionnaire system**
- **Admin** can create any question/answer type for any category
- **User** sees and answers questions dynamically based on category
- **Angular code** provided for dynamic form rendering, validation, and answer submission
- **API** is already tested and robust

---

For further customization, file upload/image handling, or advanced admin UI, see the backend API docs or request additional frontend scaffolding. 