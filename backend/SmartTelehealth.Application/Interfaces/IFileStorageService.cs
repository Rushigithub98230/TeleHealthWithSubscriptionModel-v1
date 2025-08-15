using Microsoft.AspNetCore.Http;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IFileStorageService
{
    // Core file operations
    Task<JsonModel> UploadFileAsync(byte[] fileData, string fileName, string contentType);
    Task<JsonModel> DownloadFileAsync(string filePath);
    Task<JsonModel> DeleteFileAsync(string filePath);
    Task<JsonModel> FileExistsAsync(string filePath);
    Task<JsonModel> GetFileSizeAsync(string filePath);
    Task<JsonModel> GetFileUrlAsync(string filePath);
    
    // File metadata
    Task<JsonModel> GetFileInfoAsync(string filePath);
    Task<JsonModel> GetSecureUrlAsync(string filePath, TimeSpan? expiration = null);
    
    // Directory operations
    Task<JsonModel> CreateDirectoryAsync(string directoryPath);
    Task<JsonModel> DeleteDirectoryAsync(string directoryPath);
    Task<JsonModel> ListFilesAsync(string directoryPath, string? searchPattern = null);
    
    // Security and access control
    Task<JsonModel> ValidateFileAccessAsync(string filePath, Guid userId);
    Task<JsonModel> SetFilePermissionsAsync(string filePath, FilePermissions permissions);
    
    // Encryption
    Task<JsonModel> EncryptFileAsync(byte[] fileData, string encryptionKey);
    Task<JsonModel> DecryptFileAsync(string encryptedFilePath, string encryptionKey);
    
    // Batch operations
    Task<JsonModel> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files);
    Task<JsonModel> DeleteMultipleFilesAsync(IEnumerable<string> filePaths);
    
    // Storage management
    Task<JsonModel> GetStorageInfoAsync();
    Task<JsonModel> CleanupExpiredFilesAsync();
    Task<JsonModel> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold);
} 