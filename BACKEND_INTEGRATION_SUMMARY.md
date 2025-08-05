# 🔗 **BACKEND INTEGRATION WITH CENTRALIZED DOCUMENT MANAGEMENT**

## **📋 OVERVIEW**

Successfully integrated the **centralized document management system** with existing backend services, replacing the old document handling approaches with the new robust DocumentService and DocumentTypeService.

---

## **✅ INTEGRATED SERVICES**

### **1. 🏥 AppointmentService Integration**

#### **Updated Methods:**
```csharp
// OLD: Used AppointmentDocumentDto
Task<ApiResponse<AppointmentDocumentDto>> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto)
Task<ApiResponse<IEnumerable<AppointmentDocumentDto>>> GetAppointmentDocumentsAsync(Guid appointmentId)
Task<ApiResponse<bool>> DeleteDocumentAsync(Guid documentId)

// NEW: Uses centralized DocumentDto
Task<ApiResponse<DocumentDto>> UploadDocumentAsync(Guid appointmentId, UploadDocumentDto uploadDto)
Task<ApiResponse<IEnumerable<DocumentDto>>> GetAppointmentDocumentsAsync(Guid appointmentId)
Task<ApiResponse<bool>> DeleteDocumentAsync(Guid documentId)
```

#### **Key Features:**
- **Document Type Validation** - Automatically selects appropriate document type for appointments
- **Entity Linking** - Links documents to appointments via DocumentReference
- **Document Count Tracking** - Updates appointment document count
- **Centralized Storage** - Uses LocalFileStorageService for file storage
- **Access Control** - Validates user permissions for document operations

#### **Integration Flow:**
```csharp
// 1. Validate appointment exists
var appointment = await _appointmentRepository.GetByIdAsync(appointmentId);

// 2. Get appropriate document type
var appointmentDocType = await _documentTypeService.GetAllDocumentTypesAsync(true);

// 3. Create upload request
var uploadRequest = new UploadDocumentRequest
{
    FileData = fileBytes,
    FileName = uploadDto.FileName,
    ContentType = uploadDto.FileType,
    EntityType = "Appointment",
    EntityId = appointmentId,
    ReferenceType = "appointment_document",
    DocumentTypeId = appointmentDocType.DocumentTypeId,
    CreatedById = appointment.PatientId
};

// 4. Upload using centralized service
var result = await _documentService.UploadDocumentAsync(uploadRequest);

// 5. Update appointment document count
appointment.DocumentCount++;
await _appointmentRepository.UpdateAsync(appointment);
```

### **2. 👤 UserService Integration**

#### **Updated Methods:**
```csharp
// OLD: Simple file upload
Task<ApiResponse<object>> UploadProfilePictureAsync(Guid userId, IFormFile file)

// NEW: Centralized document management
Task<ApiResponse<DocumentDto>> UploadProfilePictureAsync(Guid userId, IFormFile file)
Task<ApiResponse<IEnumerable<DocumentDto>>> GetUserDocumentsAsync(Guid userId, string? referenceType = null)
Task<ApiResponse<DocumentDto>> UploadUserDocumentAsync(Guid userId, UploadDocumentRequest request)
Task<ApiResponse<bool>> DeleteUserDocumentAsync(Guid documentId, Guid userId)
```

#### **Key Features:**
- **Profile Picture Management** - Enhanced with document type validation
- **User Document Organization** - Categorized by reference type (profile_picture, id_document, etc.)
- **File Validation** - Type and size validation for profile pictures
- **Public Access** - Profile pictures are marked as public
- **URL Management** - Updates user profile picture URL automatically

