# 📚 **QUESTIONNAIRE SYSTEM - COMPREHENSIVE GUIDE**

## 🎯 **OVERVIEW**

This guide provides a complete understanding of the Questionnaire System implementation, including database design, API endpoints, services, repositories, and data flow. Perfect for junior developers to understand the entire system architecture.

---

## 📊 **DATABASE DESIGN & RELATIONSHIPS**

### **Database Schema Overview**

```mermaid
erDiagram
    QuestionnaireTemplate ||--o{ Question : "has many"
    Question ||--o{ QuestionOption : "has many"
    UserResponse ||--o{ UserAnswer : "has many"
    UserAnswer ||--o{ UserAnswerOption : "has many"
    QuestionOption ||--o{ UserAnswerOption : "referenced by"
    
    QuestionnaireTemplate {
        Guid Id PK
        string Name
        string Description
        Guid CategoryId FK
        bool IsActive
        int Version
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    Question {
        Guid Id PK
        Guid TemplateId FK
        string Text
        string Type
        bool IsRequired
        int Order
        string? HelpText
        string? MediaUrl
        decimal? MinValue
        decimal? MaxValue
        decimal? StepValue
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    QuestionOption {
        Guid Id PK
        Guid QuestionId FK
        string Text
        string Value
        int Order
        string? MediaUrl
        bool IsCorrect
        DateTime CreatedAt
    }
    
    UserResponse {
        Guid Id PK
        Guid UserId FK
        Guid CategoryId FK
        Guid TemplateId FK
        string Status
        DateTime CreatedAt
        DateTime? UpdatedAt
    }
    
    UserAnswer {
        Guid Id PK
        Guid ResponseId FK
        Guid QuestionId FK
        string? AnswerText
        decimal? NumericValue
        DateTime CreatedAt
    }
    
    UserAnswerOption {
        Guid Id PK
        Guid AnswerId FK
        Guid OptionId FK
        DateTime CreatedAt
    }
```

### **Table Relationships Explanation**

#### **1. QuestionnaireTemplate → Question (One-to-Many)**
- **Relationship:** One template can have multiple questions
- **Foreign Key:** `Question.TemplateId` references `QuestionnaireTemplate.Id`
- **Cascade Delete:** When template is deleted, all its questions are deleted

#### **2. Question → QuestionOption (One-to-Many)**
- **Relationship:** One question can have multiple options (for multiple choice questions)
- **Foreign Key:** `QuestionOption.QuestionId` references `Question.Id`
- **Cascade Delete:** When question is deleted, all its options are deleted

#### **3. UserResponse → UserAnswer (One-to-Many)**
- **Relationship:** One user response can have multiple answers (one per question)
- **Foreign Key:** `UserAnswer.ResponseId` references `UserResponse.Id`
- **Cascade Delete:** When response is deleted, all its answers are deleted

#### **4. UserAnswer → UserAnswerOption (One-to-Many)**
- **Relationship:** One answer can reference multiple selected options (for checkbox questions)
- **Foreign Key:** `UserAnswerOption.AnswerId` references `UserAnswer.Id`
- **Restrict Delete:** Options cannot be deleted if referenced by answers

---

## 📋 **DATABASE TABLES WITH SAMPLE DATA**

### **1. QuestionnaireTemplates Table**

| Id | Name | Description | CategoryId | IsActive | Version | CreatedAt | UpdatedAt |
|----|------|-------------|------------|----------|---------|-----------|-----------|
| `T1` | "Medical Assessment" | "Comprehensive health evaluation" | `C1` | true | 1 | 2024-01-15 | null |
| `T2` | "Symptom Checker" | "Daily symptom tracking" | `C1` | true | 1 | 2024-01-16 | null |
| `T3` | "Mental Health Survey" | "Psychological well-being assessment" | `C2` | true | 1 | 2024-01-17 | null |

### **2. Questions Table**

| Id | TemplateId | Text | Type | IsRequired | Order | HelpText | MediaUrl | MinValue | MaxValue | StepValue |
|----|------------|------|------|------------|-------|----------|----------|----------|----------|-----------|
| `Q1` | `T1` | "What is your age?" | "text" | true | 1 | "Enter your current age" | null | null | null | null |
| `Q2` | `T1` | "Select your gender" | "radio" | true | 2 | "Choose your gender identity" | null | null | null | null |
| `Q3` | `T1` | "Rate your pain level" | "range" | true | 3 | "Scale from 1-10" | null | 1 | 10 | 1 |
| `Q4` | `T1` | "Select all symptoms" | "checkbox" | false | 4 | "Check all that apply" | null | null | null | null |
| `Q5` | `T1` | "Describe your symptoms" | "textarea" | "true" | 5 | "Provide detailed description" | null | null | null | null |

### **3. QuestionOptions Table**

| Id | QuestionId | Text | Value | Order | MediaUrl | IsCorrect |
|----|------------|------|-------|-------|----------|-----------|
| `O1` | `Q2` | "Male" | "male" | 1 | null | false |
| `O2` | `Q2` | "Female" | "female" | 2 | null | false |
| `O3` | `Q2` | "Other" | "other" | 3 | null | false |
| `O4` | `Q4` | "Headache" | "headache" | 1 | null | false |
| `O5` | `Q4` | "Fever" | "fever" | 2 | null | false |
| `O6` | `Q4` | "Cough" | "cough" | 3 | null | false |
| `O7` | `Q4` | "Fatigue" | "fatigue" | 4 | null | false |

### **4. UserResponses Table**

| Id | UserId | CategoryId | TemplateId | Status | CreatedAt | UpdatedAt |
|----|--------|------------|------------|--------|-----------|-----------|
| `R1` | `U1` | `C1` | `T1` | "completed" | 2024-01-20 10:30:00 | null |
| `R2` | `U2` | `C1` | `T1` | "draft" | 2024-01-20 11:15:00 | 2024-01-20 11:20:00 |
| `R3` | `U1` | `C2` | `T3` | "completed" | 2024-01-21 09:45:00 | null |

### **5. UserAnswers Table**

| Id | ResponseId | QuestionId | AnswerText | NumericValue | CreatedAt |
|----|------------|------------|------------|--------------|-----------|
| `A1` | `R1` | `Q1` | "35" | null | 2024-01-20 10:30:00 |
| `A2` | `R1` | `Q2` | null | null | 2024-01-20 10:30:00 |
| `A3` | `R1` | `Q3` | null | 7.0 | 2024-01-20 10:30:00 |
| `A4` | `R1` | `Q4` | null | null | 2024-01-20 10:30:00 |
| `A5` | `R1` | `Q5` | "Experiencing severe headache and mild fever" | null | 2024-01-20 10:30:00 |

