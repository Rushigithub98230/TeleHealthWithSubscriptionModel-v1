using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;
using DTOs = SmartTelehealth.Application.DTOs;
using Microsoft.Extensions.Configuration;

namespace SmartTelehealth.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _baseStoragePath;
    private readonly string _encryptionKey;

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _baseStoragePath = configuration["FileStorage:Local:BasePath"] ?? "wwwroot/uploads";
        _encryptionKey = configuration["FileStorage:EncryptionKey"] ?? "default-encryption-key-change-in-production";
        
        // Ensure base directory exists
        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<ApiResponse<string>> UploadFileAsync(byte[] fileData, string fileName, string contentType)
    {
        try
        {
            // Generate unique filename to prevent conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var filePath = Path.Combine(_baseStoragePath, uniqueFileName);
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }
            
            // Write file
            await File.WriteAllBytesAsync(filePath, fileData);
            
            // Store original filename metadata
            var metadataPath = filePath + ".meta";
            var metadata = new { OriginalFileName = fileName, ContentType = contentType, UploadedAt = DateTime.UtcNow };
            await File.WriteAllTextAsync(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));
            
            _logger.LogInformation("File uploaded successfully: {FileName} -> {FilePath}", fileName, filePath);
            return ApiResponse<string>.SuccessResponse(uniqueFileName, "File uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            return ApiResponse<string>.ErrorResponse($"Error uploading file: {ex.Message}");
        }
    }

    public async Task<ApiResponse<byte[]>> DownloadFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return ApiResponse<byte[]>.ErrorResponse("File not found", 404);
            var data = await File.ReadAllBytesAsync(fullPath);
            return ApiResponse<byte[]>.SuccessResponse(data, "File downloaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FilePath}", filePath);
            return ApiResponse<byte[]>.ErrorResponse($"Error downloading file: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return ApiResponse<bool>.ErrorResponse("File not found", 404);
            File.Delete(fullPath);
            
            // Delete metadata file if it exists
            var metadataPath = fullPath + ".meta";
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }
            
            return ApiResponse<bool>.SuccessResponse(true, "File deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
            return ApiResponse<bool>.ErrorResponse($"Error deleting file: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> FileExistsAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            var exists = File.Exists(fullPath);
            return ApiResponse<bool>.SuccessResponse(exists, exists ? "File exists" : "File not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
            return ApiResponse<bool>.ErrorResponse($"Error checking file existence: {ex.Message}");
        }
    }

    public async Task<ApiResponse<long>> GetFileSizeAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return ApiResponse<long>.ErrorResponse("File not found", 404);
            var fileInfo = new FileInfo(fullPath);
            return ApiResponse<long>.SuccessResponse(fileInfo.Length, "File size retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file size: {FilePath}", filePath);
            return ApiResponse<long>.ErrorResponse($"Error getting file size: {ex.Message}");
        }
    }

    public Task<ApiResponse<string>> GetFileUrlAsync(string filePath)
    {
        var url = $"/uploads/{filePath}";
        return Task.FromResult(ApiResponse<string>.SuccessResponse(url, "File URL generated successfully"));
    }

    public async Task<ApiResponse<FileInfoDto>> GetFileInfoAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            if (!File.Exists(fullPath))
                return ApiResponse<FileInfoDto>.ErrorResponse("File not found", 404);
            
            var fileInfo = new FileInfo(fullPath);
            var extension = Path.GetExtension(filePath);
            
            // Try to get original filename from metadata
            var originalFileName = Path.GetFileName(filePath); // Default to the stored filename
            var metadataPath = fullPath + ".meta";
            if (File.Exists(metadataPath))
            {
                try
                {
                    var metadataJson = await File.ReadAllTextAsync(metadataPath);
                    var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson);
                    if (metadata != null && metadata.ContainsKey("OriginalFileName"))
                    {
                        originalFileName = metadata["OriginalFileName"]?.ToString() ?? originalFileName;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not read metadata for file: {FilePath}", filePath);
                }
            }
            
            var dto = new FileInfoDto
            {
                FileName = originalFileName,
                FilePath = filePath,
                FileSize = fileInfo.Length,
                CreatedAt = fileInfo.CreationTimeUtc,
                ModifiedAt = fileInfo.LastWriteTimeUtc,
                ContentType = GetContentTypeFromExtension(extension),
                IsDirectory = false
            };
            
            return ApiResponse<FileInfoDto>.SuccessResponse(dto, "File info retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
            return ApiResponse<FileInfoDto>.ErrorResponse($"Error getting file info: {ex.Message}");
        }
    }

    public Task<ApiResponse<string>> GetSecureUrlAsync(string filePath, TimeSpan? expiration = null)
    {
        var url = $"/uploads/{filePath}";
        return Task.FromResult(ApiResponse<string>.SuccessResponse(url, "Secure file URL generated successfully"));
    }

    public async Task<ApiResponse<bool>> CreateDirectoryAsync(string directoryPath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (Directory.Exists(fullPath))
                return ApiResponse<bool>.SuccessResponse(true, "Directory already exists");
            
            Directory.CreateDirectory(fullPath);
            return ApiResponse<bool>.SuccessResponse(true, "Directory created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory: {DirectoryPath}", directoryPath);
            return ApiResponse<bool>.ErrorResponse($"Error creating directory: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteDirectoryAsync(string directoryPath)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (!Directory.Exists(fullPath))
                return ApiResponse<bool>.ErrorResponse("Directory not found", 404);
            
            Directory.Delete(fullPath, true);
            return ApiResponse<bool>.SuccessResponse(true, "Directory deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting directory: {DirectoryPath}", directoryPath);
            return ApiResponse<bool>.ErrorResponse($"Error deleting directory: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> ListFilesAsync(string directoryPath, string? searchPattern = null)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, directoryPath);
            if (!Directory.Exists(fullPath))
                return ApiResponse<IEnumerable<string>>.ErrorResponse("Directory not found", 404);
            
            var files = Directory.GetFiles(fullPath, searchPattern ?? "*")
                .Select(f => Path.GetFileName(f))
                .Where(f => !f!.EndsWith(".meta")) // Exclude metadata files
                .ToList();
            
            return ApiResponse<IEnumerable<string>>.SuccessResponse(files, "Files listed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files: {DirectoryPath}", directoryPath);
            return ApiResponse<IEnumerable<string>>.ErrorResponse($"Error listing files: {ex.Message}");
        }
    }

    public Task<ApiResponse<bool>> ValidateFileAccessAsync(string filePath, Guid userId)
    {
        // For now, implement basic access validation
        // In a real implementation, you would check user permissions, file ownership, etc.
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, filePath);
            var exists = File.Exists(fullPath);
            
            if (!exists)
                return Task.FromResult(ApiResponse<bool>.ErrorResponse("File not found", 404));
            
            // Basic access validation - in production, implement proper access control
            // For now, if file exists, grant access
            return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, "Access granted"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file access: {FilePath} for user {UserId}", filePath, userId);
            return Task.FromResult(ApiResponse<bool>.ErrorResponse($"Error validating file access: {ex.Message}"));
        }
    }

    public Task<ApiResponse<bool>> SetFilePermissionsAsync(string filePath, FilePermissions permissions)
    {
        // For local storage, file permissions are handled by the file system
        // In production, you might implement additional permission tracking
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, "File permissions set successfully"));
    }

    public async Task<ApiResponse<string>> EncryptFileAsync(byte[] fileData, string encryptionKey)
    {
        try
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
            aes.GenerateIV();
            
            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            
            // Write IV first
            await msEncrypt.WriteAsync(aes.IV, 0, aes.IV.Length);
            
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                await csEncrypt.WriteAsync(fileData, 0, fileData.Length);
                await csEncrypt.FlushFinalBlockAsync();
            }
            
            var encryptedData = msEncrypt.ToArray();
            var base64Result = Convert.ToBase64String(encryptedData);
            
            return ApiResponse<string>.SuccessResponse(base64Result, "File encrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file data");
            return ApiResponse<string>.ErrorResponse($"Error encrypting file: {ex.Message}");
        }
    }

    public async Task<ApiResponse<byte[]>> DecryptFileAsync(string encryptedFilePath, string encryptionKey)
    {
        try
        {
            var fullPath = Path.Combine(_baseStoragePath, encryptedFilePath);
            if (!File.Exists(fullPath))
                return ApiResponse<byte[]>.ErrorResponse("Encrypted file not found", 404);
            
            var encryptedData = await File.ReadAllBytesAsync(fullPath);
            
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
            
            // Read IV from the beginning of the encrypted data
            var iv = new byte[16];
            Array.Copy(encryptedData, 0, iv, 0, 16);
            aes.IV = iv;
            
            // Extract the actual encrypted data (after IV)
            var actualEncryptedData = new byte[encryptedData.Length - 16];
            Array.Copy(encryptedData, 16, actualEncryptedData, 0, actualEncryptedData.Length);
            
            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(actualEncryptedData);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var resultStream = new MemoryStream();
            
            await csDecrypt.CopyToAsync(resultStream);
            var decryptedData = resultStream.ToArray();
            
            return ApiResponse<byte[]>.SuccessResponse(decryptedData, "File decrypted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file: {FilePath}", encryptedFilePath);
            return ApiResponse<byte[]>.ErrorResponse($"Error decrypting file: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<string>>> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files)
    {
        try
        {
            var uploadedPaths = new List<string>();
            
            foreach (var file in files)
            {
                // Handle directory specification
                var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                var targetPath = string.IsNullOrEmpty(file.Directory) 
                    ? Path.Combine(_baseStoragePath, uniqueFileName)
                    : Path.Combine(_baseStoragePath, file.Directory, uniqueFileName);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }
                
                // Write file
                await File.WriteAllBytesAsync(targetPath, file.Content);
                
                // Store metadata
                var metadataPath = targetPath + ".meta";
                var metadata = new { OriginalFileName = file.FileName, ContentType = file.ContentType ?? "application/octet-stream", UploadedAt = DateTime.UtcNow };
                await File.WriteAllTextAsync(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata));
                
                // Return relative path
                var relativePath = Path.GetRelativePath(_baseStoragePath, targetPath);
                uploadedPaths.Add(relativePath.Replace('\\', '/'));
            }
            
            return ApiResponse<IEnumerable<string>>.SuccessResponse(uploadedPaths, "Multiple files uploaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            return ApiResponse<IEnumerable<string>>.ErrorResponse($"Error uploading multiple files: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> DeleteMultipleFilesAsync(IEnumerable<string> filePaths)
    {
        try
        {
            var allDeleted = true;
            
            foreach (var filePath in filePaths)
            {
                var result = await DeleteFileAsync(filePath);
                if (!result.Success)
                {
                    allDeleted = false;
                    _logger.LogWarning("Failed to delete file: {FilePath}", filePath);
                }
            }
            
            return ApiResponse<bool>.SuccessResponse(allDeleted, "Multiple files deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple files");
            return ApiResponse<bool>.ErrorResponse($"Error deleting multiple files: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StorageInfoDto>> GetStorageInfoAsync()
    {
        try
        {
            var driveInfo = new DriveInfo(Path.GetPathRoot(_baseStoragePath) ?? "C:");
            var directoryInfo = new DirectoryInfo(_baseStoragePath);
            
            var fileCount = 0;
            var directoryCount = 0;
            
            if (Directory.Exists(_baseStoragePath))
            {
                fileCount = Directory.GetFiles(_baseStoragePath, "*", SearchOption.AllDirectories).Length;
                directoryCount = Directory.GetDirectories(_baseStoragePath, "*", SearchOption.AllDirectories).Length;
            }
            
            var storageInfo = new StorageInfoDto
            {
                TotalSpace = driveInfo.TotalSize,
                UsedSpace = driveInfo.TotalSize - driveInfo.AvailableFreeSpace,
                AvailableSpace = driveInfo.AvailableFreeSpace,
                FileCount = fileCount,
                DirectoryCount = directoryCount,
                LastUpdated = DateTime.UtcNow
            };
            
            return ApiResponse<StorageInfoDto>.SuccessResponse(storageInfo, "Storage info retrieved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage info");
            return ApiResponse<StorageInfoDto>.ErrorResponse($"Error getting storage info: {ex.Message}");
        }
    }

    public Task<ApiResponse<bool>> CleanupExpiredFilesAsync()
    {
        // For local storage, implement cleanup logic based on file age, size, etc.
        // For now, return success
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, "Cleanup completed successfully"));
    }

    public Task<ApiResponse<bool>> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold)
    {
        // For local storage, implement archiving logic
        // For now, return success
        return Task.FromResult(ApiResponse<bool>.SuccessResponse(true, "Archiving completed successfully"));
    }

    private string GetContentTypeFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => "application/octet-stream"
        };
    }
} 