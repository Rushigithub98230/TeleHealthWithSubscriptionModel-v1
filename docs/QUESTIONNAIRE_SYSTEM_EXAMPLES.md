# 📋 **QUESTIONNAIRE SYSTEM - COMPLETE IMPLEMENTATION GUIDE**

## 🎯 **FOCUSED FEATURES (No Analytics/Session Management)**

This system provides a **clean, focused questionnaire implementation** that handles multiple question types without any analytics or session management complexity.

---

## 📊 **SUPPORTED QUESTION TYPES**

### **1. Text Input (`"text"`)**
- **Purpose**: Single line text input
- **Use Cases**: Name, age, short answers
- **Storage**: `UserAnswer.AnswerText`
- **Validation**: Required/optional, length limits

### **2. Textarea (`"textarea"`)**
- **Purpose**: Multi-line text input
- **Use Cases**: Detailed descriptions, symptoms, medical history
- **Storage**: `UserAnswer.AnswerText`
- **Validation**: Required/optional, character limits

### **3. Radio Buttons (`"radio"`)**
- **Purpose**: Single selection from multiple options
- **Use Cases**: Gender, yes/no questions, single choice
- **Storage**: `UserAnswer.SelectedOptions` (one option)
- **Validation**: Must select exactly one option if required

### **4. Checkboxes (`"checkbox"`)**
- **Purpose**: Multiple selections from options
- **Use Cases**: Symptoms, medications, multiple choice
- **Storage**: `UserAnswer.SelectedOptions` (multiple options)
- **Validation**: Must select at least one if required

### **5. Dropdown (`"dropdown"`)**
- **Purpose**: Single selection from dropdown
- **Use Cases**: Insurance provider, age group, category selection
- **Storage**: `UserAnswer.SelectedOptions` (one option)
- **Validation**: Must select exactly one option if required

### **6. Range Slider (`"range"`)**
- **Purpose**: Numeric value within a range
- **Use Cases**: Pain scale, rating scales, numeric input
- **Storage**: `UserAnswer.NumericValue`
- **Validation**: Value must be within MinValue-MaxValue range

---

## 🗄️ **DATABASE STORAGE PATTERNS**

### **Complete Example: Medical Intake Questionnaire**

#### **Step 1: Create Template**
```json
POST /api/questionnaire/templates
{
  "name": "Diabetes Intake Form",
  "description": "Initial assessment for diabetes patients",
  "categoryId": "category-diabetes-guid",
  "isActive": true,
  "questions": [
    {
      "text": "What is your full name?",
      "type": "text",
      "isRequired": true,
      "order": 1,
      "helpText": "Enter your legal name",
      "options": []
    },
    {
      "text": "What is your age?",
      "type": "text",
      "isRequired": true,
      "order": 2,
      "helpText": "Enter your age in years",
      "options": []
    },
    {
      "text": "Select your gender",
      "type": "radio",
      "isRequired": true,
      "order": 3,
      "helpText": "Choose your gender",
      "options": [
        { "text": "Male", "value": "male", "order": 1 },
        { "text": "Female", "value": "female", "order": 2 },
        { "text": "Other", "value": "other", "order": 3 }
      ]
    },
    {
      "text": "Describe your symptoms in detail",
      "type": "textarea",
      "isRequired": true,
      "order": 4,
      "helpText": "Please be detailed about your symptoms",
      "options": []
    },
    {
      "text": "Select all medications you are currently taking",
      "type": "checkbox",
      "isRequired": false,
      "order": 5,
      "helpText": "Select all that apply",
      "options": [
        { "text": "Metformin", "value": "metformin", "order": 1 },
        { "text": "Insulin", "value": "insulin", "order": 2 },
        { "text": "Glipizide", "value": "glipizide", "order": 3 },
        { "text": "None", "value": "none", "order": 4 }
      ]
    },
    {
      "text": "Rate your pain level (1-10)",
      "type": "range",
      "isRequired": true,
      "order": 6,
      "helpText": "Scale of 1-10",
      "minValue": 1,
      "maxValue": 10,
      "stepValue": 1,
      "options": []
    },
    {
      "text": "Select your insurance provider",
      "type": "dropdown",
      "isRequired": true,
      "order": 7,
      "helpText": "Choose from list",
      "options": [
        { "text": "Blue Cross", "value": "blue_cross", "order": 1 },
        { "text": "Aetna", "value": "aetna", "order": 2 },
        { "text": "Cigna", "value": "cigna", "order": 3 },
        { "text": "Medicare", "value": "medicare", "order": 4 }
      ]
    }
  ]
}
```

