using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FileStorageController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileStorageController> _logger;

    public FileStorageController(IFileStorageService fileStorageService, ILogger<FileStorageController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Upload a single file
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<JsonModel> UploadFile(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(JsonModel.ErrorResponse("No file provided", 400));
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var result = await _fileStorageService.UploadFileAsync(fileData, file.FileName, file.ContentType);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file?.FileName);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Upload multiple files
    /// </summary>
    [HttpPost("upload-multiple")]
    public async Task<ActionResult<JsonModel>> UploadMultipleFiles(IFormFileCollection files)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return BadRequest(JsonModel>.ErrorResponse("No files provided", 400));
            }

            var fileUploads = new List<FileUploadDto>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    using var memoryStream = new MemoryStream();
                    await file.CopyToAsync(memoryStream);
                    
                    fileUploads.Add(new FileUploadDto
                    {
                        Content = memoryStream.ToArray(),
                        FileName = file.FileName,
                        ContentType = file.ContentType
                    });
                }
            }

            var result = await _fileStorageService.UploadMultipleFilesAsync(fileUploads);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple files");
            return StatusCode(500, JsonModel>.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Download a file
    /// </summary>
    [HttpGet("download/{filePath}")]
    public async Task<ActionResult> DownloadFile(string filePath)
    {
        try
        {
            var result = await _fileStorageService.DownloadFileAsync(filePath);
            
            if (!result.Success)
            {
                return NotFound(JsonModel.ErrorResponse("File not found", 404));
            }

            var fileInfo = await _fileStorageService.GetFileInfoAsync(filePath);
            if (!fileInfo.Success)
            {
                return NotFound(JsonModel.ErrorResponse("File info not found", 404));
            }

            return File(result.Data, fileInfo.Data.ContentType, fileInfo.Data.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FilePath}", filePath);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get file information
    /// </summary>
    [HttpGet("info/{filePath}")]
    public async Task<ActionResult<JsonModel> GetFileInfo(string filePath)
    {
        try
        {
            var result = await _fileStorageService.GetFileInfoAsync(filePath);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file info {FilePath}", filePath);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get secure URL for file access
    /// </summary>
    [HttpGet("secure-url/{filePath}")]
    public async Task<ActionResult<JsonModel> GetSecureUrl(string filePath, [FromQuery] int? expirationHours = 1)
    {
        try
        {
            var expiration = expirationHours.HasValue ? TimeSpan.FromHours(expirationHours.Value) : TimeSpan.FromHours(1);
            var result = await _fileStorageService.GetSecureUrlAsync(filePath, expiration);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting secure URL {FilePath}", filePath);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete("{filePath}")]
    public async Task<ActionResult<JsonModel> DeleteFile(string filePath)
    {
        try
        {
            var result = await _fileStorageService.DeleteFileAsync(filePath);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FilePath}", filePath);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Delete multiple files
    /// </summary>
    [HttpDelete("multiple")]
    public async Task<ActionResult<JsonModel> DeleteMultipleFiles([FromBody] List<string> filePaths)
    {
        try
        {
            if (filePaths == null || !filePaths.Any())
            {
                return BadRequest(JsonModel.ErrorResponse("No file paths provided", 400));
            }

            var result = await _fileStorageService.DeleteMultipleFilesAsync(filePaths);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple files");
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// List files in a directory
    /// </summary>
    [HttpGet("list/{directoryPath}")]
    public async Task<ActionResult<JsonModel>> ListFiles(string directoryPath, [FromQuery] string? searchPattern = null)
    {
        try
        {
            var result = await _fileStorageService.ListFilesAsync(directoryPath, searchPattern);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return NotFound(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in directory {DirectoryPath}", directoryPath);
            return StatusCode(500, JsonModel>.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Get storage information
    /// </summary>
    [HttpGet("storage-info")]
    public async Task<ActionResult<JsonModel> GetStorageInfo()
    {
        try
        {
            var result = await _fileStorageService.GetStorageInfoAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage info");
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Cleanup expired files
    /// </summary>
    [HttpPost("cleanup")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel> CleanupExpiredFiles()
    {
        try
        {
            var result = await _fileStorageService.CleanupExpiredFilesAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired files");
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Archive old files
    /// </summary>
    [HttpPost("archive")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<JsonModel> ArchiveOldFiles([FromBody] ArchiveFilesRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.SourcePath) || string.IsNullOrEmpty(request.ArchivePath))
            {
                return BadRequest(JsonModel.ErrorResponse("Source path and archive path are required", 400));
            }

            var ageThreshold = TimeSpan.FromDays(request.AgeThresholdDays);
            var result = await _fileStorageService.ArchiveOldFilesAsync(request.SourcePath, request.ArchivePath, ageThreshold);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving old files");
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Encrypt a file
    /// </summary>
    [HttpPost("encrypt")]
    public async Task<ActionResult<JsonModel> EncryptFile(IFormFile file, [FromQuery] string encryptionKey)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(JsonModel.ErrorResponse("No file provided", 400));
            }

            if (string.IsNullOrEmpty(encryptionKey))
            {
                return BadRequest(JsonModel.ErrorResponse("Encryption key is required", 400));
            }

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            var fileData = memoryStream.ToArray();

            var result = await _fileStorageService.EncryptFileAsync(fileData, encryptionKey);
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file {FileName}", file?.FileName);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }

    /// <summary>
    /// Decrypt a file
    /// </summary>
    [HttpPost("decrypt/{encryptedFilePath}")]
    public async Task<ActionResult> DecryptFile(string encryptedFilePath, [FromQuery] string encryptionKey)
    {
        try
        {
            if (string.IsNullOrEmpty(encryptionKey))
            {
                return BadRequest(JsonModel.ErrorResponse("Encryption key is required", 400));
            }

            var result = await _fileStorageService.DecryptFileAsync(encryptedFilePath, encryptionKey);
            
            if (!result.Success)
            {
                return NotFound(JsonModel.ErrorResponse("Encrypted file not found", 404));
            }

            return File(result.Data, "application/octet-stream", $"decrypted_{Path.GetFileName(encryptedFilePath)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrypting file {FilePath}", encryptedFilePath);
            return StatusCode(500, JsonModel.ErrorResponse("Internal server error", 500));
        }
    }
}

public class ArchiveFilesRequest
{
    public string SourcePath { get; set; } = string.Empty;
    public string ArchivePath { get; set; } = string.Empty;
    public int AgeThresholdDays { get; set; } = 30;
} 