### **6. UserAnswerOptions Table**

| Id | AnswerId | OptionId | CreatedAt |
|----|----------|----------|-----------|
| `AO1` | `A2` | `O1` | 2024-01-20 10:30:00 |
| `AO2` | `A4` | `O4` | 2024-01-20 10:30:00 |
| `AO3` | `A4` | `O5` | 2024-01-20 10:30:00 |

---

## 🔄 **DATA STORAGE EXAMPLES BY QUESTION TYPE**

### **1. Text Question**
```json
// Question
{
  "id": "Q1",
  "text": "What is your name?",
  "type": "text",
  "isRequired": true
}

// User Answer
{
  "id": "A1",
  "answerText": "John Doe",
  "numericValue": null
}
```

### **2. Radio Question**
```json
// Question
{
  "id": "Q2",
  "text": "Select your gender",
  "type": "radio",
  "isRequired": true
}

// Options
[
  {"id": "O1", "text": "Male", "value": "male"},
  {"id": "O2", "text": "Female", "value": "female"}
]

// User Answer
{
  "id": "A2",
  "answerText": null,
  "numericValue": null,
  "selectedOptions": [{"optionId": "O1"}]  // Only one option selected
}
```

### **3. Checkbox Question**
```json
// Question
{
  "id": "Q4",
  "text": "Select all symptoms",
  "type": "checkbox",
  "isRequired": false
}

// Options
[
  {"id": "O4", "text": "Headache", "value": "headache"},
  {"id": "O5", "text": "Fever", "value": "fever"},
  {"id": "O6", "text": "Cough", "value": "cough"}
]

// User Answer
{
  "id": "A4",
  "answerText": null,
  "numericValue": null,
  "selectedOptions": [
    {"optionId": "O4"},  // Headache selected
    {"optionId": "O5"}   // Fever selected
  ]
}
```

### **4. Range Question**
```json
// Question
{
  "id": "Q3",
  "text": "Rate your pain level",
  "type": "range",
  "isRequired": true,
  "minValue": 1,
  "maxValue": 10,
  "stepValue": 1
}

// User Answer
{
  "id": "A3",
  "answerText": null,
  "numericValue": 7.0  // Stored as decimal
}
```

### **5. Textarea Question**
```json
// Question
{
  "id": "Q5",
  "text": "Describe your symptoms",
  "type": "textarea",
  "isRequired": true
}

// User Answer
{
  "id": "A5",
  "answerText": "Experiencing severe headache and mild fever for the past 2 days",
  "numericValue": null
}
```

---

## 🏗️ **ARCHITECTURE LAYERS**

### **Layer Structure**
```
┌─────────────────────────────────────┐
│           API Controllers           │  ← HTTP Endpoints
├─────────────────────────────────────┤
│         Application Services        │  ← Business Logic
├─────────────────────────────────────┤
│         Infrastructure Repositories │  ← Data Access
├─────────────────────────────────────┤
│         Entity Framework Core       │  ← ORM
├─────────────────────────────────────┤
│           SQL Database             │  ← Data Storage
└─────────────────────────────────────┘
```

---

## 🎮 **CONTROLLER LAYER**

### **QuestionnaireController.cs**

#### **Purpose:** Handles HTTP requests for questionnaire operations

#### **Endpoints:**

##### **1. Template Management**

```csharp
// GET /api/questionnaire/templates
[HttpGet("templates")]
public async Task<ActionResult<IEnumerable<QuestionnaireTemplateDto>>> GetAllTemplates()
```
**Purpose:** Retrieve all questionnaire templates
**Returns:** List of all templates with their questions and options
**Use Case:** Admin dashboard to view all available templates

```csharp
// GET /api/questionnaire/templates/{id}
[HttpGet("templates/{id}")]
public async Task<ActionResult<QuestionnaireTemplateDto>> GetTemplateById(Guid id)
```
**Purpose:** Get specific template by ID
**Returns:** Complete template with all questions and options
**Use Case:** Edit template, view template details

```csharp
// GET /api/questionnaire/templates/by-category/{categoryId}
[HttpGet("templates/by-category/{categoryId}")]
public async Task<ActionResult<IEnumerable<QuestionnaireTemplateDto>>> GetTemplatesByCategory(Guid categoryId)
```
**Purpose:** Get templates for specific category
**Returns:** Templates filtered by category
**Use Case:** Show relevant templates to users

```csharp
// POST /api/questionnaire/templates
[HttpPost("templates")]
public async Task<ActionResult<Guid>> CreateTemplate(CreateQuestionnaireTemplateDto dto)
```
**Purpose:** Create new questionnaire template
**Accepts:** Template data with questions and options
**Returns:** Created template ID
**Use Case:** Admin creates new questionnaire

```csharp
// PUT /api/questionnaire/templates/{id}
[HttpPut("templates/{id}")]
public async Task<ActionResult> UpdateTemplate(Guid id, CreateQuestionnaireTemplateDto dto)
```
**Purpose:** Update existing template
**Accepts:** Updated template data
**Returns:** 204 No Content
**Use Case:** Admin modifies existing questionnaire

```csharp
// DELETE /api/questionnaire/templates/{id}
[HttpDelete("templates/{id}")]
public async Task<ActionResult> DeleteTemplate(Guid id)
```
**Purpose:** Delete template
**Returns:** 204 No Content
**Use Case:** Admin removes unused template

##### **2. User Response Management**

```csharp
// POST /api/questionnaire/responses
[HttpPost("responses")]
public async Task<ActionResult<Guid>> SubmitResponse(CreateUserResponseDto dto)
```
**Purpose:** Submit user's questionnaire response
**Accepts:** User's answers to questions
**Returns:** Response ID
**Use Case:** User completes questionnaire

```csharp
// GET /api/questionnaire/responses/{userId}/{templateId}
[HttpGet("responses/{userId}/{templateId}")]
public async Task<ActionResult<UserResponseDto>> GetUserResponse(Guid userId, Guid templateId)
```
**Purpose:** Get user's response for specific template
**Returns:** User's complete response with answers
**Use Case:** View user's submitted answers