#### **Integration Flow:**
```csharp
// 1. Validate user and file
var user = await _userRepository.GetByIdAsync(userId);
if (file.Length > 5 * 1024 * 1024) // 5MB limit
    return ErrorResponse("File size too large");

// 2. Get profile picture document type
var profilePictureType = await _documentTypeService.GetAllDocumentTypesAsync(true)
    .Data?.FirstOrDefault(dt => dt.Name.ToLower().Contains("profile"));

// 3. Create upload request
var uploadRequest = new UploadDocumentRequest
{
    FileData = fileBytes,
    FileName = file.FileName,
    ContentType = file.ContentType,
    EntityType = "User",
    EntityId = userId,
    ReferenceType = "profile_picture",
    IsPublic = true,
    DocumentTypeId = profilePictureType.DocumentTypeId,
    CreatedById = userId
};

// 4. Upload and update user profile
var result = await _documentService.UploadDocumentAsync(uploadRequest);
user.ProfilePicture = result.Data?.DownloadUrl;
await _userRepository.UpdateAsync(user);
```

---

## **🔧 DEPENDENCY INJECTION UPDATES**

### **Updated Service Registrations:**
```csharp
// Register Document Services
services.AddScoped<IDocumentService, DocumentService>();
services.AddScoped<IDocumentTypeService, DocumentTypeService>();

// Updated Service Constructors
public AppointmentService(
    // ... existing dependencies ...
    IDocumentService documentService,
    IDocumentTypeService documentTypeService
)

public UserService(
    // ... existing dependencies ...
    IDocumentService documentService,
    IDocumentTypeService documentTypeService
)
```

---

## **📊 DATABASE RELATIONSHIPS**

### **Documents Table Integration:**
```sql
-- Documents table with DocumentTypeId foreign key
CREATE TABLE Documents (
    DocumentId UNIQUEIDENTIFIER PRIMARY KEY,
    OriginalName NVARCHAR(255) NOT NULL,
    UniqueName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FolderPath NVARCHAR(200) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    Description NVARCHAR(1000),
    DocumentTypeId UNIQUEIDENTIFIER NOT NULL, -- NEW: FK to DocumentTypes
    DocumentCategory NVARCHAR(50), -- For backward compatibility
    IsEncrypted BIT DEFAULT 0,
    IsPublic BIT DEFAULT 0,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (DocumentTypeId) REFERENCES DocumentTypes(DocumentTypeId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
);

-- DocumentReferences table for entity relationships
CREATE TABLE DocumentReferences (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(50) NOT NULL, -- "Appointment", "User", "Chat"
    EntityId UNIQUEIDENTIFIER NOT NULL,
    ReferenceType NVARCHAR(100), -- "profile_picture", "appointment_document"
    Description NVARCHAR(500),
    IsPublic BIT DEFAULT 0,
    ExpiresAt DATETIME2,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsDeleted BIT DEFAULT 0,
    FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(UserId)
);
```

---

## **🎯 USAGE EXAMPLES**

### **1. Upload Appointment Document:**
```csharp
// Controller action
[HttpPost("appointments/{appointmentId}/documents")]
public async Task<IActionResult> UploadAppointmentDocument(Guid appointmentId, IFormFile file)
{
    var uploadDto = new UploadDocumentDto
    {
        FileName = file.FileName,
        FileType = file.ContentType,
        FileSize = file.Length,
        FileContent = Convert.ToBase64String(await GetFileBytes(file))
    };

    var result = await _appointmentService.UploadDocumentAsync(appointmentId, uploadDto);
    return Ok(result);
}
```

### **2. Get Appointment Documents:**
```csharp
// Controller action
[HttpGet("appointments/{appointmentId}/documents")]
public async Task<IActionResult> GetAppointmentDocuments(Guid appointmentId)
{
    var result = await _appointmentService.GetAppointmentDocumentsAsync(appointmentId);
    return Ok(result);
}
```

### **3. Upload User Profile Picture:**
```csharp
// Controller action
[HttpPost("users/{userId}/profile-picture")]
public async Task<IActionResult> UploadProfilePicture(Guid userId, IFormFile file)
{
    var result = await _userService.UploadProfilePictureAsync(userId, file);
    return Ok(result);
}
```