#### **Step 2: User Submits Response**
```json
POST /api/questionnaire/responses
{
  "userId": "user-123-guid",
  "categoryId": "category-diabetes-guid",
  "templateId": "template-1-guid",
  "status": "completed",
  "answers": [
    {
      "questionId": "q1-guid",
      "answerText": "John Smith",
      "numericValue": null,
      "selectedOptionIds": []
    },
    {
      "questionId": "q2-guid",
      "answerText": "45",
      "numericValue": null,
      "selectedOptionIds": []
    },
    {
      "questionId": "q3-guid",
      "answerText": null,
      "numericValue": null,
      "selectedOptionIds": ["opt-female-guid"]
    },
    {
      "questionId": "q4-guid",
      "answerText": "I have been experiencing frequent urination and increased thirst for the past 2 weeks. I also feel tired all the time.",
      "numericValue": null,
      "selectedOptionIds": []
    },
    {
      "questionId": "q5-guid",
      "answerText": null,
      "numericValue": null,
      "selectedOptionIds": ["opt-metformin-guid", "opt-insulin-guid"]
    },
    {
      "questionId": "q6-guid",
      "answerText": null,
      "numericValue": 7.0,
      "selectedOptionIds": []
    },
    {
      "questionId": "q7-guid",
      "answerText": null,
      "numericValue": null,
      "selectedOptionIds": ["opt-aetna-guid"]
    }
  ]
}
```

---

## 🗄️ **DATABASE STORAGE DETAILS**

### **How Data is Stored in Tables**

#### **1. Template Creation**
```sql
-- QuestionnaireTemplates
INSERT INTO QuestionnaireTemplates (Id, Name, Description, CategoryId, IsActive, Version)
VALUES ('template-1', 'Diabetes Intake Form', 'Initial assessment for diabetes patients', 'category-diabetes', 1, 1);

-- Questions
INSERT INTO Questions (Id, TemplateId, Text, Type, IsRequired, Order, HelpText, MinValue, MaxValue, StepValue)
VALUES 
('q1', 'template-1', 'What is your full name?', 'text', 1, 1, 'Enter your legal name', NULL, NULL, NULL),
('q2', 'template-1', 'What is your age?', 'text', 1, 2, 'Enter your age in years', NULL, NULL, NULL),
('q3', 'template-1', 'Select your gender', 'radio', 1, 3, 'Choose your gender', NULL, NULL, NULL),
('q4', 'template-1', 'Describe your symptoms in detail', 'textarea', 1, 4, 'Please be detailed', NULL, NULL, NULL),
('q5', 'template-1', 'Select all medications you are taking', 'checkbox', 0, 5, 'Select all that apply', NULL, NULL, NULL),
('q6', 'template-1', 'Rate your pain level (1-10)', 'range', 1, 6, 'Scale of 1-10', 1, 10, 1),
('q7', 'template-1', 'Select your insurance provider', 'dropdown', 1, 7, 'Choose from list', NULL, NULL, NULL);

-- QuestionOptions (for multiple choice questions)
INSERT INTO QuestionOptions (Id, QuestionId, Text, Value, Order)
VALUES 
-- Gender options (q3)
('opt-male', 'q3', 'Male', 'male', 1),
('opt-female', 'q3', 'Female', 'female', 2),
('opt-other', 'q3', 'Other', 'other', 3),

-- Medication options (q5)
('opt-metformin', 'q5', 'Metformin', 'metformin', 1),
('opt-insulin', 'q5', 'Insulin', 'insulin', 2),
('opt-glipizide', 'q5', 'Glipizide', 'glipizide', 3),
('opt-none', 'q5', 'None', 'none', 4),

-- Insurance options (q7)
('opt-blue-cross', 'q7', 'Blue Cross', 'blue_cross', 1),
('opt-aetna', 'q7', 'Aetna', 'aetna', 2),
('opt-cigna', 'q7', 'Cigna', 'cigna', 3),
('opt-medicare', 'q7', 'Medicare', 'medicare', 4);
```

