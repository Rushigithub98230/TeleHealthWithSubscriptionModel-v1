# 📥 **FILE RETRIEVAL GUIDE - HOW SERVICES CAN RETRIEVE FILES**

## **📋 OVERVIEW**

This guide shows how any service can use the **FileStorageService** to retrieve files. The service provides multiple retrieval methods for different use cases.

---

## **🔧 SERVICE INTEGRATION**

### **1. Basic Service Setup**

```csharp
public class MyService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<MyService> _logger;

    public MyService(IFileStorageService fileStorageService, ILogger<MyService> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }
}
```

---

## **📥 FILE RETRIEVAL METHODS**

### **1. Download File as Bytes**

```csharp
public async Task<byte[]> GetFileBytesAsync(string filePath)
{
    var result = await _fileStorageService.DownloadFileAsync(filePath);
    
    if (!result.Success)
    {
        throw new FileNotFoundException($"File not found: {filePath}");
    }
    
    return result.Data!;
}

// Usage
var fileBytes = await GetFileBytesAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf");
```

### **2. Get File Information**

```csharp
public async Task<FileInfoDto> GetFileInfoAsync(string filePath)
{
    var result = await _fileStorageService.GetFileInfoAsync(filePath);
    
    if (!result.Success)
    {
        throw new FileNotFoundException($"File not found: {filePath}");
    }
    
    return result.Data!;
}

// Usage
var fileInfo = await GetFileInfoAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf");
Console.WriteLine($"File: {fileInfo.FileName}, Size: {fileInfo.FileSize}, Type: {fileInfo.ContentType}");
```

### **3. Check File Existence**

```csharp
public async Task<bool> FileExistsAsync(string filePath)
{
    var result = await _fileStorageService.FileExistsAsync(filePath);
    return result.Success && result.Data;
}

// Usage
if (await FileExistsAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf"))
{
    // File exists, proceed with operations
}
```

### **4. Get File Size**

```csharp
public async Task<long> GetFileSizeAsync(string filePath)
{
    var result = await _fileStorageService.GetFileSizeAsync(filePath);
    
    if (!result.Success)
    {
        throw new FileNotFoundException($"File not found: {filePath}");
    }
    
    return result.Data;
}

// Usage
var fileSize = await GetFileSizeAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf");
Console.WriteLine($"File size: {fileSize} bytes");
```

### **5. Get File URL**

```csharp
public async Task<string> GetFileUrlAsync(string filePath)
{
    var result = await _fileStorageService.GetFileUrlAsync(filePath);
    return result.Data!;
}

// Usage
var fileUrl = await GetFileUrlAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf");
// Returns: "/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf"
```

### **6. Get Secure URL (Time-limited)**

```csharp
public async Task<string> GetSecureUrlAsync(string filePath, int expirationHours = 1)
{
    var expiration = TimeSpan.FromHours(expirationHours);
    var result = await _fileStorageService.GetSecureUrlAsync(filePath, expiration);
    return result.Data!;
}

// Usage
var secureUrl = await GetSecureUrlAsync("a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf", 2);
// Returns secure URL valid for 2 hours
```

---

## **🎯 REAL SERVICE EXAMPLES**

### **1. Document Service**

```csharp
public class DocumentService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(IFileStorageService fileStorageService, ILogger<DocumentService> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<DocumentDto> GetDocumentAsync(string filePath)
    {
        try
        {
            // Get file info
            var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
            if (!fileInfo.Success)
            {
                throw new FileNotFoundException($"Document not found: {filePath}");
            }

            // Get file content
            var fileBytes = await _fileStorageService.DownloadFileAsync(filePath);
            if (!fileBytes.Success)
            {
                throw new Exception($"Failed to download document: {filePath}");
            }

            return new DocumentDto
            {
                FileName = fileInfo.Data!.FileName,
                ContentType = fileInfo.Data!.ContentType,
                FileSize = fileInfo.Data!.FileSize,
                Content = fileBytes.Data!,
                CreatedAt = fileInfo.Data!.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> ValidateDocumentAccessAsync(string filePath, Guid userId)
    {
        var result = await _fileStorageService.ValidateFileAccessAsync(filePath, userId);
        return result.Success && result.Data;
    }
}
```

### **2. Image Service**

```csharp
public class ImageService
{
    private readonly IFileStorageService _fileStorageService;

    public ImageService(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<ImageDto> GetImageAsync(string filePath)
    {
        // Check if file exists
        var exists = await _fileStorageService.FileExistsAsync(filePath);
        if (!exists.Success || !exists.Data)
        {
            throw new FileNotFoundException($"Image not found: {filePath}");
        }

        // Get file info
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
        if (!fileInfo.Success)
        {
            throw new Exception($"Failed to get image info: {filePath}");
        }

        // Validate it's an image
        if (!fileInfo.Data!.ContentType.StartsWith("image/"))
        {
            throw new ArgumentException($"File is not an image: {filePath}");
        }

        // Get image bytes
        var imageBytes = await _fileStorageService.DownloadFileAsync(filePath);
        if (!imageBytes.Success)
        {
            throw new Exception($"Failed to download image: {filePath}");
        }

        return new ImageDto
        {
            FileName = fileInfo.Data!.FileName,
            ContentType = fileInfo.Data!.ContentType,
            FileSize = fileInfo.Data!.FileSize,
            ImageBytes = imageBytes.Data!,
            Url = await _fileStorageService.GetFileUrlAsync(filePath)
        };
    }

    public async Task<string> GetImageUrlAsync(string filePath)
    {
        var result = await _fileStorageService.GetFileUrlAsync(filePath);
        return result.Data!;
    }
}
```

