# 📊 **QUESTIONNAIRE SYSTEM - ENTITIES & RELATIONSHIPS COMPLETE GUIDE**

## 🎯 **OVERVIEW**

This document provides a comprehensive overview of all entities in the Questionnaire System, their properties, relationships, and how they work together to create a robust healthcare questionnaire platform.

---

## 🗄️ **ENTITY RELATIONSHIP DIAGRAM**

```mermaid
erDiagram
    %% Core Entities
    QuestionnaireTemplate ||--o{ Question : "has many"
    Question ||--o{ QuestionOption : "has many"
    UserResponse ||--o{ UserAnswer : "has many"
    UserAnswer ||--o{ UserAnswerOption : "has many"
    QuestionOption ||--o{ UserAnswerOption : "referenced by"
    
    %% Base Entity Inheritance
    BaseEntity ||--|| QuestionnaireTemplate : "inherits"
    BaseEntity ||--|| Question : "inherits"
    BaseEntity ||--|| QuestionOption : "inherits"
    BaseEntity ||--|| UserResponse : "inherits"
    BaseEntity ||--|| UserAnswer : "inherits"
    BaseEntity ||--|| UserAnswerOption : "inherits"
    
    %% External References (not shown in detail)
    User ||--o{ UserResponse : "creates"
    Category ||--o{ QuestionnaireTemplate : "belongs to"
    Category ||--o{ UserResponse : "belongs to"
    
    %% Entity Details
    QuestionnaireTemplate {
        Guid Id PK
        string Name "200 chars"
        string Description "500 chars"
        Guid CategoryId FK
        bool IsActive
        int Version
        DateTime CreatedAt
        DateTime? UpdatedAt
        bool IsDeleted
    }
    
    Question {
        Guid Id PK
        Guid TemplateId FK
        string Text "500 chars"
        string Type "50 chars"
        bool IsRequired
        int Order
        string? HelpText "200 chars"
        string? MediaUrl "500 chars"
        decimal? MinValue "18,2"
        decimal? MaxValue "18,2"
        decimal? StepValue "18,2"
        DateTime CreatedAt
        DateTime? UpdatedAt
        bool IsDeleted
    }
    
    QuestionOption {
        Guid Id PK
        Guid QuestionId FK
        string Text "200 chars"
        string Value "100 chars"
        int Order
        string? MediaUrl "500 chars"
        bool IsCorrect
        DateTime CreatedAt
        bool IsDeleted
    }
    
    UserResponse {
        Guid Id PK
        Guid UserId FK
        Guid CategoryId FK
        Guid TemplateId FK
        string Status "50 chars"
        DateTime CreatedAt
        DateTime? UpdatedAt
        bool IsDeleted
    }
    
    UserAnswer {
        Guid Id PK
        Guid ResponseId FK
        Guid QuestionId FK
        string? AnswerText "4000 chars"
        decimal? NumericValue "18,2"
        DateTime? DateTimeValue
        DateTime CreatedAt
        bool IsDeleted
    }
    
    UserAnswerOption {
        Guid Id PK
        Guid AnswerId FK
        Guid OptionId FK
        DateTime CreatedAt
        bool IsDeleted
    }
    
    BaseEntity {
        Guid Id PK
        DateTime CreatedAt
        DateTime? UpdatedAt
        bool IsDeleted
    }
```

---

## 🏗️ **BASE ENTITY**

### **BaseEntity.cs**
```csharp
public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; }
    
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}
```

**Purpose:** Base class for all entities providing common properties
**Key Features:**
- **Id**: Unique identifier (GUID)
- **CreatedAt**: Timestamp when entity was created
- **UpdatedAt**: Timestamp when entity was last modified
- **IsDeleted**: Soft delete flag for data retention
- **Auto-initialization**: Constructor sets default values

---

## 📋 **CORE ENTITIES**

### **1. QuestionnaireTemplate Entity**