### **4. Get User Documents:**
```csharp
// Controller action
[HttpGet("users/{userId}/documents")]
public async Task<IActionResult> GetUserDocuments(Guid userId, string? referenceType = null)
{
    var result = await _userService.GetUserDocumentsAsync(userId, referenceType);
    return Ok(result);
}
```

---

## **🔐 SECURITY & ACCESS CONTROL**

### **Document Access Validation:**
```csharp
// In DocumentService
public async Task<ApiResponse<bool>> ValidateDocumentAccessAsync(Guid documentId, Guid userId)
{
    var document = await _documentRepository.GetByIdAsync(documentId);
    
    // Public documents
    if (document.IsPublic) return SuccessResponse(true);
    
    // Creator access
    if (document.CreatedById == userId) return SuccessResponse(true);
    
    // Entity-based access
    var references = await _referenceRepository.GetAllAsync(r => r.DocumentId == documentId);
    foreach (var reference in references)
    {
        if (await ValidateEntityAccessAsync(reference.EntityType, reference.EntityId, userId))
            return SuccessResponse(true);
    }
    
    return SuccessResponse(false);
}
```

### **Entity Access Validation:**
```csharp
private async Task<bool> ValidateEntityAccessAsync(string entityType, Guid entityId, Guid userId)
{
    switch (entityType)
    {
        case "Appointment":
            // Check if user is participant in appointment
            var appointment = await _appointmentRepository.GetByIdAsync(entityId);
            return appointment?.PatientId == userId || appointment?.ProviderId == userId;
            
        case "User":
            // Users can only access their own documents
            return entityId == userId;
            
        default:
            return false;
    }
}
```

---

## **📈 BENEFITS ACHIEVED**

### **✅ Centralized Management**
- **Single Source of Truth** - All documents managed through DocumentService
- **Consistent Operations** - Same upload/download/delete patterns across services
- **Unified Storage** - All files stored using LocalFileStorageService
- **Standardized Metadata** - Consistent document information across the system

### **✅ Enhanced Security**
- **Access Control** - Document-level permission validation
- **Entity Linking** - Documents linked to specific entities with proper relationships
- **Audit Trail** - Complete tracking of document operations
- **Public/Private Control** - Granular visibility settings

### **✅ Improved Organization**
- **Document Types** - Categorized documents with proper types
- **Reference Types** - Specific categorization (profile_picture, appointment_document)
- **Entity Relationships** - Clear links between documents and business entities
- **Search & Filtering** - Advanced document search capabilities

### **✅ Better User Experience**
- **Automatic Type Selection** - Smart document type detection
- **File Validation** - Type and size validation
- **URL Management** - Automatic URL updates for profile pictures
- **Document Counts** - Real-time document count tracking

---

## **🚀 IMPLEMENTATION STATUS**

### **✅ COMPLETED**
1. **AppointmentService Integration** - Full document management integration
2. **UserService Integration** - Profile picture and user document management
3. **Interface Updates** - Updated IAppointmentService and IUserService
4. **Dependency Injection** - Registered all document services
5. **Security Integration** - Access control and validation
6. **Database Relationships** - Proper foreign key relationships

### **🔄 NEXT STEPS**
1. **API Controllers** - Update controllers to use new document methods
2. **Frontend Integration** - Update UI to work with new document structure
3. **Testing** - Comprehensive integration tests
4. **Migration Scripts** - Convert existing document data
5. **Documentation** - API documentation updates

---

## **🎉 CONCLUSION**

The backend integration successfully **replaces the old document handling approaches** with the **centralized document management system**, providing:

✅ **Consistent Document Operations** across all services  
✅ **Enhanced Security** with proper access control  
✅ **Better Organization** with document types and references  
✅ **Improved User Experience** with automatic type selection  
✅ **Scalable Architecture** ready for future enhancements  

This integration transforms the document handling from **service-specific implementations** to a **unified, robust system** that can handle all document types across the entire application! 🚀 