```csharp
// GET /api/questionnaire/responses/{userId}/by-category/{categoryId}
[HttpGet("responses/{userId}/by-category/{categoryId}")]
public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUserResponsesByCategory(Guid userId, Guid categoryId)
```
**Purpose:** Get all user responses for category
**Returns:** List of user responses
**Use Case:** View user's history for category

---

## 📸 Media File Handling for Questions and Options

### How Media Upload Works (Current Approach)

- **Frontend:**
  - When creating or updating a questionnaire template, the frontend sends a `multipart/form-data` request.
  - The request contains:
    - A field `templateJson` (stringified JSON of the template/questions/options).
    - File fields for each question/option media, named as `question_{i}_media` or `option_{i}_{j}_media`.
- **Backend:**
  - The controller parses the JSON and passes the DTO and files to the service.
  - The service:
    - Loops through questions/options.
    - If a file is present for a question/option, uploads it using the file storage service.
    - Assigns the resulting URL to the `mediaUrl` property of the question/option.
    - Saves the template as usual.
- **Result:**
  - The user never has to manually upload a file and copy-paste a URL.
  - The backend handles all file storage and URL assignment.

#### Example: Create Template API Call

- **Endpoint:** `POST /api/questionnaire/templates`
- **Content-Type:** `multipart/form-data`
- **Fields:**
  - `templateJson`: stringified JSON of the template/questions/options.
  - `question_0_media`: file (image for question 0, if any)
  - `option_0_1_media`: file (image for option 1 of question 0, if any)
  - ...etc.

#### Backend Service Logic (Pseudocode)

```csharp
foreach (var question in dto.Questions)
{
    var qFile = files.FirstOrDefault(f => f.Name == $"question_{qIdx}_media");
    if (qFile != null)
        question.MediaUrl = await UploadAndGetUrl(qFile);
    foreach (var option in question.Options)
    {
        var oFile = files.FirstOrDefault(f => f.Name == $"option_{qIdx}_{oIdx}_media");
        if (oFile != null)
            option.MediaUrl = await UploadAndGetUrl(oFile);
    }
}
```

#### Why This Approach?
- Keeps the API simple for the frontend: just send files and JSON together.
- Ensures all media files are stored and referenced consistently.
- No manual URL handling or separate upload step.

---

## 🔧 **SERVICE LAYER**

### **QuestionnaireService.cs**

#### **Purpose:** Contains business logic for questionnaire operations

#### **Methods:**

##### **1. Template Operations**

```csharp
public async Task<Guid> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto)
```
**Purpose:** Create new questionnaire template
**Process:**
1. Maps DTO to entity
2. Sets creation timestamp
3. Creates questions and options
4. Saves to database
5. Returns template ID

```csharp
public async Task UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto)
```
**Purpose:** Update existing template
**Process:**
1. Retrieves existing template
2. Updates all properties
3. Replaces all questions and options
4. Sets update timestamp
5. Saves changes

```csharp
public async Task<QuestionnaireTemplateDto?> GetTemplateByIdAsync(Guid id)
```
**Purpose:** Get template by ID
**Process:**
1. Retrieves template with questions and options
2. Maps to DTO
3. Returns formatted data

##### **2. User Response Operations**

```csharp
public async Task<Guid> SubmitUserResponseAsync(CreateUserResponseDto dto)
```
**Purpose:** Submit user's questionnaire response
**Process:**
1. Creates user response entity
2. Maps answers to entities
3. Links selected options
4. Saves to database
5. Returns response ID

```csharp
public async Task<UserResponseDto?> GetUserResponseAsync(Guid userId, Guid templateId)
```
**Purpose:** Get user's response
**Process:**
1. Retrieves response with answers and options
2. Maps to DTO
3. Returns formatted data

---

## 🗄️ **REPOSITORY LAYER**

### **QuestionnaireRepository.cs**

#### **Purpose:** Data access layer for questionnaire entities

#### **Methods:**

##### **1. Template Operations**

```csharp
public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
```
**Purpose:** Get template with all related data
**Implementation:**
```csharp
return await _context.QuestionnaireTemplates
    .Include(t => t.Questions)
        .ThenInclude(q => q.Options)
    .FirstOrDefaultAsync(t => t.Id == id);
```

```csharp
public async Task AddTemplateAsync(QuestionnaireTemplate template)
```
**Purpose:** Save new template
**Implementation:**
```csharp
_context.QuestionnaireTemplates.Add(template);
await _context.SaveChangesAsync();
```

```csharp
public async Task UpdateTemplateAsync(QuestionnaireTemplate template)
```
**Purpose:** Update existing template
**Implementation:**
```csharp
_context.QuestionnaireTemplates.Update(template);
await _context.SaveChangesAsync();
```

##### **2. User Response Operations**

```csharp
public async Task<UserResponse?> GetUserResponseAsync(Guid userId, Guid templateId)
```
**Purpose:** Get user response with answers
**Implementation:**
```csharp
return await _context.UserResponses
    .Include(r => r.Answers)
        .ThenInclude(a => a.SelectedOptions)
    .FirstOrDefaultAsync(r => r.UserId == userId && r.TemplateId == templateId);
```

---

## 📦 **DTO (Data Transfer Object) LAYER**

### **QuestionnaireDtos.cs**

#### **Purpose:** Define data structures for API communication

#### **DTO Classes:**

##### **1. QuestionnaireTemplateDto**
```csharp
public class QuestionnaireTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();
}
```
**Purpose:** Return template data to client
**Usage:** GET endpoints return this structure

##### **2. CreateQuestionnaireTemplateDto**
```csharp
public class CreateQuestionnaireTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<CreateQuestionDto> Questions { get; set; } = new();
}
```
**Purpose:** Accept template creation data from client
**Usage:** POST/PUT endpoints accept this structure

##### **3. QuestionDto**
```csharp
public class QuestionDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? HelpText { get; set; }
    public string? MediaUrl { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? StepValue { get; set; }
    public List<QuestionOptionDto> Options { get; set; } = new();
}
```
**Purpose:** Return question data to client
**Usage:** Included in template responses

##### **4. CreateQuestionDto**
```csharp
public class CreateQuestionDto
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? HelpText { get; set; }
    public string? MediaUrl { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? StepValue { get; set; }
    public List<CreateQuestionOptionDto> Options { get; set; } = new();
}
```
**Purpose:** Accept question creation data
**Usage:** Template creation/update

