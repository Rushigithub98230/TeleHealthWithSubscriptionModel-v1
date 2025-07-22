# Dynamic Category-Based Questionnaire System (Angular Client)

## Table of Contents
1. [Overview](#overview)
2. [Features](#features)
3. [API Endpoints](#api-endpoints)
4. [Angular Implementation](#angular-implementation)
    - [1. Models & Interfaces](#1-models--interfaces)
    - [2. API Service](#2-api-service)
    - [3. Admin UI: Create/Edit Templates & Questions](#3-admin-ui-createedit-templates--questions)
    - [4. User UI: Dynamic Questionnaire](#4-user-ui-dynamic-questionnaire)
    - [5. Validation & Error Handling](#5-validation--error-handling)
    - [6. Extending for New Input Types](#6-extending-for-new-input-types)
5. [Example Payloads](#example-payloads)
6. [Summary](#summary)

---

## Overview
This system enables dynamic, config-driven questionnaires for each category. Admins can create templates and questions (text, image, or both) and define answer types (textbox, textarea, radio, checkbox, dropdown, range, etc.) for each category. Users see relevant questions when selecting a category and must answer them before proceeding.

---

## Features
- **Admin:**
  - Create/edit/delete questionnaire templates for any category
  - Add/edit/delete questions and options
  - Configure each question as text, image, or both
  - Select answer type: textbox, textarea, radio, checkbox, dropdown, range, etc.
  - Set required/optional, order, help text, and media
- **User:**
  - Select a category
  - See all relevant questions rendered dynamically
  - Fill answers using the correct input type
  - Submit answers, then proceed as needed
- **Backend:**
  - All templates, questions, types, and options are config-driven (no hardcoding)
  - API endpoints for CRUD, fetch by category, and answer submission

---

## API Endpoints
- `GET /api/questionnaire/by-category/{categoryId}` — Get templates for category (user/admin)
- `POST /api/questionnaire/templates` — Create template (admin)
- `PUT /api/questionnaire/templates/{id}` — Update template (admin)
- `DELETE /api/questionnaire/templates/{id}` — Delete template (admin)
- `POST /api/questionnaire/responses` — Submit user response (user)
- `GET /api/questionnaire/responses/{userId}/{templateId}` — Get user response for template (user)

---

## Angular Implementation

### 1. Models & Interfaces
```typescript
export interface QuestionnaireTemplate {
  id?: string;
  name: string;
  description: string;
  categoryId: string;
  isActive: boolean;
  version: number;
  questions: Question[];
}

export interface Question {
  id?: string;
  templateId?: string;
  text: string;
  type: string; // 'text', 'textarea', 'radio', 'checkbox', 'dropdown', 'range', etc.
  isRequired: boolean;
  order: number;
  helpText?: string;
  mediaUrl?: string;
  options: QuestionOption[];
}

export interface QuestionOption {
  id?: string;
  questionId?: string;
  text: string;
  value: string;
  order: number;
  mediaUrl?: string;
}

export interface UserResponse {
  id?: string;
  userId: string;
  categoryId: string;
  templateId: string;
  answers: UserAnswer[];
}

export interface UserAnswer {
  id?: string;
  questionId: string;
  answerText?: string;
  selectedOptionIds: string[];
}
```

---

### 2. API Service
```typescript
@Injectable({ providedIn: 'root' })
export class QuestionnaireService {
  constructor(private http: HttpClient) {}

  // Admin
  getTemplatesByCategory(categoryId: string): Observable<QuestionnaireTemplate[]> {
    return this.http.get<QuestionnaireTemplate[]>(`/api/questionnaire/by-category/${categoryId}`);
  }
  createTemplate(template: QuestionnaireTemplate): Observable<any> {
    return this.http.post('/api/questionnaire/templates', template);
  }
  updateTemplate(id: string, template: QuestionnaireTemplate): Observable<any> {
    return this.http.put(`/api/questionnaire/templates/${id}`, template);
  }
  deleteTemplate(id: string): Observable<any> {
    return this.http.delete(`/api/questionnaire/templates/${id}`);
  }

  // User
  submitUserResponse(response: UserResponse): Observable<any> {
    return this.http.post('/api/questionnaire/responses', response);
  }
  getUserResponse(userId: string, templateId: string): Observable<UserResponse> {
    return this.http.get<UserResponse>(`/api/questionnaire/responses/${userId}/${templateId}`);
  }
}
```

---

### 3. Admin UI: Create/Edit Templates & Questions
- Use a form to build a `QuestionnaireTemplate` object with a list of `Question` objects.
- Each `Question` can have a list of `QuestionOption` objects for types like radio, checkbox, dropdown.
- Submit the full template to `/api/questionnaire/templates`.

**Example Admin Form (TypeScript):**
```typescript
// src/app/components/admin-template-form/admin-template-form.component.ts
// ... imports ...
@Component({
  selector: 'app-admin-template-form',
  templateUrl: './admin-template-form.component.html',
})
export class AdminTemplateFormComponent implements OnInit {
  // ... form setup for template, questions, options ...
  // See README for detailed example
}
```

**Example Admin Form (HTML):**
```html
<!-- Render template fields, then for each question: -->
<div *ngFor="let q of questions; let i = index">
  <input [(ngModel)]="q.text" placeholder="Question Text" />
  <select [(ngModel)]="q.type">
    <option value="text">Text</option>
    <option value="textarea">Textarea</option>
    <option value="radio">Radio</option>
    <option value="checkbox">Checkbox</option>
    <option value="dropdown">Dropdown</option>
    <option value="range">Range</option>
  </select>
  <!-- Render options for radio/checkbox/dropdown -->
  <div *ngIf="['radio','checkbox','dropdown'].includes(q.type)">
    <div *ngFor="let opt of q.options; let j = index">
      <input [(ngModel)]="opt.text" placeholder="Option Text" />
      <input [(ngModel)]="opt.value" placeholder="Option Value" />
    </div>
    <button (click)="addOption(i)">Add Option</button>
  </div>
  <!-- Range min/max -->
  <div *ngIf="q.type === 'range'">
    <input [(ngModel)]="q.helpText" placeholder="Help Text (e.g. min/max)" />
  </div>
  <input [(ngModel)]="q.mediaUrl" placeholder="Media URL (optional)" />
  <input type="checkbox" [(ngModel)]="q.isRequired" /> Required
  <input type="number" [(ngModel)]="q.order" placeholder="Order" />
</div>
```

---

### 4. User UI: Dynamic Questionnaire
- Fetch the template for the selected category.
- Render each question based on its `type`:
  - `text`/`textarea`: `<input>` or `<textarea>`
  - `radio`: `<input type="radio">` for each option
  - `checkbox`: `<input type="checkbox">` for each option
  - `dropdown`: `<select>` with options
  - `range`: `<input type="range">`
- Collect answers into a `UserResponse` object and submit to `/api/questionnaire/responses`.

**Example User Component (TypeScript):**
```typescript
// src/app/components/dynamic-questionnaire/dynamic-questionnaire.component.ts
// ... imports ...
@Component({
  selector: 'app-dynamic-questionnaire',
  templateUrl: './dynamic-questionnaire.component.html',
})
export class DynamicQuestionnaireComponent implements OnInit {
  // ... fetch template, build form, handle submission ...
  // See README for detailed example
}
```

**Example User Component (HTML):**
```html
<form [formGroup]="form" (ngSubmit)="submit()">
  <div *ngFor="let q of questions">
    <label>{{q.text}}</label>
    <ng-container [ngSwitch]="q.type">
      <input *ngSwitchCase="'text'" [formControlName]="q.id" type="text" />
      <textarea *ngSwitchCase="'textarea'" [formControlName]="q.id"></textarea>
      <div *ngSwitchCase="'radio'">
        <label *ngFor="let opt of q.options">
          <input type="radio" [formControlName]="q.id" [value]="opt.id" /> {{opt.text}}
        </label>
      </div>
      <div *ngSwitchCase="'checkbox'">
        <label *ngFor="let opt of q.options">
          <input type="checkbox" (change)="onCheckboxChange($event, q.id, opt.id)" /> {{opt.text}}
        </label>
      </div>
      <select *ngSwitchCase="'dropdown'" [formControlName]="q.id">
        <option *ngFor="let opt of q.options" [value]="opt.id">{{opt.text}}</option>
      </select>
      <input *ngSwitchCase="'range'" type="range" [formControlName]="q.id" [min]="getMin(q)" [max]="getMax(q)" />
    </ng-container>
    <div *ngIf="form.get(q.id)?.invalid && form.get(q.id)?.touched" style="color:red;">This field is required</div>
  </div>
  <button type="submit">Submit</button>
</form>
```

---

### 5. Validation & Error Handling
- All required fields are validated in the form.
- For radio/checkbox, at least one option must be selected if required.
- For range, value must be within min/max.
- Errors and success messages are displayed to the user.

---

### 6. Extending for New Input Types
- Add new cases in the template and logic in the component for new types (e.g., date, file upload).
- Update backend to accept and store new types as needed.

---

## Example Payloads

### **Admin: Create Template**
```json
{
  "name": "Diabetes Intake",
  "description": "Initial intake for diabetes management",
  "categoryId": "CATEGORY-GUID",
  "isActive": true,
  "version": 1,
  "questions": [
    {
      "text": "What is your age?",
      "type": "text",
      "isRequired": true,
      "order": 1,
      "options": []
    },
    {
      "text": "Select your symptoms:",
      "type": "checkbox",
      "isRequired": false,
      "order": 2,
      "options": [
        { "text": "Fatigue", "value": "fatigue", "order": 1 },
        { "text": "Thirst", "value": "thirst", "order": 2 }
      ]
    }
  ]
}
```

### **User: Submit Response**
```json
{
  "userId": "USER-GUID",
  "categoryId": "CATEGORY-GUID",
  "templateId": "TEMPLATE-GUID",
  "answers": [
    { "questionId": "Q1", "answerText": "45", "selectedOptionIds": [] },
    { "questionId": "Q2", "selectedOptionIds": ["OPT1", "OPT2"] }
  ]
}
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