#### **Definition**
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
    
    // Navigation Properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
}
```

#### **Properties**
| Property | Type | Required | Max Length | Description |
|----------|------|----------|------------|-------------|
| `Id` | `Guid` | ✅ | - | Unique identifier |
| `Name` | `string` | ✅ | 200 | Template name/title |
| `Description` | `string` | ❌ | 500 | Template description |
| `CategoryId` | `Guid` | ✅ | - | Category reference |
| `IsActive` | `bool` | ❌ | - | Template availability |
| `Version` | `int` | ❌ | - | Template version number |
| `CreatedAt` | `DateTime` | ✅ | - | Creation timestamp |
| `UpdatedAt` | `DateTime?` | ❌ | - | Last update timestamp |
| `IsDeleted` | `bool` | ❌ | - | Soft delete flag |

#### **Relationships**
- **One-to-Many with Question**: One template has many questions
- **One-to-Many with UserResponse**: One template can have many user responses
- **Many-to-One with Category**: Many templates belong to one category

#### **Database Configuration**
```csharp
builder.Entity<QuestionnaireTemplate>(entity =>
{
    entity.ToTable("QuestionnaireTemplates");
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Description).HasMaxLength(500);
    entity.Property(e => e.IsActive).IsRequired();
    entity.Property(e => e.Version).IsRequired();
    entity.Property(e => e.CreatedAt).IsRequired();
    entity.HasMany(q => q.Questions)
          .WithOne(q => q.Template)
          .HasForeignKey(q => q.TemplateId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

---

### **2. Question Entity**

#### **Definition**
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
    public string Type { get; set; } = string.Empty; // text, textarea, radio, checkbox, dropdown, range, date, datetime, time
    
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
    
    // Navigation Properties
    public virtual QuestionnaireTemplate Template { get; set; } = null!;
    public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    
    // Helper methods for validation
    public bool IsMultipleChoice => Type == "radio" || Type == "checkbox" || Type == "dropdown";
    public bool IsTextBased => Type == "text" || Type == "textarea";
    public bool IsRange => Type == "range";
    public bool IsDateTimeBased => Type == "date" || Type == "datetime" || Type == "time";
    public bool HasOptions => Options.Count > 0;
}
```

#### **Properties**
| Property | Type | Required | Max Length | Description |
|----------|------|----------|------------|-------------|
| `Id` | `Guid` | ✅ | - | Unique identifier |
| `TemplateId` | `Guid` | ✅ | - | Parent template reference |
| `Text` | `string` | ✅ | 500 | Question text |
| `Type` | `string` | ✅ | 50 | Question type |
| `IsRequired` | `bool` | ❌ | - | Whether answer is mandatory |
| `Order` | `int` | ✅ | - | Display order |
| `HelpText` | `string?` | ❌ | 200 | Additional instructions |
| `MediaUrl` | `string?` | ❌ | 500 | Image/video URL |
| `MinValue` | `decimal?` | ❌ | 18,2 | Range minimum value |
| `MaxValue` | `decimal?` | ❌ | 18,2 | Range maximum value |
| `StepValue` | `decimal?` | ❌ | 18,2 | Range step value |

#### **Question Types**
| Type | Description | Answer Storage | Use Cases |
|------|-------------|----------------|-----------|
| `"text"` | Single line text | `AnswerText` | Names, short answers |
| `"textarea"` | Multi-line text | `AnswerText` | Descriptions, long answers |
| `"radio"` | Single selection | `SelectedOptions` | Gender, single choice |
| `"checkbox"` | Multiple selection | `SelectedOptions` | Symptoms, multiple choice |
| `"dropdown"` | Dropdown selection | `SelectedOptions` | Categories, single choice |
| `"range"` | Numeric range | `NumericValue` | Pain scale, ratings |
| `"date"` | Date picker | `DateTimeValue` | Birth dates, appointments |
| `"datetime"` | Date and time | `DateTimeValue` | Event scheduling |
| `"time"` | Time picker | `DateTimeValue` | Medication times |

#### **Relationships**
- **Many-to-One with QuestionnaireTemplate**: Many questions belong to one template
- **One-to-Many with QuestionOption**: One question can have many options
- **One-to-Many with UserAnswer**: One question can have many user answers

#### **Database Configuration**
```csharp
builder.Entity<Question>(entity =>
{
    entity.ToTable("Questions");
    entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
    entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
    entity.Property(e => e.HelpText).HasMaxLength(200);
    entity.Property(e => e.MediaUrl).HasMaxLength(500);
    entity.Property(e => e.MaxValue).HasPrecision(18, 2);
    entity.Property(e => e.MinValue).HasPrecision(18, 2);
    entity.Property(e => e.StepValue).HasPrecision(18, 2);
    entity.HasMany(q => q.Options)
          .WithOne(o => o.Question)
          .HasForeignKey(o => o.QuestionId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

---

### **3. QuestionOption Entity**

#### **Definition**
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
    
    // Navigation Properties
    public virtual Question Question { get; set; } = null!;
    public virtual ICollection<UserAnswerOption> UserAnswerOptions { get; set; } = new List<UserAnswerOption>();
}
```

#### **Properties**
| Property | Type | Required | Max Length | Description |
|----------|------|----------|------------|-------------|
| `Id` | `Guid` | ✅ | - | Unique identifier |
| `QuestionId` | `Guid` | ✅ | - | Parent question reference |
| `Text` | `string` | ✅ | 200 | Option display text |
| `Value` | `string` | ✅ | 100 | Option value |
| `Order` | `int` | ✅ | - | Display order |
| `MediaUrl` | `string?` | ❌ | 500 | Option image URL |
| `IsCorrect` | `bool` | ❌ | - | For scoring/validation |

#### **Relationships**
- **Many-to-One with Question**: Many options belong to one question
- **One-to-Many with UserAnswerOption**: One option can be selected in many answers

#### **Database Configuration**
```csharp
builder.Entity<QuestionOption>(entity =>
{
    entity.ToTable("QuestionOptions");
    entity.Property(e => e.Text).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Value).IsRequired().HasMaxLength(100);
    entity.Property(e => e.MediaUrl).HasMaxLength(500);
    entity.Property(e => e.IsCorrect).IsRequired();
});
```

---

### **4. UserResponse Entity**

#### **Definition**
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
    
    // Navigation Properties
    public virtual QuestionnaireTemplate Template { get; set; } = null!;
    public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
    
    // Helper methods
    public bool IsCompleted => Status == "completed" || Status == "submitted";
    public bool IsDraft => Status == "draft";
}
```

#### **Properties**
| Property | Type | Required | Max Length | Description |
|----------|------|----------|------------|-------------|
| `Id` | `Guid` | ✅ | - | Unique identifier |
| `UserId` | `Guid` | ✅ | - | User who submitted response |
| `CategoryId` | `Guid` | ✅ | - | Category reference |
| `TemplateId` | `Guid` | ✅ | - | Template reference |
| `Status` | `string` | ❌ | 50 | Response status |

#### **Status Values**
| Status | Description | Use Case |
|--------|-------------|----------|
| `"draft"` | Incomplete response | User saved progress |
| `"completed"` | Finished response | User completed questionnaire |
| `"submitted"` | Submitted response | User submitted for review |

#### **Relationships**
- **Many-to-One with User**: Many responses belong to one user
- **Many-to-One with Category**: Many responses belong to one category
- **Many-to-One with QuestionnaireTemplate**: Many responses belong to one template
- **One-to-Many with UserAnswer**: One response has many answers

#### **Database Configuration**
```csharp
builder.Entity<UserResponse>(entity =>
{
    entity.ToTable("UserResponses");
    entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
    entity.HasOne(r => r.Template)
          .WithMany(t => t.UserResponses)
          .HasForeignKey(r => r.TemplateId)
          .OnDelete(DeleteBehavior.NoAction);
    entity.HasMany(r => r.Answers)
          .WithOne(a => a.Response)
          .HasForeignKey(a => a.ResponseId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

---

### **5. UserAnswer Entity**

#### **Definition**
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
    
    public DateTime? DateTimeValue { get; set; } // For date, datetime, time answers
    
    // Navigation Properties
    public virtual UserResponse Response { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
    
    // Helper methods
    public bool HasTextAnswer => !string.IsNullOrEmpty(AnswerText);
    public bool HasNumericAnswer => NumericValue.HasValue;
    public bool HasDateTimeAnswer => DateTimeValue.HasValue;
    public bool HasSelectedOptions => SelectedOptions.Count > 0;
    public bool IsAnswered => HasTextAnswer || HasNumericAnswer || HasDateTimeAnswer || HasSelectedOptions;
}
```

#### **Properties**
| Property | Type | Required | Max Length | Description |
|----------|------|----------|------------|-------------|
| `Id` | `Guid` | ✅ | - | Unique identifier |
| `ResponseId` | `Guid` | ✅ | - | Parent response reference |
| `QuestionId` | `Guid` | ✅ | - | Question reference |
| `AnswerText` | `string?` | ❌ | 4000 | Text answer |
| `NumericValue` | `decimal?` | ❌ | 18,2 | Numeric answer |
| `DateTimeValue` | `DateTime?` | ❌ | - | Date/time answer |

#### **Answer Storage by Question Type**
| Question Type | Primary Storage | Secondary Storage | Example |
|---------------|-----------------|-------------------|---------|
| `"text"` | `AnswerText` | - | `"John Doe"` |
| `"textarea"` | `AnswerText` | - | `"Experiencing severe headache"` |
| `"radio"` | `SelectedOptions` | - | `[OptionId: "O1"]` |
| `"checkbox"` | `SelectedOptions` | - | `[OptionId: "O1", OptionId: "O2"]` |
| `"dropdown"` | `SelectedOptions` | - | `[OptionId: "O3"]` |
| `"range"` | `NumericValue` | - | `7.5` |
| `"date"` | `DateTimeValue` | - | `1990-05-15T00:00:00Z` |
| `"datetime"` | `DateTimeValue` | - | `2024-12-25T14:30:00Z` |
| `"time"` | `DateTimeValue` | - | `2024-01-01T08:30:00Z` |

#### **Relationships**
- **Many-to-One with UserResponse**: Many answers belong to one response
- **Many-to-One with Question**: Many answers belong to one question
- **One-to-Many with UserAnswerOption**: One answer can have many selected options

#### **Database Configuration**
```csharp
builder.Entity<UserAnswer>(entity =>
{
    entity.ToTable("UserAnswers");
    entity.Property(e => e.AnswerText).HasMaxLength(4000);
    entity.Property(e => e.NumericValue).HasPrecision(18, 2);
    entity.Property(e => e.DateTimeValue).IsRequired(false);
    entity.HasMany(a => a.SelectedOptions)
          .WithOne(o => o.Answer)
          .HasForeignKey(o => o.AnswerId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

---

### **6. UserAnswerOption Entity**

#### **Definition**
```csharp
public class UserAnswerOption : BaseEntity
{
    [Required]
    public Guid AnswerId { get; set; }
    
    [Required]
    public Guid OptionId { get; set; }
    
    // Navigation Properties
    public virtual UserAnswer Answer { get; set; } = null!;
    public virtual QuestionOption Option { get; set; } = null!;
}
```

#### **Properties**
| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `Id` | `Guid` | ✅ | Unique identifier |
| `AnswerId` | `Guid` | ✅ | Parent answer reference |
| `OptionId` | `Guid` | ✅ | Selected option reference |

#### **Purpose**
This entity serves as a **junction table** that links user answers to selected options for multiple choice questions (radio, checkbox, dropdown).

#### **Relationships**
- **Many-to-One with UserAnswer**: Many selections belong to one answer
- **Many-to-One with QuestionOption**: Many selections reference one option

#### **Database Configuration**
```csharp
builder.Entity<UserAnswerOption>(entity =>
{
    entity.ToTable("UserAnswerOptions");
    entity.HasOne(o => o.Answer)
          .WithMany(a => a.SelectedOptions)
          .HasForeignKey(o => o.AnswerId)
          .OnDelete(DeleteBehavior.Restrict);
    entity.HasOne(o => o.Option)
          .WithMany()
          .HasForeignKey(o => o.OptionId)
          .OnDelete(DeleteBehavior.Restrict);
});
```

---

## 🔄 **COMPLETE DATA FLOW EXAMPLE**

### **Scenario: Medical Assessment Questionnaire**

#### **1. Template Creation**
```json
{
  "name": "Diabetes Screening",
  "description": "Initial diabetes assessment",
  "categoryId": "category-guid",
  "isActive": true,
  "questions": [
    {
      "text": "What is your age?",
      "type": "text",
      "isRequired": true,
      "order": 1
    },
    {
      "text": "Select your gender",
      "type": "radio",
      "isRequired": true,
      "order": 2,
      "options": [
        {"text": "Male", "value": "male", "order": 1},
        {"text": "Female", "value": "female", "order": 2}
      ]
    },
    {
      "text": "What is your birth date?",
      "type": "date",
      "isRequired": true,
      "order": 3
    },
    {
      "text": "Rate your pain level (1-10)",
      "type": "range",
      "isRequired": true,
      "order": 4,
      "minValue": 1,
      "maxValue": 10,
      "stepValue": 1
    }
  ]
}
```

#### **2. Database Storage**

**QuestionnaireTemplates Table:**
```sql
INSERT INTO QuestionnaireTemplates (Id, Name, Description, CategoryId, IsActive, Version, CreatedAt)
VALUES ('T1', 'Diabetes Screening', 'Initial diabetes assessment', 'C1', 1, 1, '2024-01-15 10:00:00');
```

**Questions Table:**
```sql
INSERT INTO Questions (Id, TemplateId, Text, Type, IsRequired, Order, CreatedAt) VALUES
('Q1', 'T1', 'What is your age?', 'text', 1, 1, '2024-01-15 10:00:00'),
('Q2', 'T1', 'Select your gender', 'radio', 1, 2, '2024-01-15 10:00:00'),
('Q3', 'T1', 'What is your birth date?', 'date', 1, 3, '2024-01-15 10:00:00'),
('Q4', 'T1', 'Rate your pain level (1-10)', 'range', 1, 4, '2024-01-15 10:00:00');
```

**QuestionOptions Table:**
```sql
INSERT INTO QuestionOptions (Id, QuestionId, Text, Value, Order, CreatedAt) VALUES
('O1', 'Q2', 'Male', 'male', 1, '2024-01-15 10:00:00'),
('O2', 'Q2', 'Female', 'female', 2, '2024-01-15 10:00:00');
```

#### **3. User Response Submission**
```json
{
  "userId": "user-guid",
  "categoryId": "category-guid",
  "templateId": "T1",
  "status": "completed",
  "answers": [
    {
      "questionId": "Q1",
      "answerText": "35"
    },
    {
      "questionId": "Q2",
      "selectedOptionIds": ["O1"]
    },
    {
      "questionId": "Q3",
      "dateTimeValue": "1990-05-15T00:00:00Z"
    },
    {
      "questionId": "Q4",
      "numericValue": 7.0
    }
  ]
}
```

#### **4. Response Storage**

**UserResponses Table:**
```sql
INSERT INTO UserResponses (Id, UserId, CategoryId, TemplateId, Status, CreatedAt)
VALUES ('R1', 'user-guid', 'category-guid', 'T1', 'completed', '2024-01-20 14:30:00');
```

**UserAnswers Table:**
```sql
INSERT INTO UserAnswers (Id, ResponseId, QuestionId, AnswerText, NumericValue, DateTimeValue, CreatedAt) VALUES
('A1', 'R1', 'Q1', '35', NULL, NULL, '2024-01-20 14:30:00'),
('A2', 'R1', 'Q2', NULL, NULL, NULL, '2024-01-20 14:30:00'),
('A3', 'R1', 'Q3', NULL, NULL, '1990-05-15T00:00:00Z', '2024-01-20 14:30:00'),
('A4', 'R1', 'Q4', NULL, 7.0, NULL, '2024-01-20 14:30:00');
```

**UserAnswerOptions Table:**
```sql
INSERT INTO UserAnswerOptions (Id, AnswerId, OptionId, CreatedAt)
VALUES ('AO1', 'A2', 'O1', '2024-01-20 14:30:00');
```

---

## 🔗 **RELATIONSHIP DETAILS**

### **Cascade Delete Behavior**

| Relationship | Delete Behavior | Explanation |
|--------------|-----------------|-------------|
| `QuestionnaireTemplate → Question` | `Cascade` | Delete template → delete all questions |
| `Question → QuestionOption` | `Cascade` | Delete question → delete all options |
| `UserResponse → UserAnswer` | `Cascade` | Delete response → delete all answers |
| `UserAnswer → UserAnswerOption` | `Cascade` | Delete answer → delete all selections |
| `UserResponse → QuestionnaireTemplate` | `NoAction` | Prevent template deletion if responses exist |
| `UserAnswerOption → QuestionOption` | `Restrict` | Prevent option deletion if selected |

### **Foreign Key Constraints**

| Entity | Foreign Key | References | Constraint |
|--------|-------------|------------|------------|
| `Question` | `TemplateId` | `QuestionnaireTemplate.Id` | Required |
| `QuestionOption` | `QuestionId` | `Question.Id` | Required |
| `UserResponse` | `TemplateId` | `QuestionnaireTemplate.Id` | Required |
| `UserResponse` | `UserId` | `User.Id` | Required |
| `UserResponse` | `CategoryId` | `Category.Id` | Required |
| `UserAnswer` | `ResponseId` | `UserResponse.Id` | Required |
| `UserAnswer` | `QuestionId` | `Question.Id` | Required |
| `UserAnswerOption` | `AnswerId` | `UserAnswer.Id` | Required |
| `UserAnswerOption` | `OptionId` | `QuestionOption.Id` | Required |

---

## 📊 **DATA INTEGRITY RULES**

### **1. Required Fields**
- All entities must have `Id`, `CreatedAt`, `IsDeleted`
- `QuestionnaireTemplate`: `Name`, `CategoryId`
- `Question`: `TemplateId`, `Text`, `Type`, `Order`
- `QuestionOption`: `QuestionId`, `Text`, `Value`, `Order`
- `UserResponse`: `UserId`, `CategoryId`, `TemplateId`
- `UserAnswer`: `ResponseId`, `QuestionId`
- `UserAnswerOption`: `AnswerId`, `OptionId`

### **2. Length Constraints**
- `QuestionnaireTemplate.Name`: 200 characters
- `QuestionnaireTemplate.Description`: 500 characters
- `Question.Text`: 500 characters
- `Question.Type`: 50 characters
- `Question.HelpText`: 200 characters
- `Question.MediaUrl`: 500 characters
- `QuestionOption.Text`: 200 characters
- `QuestionOption.Value`: 100 characters
- `QuestionOption.MediaUrl`: 500 characters
- `UserResponse.Status`: 50 characters
- `UserAnswer.AnswerText`: 4000 characters

### **3. Precision Constraints**
- `Question.MinValue/MaxValue/StepValue`: decimal(18,2)
- `UserAnswer.NumericValue`: decimal(18,2)

### **4. Business Rules**
- Question `Order` must be unique within a template
- QuestionOption `Order` must be unique within a question
- UserResponse `Status` must be one of: "draft", "completed", "submitted"
- Question `Type` must be one of: "text", "textarea", "radio", "checkbox", "dropdown", "range", "date", "datetime", "time"

---

## 🧪 **VALIDATION SCENARIOS**

### **1. Template Validation**
```csharp
// Valid template
var validTemplate = new QuestionnaireTemplate
{
    Name = "Medical Assessment",
    Description = "Comprehensive health evaluation",
    CategoryId = Guid.NewGuid(),
    IsActive = true,
    Version = 1
};

// Invalid template (missing required fields)
var invalidTemplate = new QuestionnaireTemplate
{
    // Missing Name and CategoryId
    Description = "Test",
    IsActive = true
};
```

### **2. Question Validation**
```csharp
// Valid question
var validQuestion = new Question
{
    TemplateId = templateId,
    Text = "What is your age?",
    Type = "text",
    IsRequired = true,
    Order = 1
};

// Invalid question (invalid type)
var invalidQuestion = new Question
{
    TemplateId = templateId,
    Text = "Test question",
    Type = "invalid_type", // Invalid type
    IsRequired = true,
    Order = 1
};
```

### **3. Answer Validation**
```csharp
// Valid text answer
var textAnswer = new UserAnswer
{
    ResponseId = responseId,
    QuestionId = questionId,
    AnswerText = "John Doe"
};

// Valid numeric answer
var numericAnswer = new UserAnswer
{
    ResponseId = responseId,
    QuestionId = questionId,
    NumericValue = 7.5m
};

// Valid date answer
var dateAnswer = new UserAnswer
{
    ResponseId = responseId,
    QuestionId = questionId,
    DateTimeValue = new DateTime(1990, 5, 15)
};
```

---

## 🔮 **FUTURE ENHANCEMENTS**

### **1. Additional Question Types**
- **File Upload**: For document/image uploads
- **Signature**: For digital signatures
- **Location**: For GPS coordinates
- **Rating**: For star ratings
- **Matrix**: For grid-style questions

### **2. Enhanced Validation**
- **Date Range Validation**: Min/max dates for date questions
- **Numeric Range Validation**: Min/max values for numeric questions
- **Pattern Validation**: Regex patterns for text questions
- **Conditional Logic**: Show/hide questions based on answers

### **3. Advanced Features**
- **Question Dependencies**: Questions that depend on other answers
- **Scoring System**: Automatic scoring based on answers
- **Branching Logic**: Different question paths based on answers
- **Multi-language Support**: Internationalization for questions

---

## ✅ **SUMMARY**

The Questionnaire System consists of **6 core entities** with well-defined relationships:

1. **QuestionnaireTemplate**: Container for questionnaires
2. **Question**: Individual questions within templates
3. **QuestionOption**: Options for multiple choice questions
4. **UserResponse**: User's response to a template
5. **UserAnswer**: User's answer to a specific question
6. **UserAnswerOption**: Links answers to selected options

**Key Features:**
- ✅ **Comprehensive question types** (text, radio, checkbox, dropdown, range, date, datetime, time)
- ✅ **Flexible answer storage** (text, numeric, datetime, options)
- ✅ **Robust relationships** with proper cascade delete behavior
- ✅ **Data integrity** with constraints and validation
- ✅ **Soft delete support** for data retention
- ✅ **Audit trail** with creation/update timestamps
- ✅ **Scalable architecture** ready for future enhancements

The system provides a solid foundation for healthcare questionnaire applications with room for growth and customization. 