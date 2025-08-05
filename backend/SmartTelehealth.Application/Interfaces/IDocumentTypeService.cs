using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IDocumentTypeService
{
    // Core document type operations
    Task<ApiResponse<DocumentTypeDto>> CreateDocumentTypeAsync(CreateDocumentTypeRequest request);
    Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeAsync(Guid documentTypeId);
    Task<ApiResponse<DocumentTypeDto>> UpdateDocumentTypeAsync(Guid documentTypeId, UpdateDocumentTypeRequest request);
    Task<ApiResponse<bool>> DeleteDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<ApiResponse<bool>> SoftDeleteDocumentTypeAsync(Guid documentTypeId, Guid userId);
    
    // Document type listing and search
    Task<ApiResponse<List<DocumentTypeDto>>> GetAllDocumentTypesAsync(bool? isActive = null);
    Task<ApiResponse<List<DocumentTypeDto>>> SearchDocumentTypesAsync(DocumentTypeSearchRequest request);
    Task<ApiResponse<List<DocumentTypeDto>>> GetActiveDocumentTypesAsync();
    Task<ApiResponse<List<DocumentTypeDto>>> GetSystemDocumentTypesAsync();
    Task<ApiResponse<List<DocumentTypeDto>>> GetAdminDocumentTypesAsync();
    
    // Document type statistics
    Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeWithStatsAsync(Guid documentTypeId);
    Task<ApiResponse<List<DocumentTypeDto>>> GetPopularDocumentTypesAsync(int limit = 10);
    
    // Document type validation
    Task<ApiResponse<bool>> ValidateDocumentTypeAsync(Guid documentTypeId);
    Task<ApiResponse<bool>> IsDocumentTypeActiveAsync(Guid documentTypeId);
    Task<ApiResponse<DocumentTypeValidationResponse>> ValidateFileAgainstDocumentTypeAsync(DocumentTypeValidationRequest request);
    Task<ApiResponse<bool>> ValidateFileExtensionAsync(Guid documentTypeId, string fileName);
    Task<ApiResponse<bool>> ValidateFileSizeAsync(Guid documentTypeId, long fileSizeBytes);
    
    // Bulk operations
    Task<ApiResponse<bool>> ActivateDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<ApiResponse<bool>> DeactivateDocumentTypeAsync(Guid documentTypeId, Guid userId);
    Task<ApiResponse<bool>> ActivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId);
    Task<ApiResponse<bool>> DeactivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId);
    
    // Usage tracking
    Task<ApiResponse<bool>> IncrementUsageCountAsync(Guid documentTypeId);
    Task<ApiResponse<bool>> UpdateLastUsedAtAsync(Guid documentTypeId);
    
    // System document type management
    Task<ApiResponse<bool>> CreateSystemDocumentTypeAsync(CreateDocumentTypeRequest request);
    Task<ApiResponse<List<DocumentTypeDto>>> GetDocumentTypesForEntityAsync(string entityType, bool? isActive = null);
} 