# File Storage System Documentation

## Overview

The SmartTelehealth application implements a flexible file storage system that allows seamless switching between local and cloud storage providers without changing the public interface. This system is designed to handle chat media files (images, documents, etc.) with encryption, security, and scalability in mind.

## Architecture

### Storage Providers

The system supports three storage providers:

1. **Local Storage** (`LocalFileStorageService`)
   - Stores files in the local filesystem under `wwwroot/uploads`
   - Default provider for development and testing
   - Fast access but limited scalability

2. **Azure Blob Storage** (`AzureBlobStorageService`)
   - Cloud storage using Azure Blob Storage
   - Highly scalable and reliable
   - Supports secure URLs and advanced features

3. **AWS S3 Storage** (`AwsS3StorageService`)
   - Cloud storage using Amazon S3
   - Enterprise-grade scalability and reliability
   - Supports pre-signed URLs and advanced security features

### Factory Pattern

The `FileStorageFactory` implements the Factory pattern to dynamically create the appropriate storage service based on configuration:

```csharp
// Configuration-based selection
var storageProvider = _configuration["FileStorage:Provider"]; // "local", "azure", or "aws"
var storageService = factory.CreateFileStorageService(storageProvider);
```

## Configuration

### appsettings.json

```json
{
  "FileStorage": {
    "Provider": "local", // "local", "azure", or "aws"
    "EncryptionKey": "your-32-character-encryption-key-here",
    "Local": {
      "BasePath": "wwwroot/uploads"
    },
    "Azure": {
      "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=yourstorageaccount;AccountKey=yourstoragekey;EndpointSuffix=core.windows.net",
      "ContainerName": "chat-media"
    },
    "Aws": {
      "AccessKey": "your_aws_access_key",
      "SecretKey": "your_aws_secret_key",
      "Region": "us-east-1",
      "BucketName": "chat-media"
    }
  }
}
```

### Environment Variables

For production, use environment variables for sensitive configuration:

```bash
# Azure
FileStorage__Azure__ConnectionString=your_connection_string

# AWS
FileStorage__Aws__AccessKey=your_access_key
FileStorage__Aws__SecretKey=your_secret_key

# Encryption
FileStorage__EncryptionKey=your_encryption_key
```

## API Endpoints

### File Upload

```http
POST /api/filestorage/upload
Content-Type: multipart/form-data

file: [binary data]
```

### Multiple File Upload

```http
POST /api/filestorage/upload-multiple
Content-Type: multipart/form-data

files: [binary data 1]
files: [binary data 2]
...
```

### File Download

```http
GET /api/filestorage/download/{filePath}
```

### File Information

```http
GET /api/filestorage/info/{filePath}
```

### Secure URL Generation

```http
GET /api/filestorage/secure-url/{filePath}?expirationHours=1
```

### File Deletion

```http
DELETE /api/filestorage/{filePath}
```

### Multiple File Deletion

```http
DELETE /api/filestorage/multiple
Content-Type: application/json

["file1.jpg", "file2.pdf", "file3.docx"]
```

### List Files

```http
GET /api/filestorage/list/{directoryPath}?searchPattern=*.jpg
```

### Storage Information

```http
GET /api/filestorage/storage-info
```

### Cleanup Expired Files (Admin Only)

```http
POST /api/filestorage/cleanup
```

### Archive Old Files (Admin Only)

```http
POST /api/filestorage/archive
Content-Type: application/json

{
  "sourcePath": "chat/2024",
  "archivePath": "archive/2024",
  "ageThresholdDays": 30
}
```

### File Encryption

```http
POST /api/filestorage/encrypt?encryptionKey=your_key
Content-Type: multipart/form-data

file: [binary data]
```

### File Decryption

```http
POST /api/filestorage/decrypt/{encryptedFilePath}?encryptionKey=your_key
```

## Features

### Security

- **File Encryption**: All files can be encrypted using AES-256 encryption
- **Access Control**: File access validation based on user permissions
- **Secure URLs**: Time-limited secure URLs for file access
- **File Permissions**: Granular file permission control

### Performance

- **Batch Operations**: Upload and delete multiple files efficiently
- **Streaming**: Large file handling with memory-efficient streaming
- **Caching**: File metadata caching for improved performance
- **Compression**: Automatic file compression for storage optimization

### Management

- **Storage Monitoring**: Real-time storage usage and statistics
- **File Lifecycle**: Automatic cleanup of expired files
- **Archiving**: Automated archiving of old files
- **Backup**: Integration with cloud backup services

### Integration

- **Chat System**: Seamless integration with chat message attachments
- **Notification System**: File upload/download notifications
- **Audit Logging**: Comprehensive audit trail for file operations
- **Error Handling**: Robust error handling and recovery

## Usage Examples

### Switching Storage Providers

```csharp
// In Startup.cs or Program.cs
services.AddScoped<IFileStorageService>(provider =>
{
    var factory = provider.GetRequiredService<FileStorageFactory>();
    return factory.CreateFileStorageService("azure"); // or "aws", "local"
});
```