#### **2. User Response Storage**
```sql
-- UserResponses (Header)
INSERT INTO UserResponses (Id, UserId, CategoryId, TemplateId, Status)
VALUES ('response-1', 'user-123', 'category-diabetes', 'template-1', 'completed');

-- UserAnswers (Individual answers)
INSERT INTO UserAnswers (Id, ResponseId, QuestionId, AnswerText, NumericValue)
VALUES 
-- Text answers
('ua1', 'response-1', 'q1', 'John Smith', NULL),
('ua2', 'response-1', 'q2', '45', NULL),
('ua4', 'response-1', 'q4', 'I have been experiencing frequent urination and increased thirst for the past 2 weeks. I also feel tired all the time.', NULL),

-- Range answer
('ua6', 'response-1', 'q6', NULL, 7.0),

-- Multiple choice answers (no text/numeric, only options)
('ua3', 'response-1', 'q3', NULL, NULL),
('ua5', 'response-1', 'q5', NULL, NULL),
('ua7', 'response-1', 'q7', NULL, NULL);

-- UserAnswerOptions (for selected options)
INSERT INTO UserAnswerOptions (Id, AnswerId, OptionId)
VALUES 
-- Radio selection (q3 - gender)
('uao1', 'ua3', 'opt-female'),

-- Checkbox selections (q5 - medications)
('uao2', 'ua5', 'opt-metformin'),
('uao3', 'ua5', 'opt-insulin'),

-- Dropdown selection (q7 - insurance)
('uao4', 'ua7', 'opt-aetna');
```

---

## 🔍 **QUERYING EXAMPLES**

### **Get Complete User Response**
```sql
SELECT 
    ur.Id as ResponseId,
    ur.Status,
    ur.CreatedAt,
    q.Text as QuestionText,
    q.Type as QuestionType,
    ua.AnswerText,
    ua.NumericValue,
    STRING_AGG(qo.Text, ', ') as SelectedOptions
FROM UserResponses ur
JOIN UserAnswers ua ON ur.Id = ua.ResponseId
JOIN Questions q ON ua.QuestionId = q.Id
LEFT JOIN UserAnswerOptions uao ON ua.Id = uao.AnswerId
LEFT JOIN QuestionOptions qo ON uao.OptionId = qo.Id
WHERE ur.Id = 'response-1'
GROUP BY ur.Id, ur.Status, ur.CreatedAt, q.Text, q.Type, ua.AnswerText, ua.NumericValue
ORDER BY q.Order;
```

**Result:**
```
ResponseId    Status      QuestionText                    QuestionType  AnswerText                    NumericValue  SelectedOptions
response-1    completed   What is your full name?         text          John Smith                    NULL          NULL
response-1    completed   What is your age?               text          45                            NULL          NULL
response-1    completed   Select your gender              radio         NULL                          NULL          Female
response-1    completed   Describe your symptoms          textarea      I have been experiencing...   NULL          NULL
response-1    completed   Select all medications          checkbox      NULL                          NULL          Metformin, Insulin
response-1    completed   Rate your pain level            range         NULL                          7.0           NULL
response-1    completed   Select your insurance provider  dropdown      NULL                          NULL          Aetna
```

---

## ✅ **KEY BENEFITS**

1. **Clean & Focused**: No analytics or session management complexity
2. **Flexible**: Supports all question types with single schema
3. **Scalable**: Normalized design with proper relationships
4. **Queryable**: Easy to retrieve complete responses
5. **Extensible**: Easy to add new question types
6. **Data Integrity**: Foreign key constraints ensure consistency
7. **Performance**: Proper indexing on key fields
8. **Simple**: Straightforward API without unnecessary features

---

## 🚀 **USAGE PATTERNS**

### **Admin Workflow**
1. Create questionnaire template with questions
2. Configure question types and options
3. Activate template for category
4. Monitor user responses

### **User Workflow**
1. Select category
2. Get questionnaire template
3. Answer questions based on type
4. Submit response
5. View/retrieve responses

This system provides a **robust, focused questionnaire implementation** that handles multiple question types effectively without any unnecessary complexity. 