### **3. Chat Attachment Service**

```csharp
public class ChatAttachmentService
{
    private readonly IFileStorageService _fileStorageService;

    public ChatAttachmentService(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public async Task<AttachmentDto> GetChatAttachmentAsync(string filePath, Guid userId)
    {
        // Validate user access
        var hasAccess = await _fileStorageService.ValidateFileAccessAsync(filePath, userId);
        if (!hasAccess.Success || !hasAccess.Data)
        {
            throw new UnauthorizedAccessException($"Access denied to file: {filePath}");
        }

        // Get file info
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
        if (!fileInfo.Success)
        {
            throw new FileNotFoundException($"Attachment not found: {filePath}");
        }

        // Get file content
        var fileBytes = await _fileStorageService.DownloadFileAsync(filePath);
        if (!fileBytes.Success)
        {
            throw new Exception($"Failed to download attachment: {filePath}");
        }

        return new AttachmentDto
        {
            FileName = fileInfo.Data!.FileName,
            ContentType = fileInfo.Data!.ContentType,
            FileSize = fileInfo.Data!.FileSize,
            Content = fileBytes.Data!,
            DownloadUrl = await _fileStorageService.GetSecureUrlAsync(filePath, 1) // 1 hour expiry
        };
    }
}
```

---

## **📡 API ENDPOINTS FOR RETRIEVAL**

### **1. Download File**
```http
GET /api/filestorage/download/{filePath}
```

**Example:**
```http
GET /api/filestorage/download/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf
```

**Response:** File content with proper headers

### **2. Get File Information**
```http
GET /api/filestorage/info/{filePath}
```

**Example:**
```http
GET /api/filestorage/info/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf
```

**Response:**
```json
{
  "success": true,
  "data": {
    "fileName": "document.pdf",
    "filePath": "a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf",
    "fileSize": 1024,
    "contentType": "application/pdf",
    "createdAt": "2025-01-16T10:30:00.000Z",
    "modifiedAt": "2025-01-16T10:30:00.000Z",
    "isDirectory": false
  },
  "message": "File info retrieved successfully"
}
```

### **3. Get Secure URL**
```http
GET /api/filestorage/secure-url/{filePath}?expirationHours=2
```

**Example:**
```http
GET /api/filestorage/secure-url/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf?expirationHours=2
```

**Response:**
```json
{
  "success": true,
  "data": "/uploads/a1b2c3d4-e5f6-7890-abcd-ef1234567890_document.pdf?token=abc123",
  "message": "Secure file URL generated successfully"
}
```

### **4. List Files in Directory**
```http
GET /api/filestorage/list/{directoryPath}?searchPattern=*.pdf
```

**Example:**
```http
GET /api/filestorage/list/appointments/123?searchPattern=*.pdf
```

**Response:**
```json
{
  "success": true,
  "data": [
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890_report.pdf",
    "b2c3d4e5-f6g7-8901-bcde-f23456789012_scan.pdf"
  ],
  "message": "Files listed successfully"
}
```

---

## **🔐 SECURITY RETRIEVAL PATTERNS**

### **1. Access Validation**

```csharp
public async Task<byte[]> GetSecureFileAsync(string filePath, Guid userId)
{
    // First validate access
    var hasAccess = await _fileStorageService.ValidateFileAccessAsync(filePath, userId);
    if (!hasAccess.Success || !hasAccess.Data)
    {
        throw new UnauthorizedAccessException("Access denied");
    }

    // Then download file
    var result = await _fileStorageService.DownloadFileAsync(filePath);
    if (!result.Success)
    {
        throw new FileNotFoundException($"File not found: {filePath}");
    }

    return result.Data!;
}
```

### **2. Encrypted File Retrieval**

```csharp
public async Task<byte[]> GetEncryptedFileAsync(string encryptedFilePath, string encryptionKey)
{
    // Download encrypted file
    var encryptedResult = await _fileStorageService.DownloadFileAsync(encryptedFilePath);
    if (!encryptedResult.Success)
    {
        throw new FileNotFoundException($"Encrypted file not found: {encryptedFilePath}");
    }

    // Decrypt the file
    var decryptedResult = await _fileStorageService.DecryptFileAsync(encryptedFilePath, encryptionKey);
    if (!decryptedResult.Success)
    {
        throw new Exception("Failed to decrypt file");
    }

    return decryptedResult.Data!;
}
```

---

## **📊 BATCH RETRIEVAL PATTERNS**

### **1. Get Multiple Files**

