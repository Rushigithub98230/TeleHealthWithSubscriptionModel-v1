using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Infrastructure.Services;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace SmartTelehealth.Infrastructure.Tests;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _service;
    private readonly string _testStoragePath;
    private readonly Mock<ILogger<LocalFileStorageService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public LocalFileStorageServiceTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), "LocalFileStorageTests", Guid.NewGuid().ToString());
        
        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Setup configuration
        _mockConfiguration.Setup(x => x["FileStorage:Local:BasePath"]).Returns(_testStoragePath);
        _mockConfiguration.Setup(x => x["FileStorage:EncryptionKey"]).Returns("test-encryption-key-32-chars-long");
        
        _service = new LocalFileStorageService(_mockLogger.Object, _mockConfiguration.Object);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testStoragePath))
        {
            Directory.Delete(_testStoragePath, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_ValidFile_ShouldSucceed()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("Test file content");
        var fileName = "test.txt";
        var contentType = "text/plain";

        // Act
        var result = await _service.UploadFileAsync(fileData, fileName, contentType);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains(fileName, result.Data);
        Assert.Equal("File uploaded successfully", result.Message);
    }

    [Fact]
    public async Task UploadFileAsync_EmptyFile_ShouldSucceed()
    {
        // Arrange
        var fileData = Array.Empty<byte>();
        var fileName = "empty.txt";
        var contentType = "text/plain";

        // Act
        var result = await _service.UploadFileAsync(fileData, fileName, contentType);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Contains(fileName, result.Data);
    }

    [Fact]
    public async Task DownloadFileAsync_ExistingFile_ShouldSucceed()
    {
        // Arrange
        var originalContent = "Test file content";
        var fileData = Encoding.UTF8.GetBytes(originalContent);
        var fileName = "test.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.DownloadFileAsync(uploadResult.Data);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(originalContent, Encoding.UTF8.GetString(result.Data));
    }

    [Fact]
    public async Task DownloadFileAsync_NonExistentFile_ShouldReturnError()
    {
        // Act
        var result = await _service.DownloadFileAsync("non-existent-file.txt");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ShouldSucceed()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("Test content");
        var fileName = "test.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.DeleteFileAsync(uploadResult.Data);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistentFile_ShouldReturnError()
    {
        // Act
        var result = await _service.DeleteFileAsync("non-existent-file.txt");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task FileExistsAsync_ExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("Test content");
        var fileName = "test.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.FileExistsAsync(uploadResult.Data);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal("File exists", result.Message);
    }

    [Fact]
    public async Task FileExistsAsync_NonExistentFile_ShouldReturnFalse()
    {
        // Act
        var result = await _service.FileExistsAsync("non-existent-file.txt");

        // Assert
        Assert.True(result.Success);
        Assert.False(result.Data);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task GetFileSizeAsync_ExistingFile_ShouldReturnCorrectSize()
    {
        // Arrange
        var content = "Test file content";
        var fileData = Encoding.UTF8.GetBytes(content);
        var fileName = "test.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.GetFileSizeAsync(uploadResult.Data);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(content.Length, result.Data);
    }

    [Fact]
    public async Task GetFileSizeAsync_NonExistentFile_ShouldReturnError()
    {
        // Act
        var result = await _service.GetFileSizeAsync("non-existent-file.txt");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task GetFileUrlAsync_ShouldReturnCorrectUrl()
    {
        // Arrange
        var filePath = "test-file.txt";

        // Act
        var result = await _service.GetFileUrlAsync(filePath);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("/uploads/", result.Data);
        Assert.Contains(filePath, result.Data);
    }

    [Fact]
    public async Task GetFileInfoAsync_ExistingFile_ShouldReturnCorrectInfo()
    {
        // Arrange
        var content = "Test file content";
        var fileData = Encoding.UTF8.GetBytes(content);
        var fileName = "test.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.GetFileInfoAsync(uploadResult.Data!);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(fileName, result.Data!.FileName); // Should return original filename from metadata
        Assert.Equal(content.Length, result.Data!.FileSize);
        Assert.Equal("text/plain", result.Data!.ContentType);
        Assert.False(result.Data!.IsDirectory);
    }

    [Fact]
    public async Task GetFileInfoAsync_NonExistentFile_ShouldReturnError()
    {
        // Act
        var result = await _service.GetFileInfoAsync("non-existent-file.txt");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("File not found", result.Message);
    }

    [Fact]
    public async Task GetSecureUrlAsync_ShouldReturnUrl()
    {
        // Arrange
        var filePath = "test-file.txt";

        // Act
        var result = await _service.GetSecureUrlAsync(filePath);

        // Assert
        Assert.True(result.Success);
        Assert.Contains("/uploads/", result.Data);
        Assert.Contains(filePath, result.Data);
    }

    [Fact]
    public async Task CreateDirectoryAsync_NewDirectory_ShouldSucceed()
    {
        // Arrange
        var directoryPath = "test-directory";

        // Act
        var result = await _service.CreateDirectoryAsync(directoryPath);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal("Directory created successfully", result.Message);
    }

    [Fact]
    public async Task CreateDirectoryAsync_ExistingDirectory_ShouldSucceed()
    {
        // Arrange
        var directoryPath = "test-directory";
        await _service.CreateDirectoryAsync(directoryPath);

        // Act
        var result = await _service.CreateDirectoryAsync(directoryPath);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Equal("Directory already exists", result.Message);
    }

    [Fact]
    public async Task DeleteDirectoryAsync_ExistingDirectory_ShouldSucceed()
    {
        // Arrange
        var directoryPath = "test-directory";
        await _service.CreateDirectoryAsync(directoryPath);

        // Act
        var result = await _service.DeleteDirectoryAsync(directoryPath);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteDirectoryAsync_NonExistentDirectory_ShouldReturnError()
    {
        // Act
        var result = await _service.DeleteDirectoryAsync("non-existent-directory");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Directory not found", result.Message);
    }

    [Fact]
    public async Task ListFilesAsync_EmptyDirectory_ShouldReturnEmptyList()
    {
        // Arrange
        var directoryPath = "test-directory";
        await _service.CreateDirectoryAsync(directoryPath);

        // Act
        var result = await _service.ListFilesAsync(directoryPath);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task ListFilesAsync_DirectoryWithFiles_ShouldReturnFiles()
    {
        // Arrange
        var directoryPath = "test-directory";
        await _service.CreateDirectoryAsync(directoryPath);

        // Upload files to the specific directory
        var file1 = new FileUploadDto { FileName = "file1.txt", Content = Encoding.UTF8.GetBytes("content1"), ContentType = "text/plain", Directory = directoryPath };
        var file2 = new FileUploadDto { FileName = "file2.txt", Content = Encoding.UTF8.GetBytes("content2"), ContentType = "text/plain", Directory = directoryPath };

        await _service.UploadMultipleFilesAsync(new[] { file1, file2 });

        // Act
        var result = await _service.ListFilesAsync(directoryPath);

        // Assert
        Assert.True(result.Success);
        Assert.NotEmpty(result.Data!);
        Assert.Equal(2, result.Data!.Count());
    }

    [Fact]
    public async Task ValidateFileAccessAsync_ShouldReturnTrue()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("Test content");
        var fileName = "test-file.txt";
        var contentType = "text/plain";

        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        var userId = Guid.NewGuid();

        // Act
        var result = await _service.ValidateFileAccessAsync(uploadResult.Data!, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task SetFilePermissionsAsync_ShouldSucceed()
    {
        // Arrange
        var filePath = "test-file.txt";
        var permissions = FilePermissions.ReadOnly;

        // Act
        var result = await _service.SetFilePermissionsAsync(filePath, permissions);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task EncryptFileAsync_ValidData_ShouldSucceed()
    {
        // Arrange
        var fileData = Encoding.UTF8.GetBytes("Test content to encrypt");
        var encryptionKey = "test-encryption-key-32-chars-long";

        // Act
        var result = await _service.EncryptFileAsync(fileData, encryptionKey);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    [Fact]
    public async Task DecryptFileAsync_ValidEncryptedData_ShouldSucceed()
    {
        // Arrange
        var originalContent = "Test content to encrypt and decrypt";
        var fileData = Encoding.UTF8.GetBytes(originalContent);
        var encryptionKey = "test-encryption-key-32-chars-long";

        var encryptResult = await _service.EncryptFileAsync(fileData, encryptionKey);
        Assert.True(encryptResult.Success);

        // Create a temporary file with encrypted content
        var tempFile = Path.Combine(_testStoragePath, "encrypted.txt");
        var encryptedBytes = Convert.FromBase64String(encryptResult.Data!);
        await File.WriteAllBytesAsync(tempFile, encryptedBytes);

        // Act
        var result = await _service.DecryptFileAsync("encrypted.txt", encryptionKey);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(originalContent, Encoding.UTF8.GetString(result.Data!));
    }

    [Fact]
    public async Task UploadMultipleFilesAsync_ValidFiles_ShouldSucceed()
    {
        // Arrange
        var files = new[]
        {
            new FileUploadDto { FileName = "file1.txt", Content = Encoding.UTF8.GetBytes("content1"), ContentType = "text/plain" },
            new FileUploadDto { FileName = "file2.txt", Content = Encoding.UTF8.GetBytes("content2"), ContentType = "text/plain" },
            new FileUploadDto { FileName = "file3.txt", Content = Encoding.UTF8.GetBytes("content3"), ContentType = "text/plain" }
        };

        // Act
        var result = await _service.UploadMultipleFilesAsync(files);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.Data.Count());
    }

    [Fact]
    public async Task DeleteMultipleFilesAsync_ValidFiles_ShouldSucceed()
    {
        // Arrange
        var files = new[]
        {
            new FileUploadDto { FileName = "file1.txt", Content = Encoding.UTF8.GetBytes("content1"), ContentType = "text/plain" },
            new FileUploadDto { FileName = "file2.txt", Content = Encoding.UTF8.GetBytes("content2"), ContentType = "text/plain" }
        };

        var uploadResult = await _service.UploadMultipleFilesAsync(files);
        Assert.True(uploadResult.Success);

        // Act
        var result = await _service.DeleteMultipleFilesAsync(uploadResult.Data);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task GetStorageInfoAsync_ShouldReturnValidInfo()
    {
        // Act
        var result = await _service.GetStorageInfoAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.TotalSpace > 0);
        Assert.True(result.Data.AvailableSpace > 0);
        Assert.True(result.Data.FileCount >= 0);
        Assert.True(result.Data.DirectoryCount >= 0);
    }

    [Fact]
    public async Task CleanupExpiredFilesAsync_ShouldSucceed()
    {
        // Act
        var result = await _service.CleanupExpiredFilesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ArchiveOldFilesAsync_ShouldSucceed()
    {
        // Arrange
        var sourcePath = "source";
        var archivePath = "archive";
        var ageThreshold = TimeSpan.FromDays(30);

        // Act
        var result = await _service.ArchiveOldFilesAsync(sourcePath, archivePath, ageThreshold);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task IntegrationTest_FullFileLifecycle_ShouldWork()
    {
        // Arrange
        var originalContent = "Integration test content";
        var fileData = Encoding.UTF8.GetBytes(originalContent);
        var fileName = "integration-test.txt";
        var contentType = "text/plain";

        // 1. Upload file
        var uploadResult = await _service.UploadFileAsync(fileData, fileName, contentType);
        Assert.True(uploadResult.Success);

        // 2. Check file exists
        var existsResult = await _service.FileExistsAsync(uploadResult.Data!);
        Assert.True(existsResult.Success);
        Assert.True(existsResult.Data);

        // 3. Get file info
        var infoResult = await _service.GetFileInfoAsync(uploadResult.Data!);
        Assert.True(infoResult.Success);
        Assert.Equal(fileName, infoResult.Data!.FileName); // Should return original filename from metadata
        Assert.Equal(originalContent.Length, infoResult.Data!.FileSize);

        // 4. Download file
        var downloadResult = await _service.DownloadFileAsync(uploadResult.Data!);
        Assert.True(downloadResult.Success);
        Assert.Equal(originalContent, Encoding.UTF8.GetString(downloadResult.Data!));

        // 5. Delete file
        var deleteResult = await _service.DeleteFileAsync(uploadResult.Data!);
        Assert.True(deleteResult.Success);

        // 6. Verify file no longer exists
        var existsAfterDelete = await _service.FileExistsAsync(uploadResult.Data!);
        Assert.True(existsAfterDelete.Success);
        Assert.False(existsAfterDelete.Data);
    }
} 