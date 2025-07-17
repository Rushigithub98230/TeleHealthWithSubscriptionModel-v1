using Microsoft.AspNetCore.Http;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IFileStorageService
{
    // Core file operations
    Task<ApiResponse<string>> UploadFileAsync(byte[] fileData, string fileName, string contentType);
    Task<ApiResponse<byte[]>> DownloadFileAsync(string filePath);
    Task<ApiResponse<bool>> DeleteFileAsync(string filePath);
    Task<ApiResponse<bool>> FileExistsAsync(string filePath);
    Task<ApiResponse<long>> GetFileSizeAsync(string filePath);
    Task<ApiResponse<string>> GetFileUrlAsync(string filePath);
    
    // File metadata
    Task<ApiResponse<FileInfoDto>> GetFileInfoAsync(string filePath);
    Task<ApiResponse<string>> GetSecureUrlAsync(string filePath, TimeSpan? expiration = null);
    
    // Directory operations
    Task<ApiResponse<bool>> CreateDirectoryAsync(string directoryPath);
    Task<ApiResponse<bool>> DeleteDirectoryAsync(string directoryPath);
    Task<ApiResponse<IEnumerable<string>>> ListFilesAsync(string directoryPath, string? searchPattern = null);
    
    // Security and access control
    Task<ApiResponse<bool>> ValidateFileAccessAsync(string filePath, Guid userId);
    Task<ApiResponse<bool>> SetFilePermissionsAsync(string filePath, FilePermissions permissions);
    
    // Encryption
    Task<ApiResponse<string>> EncryptFileAsync(byte[] fileData, string encryptionKey);
    Task<ApiResponse<byte[]>> DecryptFileAsync(string encryptedFilePath, string encryptionKey);
    
    // Batch operations
    Task<ApiResponse<IEnumerable<string>>> UploadMultipleFilesAsync(IEnumerable<FileUploadDto> files);
    Task<ApiResponse<bool>> DeleteMultipleFilesAsync(IEnumerable<string> filePaths);
    
    // Storage management
    Task<ApiResponse<StorageInfoDto>> GetStorageInfoAsync();
    Task<ApiResponse<bool>> CleanupExpiredFilesAsync();
    Task<ApiResponse<bool>> ArchiveOldFilesAsync(string sourcePath, string archivePath, TimeSpan ageThreshold);
} 