# 🗂️ **CENTRALIZED DOCUMENT MANAGEMENT SYSTEM**

## **📋 OVERVIEW**

Implement a centralized document storage mechanism using a shared `Documents` table that stores metadata for every uploaded document across the application, regardless of which module or service uploads it.

---

## **🏗️ ARCHITECTURE DESIGN**

### **1. Core Documents Table**

```csharp
public class Document : BaseEntity
{
    // Primary identification
    public Guid DocumentId { get; set; } = Guid.NewGuid();
    
    // File information
    [Required]
    [MaxLength(255)]
    public string OriginalName { get; set; } = string.Empty; // e.g., "testdocument.pdf"
    
    [Required]
    [MaxLength(255)]
    public string UniqueName { get; set; } = string.Empty; // e.g., "a1b2c3d4-e5f6-7890-abcd-ef1234567890_testdocument.pdf"
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty; // Full path including folder
    
    [Required]
    [MaxLength(200)]
    public string FolderPath { get; set; } = string.Empty; // e.g., "appointments/123"
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty; // e.g., "application/pdf"
    
    public long FileSize { get; set; }
    
    // Document metadata
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [MaxLength(50)]
    public string? DocumentType { get; set; } // e.g., "appointment", "profile", "chat"
    
    // Security and access
    public bool IsEncrypted { get; set; } = false;
    
    [MaxLength(100)]
    public string? EncryptionKey { get; set; }
    
    public bool IsPublic { get; set; } = false; // Can be accessed without authentication
    
    // Audit fields
    public Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public Guid? DeletedById { get; set; }
    public virtual User? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    
    // Navigation properties
    public virtual ICollection<DocumentReference> References { get; set; } = new List<DocumentReference>();
}
```

### **2. Document References Table (Linking Table)**