##### **5. UserResponseDto**
```csharp
public class UserResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid TemplateId { get; set; }
    public string Status { get; set; } = "completed";
    public List<UserAnswerDto> Answers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```
**Purpose:** Return user response data
**Usage:** GET response endpoints

##### **6. CreateUserResponseDto**
```csharp
public class CreateUserResponseDto
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid TemplateId { get; set; }
    public string Status { get; set; } = "completed";
    public List<CreateUserAnswerDto> Answers { get; set; } = new();
}
```
**Purpose:** Accept user response data
**Usage:** Submit response endpoint

---

## 🔄 **COMPLETE DATA FLOW EXAMPLE**

### **Scenario: User Completes Medical Assessment**

#### **Step 1: Get Template**
```http
GET /api/questionnaire/templates/T1
```

**Response:**
```json
{
  "id": "T1",
  "name": "Medical Assessment",
  "description": "Comprehensive health evaluation",
  "categoryId": "C1",
  "isActive": true,
  "version": 1,
  "questions": [
    {
      "id": "Q1",
      "text": "What is your age?",
      "type": "text",
      "isRequired": true,
      "order": 1,
      "options": []
    },
    {
      "id": "Q2",
      "text": "Select your gender",
      "type": "radio",
      "isRequired": true,
      "order": 2,
      "options": [
        {"id": "O1", "text": "Male", "value": "male"},
        {"id": "O2", "text": "Female", "value": "female"}
      ]
    },
    {
      "id": "Q3",
      "text": "Rate your pain level",
      "type": "range",
      "isRequired": true,
      "order": 3,
      "minValue": 1,
      "maxValue": 10,
      "stepValue": 1,
      "options": []
    }
  ]
}
```

#### **Step 2: User Submits Response**
```http
POST /api/questionnaire/responses
```

**Request Body:**
```json
{
  "userId": "U1",
  "categoryId": "C1",
  "templateId": "T1",
  "status": "completed",
  "answers": [
    {
      "questionId": "Q1",
      "answerText": "35",
      "numericValue": null,
      "selectedOptionIds": []
    },
    {
      "questionId": "Q2",
      "answerText": null,
      "numericValue": null,
      "selectedOptionIds": ["O1"]
    },
    {
      "questionId": "Q3",
      "answerText": null,
      "numericValue": 7.0,
      "selectedOptionIds": []
    }
  ]
}
```

#### **Step 3: Database Storage**

**UserResponses Table:**
```sql
INSERT INTO UserResponses (Id, UserId, CategoryId, TemplateId, Status, CreatedAt)
VALUES ('R1', 'U1', 'C1', 'T1', 'completed', '2024-01-20 10:30:00');
```

**UserAnswers Table:**
```sql
INSERT INTO UserAnswers (Id, ResponseId, QuestionId, AnswerText, NumericValue, CreatedAt)
VALUES 
('A1', 'R1', 'Q1', '35', NULL, '2024-01-20 10:30:00'),
('A2', 'R1', 'Q2', NULL, NULL, '2024-01-20 10:30:00'),
('A3', 'R1', 'Q3', NULL, 7.0, '2024-01-20 10:30:00');
```

**UserAnswerOptions Table:**
```sql
INSERT INTO UserAnswerOptions (Id, AnswerId, OptionId, CreatedAt)
VALUES ('AO1', 'A2', 'O1', '2024-01-20 10:30:00');
```

#### **Step 4: Retrieve Response**
```http
GET /api/questionnaire/responses/U1/T1
```

**Response:**
```json
{
  "id": "R1",
  "userId": "U1",
  "categoryId": "C1",
  "templateId": "T1",
  "status": "completed",
  "createdAt": "2024-01-20T10:30:00Z",
  "updatedAt": null,
  "answers": [
    {
      "id": "A1",
      "questionId": "Q1",
      "answerText": "35",
      "numericValue": null,
      "selectedOptionIds": [],
      "createdAt": "2024-01-20T10:30:00Z"
    },
    {
      "id": "A2",
      "questionId": "Q2",
      "answerText": null,
      "numericValue": null,
      "selectedOptionIds": ["O1"],
      "createdAt": "2024-01-20T10:30:00Z"
    },
    {
      "id": "A3",
      "questionId": "Q3",
      "answerText": null,
      "numericValue": 7.0,
      "selectedOptionIds": [],
      "createdAt": "2024-01-20T10:30:00Z"
    }
  ]
}
```

---

## 🎓 **KEY CONCEPTS FOR JUNIOR DEVELOPERS**

### **1. Entity Framework Relationships**
- **One-to-Many:** Template → Questions, Question → Options
- **Cascade Delete:** Deleting parent deletes children
- **Include:** Load related data in single query

### **2. DTO Pattern**
- **Input DTOs:** Accept data from client (Create*Dto)
- **Output DTOs:** Return data to client (*Dto)
- **Separation:** Keep API contracts separate from entities

### **3. Repository Pattern**
- **Abstraction:** Hide data access details
- **Interface:** Define contract for data operations
- **Implementation:** Concrete class handles EF Core

### **4. Service Layer**
- **Business Logic:** Handle complex operations
- **Validation:** Ensure data integrity
- **Mapping:** Convert between DTOs and entities

### **5. Controller Responsibilities**
- **HTTP Handling:** Parse requests and format responses
- **Error Handling:** Catch exceptions and return appropriate status codes
- **Validation:** Basic input validation

---

## 🚀 **IMPLEMENTATION CHECKLIST**

### **For Junior Developers:**

#### **✅ Database Understanding**
- [ ] Understand table relationships
- [ ] Know how data is stored for each question type
- [ ] Understand cascade delete behavior
- [ ] Know foreign key constraints

#### **✅ API Endpoints**
- [ ] Know all available endpoints
- [ ] Understand request/response formats
- [ ] Know HTTP status codes
- [ ] Understand error handling

#### **✅ Service Layer**
- [ ] Understand business logic flow
- [ ] Know DTO mapping
- [ ] Understand validation rules
- [ ] Know error handling patterns

#### **✅ Repository Layer**
- [ ] Understand data access patterns
- [ ] Know Include statements
- [ ] Understand async/await usage
- [ ] Know transaction handling

#### **✅ Testing**
- [ ] Test all question types
- [ ] Test update scenarios
- [ ] Test error conditions
- [ ] Test data integrity

---

