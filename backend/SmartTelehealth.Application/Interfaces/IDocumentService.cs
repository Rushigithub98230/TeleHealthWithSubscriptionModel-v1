using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Interfaces;

public interface IDocumentService
{
    // Core document operations
    Task<JsonModel> UploadDocumentAsync(UploadDocumentRequest request);
    Task<JsonModel> UploadUserDocumentAsync(UploadUserDocumentRequest request);
    Task<JsonModel> GetDocumentAsync(Guid documentId, int? userId = null);
    Task<JsonModel> GetDocumentWithContentAsync(Guid documentId, int? userId = null);
    Task<JsonModel> DeleteDocumentAsync(Guid documentId, int userId);
    Task<JsonModel> SoftDeleteDocumentAsync(Guid documentId, int userId);
    
    // Document search and listing
    Task<JsonModel> GetDocumentsByEntityAsync(string entityType, Guid entityId, int? userId = null);
    Task<JsonModel> GetDocumentsByReferenceTypeAsync(string entityType, Guid entityId, string referenceType, int? userId = null);
    Task<JsonModel> GetUserDocumentsAsync(int userId, string? referenceType = null);
    Task<JsonModel> SearchDocumentsAsync(DocumentSearchRequest request, int? userId = null);
    
    // Document references
    Task<JsonModel> AddDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, string? referenceType = null, int? createdById = null);
    Task<JsonModel> RemoveDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId);
    Task<JsonModel> GetDocumentReferencesAsync(Guid documentId);
    
    // Access control
    Task<JsonModel> ValidateDocumentAccessAsync(Guid documentId, int userId);
    Task<JsonModel> UpdateDocumentAccessAsync(Guid documentId, bool isPublic, int userId);
    
    // Batch operations
    Task<JsonModel> UploadMultipleDocumentsAsync(List<UploadDocumentRequest> requests);
    Task<JsonModel> DeleteMultipleDocumentsAsync(List<Guid> documentIds, int userId);
    
    // Document metadata
    Task<JsonModel> UpdateDocumentMetadataAsync(Guid documentId, string? description, bool? isPublic, int userId);
    Task<JsonModel> SetDocumentExpirationAsync(Guid documentId, DateTime? expiresAt, int userId);
} 