```csharp
public async Task<List<FileDto>> GetMultipleFilesAsync(List<string> filePaths)
{
    var files = new List<FileDto>();

    foreach (var filePath in filePaths)
    {
        try
        {
            var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
            if (fileInfo.Success)
            {
                files.Add(new FileDto
                {
                    FilePath = filePath,
                    FileName = fileInfo.Data!.FileName,
                    FileSize = fileInfo.Data!.FileSize,
                    ContentType = fileInfo.Data!.ContentType,
                    Url = await _fileStorageService.GetFileUrlAsync(filePath)
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get file info for {FilePath}", filePath);
        }
    }

    return files;
}
```

### **2. List Files in Directory**

```csharp
public async Task<List<FileInfoDto>> GetDirectoryFilesAsync(string directoryPath, string? searchPattern = null)
{
    var result = await _fileStorageService.ListFilesAsync(directoryPath, searchPattern);
    if (!result.Success)
    {
        throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
    }

    var files = new List<FileInfoDto>();
    foreach (var fileName in result.Data!)
    {
        var filePath = Path.Combine(directoryPath, fileName).Replace('\\', '/');
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
        if (fileInfo.Success)
        {
            files.Add(fileInfo.Data!);
        }
    }

    return files;
}
```

---

## **🔄 ERROR HANDLING PATTERNS**

### **1. Graceful File Retrieval**

```csharp
public async Task<FileDto?> TryGetFileAsync(string filePath)
{
    try
    {
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
        if (!fileInfo.Success)
        {
            return null;
        }

        var fileBytes = await _fileStorageService.DownloadFileAsync(filePath);
        if (!fileBytes.Success)
        {
            return null;
        }

        return new FileDto
        {
            FileName = fileInfo.Data!.FileName,
            ContentType = fileInfo.Data!.ContentType,
            FileSize = fileInfo.Data!.FileSize,
            Content = fileBytes.Data!
        };
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Failed to retrieve file {FilePath}", filePath);
        return null;
    }
}
```

### **2. Retry Pattern**

```csharp
public async Task<byte[]> GetFileWithRetryAsync(string filePath, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var result = await _fileStorageService.DownloadFileAsync(filePath);
            if (result.Success)
            {
                return result.Data!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Retry {RetryCount} failed for file {FilePath}", i + 1, filePath);
            
            if (i == maxRetries - 1)
            {
                throw;
            }
            
            await Task.Delay(1000 * (i + 1)); // Exponential backoff
        }
    }

    throw new FileNotFoundException($"File not found after {maxRetries} retries: {filePath}");
}
```

---

## **🎯 INTEGRATION EXAMPLES**

### **1. Appointment Document Retrieval**

```csharp
public class AppointmentService
{
    private readonly IFileStorageService _fileStorageService;

    public async Task<DocumentDto> GetAppointmentDocumentAsync(string filePath, Guid appointmentId, Guid userId)
    {
        // Validate user has access to this appointment
        if (!await ValidateAppointmentAccessAsync(appointmentId, userId))
        {
            throw new UnauthorizedAccessException("Access denied to appointment");
        }

        // Get document
        var fileBytes = await _fileStorageService.DownloadFileAsync(filePath);
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);

        return new DocumentDto
        {
            FileName = fileInfo.Data!.FileName,
            ContentType = fileInfo.Data!.ContentType,
            FileSize = fileInfo.Data!.FileSize,
            Content = fileBytes.Data!,
            DownloadUrl = await _fileStorageService.GetSecureUrlAsync(filePath, 1)
        };
    }
}
```

### **2. User Profile Picture Retrieval**

```csharp
public class UserService
{
    private readonly IFileStorageService _fileStorageService;

    public async Task<ProfilePictureDto> GetProfilePictureAsync(string filePath)
    {
        var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
        if (!fileInfo.Success)
        {
            throw new FileNotFoundException("Profile picture not found");
        }

        return new ProfilePictureDto
        {
            FileName = fileInfo.Data!.FileName,
            ContentType = fileInfo.Data!.ContentType,
            FileSize = fileInfo.Data!.FileSize,
            Url = await _fileStorageService.GetFileUrlAsync(filePath),
            SecureUrl = await _fileStorageService.GetSecureUrlAsync(filePath, 24) // 24 hours
        };
    }
}
```

---

## **✅ SUMMARY**

### **Retrieval Methods Available:**

1. **📥 Download File** - Get file as bytes
2. **📋 Get File Info** - Get metadata (name, size, type)
3. **🔍 Check Existence** - Verify file exists
4. **📏 Get File Size** - Get file size in bytes
5. **🌐 Get File URL** - Get web-accessible URL
6. **🔐 Get Secure URL** - Get time-limited secure URL
7. **📁 List Files** - List files in directory
8. **🔓 Validate Access** - Check user permissions

### **Integration Patterns:**

- **Service Injection** - Inject `IFileStorageService` into any service
- **Error Handling** - Proper exception handling for missing files
- **Security** - Access validation before retrieval
- **Batch Operations** - Retrieve multiple files efficiently
- **Retry Logic** - Handle temporary failures

**Any service can now easily retrieve files using the unified FileStorageService interface!** 🚀 