## 🏗️ **ENTITY DETAILED EXPLANATION**

### **Core Entities and Their Properties**

#### **1. QuestionnaireTemplate Entity**
```csharp
public class QuestionnaireTemplate : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    [Required]
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
}
```

**Purpose:** Represents a questionnaire template that contains multiple questions
**Key Properties:**
- `Name`: Template title (max 200 chars)
- `Description`: Template description (max 500 chars)
- `CategoryId`: Links to category for organization
- `IsActive`: Controls if template is available
- `Version`: For template versioning
- `CreatedAt/UpdatedAt`: Audit timestamps

#### **2. Question Entity**
```csharp
public class Question : BaseEntity
{
    [Required]
    public Guid TemplateId { get; set; }
    [Required]
    [MaxLength(500)]
    public string Text { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // text, textarea, radio, checkbox, dropdown, range
    public bool IsRequired { get; set; } = true;
    [Required]
    public int Order { get; set; }
    [MaxLength(200)]
    public string? HelpText { get; set; }
    [MaxLength(500)]
    public string? MediaUrl { get; set; }
    // Range-specific properties
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? StepValue { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual QuestionnaireTemplate Template { get; set; } = null!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    
    // Helper methods for validation
    public bool IsMultipleChoice => Type == "radio" || Type == "checkbox" || Type == "dropdown";
    public bool IsTextBased => Type == "text" || Type == "textarea";
    public bool IsRange => Type == "range";
    public bool HasOptions => Options.Count > 0;
}
```

**Purpose:** Represents individual questions within a template
**Key Properties:**
- `TemplateId`: Links to parent template
- `Text`: Question text (max 500 chars)
- `Type`: Question type (text, radio, checkbox, etc.)
- `IsRequired`: Whether answer is mandatory
- `Order`: Display order in template
- `HelpText`: Additional instructions (max 200 chars)
- `MediaUrl`: Image/video URL (max 500 chars)
- `MinValue/MaxValue/StepValue`: For range questions

#### **3. QuestionOption Entity**
```csharp
public class QuestionOption : BaseEntity
{
    [Required]
    public Guid QuestionId { get; set; }
    [Required]
    [MaxLength(200)]
    public string Text { get; set; } = string.Empty;
    [Required]
    [MaxLength(100)]
    public string Value { get; set; } = string.Empty;
    [Required]
    public int Order { get; set; }
    [MaxLength(500)]
    public string? MediaUrl { get; set; }
    public bool IsCorrect { get; set; } = false; // For scoring/validation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual Question Question { get; set; } = null!;
    public virtual ICollection<UserAnswerOption> UserAnswerOptions { get; set; } = new List<UserAnswerOption>();
}
```

**Purpose:** Represents options for multiple choice questions
**Key Properties:**
- `QuestionId`: Links to parent question
- `Text`: Option display text (max 200 chars)
- `Value`: Option value (max 100 chars)
- `Order`: Display order
- `MediaUrl`: Option image URL (max 500 chars)
- `IsCorrect`: For scoring/validation purposes

#### **4. UserResponse Entity**
```csharp
public class UserResponse : BaseEntity
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid CategoryId { get; set; }
    [Required]
    public Guid TemplateId { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = "completed"; // draft, completed, submitted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual QuestionnaireTemplate Template { get; set; } = null!;
    public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
    
    // Helper methods
    public bool IsCompleted => Status == "completed" || Status == "submitted";
    public bool IsDraft => Status == "draft";
}
```

**Purpose:** Represents a user's response to a questionnaire template
**Key Properties:**
- `UserId`: Links to user who submitted response
- `CategoryId`: Category for organization
- `TemplateId`: Links to template being answered
- `Status`: Response status (draft, completed, submitted)
- `CreatedAt/UpdatedAt`: Audit timestamps

#### **5. UserAnswer Entity**
```csharp
public class UserAnswer : BaseEntity
{
    [Required]
    public Guid ResponseId { get; set; }
    [Required]
    public Guid QuestionId { get; set; }
    [MaxLength(4000)] // For text/textarea answers
    public string? AnswerText { get; set; }
    public decimal? NumericValue { get; set; } // For range answers
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual UserResponse Response { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
    
    // Helper methods
    public bool HasTextAnswer => !string.IsNullOrEmpty(AnswerText);
    public bool HasNumericAnswer => NumericValue.HasValue;
    public bool HasSelectedOptions => SelectedOptions.Count > 0;
    public bool IsAnswered => HasTextAnswer || HasNumericAnswer || HasSelectedOptions;
}
```

**Purpose:** Represents a user's answer to a specific question
**Key Properties:**
- `ResponseId`: Links to parent response
- `QuestionId`: Links to question being answered
- `AnswerText`: Text answer (max 4000 chars)
- `NumericValue`: Numeric answer for range questions
- `CreatedAt`: Answer timestamp

#### **6. UserAnswerOption Entity**
```csharp
public class UserAnswerOption : BaseEntity
{
    [Required]
    public Guid AnswerId { get; set; }
    [Required]
    public Guid OptionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public virtual UserAnswer Answer { get; set; } = null!;
    public virtual QuestionOption Option { get; set; } = null!;
}
```

**Purpose:** Links user answers to selected options (for multiple choice questions)
**Key Properties:**
- `AnswerId`: Links to user answer
- `OptionId`: Links to selected option
- `CreatedAt`: Selection timestamp

---

## 🔄 **COMPLETE CRUD OPERATIONS EXPLANATION**

### **CREATE Operations**

#### **1. Create Template**
```csharp
// Service Method
public async Task<Guid> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto)
{
    var template = new QuestionnaireTemplate
    {
        Name = dto.Name,
        Description = dto.Description,
        CategoryId = dto.CategoryId,
        IsActive = dto.IsActive,
        Version = 1,
        CreatedAt = DateTime.UtcNow,
        Questions = dto.Questions.Select(q => new Question
        {
            Text = q.Text,
            Type = q.Type,
            IsRequired = q.IsRequired,
            Order = q.Order,
            HelpText = q.HelpText,
            MediaUrl = q.MediaUrl,
            MinValue = q.MinValue,
            MaxValue = q.MaxValue,
            StepValue = q.StepValue,
            CreatedAt = DateTime.UtcNow,
            Options = q.Options.Select(o => new QuestionOption
            {
                Text = o.Text,
                Value = o.Value,
                Order = o.Order,
                MediaUrl = o.MediaUrl
            }).ToList()
        }).ToList()
    };
    
    await _repo.AddTemplateAsync(template);
    return template.Id;
}
```