```csharp
public class DocumentReference : BaseEntity
{
    // Primary identification
    public Guid DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    // Reference information
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // e.g., "Appointment", "User", "Chat"
    
    public Guid EntityId { get; set; } // ID of the referenced entity
    
    [MaxLength(100)]
    public string? ReferenceType { get; set; } // e.g., "profile_picture", "medical_report", "chat_attachment"
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Access control
    public bool IsPublic { get; set; } = false;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Audit
    public Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

---

## **🔄 WORKFLOW IMPLEMENTATION**

### **1. Document Upload Process**

```csharp
public class DocumentService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly IGenericRepository<DocumentReference> _referenceRepository;
    private readonly ILogger<DocumentService> _logger;

    public async Task<ApiResponse<DocumentDto>> UploadDocumentAsync(
        byte[] fileData, 
        string originalFileName, 
        string contentType, 
        string entityType, 
        Guid entityId, 
        string? referenceType = null,
        Guid? createdById = null)
    {
        try
        {
            // 1. Upload file to storage
            var uploadResult = await _fileStorageService.UploadFileAsync(fileData, originalFileName, contentType);
            if (!uploadResult.Success)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Failed to upload file", 500);
            }

            // 2. Create document record
            var document = new Document
            {
                OriginalName = originalFileName,
                UniqueName = Path.GetFileName(uploadResult.Data!),
                FilePath = uploadResult.Data!,
                FolderPath = Path.GetDirectoryName(uploadResult.Data!) ?? "",
                ContentType = contentType,
                FileSize = fileData.Length,
                DocumentType = entityType.ToLower(),
                CreatedById = createdById ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            // 3. Create document reference
            var reference = new DocumentReference
            {
                DocumentId = document.DocumentId,
                EntityType = entityType,
                EntityId = entityId,
                ReferenceType = referenceType,
                CreatedById = createdById ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _referenceRepository.AddAsync(reference);
            await _referenceRepository.SaveChangesAsync();

            // 4. Return document DTO
            return ApiResponse<DocumentDto>.SuccessResponse(new DocumentDto
            {
                DocumentId = document.DocumentId,
                OriginalName = document.OriginalName,
                UniqueName = document.UniqueName,
                FilePath = document.FilePath,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                CreatedAt = document.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName}", originalFileName);
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }
}
```

### **2. Document Retrieval Process**

```csharp
public async Task<ApiResponse<DocumentDto>> GetDocumentAsync(Guid documentId, Guid? userId = null)
{
    try
    {
        // 1. Get document with references
        var document = await _documentRepository.GetByIdAsync(documentId);
        if (document == null || document.IsDeleted)
        {
            return ApiResponse<DocumentDto>.ErrorResponse("Document not found", 404);
        }

        // 2. Check access permissions
        if (!document.IsPublic && userId.HasValue)
        {
            var hasAccess = await _fileStorageService.ValidateFileAccessAsync(document.FilePath, userId.Value);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Access denied", 403);
            }
        }

        // 3. Get file content
        var fileBytes = await _fileStorageService.DownloadFileAsync(document.FilePath);
        if (!fileBytes.Success)
        {
            return ApiResponse<DocumentDto>.ErrorResponse("Failed to retrieve file", 500);
        }

        // 4. Return document DTO with content
        return ApiResponse<DocumentDto>.SuccessResponse(new DocumentDto
        {
            DocumentId = document.DocumentId,
            OriginalName = document.OriginalName,
            UniqueName = document.UniqueName,
            FilePath = document.FilePath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            Content = fileBytes.Data!,
            CreatedAt = document.CreatedAt
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
        return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
    }
}
```

---

## **🎯 MODULE INTEGRATION EXAMPLES**

### **1. Appointment Documents**

```csharp
public class AppointmentService
{
    private readonly IDocumentService _documentService;
    private readonly IGenericRepository<DocumentReference> _referenceRepository;

    public async Task<ApiResponse<DocumentDto>> UploadAppointmentDocumentAsync(
        Guid appointmentId, 
        byte[] fileData, 
        string fileName, 
        string contentType, 
        Guid uploadedById)
    {
        return await _documentService.UploadDocumentAsync(
            fileData, 
            fileName, 
            contentType, 
            "Appointment", 
            appointmentId, 
            "appointment_document",
            uploadedById);
    }

    public async Task<List<DocumentDto>> GetAppointmentDocumentsAsync(Guid appointmentId)
    {
        var references = await _referenceRepository.GetAllAsync(r => 
            r.EntityType == "Appointment" && 
            r.EntityId == appointmentId && 
            !r.IsDeleted);

        var documents = new List<DocumentDto>();
        foreach (var reference in references)
        {
            var documentResult = await _documentService.GetDocumentAsync(reference.DocumentId);
            if (documentResult.Success)
            {
                documents.Add(documentResult.Data!);
            }
        }

        return documents;
    }
}
```

### **2. User Profile Pictures**

```csharp
public class UserService
{
    private readonly IDocumentService _documentService;

    public async Task<ApiResponse<DocumentDto>> UploadProfilePictureAsync(
        Guid userId, 
        byte[] imageData, 
        string fileName, 
        Guid uploadedById)
    {
        return await _documentService.UploadDocumentAsync(
            imageData, 
            fileName, 
            "image/jpeg", 
            "User", 
            userId, 
            "profile_picture",
            uploadedById);
    }

    public async Task<DocumentDto?> GetProfilePictureAsync(Guid userId)
    {
        var references = await _referenceRepository.GetAllAsync(r => 
            r.EntityType == "User" && 
            r.EntityId == userId && 
            r.ReferenceType == "profile_picture" && 
            !r.IsDeleted);

        var reference = references.FirstOrDefault();
        if (reference == null) return null;

        var documentResult = await _documentService.GetDocumentAsync(reference.DocumentId, userId);
        return documentResult.Success ? documentResult.Data : null;
    }
}
```

### **3. Chat Attachments**

```csharp
public class ChatService
{
    private readonly IDocumentService _documentService;

    public async Task<ApiResponse<DocumentDto>> UploadChatAttachmentAsync(
        Guid chatRoomId, 
        byte[] fileData, 
        string fileName, 
        string contentType, 
        Guid uploadedById)
    {
        return await _documentService.UploadDocumentAsync(
            fileData, 
            fileName, 
            contentType, 
            "ChatRoom", 
            chatRoomId, 
            "chat_attachment",
            uploadedById);
    }
}
```

---

## **📊 DATABASE SCHEMA**

### **Documents Table**
```sql
CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL UNIQUE,
    OriginalName NVARCHAR(255) NOT NULL,
    UniqueName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FolderPath NVARCHAR(200) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    FileSize BIGINT NOT NULL,
    Description NVARCHAR(1000) NULL,
    DocumentType NVARCHAR(50) NULL,
    IsEncrypted BIT NOT NULL DEFAULT 0,
    EncryptionKey NVARCHAR(100) NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    DeletedById UNIQUEIDENTIFIER NULL,
    DeletedAt DATETIME2 NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL
);
```

### **DocumentReferences Table**
```sql
CREATE TABLE DocumentReferences (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    DocumentId UNIQUEIDENTIFIER NOT NULL,
    EntityType NVARCHAR(50) NOT NULL,
    EntityId UNIQUEIDENTIFIER NOT NULL,
    ReferenceType NVARCHAR(100) NULL,
    Description NVARCHAR(500) NULL,
    IsPublic BIT NOT NULL DEFAULT 0,
    ExpiresAt DATETIME2 NULL,
    CreatedById UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    FOREIGN KEY (DocumentId) REFERENCES Documents(DocumentId),
    FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);
```

---

## **🔐 SECURITY & ACCESS CONTROL**

### **1. Access Validation**
```csharp
public async Task<bool> ValidateDocumentAccessAsync(Guid documentId, Guid userId)
{
    var document = await _documentRepository.GetByIdAsync(documentId);
    if (document == null || document.IsDeleted) return false;

    // Public documents
    if (document.IsPublic) return true;

    // Check if user created the document
    if (document.CreatedById == userId) return true;

    // Check references for access
    var references = await _referenceRepository.GetAllAsync(r => 
        r.DocumentId == documentId && 
        !r.IsDeleted);

    foreach (var reference in references)
    {
        if (await ValidateEntityAccessAsync(reference.EntityType, reference.EntityId, userId))
        {
            return true;
        }
    }

    return false;
}
```

### **2. Entity-Specific Access Control**
```csharp
private async Task<bool> ValidateEntityAccessAsync(string entityType, Guid entityId, Guid userId)
{
    switch (entityType)
    {
        case "Appointment":
            return await ValidateAppointmentAccessAsync(entityId, userId);
        case "User":
            return entityId == userId; // Users can only access their own documents
        case "ChatRoom":
            return await ValidateChatRoomAccessAsync(entityId, userId);
        default:
            return false;
    }
}
```

---

## **📈 BENEFITS**

### **✅ Single Source of Truth**
- All document metadata in one place
- Consistent file handling across modules
- Centralized audit logging

### **✅ Easy Extension**
- Add new document types without schema changes
- Flexible reference system
- Scalable architecture

### **✅ Simplified Management**
- Unified document deletion
- Consistent naming conventions
- Centralized security policies

### **✅ Performance Optimization**
- Efficient queries with proper indexing
- Reduced data duplication
- Better caching strategies

---

## **🚀 IMPLEMENTATION STEPS**

1. **Create Database Tables** - Documents and DocumentReferences
2. **Implement DocumentService** - Core upload/retrieve logic
3. **Update FileStorageService** - Integration with document tracking
4. **Migrate Existing Data** - Convert current document tables
5. **Update Module Services** - Integrate with new document system
6. **Add Security Layer** - Access control and validation
7. **Testing & Validation** - Comprehensive test coverage

---

## **🎯 NEXT STEPS**

This centralized approach will provide a robust, scalable document management system that can handle all document types across the application while maintaining security and performance. 