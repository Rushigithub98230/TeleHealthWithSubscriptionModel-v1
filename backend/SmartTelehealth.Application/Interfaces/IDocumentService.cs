using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IDocumentService
{
    // Core document operations
    Task<ApiResponse<DocumentDto>> UploadDocumentAsync(UploadDocumentRequest request);
    Task<ApiResponse<DocumentDto>> GetDocumentAsync(Guid documentId, Guid? userId = null);
    Task<ApiResponse<DocumentDto>> GetDocumentWithContentAsync(Guid documentId, Guid? userId = null);
    Task<ApiResponse<bool>> DeleteDocumentAsync(Guid documentId, Guid userId);
    Task<ApiResponse<bool>> SoftDeleteDocumentAsync(Guid documentId, Guid userId);
    
    // Document search and listing
    Task<ApiResponse<List<DocumentDto>>> GetDocumentsByEntityAsync(string entityType, Guid entityId, Guid? userId = null);
    Task<ApiResponse<List<DocumentDto>>> GetDocumentsByReferenceTypeAsync(string entityType, Guid entityId, string referenceType, Guid? userId = null);
    Task<ApiResponse<List<DocumentDto>>> SearchDocumentsAsync(DocumentSearchRequest request, Guid? userId = null);
    
    // Document references
    Task<ApiResponse<DocumentReferenceDto>> AddDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, string? referenceType = null, Guid? createdById = null);
    Task<ApiResponse<bool>> RemoveDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId);
    Task<ApiResponse<List<DocumentReferenceDto>>> GetDocumentReferencesAsync(Guid documentId);
    
    // Access control
    Task<ApiResponse<bool>> ValidateDocumentAccessAsync(Guid documentId, Guid userId);
    Task<ApiResponse<bool>> UpdateDocumentAccessAsync(Guid documentId, bool isPublic, Guid userId);
    
    // Batch operations
    Task<ApiResponse<List<DocumentDto>>> UploadMultipleDocumentsAsync(List<UploadDocumentRequest> requests);
    Task<ApiResponse<bool>> DeleteMultipleDocumentsAsync(List<Guid> documentIds, Guid userId);
    
    // Document metadata
    Task<ApiResponse<DocumentDto>> UpdateDocumentMetadataAsync(Guid documentId, string? description, bool? isPublic, Guid userId);
    Task<ApiResponse<bool>> SetDocumentExpirationAsync(Guid documentId, DateTime? expiresAt, Guid userId);
} 