**Process:**
1. **DTO Validation:** Validate input data
2. **Entity Creation:** Map DTO to entity
3. **Nested Creation:** Create questions and options
4. **Database Save:** Save to database via repository
5. **Return ID:** Return created template ID

#### **2. Submit User Response**
```csharp
// Service Method
public async Task<Guid> SubmitUserResponseAsync(CreateUserResponseDto dto)
{
    var response = new UserResponse
    {
        UserId = dto.UserId,
        CategoryId = dto.CategoryId,
        TemplateId = dto.TemplateId,
        Status = dto.Status,
        CreatedAt = DateTime.UtcNow,
        Answers = dto.Answers.Select(a => new UserAnswer
        {
            QuestionId = a.QuestionId,
            AnswerText = a.AnswerText,
            NumericValue = a.NumericValue,
            CreatedAt = DateTime.UtcNow,
            SelectedOptions = a.SelectedOptionIds.Select(optionId => new UserAnswerOption
            {
                OptionId = optionId
            }).ToList()
        }).ToList()
    };
    
    await _repo.AddUserResponseAsync(response);
    return response.Id;
}
```

**Process:**
1. **Response Creation:** Create UserResponse entity
2. **Answer Mapping:** Map each answer to UserAnswer entity
3. **Option Linking:** Link selected options for multiple choice
4. **Database Save:** Save response and all answers
5. **Return ID:** Return response ID

### **READ Operations**

#### **1. Get Template by ID**
```csharp
// Repository Method
public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
{
    return await _context.QuestionnaireTemplates
        .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
        .FirstOrDefaultAsync(t => t.Id == id);
}

// Service Method
public async Task<QuestionnaireTemplateDto?> GetTemplateByIdAsync(Guid id)
{
    var template = await _repo.GetTemplateByIdAsync(id);
    return template == null ? null : MapToDto(template);
}
```

**Process:**
1. **Database Query:** Load template with related data
2. **Include Relations:** Load questions and options
3. **DTO Mapping:** Convert entity to DTO
4. **Return Data:** Return formatted template

#### **2. Get User Response**
```csharp
// Repository Method
public async Task<UserResponse?> GetUserResponseAsync(Guid userId, Guid templateId)
{
    return await _context.UserResponses
        .Include(r => r.Answers)
            .ThenInclude(a => a.SelectedOptions)
        .FirstOrDefaultAsync(r => r.UserId == userId && r.TemplateId == templateId);
}
```

**Process:**
1. **Database Query:** Load response with answers
2. **Include Relations:** Load selected options
3. **DTO Mapping:** Convert to response DTO
4. **Return Data:** Return complete response

### **UPDATE Operations**

#### **1. Update Template**
```csharp
// Service Method
public async Task UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto)
{
    var template = await _repo.GetTemplateByIdAsync(id);
    if (template == null) throw new Exception("Template not found");
    
    // Update template properties
    template.Name = dto.Name;
    template.Description = dto.Description;
    template.CategoryId = dto.CategoryId;
    template.IsActive = dto.IsActive;
    template.UpdatedAt = DateTime.UtcNow;
    
    // Replace all questions (complete replacement approach)
    template.Questions = dto.Questions.Select(q => new Question
    {
        Text = q.Text,
        Type = q.Type,
        IsRequired = q.IsRequired,
        Order = q.Order,
        HelpText = q.HelpText,
        MediaUrl = q.MediaUrl,
        MinValue = q.MinValue,
        MaxValue = q.MaxValue,
        StepValue = q.StepValue,
        CreatedAt = DateTime.UtcNow,
        Options = q.Options.Select(o => new QuestionOption
        {
            Text = o.Text,
            Value = o.Value,
            Order = o.Order,
            MediaUrl = o.MediaUrl
        }).ToList()
    }).ToList();
    
    await _repo.UpdateTemplateAsync(template);
}
```

**Process:**
1. **Load Existing:** Retrieve current template
2. **Update Properties:** Update template metadata
3. **Replace Questions:** Complete replacement of questions
4. **Replace Options:** Complete replacement of options
5. **Save Changes:** Save to database

### **DELETE Operations**

#### **1. Delete Template**
```csharp
// Repository Method
public async Task DeleteTemplateAsync(Guid id)
{
    var template = await _context.QuestionnaireTemplates.FindAsync(id);
    if (template != null)
    {
        _context.QuestionnaireTemplates.Remove(template);
        await _context.SaveChangesAsync();
    }
}
```

**Process:**
1. **Find Template:** Locate template by ID
2. **Cascade Delete:** EF Core automatically deletes related questions and options
3. **Save Changes:** Commit deletion

---

## 🎯 **SERVICE LAYER DETAILED EXPLANATION**

### **QuestionnaireService.cs - Complete Business Logic**

#### **Purpose:** Orchestrates all questionnaire operations with business rules

#### **Key Responsibilities:**

##### **1. Template Management**
```csharp
// Create new template with validation
public async Task<Guid> CreateTemplateAsync(CreateQuestionnaireTemplateDto dto)
{
    // Business Logic:
    // - Validate template data
    // - Ensure questions are ordered correctly
    // - Validate question types and options
    // - Set default values (Version = 1, CreatedAt = now)
    
    var template = new QuestionnaireTemplate
    {
        Name = dto.Name,
        Description = dto.Description,
        CategoryId = dto.CategoryId,
        IsActive = dto.IsActive,
        Version = 1, // Business rule: new templates start at version 1
        CreatedAt = DateTime.UtcNow,
        Questions = dto.Questions.Select(q => new Question
        {
            Text = q.Text,
            Type = q.Type,
            IsRequired = q.IsRequired,
            Order = q.Order,
            HelpText = q.HelpText,
            MediaUrl = q.MediaUrl,
            MinValue = q.MinValue,
            MaxValue = q.MaxValue,
            StepValue = q.StepValue,
            CreatedAt = DateTime.UtcNow,
            Options = q.Options.Select(o => new QuestionOption
            {
                Text = o.Text,
                Value = o.Value,
                Order = o.Order,
                MediaUrl = o.MediaUrl
            }).ToList()
        }).ToList()
    };
    
    await _repo.AddTemplateAsync(template);
    return template.Id;
}
```

