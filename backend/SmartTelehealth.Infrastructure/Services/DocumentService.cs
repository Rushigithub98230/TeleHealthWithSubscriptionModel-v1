using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Linq.Expressions;

namespace SmartTelehealth.Infrastructure.Services;

public class DocumentService : IDocumentService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly IGenericRepository<DocumentReference> _referenceRepository;
    private readonly IGenericRepository<DocumentType> _documentTypeRepository;
    private readonly ILogger<DocumentService> _logger;
    private readonly IDocumentTypeService _documentTypeService;

    public DocumentService(
        IFileStorageService fileStorageService,
        IGenericRepository<Document> documentRepository,
        IGenericRepository<DocumentReference> referenceRepository,
        IGenericRepository<DocumentType> documentTypeRepository,
        ILogger<DocumentService> logger,
        IDocumentTypeService documentTypeService)
    {
        _fileStorageService = fileStorageService;
        _documentRepository = documentRepository;
        _referenceRepository = referenceRepository;
        _documentTypeRepository = documentTypeRepository;
        _logger = logger;
        _documentTypeService = documentTypeService;
    }

    public async Task<ApiResponse<DocumentDto>> UploadDocumentAsync(UploadDocumentRequest request)
    {
        try
        {
            // 1. Validate document type
            var documentType = await _documentTypeRepository.GetByIdAsync(request.DocumentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Invalid or inactive document type", 400);
            }

            // 2. Validate file against document type rules
            var validationRequest = new DocumentTypeValidationRequest
            {
                DocumentTypeId = request.DocumentTypeId,
                FileName = request.FileName,
                FileSizeBytes = request.FileData.Length
            };

            var validationResult = await _documentTypeService.ValidateFileAgainstDocumentTypeAsync(validationRequest);
            if (!validationResult.Success)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Document type validation failed", 400);
            }

            if (!validationResult.Data.IsValid)
            {
                var errorMessage = string.Join("; ", validationResult.Data.ValidationErrors);
                return ApiResponse<DocumentDto>.ErrorResponse($"File validation failed: {errorMessage}", 400);
            }

            // 3. Upload file to storage
            var uploadResult = await _fileStorageService.UploadFileAsync(request.FileData, request.FileName, request.ContentType);
            if (!uploadResult.Success)
            {
                return ApiResponse<DocumentDto>.ErrorResponse(uploadResult.Message ?? "File upload failed", uploadResult.StatusCode);
            }

            // 4. Create document record
            var document = new Document
            {
                OriginalName = request.FileName,
                UniqueName = Path.GetFileName(uploadResult.Data!), // Assuming UniqueName is the uploaded file name
                FilePath = uploadResult.Data!,
                FolderPath = Path.GetDirectoryName(uploadResult.Data!) ?? "",
                ContentType = request.ContentType,
                FileSize = request.FileData.Length,
                DocumentTypeId = request.DocumentTypeId, // Storing DocumentTypeId
                DocumentCategory = request.EntityType.ToLower(), // For backward compatibility
                Description = request.Description,
                IsEncrypted = request.IsEncrypted,
                IsPublic = request.IsPublic,
                CreatedBy = request.CreatedById ?? 0, // Ensure CreatedBy is set
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                IsDeleted = false
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            // 5. Create document reference
            var reference = new DocumentReference
            {
                DocumentId = document.Id,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ReferenceType = request.ReferenceType,
                IsPublic = request.IsPublic,
                ExpiresAt = request.ExpiresAt,
                CreatedBy = request.CreatedById ?? 0,
                CreatedDate = DateTime.UtcNow
            };

            await _referenceRepository.AddAsync(reference);
            await _referenceRepository.SaveChangesAsync();

            // 6. Update document type usage statistics
            await _documentTypeService.IncrementUsageCountAsync(request.DocumentTypeId);

            // 7. Return document DTO with document type information
            return ApiResponse<DocumentDto>.SuccessResponse(new DocumentDto
            {
                DocumentId = document.Id,
                OriginalName = document.OriginalName,
                UniqueName = document.UniqueName,
                FilePath = document.FilePath,
                FolderPath = document.FolderPath,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                Description = document.Description,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = new DocumentTypeDto // Include DocumentTypeDto
                {
                    DocumentTypeId = documentType.Id,
                    Name = documentType.Name,
                    Description = documentType.Description,
                    IsSystemDefined = documentType.IsSystemDefined,
                    IsActive = documentType.IsActive,
                    IsDeleted = documentType.IsDeleted,
                    AllowedExtensions = documentType.AllowedExtensions,
                    MaxFileSizeBytes = documentType.MaxFileSizeBytes,
                    RequireFileValidation = documentType.RequireFileValidation,
                    Icon = documentType.Icon,
                    Color = documentType.Color,
                    DisplayOrder = documentType.DisplayOrder,
                    UsageCount = documentType.UsageCount,
                    LastUsedAt = documentType.LastUsedAt,
                                         CreatedById = documentType.CreatedBy,
                     CreatedAt = documentType.CreatedDate ?? DateTime.UtcNow,
                    UpdatedAt = documentType.UpdatedDate,
                    DeletedAt = documentType.DeletedDate,
                    MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
                    AllowedExtensionsList = documentType.GetAllowedExtensionsList()
                },
                DocumentCategory = document.DocumentCategory,
                IsEncrypted = document.IsEncrypted,
                IsPublic = document.IsPublic,
                CreatedById = document.CreatedBy,
                CreatedAt = document.CreatedDate ?? DateTime.UtcNow,
                DeletedAt = document.DeletedDate,
                IsActive = document.IsActive,
                IsDeleted = document.IsDeleted,
                DownloadUrl = (await _fileStorageService.GetFileUrlAsync(document.FilePath)).Data,
                SecureUrl = (await _fileStorageService.GetSecureUrlAsync(document.FilePath)).Data,
                References = new List<DocumentReferenceDto> { new DocumentReferenceDto { DocumentId = reference.DocumentId, EntityType = reference.EntityType, EntityId = reference.EntityId, ReferenceType = reference.ReferenceType } }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document {FileName}", request.FileName);
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentDto>> GetDocumentAsync(Guid documentId, int? userId = null)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Document not found", 404);
            }

            // 2. Check access permissions
            if (!document.IsPublic && userId.HasValue)
            {
                var hasAccess = await ValidateDocumentAccessAsync(documentId, userId.Value);
                if (!hasAccess.Success || !hasAccess.Data)
                {
                    return ApiResponse<DocumentDto>.ErrorResponse("Access denied", 403);
                }
            }

            // 3. Get document type
            var documentType = await _documentTypeRepository.GetByIdAsync(document.DocumentTypeId);
            var documentTypeDto = documentType != null ? new DocumentTypeDto
            {
                DocumentTypeId = documentType.Id,
                Name = documentType.Name,
                Description = documentType.Description,
                IsActive = documentType.IsActive,
                IsDeleted = documentType.IsDeleted,
                CreatedById = documentType.CreatedBy,
                CreatedAt = documentType.CreatedDate ?? DateTime.UtcNow,
                UpdatedAt = documentType.UpdatedDate,
                DeletedAt = documentType.DeletedDate
            } : null;

            // 4. Get references
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);

            // 5. Return document DTO
            return ApiResponse<DocumentDto>.SuccessResponse(new DocumentDto
            {
                DocumentId = document.Id,
                OriginalName = document.OriginalName,
                UniqueName = document.UniqueName,
                FilePath = document.FilePath,
                FolderPath = document.FolderPath,
                ContentType = document.ContentType,
                FileSize = document.FileSize,
                Description = document.Description,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = documentTypeDto,
                DocumentCategory = document.DocumentCategory,
                IsEncrypted = document.IsEncrypted,
                IsPublic = document.IsPublic,
                CreatedById = document.CreatedBy,
                CreatedAt = document.CreatedDate ?? DateTime.UtcNow,
                DeletedAt = document.DeletedDate,
                IsActive = document.IsActive,
                IsDeleted = document.IsDeleted,
                References = references.Select(r => new DocumentReferenceDto
                {
                    Id = r.Id,
                    DocumentId = r.DocumentId,
                    EntityType = r.EntityType,
                    EntityId = r.EntityId,
                    ReferenceType = r.ReferenceType,
                    Description = r.Description,
                    IsPublic = r.IsPublic,
                    ExpiresAt = r.ExpiresAt,
                    CreatedById = r.CreatedBy ?? 0,
                    CreatedAt = r.CreatedDate ?? DateTime.UtcNow
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {DocumentId}", documentId);
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentDto>> GetDocumentWithContentAsync(Guid documentId, int? userId = null)
    {
        try
        {
            // 1. Get document metadata
            var documentResult = await GetDocumentAsync(documentId, userId);
            if (!documentResult.Success)
            {
                return documentResult;
            }

            // 2. Get file content
            var fileBytes = await _fileStorageService.DownloadFileAsync(documentResult.Data!.FilePath);
            if (!fileBytes.Success)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Failed to retrieve file content", 500);
            }

            // 3. Add content to DTO
            documentResult.Data!.Content = fileBytes.Data;
            return documentResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document content {DocumentId}", documentId);
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteDocumentAsync(Guid documentId, int userId)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document not found", 404);
            }

            // 2. Check access permissions
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<bool>.ErrorResponse("Access denied", 403);
            }

            // 3. Delete from storage
            var deleteResult = await _fileStorageService.DeleteFileAsync(document.FilePath);
            if (!deleteResult.Success)
            {
                _logger.LogWarning("Failed to delete file from storage: {FilePath}", document.FilePath);
            }

            // 4. Delete references
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId);
            foreach (var reference in references)
            {
                reference.IsDeleted = true;
                reference.UpdatedDate = DateTime.UtcNow;
            }
            await _referenceRepository.SaveChangesAsync();

            // 5. Delete document
            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> SoftDeleteDocumentAsync(Guid documentId, int userId)
    {
        try
        {
            // 1. Get document
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document not found", 404);
            }

            // 2. Check access permissions
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<bool>.ErrorResponse("Access denied", 403);
            }

            // 3. Soft delete document
            document.IsDeleted = true;
            document.DeletedBy = userId;
            document.DeletedDate = DateTime.UtcNow;
            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetDocumentsByEntityAsync(string entityType, Guid entityId, int? userId = null)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                !r.IsDeleted);

            var documents = new List<DocumentDto>();
            foreach (var reference in references)
            {
                var documentResult = await GetDocumentAsync(reference.DocumentId, userId);
                if (documentResult.Success)
                {
                    documents.Add(documentResult.Data!);
                }
            }

            return ApiResponse<List<DocumentDto>>.SuccessResponse(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for entity {EntityType} {EntityId}", entityType, entityId);
            return ApiResponse<List<DocumentDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentDto>>> GetDocumentsByReferenceTypeAsync(string entityType, Guid entityId, string referenceType, int? userId = null)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                r.ReferenceType == referenceType && 
                !r.IsDeleted);

            var documents = new List<DocumentDto>();
            foreach (var reference in references)
            {
                var documentResult = await GetDocumentAsync(reference.DocumentId, userId);
                if (documentResult.Success)
                {
                    documents.Add(documentResult.Data!);
                }
            }

            return ApiResponse<List<DocumentDto>>.SuccessResponse(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for entity {EntityType} {EntityId} with reference type {ReferenceType}", entityType, entityId, referenceType);
            return ApiResponse<List<DocumentDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentDto>>> SearchDocumentsAsync(DocumentSearchRequest request, int? userId = null)
    {
        try
        {
            // Build search expression
            Expression<Func<Document, bool>> searchExpression = d => !d.IsDeleted;

            if (!string.IsNullOrEmpty(request.EntityType))
            {
                searchExpression = d => d.DocumentCategory == request.EntityType.ToLower() && !d.IsDeleted;
            }

            if (request.DocumentTypeId.HasValue)
            {
                searchExpression = d => d.DocumentTypeId == request.DocumentTypeId.Value && !d.IsDeleted;
            }

            if (!string.IsNullOrEmpty(request.DocumentTypeName))
            {
                // This would require a join with DocumentType table
                // For now, we'll filter by document type name in memory
                var documentTypes = await _documentTypeRepository.FindAsync(dt => 
                    dt.Name.ToLower().Contains(request.DocumentTypeName.ToLower()) && !dt.IsDeleted);
                var documentTypeIds = documentTypes.Select(dt => dt.Id).ToList();
                searchExpression = d => documentTypeIds.Contains(d.DocumentTypeId) && !d.IsDeleted;
            }

            if (request.IsPublic.HasValue)
            {
                searchExpression = d => d.IsPublic == request.IsPublic.Value && !d.IsDeleted;
            }

            if (request.CreatedFrom.HasValue)
            {
                searchExpression = d => d.CreatedDate >= request.CreatedFrom.Value && !d.IsDeleted;
            }

            if (request.CreatedTo.HasValue)
            {
                searchExpression = d => d.CreatedDate <= request.CreatedTo.Value && !d.IsDeleted;
            }

            var documents = await _documentRepository.FindAsync(searchExpression);
            var documentDtos = new List<DocumentDto>();

            foreach (var document in documents)
            {
                var documentResult = await GetDocumentAsync(document.Id, userId);
                if (documentResult.Success)
                {
                    documentDtos.Add(documentResult.Data!);
                }
            }

            // Apply pagination
            var pagedDocuments = documentDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return ApiResponse<List<DocumentDto>>.SuccessResponse(pagedDocuments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return ApiResponse<List<DocumentDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentReferenceDto>> AddDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId, string? referenceType = null, int? createdById = null)
    {
        try
        {
            var reference = new DocumentReference
            {
                DocumentId = documentId,
                EntityType = entityType,
                EntityId = entityId,
                ReferenceType = referenceType,
                CreatedBy = createdById ?? 0,
                CreatedDate = DateTime.UtcNow
            };

            await _referenceRepository.AddAsync(reference);
            await _referenceRepository.SaveChangesAsync();

            return ApiResponse<DocumentReferenceDto>.SuccessResponse(new DocumentReferenceDto
            {
                Id = reference.Id,
                DocumentId = reference.DocumentId,
                EntityType = reference.EntityType,
                EntityId = reference.EntityId,
                ReferenceType = reference.ReferenceType,
                Description = reference.Description,
                IsPublic = reference.IsPublic,
                ExpiresAt = reference.ExpiresAt,
                CreatedById = reference.CreatedBy ?? 0,
                CreatedAt = reference.CreatedDate ?? DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document reference for document {DocumentId}", documentId);
            return ApiResponse<DocumentReferenceDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> RemoveDocumentReferenceAsync(Guid documentId, string entityType, Guid entityId)
    {
        try
        {
            var reference = await _referenceRepository.FindAsync(r => 
                r.DocumentId == documentId && 
                r.EntityType == entityType && 
                r.EntityId == entityId && 
                !r.IsDeleted);

            var refToDelete = reference.FirstOrDefault();
            if (refToDelete == null)
            {
                return ApiResponse<bool>.ErrorResponse("Document reference not found", 404);
            }

            refToDelete.IsDeleted = true;
            refToDelete.UpdatedDate = DateTime.UtcNow;

            await _referenceRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing document reference for document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentReferenceDto>>> GetDocumentReferencesAsync(Guid documentId)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);

            var referenceDtos = references.Select(r => new DocumentReferenceDto
            {
                Id = r.Id,
                DocumentId = r.DocumentId,
                EntityType = r.EntityType,
                EntityId = r.EntityId,
                ReferenceType = r.ReferenceType,
                Description = r.Description,
                IsPublic = r.IsPublic,
                ExpiresAt = r.ExpiresAt,
                CreatedById = r.CreatedBy ?? 0,
                CreatedAt = r.CreatedDate ?? DateTime.UtcNow
            }).ToList();

            return ApiResponse<List<DocumentReferenceDto>>.SuccessResponse(referenceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document references for document {DocumentId}", documentId);
            return ApiResponse<List<DocumentReferenceDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ValidateDocumentAccessAsync(Guid documentId, int userId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<bool>.SuccessResponse(false);
            }

            // Public documents
            if (document.IsPublic)
            {
                return ApiResponse<bool>.SuccessResponse(true);
            }

            // Check if user created the document
            if (document.CreatedBy.HasValue && document.CreatedBy.Value == userId)
            {
                return ApiResponse<bool>.SuccessResponse(true);
            }

            // Check references for access
            var references = await _referenceRepository.FindAsync(r => 
                r.DocumentId == documentId && 
                !r.IsDeleted);

            foreach (var reference in references)
            {
                if (await ValidateEntityAccessAsync(reference.EntityType, reference.EntityId, userId))
                {
                    return ApiResponse<bool>.SuccessResponse(true);
                }
            }

            return ApiResponse<bool>.SuccessResponse(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document access for document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> UpdateDocumentAccessAsync(Guid documentId, bool isPublic, int userId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document not found", 404);
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<bool>.ErrorResponse("Access denied", 403);
            }

            document.IsPublic = isPublic;
            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document access for document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentDto>>> UploadMultipleDocumentsAsync(List<UploadDocumentRequest> requests)
    {
        try
        {
            var results = new List<DocumentDto>();

            foreach (var request in requests)
            {
                var result = await UploadDocumentAsync(request);
                if (result.Success)
                {
                    results.Add(result.Data!);
                }
            }

            return ApiResponse<List<DocumentDto>>.SuccessResponse(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading multiple documents");
            return ApiResponse<List<DocumentDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteMultipleDocumentsAsync(List<Guid> documentIds, int userId)
    {
        try
        {
            foreach (var documentId in documentIds)
            {
                await DeleteDocumentAsync(documentId, userId);
            }

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting multiple documents");
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentDto>> UpdateDocumentMetadataAsync(Guid documentId, string? description, bool? isPublic, int userId)
    {
        try
        {
            var document = await _documentRepository.GetByIdAsync(documentId);
            if (document == null || document.IsDeleted)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Document not found", 404);
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<DocumentDto>.ErrorResponse("Access denied", 403);
            }

            if (description != null)
            {
                document.Description = description;
            }

            if (isPublic.HasValue)
            {
                document.IsPublic = isPublic.Value;
            }

            document.UpdatedDate = DateTime.UtcNow;

            await _documentRepository.SaveChangesAsync();

            return await GetDocumentAsync(documentId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document metadata for document {DocumentId}", documentId);
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> SetDocumentExpirationAsync(Guid documentId, DateTime? expiresAt, int userId)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => r.DocumentId == documentId && !r.IsDeleted);
            var reference = references.FirstOrDefault();

            if (reference == null)
            {
                return ApiResponse<bool>.ErrorResponse("Document reference not found", 404);
            }

            // Check if user has permission to update
            var hasAccess = await ValidateDocumentAccessAsync(documentId, userId);
            if (!hasAccess.Success || !hasAccess.Data)
            {
                return ApiResponse<bool>.ErrorResponse("Access denied", 403);
            }

            reference.ExpiresAt = expiresAt;
            reference.UpdatedDate = DateTime.UtcNow;

            await _referenceRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting document expiration for document {DocumentId}", documentId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentDto>> UploadUserDocumentAsync(UploadUserDocumentRequest request)
    {
        try
        {
            // Convert UploadUserDocumentRequest to UploadDocumentRequest
            var uploadRequest = new UploadDocumentRequest
            {
                FileData = request.FileData,
                FileName = request.FileName,
                ContentType = request.ContentType,
                DocumentTypeId = request.DocumentTypeId,
                EntityType = "User",
                EntityId = Guid.Empty, // We'll handle this in the reference creation
                Description = request.Description,
                IsEncrypted = request.IsEncrypted,
                IsPublic = request.IsPublic,
                CreatedById = request.CreatedById
            };

            return await UploadDocumentAsync(uploadRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading user document");
            return ApiResponse<DocumentDto>.ErrorResponse("Internal server error", 500);
        }
    }













    public async Task<ApiResponse<List<DocumentDto>>> GetUserDocumentsAsync(int userId, string? referenceType = null)
    {
        try
        {
            var references = await _referenceRepository.FindAsync(r => 
                r.CreatedBy == userId && 
                !r.IsDeleted);

            if (!string.IsNullOrEmpty(referenceType))
            {
                references = references.Where(r => r.ReferenceType == referenceType);
            }

            var documentIds = references.Select(r => r.DocumentId).ToList();
            var documents = new List<DocumentDto>();

            foreach (var documentId in documentIds)
            {
                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document != null && !document.IsDeleted)
                {
                    documents.Add(MapToDocumentDto(document));
                }
            }

            return ApiResponse<List<DocumentDto>>.SuccessResponse(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user documents for user {UserId}", userId);
            return ApiResponse<List<DocumentDto>>.ErrorResponse("Internal server error", 500);
        }
    }











    private async Task<bool> ValidateEntityAccessAsync(string entityType, Guid entityId, int userId)
    {
        // This is a simplified implementation
        // In a real application, you would implement specific access control logic for each entity type
        switch (entityType)
        {
            case "Appointment":
                // Implement appointment access validation
                return true; // Placeholder
            case "User":
                // For User entity, we need to handle the case where EntityId is a Guid but userId is int
                // This is a temporary fix - in production, you might want to store user IDs consistently
                return entityId.ToString() == userId.ToString(); // Users can only access their own documents
            case "ChatRoom":
                // Implement chat room access validation
                return true; // Placeholder
            default:
                return false;
        }
    }

    private DocumentDto MapToDocumentDto(Document document)
    {
        return new DocumentDto
        {
            DocumentId = document.Id,
            OriginalName = document.OriginalName,
            UniqueName = document.UniqueName,
            FilePath = document.FilePath,
            FolderPath = document.FolderPath,
            ContentType = document.ContentType,
            FileSize = document.FileSize,
            DocumentTypeId = document.DocumentTypeId,
            DocumentCategory = document.DocumentCategory,
            Description = document.Description,
            IsEncrypted = document.IsEncrypted,
            IsPublic = document.IsPublic,
            CreatedById = document.CreatedBy ?? 0,
            CreatedAt = document.CreatedDate ?? DateTime.UtcNow,
            DeletedAt = document.DeletedDate,
            IsActive = document.IsActive,
            IsDeleted = document.IsDeleted
        };
    }

    private DocumentReferenceDto MapToDocumentReferenceDto(DocumentReference reference)
    {
        return new DocumentReferenceDto
        {
            Id = reference.Id,
            DocumentId = reference.DocumentId,
            EntityType = reference.EntityType,
            EntityId = reference.EntityId,
            ReferenceType = reference.ReferenceType,
            CreatedById = reference.CreatedBy ?? 0,
            CreatedAt = reference.CreatedDate ?? DateTime.UtcNow,
            ExpiresAt = reference.ExpiresAt
        };
    }
} 