### Programmatic Provider Selection

```csharp
public class FileService
{
    private readonly FileStorageFactory _factory;
    
    public FileService(FileStorageFactory factory)
    {
        _factory = factory;
    }
    
    public async Task<string> UploadFileAsync(byte[] data, string fileName, string provider = "local")
    {
        var storageService = _factory.CreateFileStorageService(provider);
        var result = await storageService.UploadFileAsync(data, fileName, "application/octet-stream");
        return result.Data;
    }
}
```

### File Upload in Chat

```csharp
public class ChatService
{
    private readonly IFileStorageService _fileStorage;
    
    public async Task<MessageDto> SendMessageWithAttachmentAsync(Guid chatRoomId, string message, IFormFile attachment)
    {
        // Upload file
        using var stream = new MemoryStream();
        await attachment.CopyToAsync(stream);
        var uploadResult = await _fileStorage.UploadFileAsync(
            stream.ToArray(), 
            attachment.FileName, 
            attachment.ContentType
        );
        
        // Create message with attachment
        var messageDto = new MessageDto
        {
            ChatRoomId = chatRoomId,
            Content = message,
            AttachmentPath = uploadResult.Data,
            AttachmentName = attachment.FileName,
            AttachmentType = attachment.ContentType
        };
        
        return await CreateMessageAsync(messageDto);
    }
}
```

## Migration Guide

### From Local to Cloud Storage

1. **Update Configuration**
   ```json
   {
     "FileStorage": {
       "Provider": "azure", // or "aws"
       "Azure": {
         "ConnectionString": "your_connection_string",
         "ContainerName": "chat-media"
       }
     }
   }
   ```

2. **Migrate Existing Files** (Optional)
   ```csharp
   public async Task MigrateFilesAsync(string sourceProvider, string targetProvider)
   {
       var sourceFactory = new FileStorageFactory(sourceProvider);
       var targetFactory = new FileStorageFactory(targetProvider);
       
       var sourceStorage = sourceFactory.CreateFileStorageService();
       var targetStorage = targetFactory.CreateFileStorageService();
       
       var files = await sourceStorage.ListFilesAsync("");
       
       foreach (var file in files)
       {
           var fileData = await sourceStorage.DownloadFileAsync(file);
           await targetStorage.UploadFileAsync(fileData.Data, Path.GetFileName(file), "application/octet-stream");
       }
   }
   ```

### Backup Strategy

```csharp
public class BackupService
{
    private readonly IFileStorageService _primaryStorage;
    private readonly IFileStorageService _backupStorage;
    
    public async Task CreateBackupAsync()
    {
        var files = await _primaryStorage.ListFilesAsync("");
        
        foreach (var file in files)
        {
            var fileData = await _primaryStorage.DownloadFileAsync(file);
            await _backupStorage.UploadFileAsync(fileData.Data, Path.GetFileName(file), "application/octet-stream");
        }
    }
}
```

## Best Practices

### Security
- Use strong encryption keys (32+ characters)
- Rotate encryption keys regularly
- Implement proper access controls
- Use secure URLs for sensitive files
- Validate file types and sizes

### Performance
- Use batch operations for multiple files
- Implement proper error handling
- Monitor storage usage
- Use appropriate file compression
- Cache frequently accessed files

### Scalability
- Use cloud storage for production
- Implement proper backup strategies
- Monitor storage costs
- Use CDN for global access
- Implement file lifecycle policies

### Monitoring
- Log all file operations
- Monitor storage usage
- Track file access patterns
- Alert on storage issues
- Monitor costs (cloud storage)

## Troubleshooting

### Common Issues

1. **File Upload Fails**
   - Check file size limits
   - Verify storage provider configuration
   - Check network connectivity (cloud storage)
   - Verify file permissions

2. **File Download Fails**
   - Check file existence
   - Verify file permissions
   - Check secure URL expiration
   - Verify encryption keys

3. **Storage Provider Issues**
   - Verify connection strings/credentials
   - Check network connectivity
   - Verify storage quotas
   - Check service availability

### Debugging

```csharp
// Enable detailed logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.AddDebug();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

### Performance Tuning

```csharp
// Configure batch sizes
services.Configure<FileStorageOptions>(options =>
{
    options.MaxBatchSize = 100;
    options.MaxFileSize = 10 * 1024 * 1024; // 10MB
    options.EnableCompression = true;
    options.CacheExpiration = TimeSpan.FromHours(1);
});
```

## Future Enhancements

1. **Additional Storage Providers**
   - Google Cloud Storage
   - DigitalOcean Spaces
   - MinIO (self-hosted)

2. **Advanced Features**
   - File versioning
   - Delta synchronization
   - Real-time collaboration
   - Advanced search capabilities

3. **Integration Features**
   - Document preview
   - Image processing
   - Video transcoding
   - OCR capabilities

4. **Security Enhancements**
   - End-to-end encryption
   - Digital signatures
   - Access audit trails
   - Compliance reporting 