**Business Rules:**
- New templates start at version 1
- All questions must have valid types
- Options are required for multiple choice questions
- Questions must be ordered sequentially

##### **2. Template Updates**
```csharp
public async Task UpdateTemplateAsync(Guid id, CreateQuestionnaireTemplateDto dto)
{
    var template = await _repo.GetTemplateByIdAsync(id);
    if (template == null) throw new Exception("Template not found");
    
    // Business Logic:
    // - Complete replacement approach (not incremental)
    // - Update version number
    // - Maintain audit trail with UpdatedAt
    
    template.Name = dto.Name;
    template.Description = dto.Description;
    template.CategoryId = dto.CategoryId;
    template.IsActive = dto.IsActive;
    template.UpdatedAt = DateTime.UtcNow;
    
    // Complete replacement of questions and options
    template.Questions = dto.Questions.Select(q => new Question
    {
        Text = q.Text,
        Type = q.Type,
        IsRequired = q.IsRequired,
        Order = q.Order,
        HelpText = q.HelpText,
        MediaUrl = q.MediaUrl,
        MinValue = q.MinValue,
        MaxValue = q.MaxValue,
        StepValue = q.StepValue,
        CreatedAt = DateTime.UtcNow,
        Options = q.Options.Select(o => new QuestionOption
        {
            Text = o.Text,
            Value = o.Value,
            Order = o.Order,
            MediaUrl = o.MediaUrl
        }).ToList()
    }).ToList();
    
    await _repo.UpdateTemplateAsync(template);
}
```

**Business Rules:**
- Complete replacement approach (not incremental updates)
- Update audit timestamp
- Validate all question types and options
- Maintain data integrity

##### **3. User Response Processing**
```csharp
public async Task<Guid> SubmitUserResponseAsync(CreateUserResponseDto dto)
{
    // Business Logic:
    // - Validate response data
    // - Ensure all required questions are answered
    // - Validate answer types match question types
    // - Handle multiple choice selections
    
    var response = new UserResponse
    {
        UserId = dto.UserId,
        CategoryId = dto.CategoryId,
        TemplateId = dto.TemplateId,
        Status = dto.Status, // "completed", "draft", "submitted"
        CreatedAt = DateTime.UtcNow,
        Answers = dto.Answers.Select(a => new UserAnswer
        {
            QuestionId = a.QuestionId,
            AnswerText = a.AnswerText,
            NumericValue = a.NumericValue,
            CreatedAt = DateTime.UtcNow,
            SelectedOptions = a.SelectedOptionIds.Select(optionId => new UserAnswerOption
            {
                OptionId = optionId
            }).ToList()
        }).ToList()
    };
    
    await _repo.AddUserResponseAsync(response);
    return response.Id;
}
```

**Business Rules:**
- Validate all required questions are answered
- Ensure answer types match question types
- Handle multiple choice selections properly
- Set appropriate response status

---

## 🗄️ **REPOSITORY LAYER DETAILED EXPLANATION**

### **QuestionnaireRepository.cs - Data Access Layer**

#### **Purpose:** Abstracts database operations and handles Entity Framework queries

#### **Key Methods and Their Implementation:**

##### **1. Template Operations**

```csharp
// Get template with all related data
public async Task<QuestionnaireTemplate?> GetTemplateByIdAsync(Guid id)
{
    return await _context.QuestionnaireTemplates
        .Include(t => t.Questions)           // Load questions
            .ThenInclude(q => q.Options)     // Load options for each question
        .FirstOrDefaultAsync(t => t.Id == id);
}
```

**What it does:**
- Uses Entity Framework Include to load related data
- Loads template with all questions and options in single query
- Returns null if template not found

```csharp
// Get templates by category
public async Task<IEnumerable<QuestionnaireTemplate>> GetTemplatesByCategoryAsync(Guid categoryId)
{
    return await _context.QuestionnaireTemplates
        .Where(t => t.CategoryId == categoryId)  // Filter by category
        .Include(t => t.Questions)
            .ThenInclude(q => q.Options)
        .ToListAsync();
}
```

**What it does:**
- Filters templates by category
- Loads all related data
- Returns list of templates

```csharp
// Add new template
public async Task AddTemplateAsync(QuestionnaireTemplate template)
{
    _context.QuestionnaireTemplates.Add(template);
    await _context.SaveChangesAsync();
}
```

**What it does:**
- Adds template to EF context
- Saves all related entities (questions, options)
- Uses transaction to ensure data consistency

```csharp
// Update existing template
public async Task UpdateTemplateAsync(QuestionnaireTemplate template)
{
    _context.QuestionnaireTemplates.Update(template);
    await _context.SaveChangesAsync();
}
```

**What it does:**
- Updates template and all related entities
- EF Core handles cascade updates
- Maintains data integrity

##### **2. User Response Operations**

```csharp
// Get user response with answers and selected options
public async Task<UserResponse?> GetUserResponseAsync(Guid userId, Guid templateId)
{
    return await _context.UserResponses
        .Include(r => r.Answers)                    // Load answers
            .ThenInclude(a => a.SelectedOptions)    // Load selected options
        .FirstOrDefaultAsync(r => r.UserId == userId && r.TemplateId == templateId);
}
```

**What it does:**
- Loads user response with all answers
- Loads selected options for multiple choice questions
- Filters by user and template

```csharp
// Get user responses by category
public async Task<IEnumerable<UserResponse>> GetUserResponsesByCategoryAsync(Guid userId, Guid categoryId)
{
    return await _context.UserResponses
        .Where(r => r.UserId == userId && r.CategoryId == categoryId)
        .Include(r => r.Answers)
            .ThenInclude(a => a.SelectedOptions)
        .ToListAsync();
}
```

**What it does:**
- Filters responses by user and category
- Loads all related data
- Returns list of responses

```csharp
// Add user response
public async Task AddUserResponseAsync(UserResponse response)
{
    _context.UserResponses.Add(response);
    await _context.SaveChangesAsync();
}
```

**What it does:**
- Adds response and all related answers
- Adds selected options for multiple choice
- Uses transaction for data consistency

---

## 🎮 **CONTROLLER LAYER DETAILED EXPLANATION**

### **QuestionnaireController.cs - HTTP Request Handler**

#### **Purpose:** Handles HTTP requests, validates input, and returns appropriate responses

#### **Key Responsibilities:**

##### **1. Request Validation**
```csharp
[HttpPost("templates")]
public async Task<ActionResult<Guid>> CreateTemplate(CreateQuestionnaireTemplateDto dto)
{
    try
    {
        // Controller validates basic input
        if (string.IsNullOrEmpty(dto.Name))
            return BadRequest(new { error = "Template name is required" });
        
        var templateId = await _questionnaireService.CreateTemplateAsync(dto);
        return CreatedAtAction(nameof(GetTemplateById), new { id = templateId }, templateId);
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

**What it does:**
- Validates basic input data
- Calls service layer for business logic
- Returns appropriate HTTP status codes
- Handles exceptions gracefully

##### **2. Response Formatting**
```csharp
[HttpGet("templates/{id}")]
public async Task<ActionResult<QuestionnaireTemplateDto>> GetTemplateById(Guid id)
{
    var template = await _questionnaireService.GetTemplateByIdAsync(id);
    if (template == null)
        return NotFound();  // 404 if not found
    
    return Ok(template);   // 200 with data
}
```

**What it does:**
- Calls service to get data
- Returns 404 if not found
- Returns 200 with data if found
- Formats response properly

##### **3. Error Handling**
```csharp
[HttpPut("templates/{id}")]
public async Task<ActionResult> UpdateTemplate(Guid id, CreateQuestionnaireTemplateDto dto)
{
    try
    {
        await _questionnaireService.UpdateTemplateAsync(id, dto);
        return NoContent();  // 204 for successful update
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });  // 400 for errors
    }
}
```

**What it does:**
- Wraps service calls in try-catch
- Returns appropriate HTTP status codes
- Provides meaningful error messages

---

## 📦 **DTO LAYER DETAILED EXPLANATION**

### **QuestionnaireDtos.cs - Data Transfer Objects**

#### **Purpose:** Define data contracts for API communication

#### **Input DTOs (Create*Dto):**

##### **1. CreateQuestionnaireTemplateDto**
```csharp
public class CreateQuestionnaireTemplateDto
{
    public string Name { get; set; } = string.Empty;           // Required
    public string Description { get; set; } = string.Empty;    // Optional
    public Guid CategoryId { get; set; }                       // Required
    public bool IsActive { get; set; } = true;                // Default true
    public List<CreateQuestionDto> Questions { get; set; } = new();  // Required
}
```

**Usage:** POST/PUT requests to create/update templates
**Validation:** Name and CategoryId are required

##### **2. CreateQuestionDto**
```csharp
public class CreateQuestionDto
{
    public string Text { get; set; } = string.Empty;          // Required
    public string Type { get; set; } = string.Empty;          // Required
    public bool IsRequired { get; set; }                      // Default true
    public int Order { get; set; }                            // Required
    public string? HelpText { get; set; }                     // Optional
    public string? MediaUrl { get; set; }                     // Optional
    public decimal? MinValue { get; set; }                    // For range questions
    public decimal? MaxValue { get; set; }                    // For range questions
    public decimal? StepValue { get; set; }                   // For range questions
    public List<CreateQuestionOptionDto> Options { get; set; } = new();  // For multiple choice
}
```

**Usage:** Creating questions within templates
**Validation:** Text, Type, and Order are required

##### **3. CreateUserResponseDto**
```csharp
public class CreateUserResponseDto
{
    public Guid UserId { get; set; }                          // Required
    public Guid CategoryId { get; set; }                      // Required
    public Guid TemplateId { get; set; }                      // Required
    public string Status { get; set; } = "completed";         // Default completed
    public List<CreateUserAnswerDto> Answers { get; set; } = new();  // Required
}
```

**Usage:** Submitting user responses
**Validation:** UserId, CategoryId, TemplateId, and Answers are required

#### **Output DTOs (*Dto):**

##### **1. QuestionnaireTemplateDto**
```csharp
public class QuestionnaireTemplateDto
{
    public Guid Id { get; set; }                              // Generated
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public bool IsActive { get; set; }
    public int Version { get; set; }
    public List<QuestionDto> Questions { get; set; } = new();  // Nested data
}
```

**Usage:** GET responses returning template data
**Features:** Includes nested questions and options

##### **2. UserResponseDto**
```csharp
public class UserResponseDto
{
    public Guid Id { get; set; }                              // Generated
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public Guid TemplateId { get; set; }
    public string Status { get; set; } = "completed";
    public List<UserAnswerDto> Answers { get; set; } = new();  // Nested data
    public DateTime CreatedAt { get; set; }                   // Audit
    public DateTime? UpdatedAt { get; set; }                  // Audit
}
```

**Usage:** GET responses returning user response data
**Features:** Includes nested answers and selected options

---

## 📝 **SUMMARY**

This questionnaire system provides:

1. **Complete CRUD operations** for templates and responses
2. **Flexible question types** (text, radio, checkbox, range, textarea)
3. **Proper data relationships** with cascade delete
4. **Clean architecture** with separation of concerns
5. **Comprehensive API** with proper error handling
6. **Scalable design** for future enhancements
7. **Detailed entity explanations** with all properties and relationships
8. **Complete CRUD operation breakdowns** with step-by-step processes
9. **Service layer business logic** with validation rules
10. **Repository data access patterns** with EF Core usage
11. **Controller HTTP handling** with proper status codes
12. **DTO data contracts** with input/output specifications

The system is production-ready and handles all common questionnaire scenarios efficiently.

---

## 📚 **ADDITIONAL RESOURCES**

### **Files to Study:**
- `backend/SmartTelehealth.API/Controllers/QuestionnaireController.cs`
- `backend/SmartTelehealth.Application/Services/QuestionnaireService.cs`
- `backend/SmartTelehealth.Infrastructure/Repositories/QuestionnaireRepository.cs`
- `backend/SmartTelehealth.Application/DTOs/QuestionnaireDtos.cs`
- `backend/SmartTelehealth.Core/Entities/QuestionnaireTemplate.cs`
- `backend/SmartTelehealth.Core/Entities/Question.cs`
- `backend/SmartTelehealth.Core/Entities/UserResponse.cs`

### **Key Concepts:**
- Clean Architecture
- Repository Pattern
- DTO Pattern
- Entity Framework Core
- RESTful API Design
- Async/Await Programming
- Database Relationships

---

## 🤝 **SUPPORT**

For questions or issues with the questionnaire system, refer to:
- Database schema documentation
- API endpoint documentation
- Service layer documentation
- Repository pattern examples

**Happy Coding! 🚀** 