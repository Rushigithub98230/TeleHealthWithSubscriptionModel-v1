using Microsoft.Extensions.Logging;
using SmartTelehealth.Application.DTOs;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;
using System.Linq.Expressions;

namespace SmartTelehealth.Infrastructure.Services;

public class DocumentTypeService : IDocumentTypeService
{
    private readonly IGenericRepository<DocumentType> _documentTypeRepository;
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly ILogger<DocumentTypeService> _logger;

    public DocumentTypeService(
        IGenericRepository<DocumentType> documentTypeRepository,
        IGenericRepository<Document> documentRepository,
        ILogger<DocumentTypeService> logger)
    {
        _documentTypeRepository = documentTypeRepository;
        _documentRepository = documentRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<DocumentTypeDto>> CreateDocumentTypeAsync(CreateDocumentTypeRequest request)
    {
        try
        {
            // Check if document type with same name already exists
            var existingType = await _documentTypeRepository.GetAllAsync(dt => 
                dt.Name.ToLower() == request.Name.ToLower() && !dt.IsDeleted);
            
            if (existingType.Any())
            {
                return ApiResponse<DocumentTypeDto>.ErrorResponse("Document type with this name already exists", 400);
            }

            var documentType = new DocumentType
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                IsSystemDefined = request.IsSystemDefined,
                AllowedExtensions = request.AllowedExtensions,
                MaxFileSizeBytes = request.MaxFileSizeBytes,
                RequireFileValidation = request.RequireFileValidation,
                Icon = request.Icon,
                Color = request.Color,
                DisplayOrder = request.DisplayOrder,
                CreatedById = request.CreatedById,
                CreatedAt = DateTime.UtcNow
            };

            await _documentTypeRepository.AddAsync(documentType);
            await _documentTypeRepository.SaveChangesAsync();

            return await GetDocumentTypeAsync(documentType.DocumentTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document type {Name}", request.Name);
            return ApiResponse<DocumentTypeDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<DocumentTypeDto>.ErrorResponse("Document type not found", 404);
            }

            // Get document count for this type
            var documentCount = await _documentRepository.GetAllAsync(d => 
                d.DocumentTypeId == documentTypeId && !d.IsDeleted);

            return ApiResponse<DocumentTypeDto>.SuccessResponse(MapToDto(documentType, documentCount.Count()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<DocumentTypeDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentTypeDto>> UpdateDocumentTypeAsync(Guid documentTypeId, UpdateDocumentTypeRequest request)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<DocumentTypeDto>.ErrorResponse("Document type not found", 404);
            }

            // Prevent modification of system-defined types
            if (documentType.IsSystemDefined)
            {
                return ApiResponse<DocumentTypeDto>.ErrorResponse("Cannot modify system-defined document types", 403);
            }

            // Check if name is being changed and if it conflicts with existing types
            if (!string.IsNullOrEmpty(request.Name) && request.Name.ToLower() != documentType.Name.ToLower())
            {
                var existingType = await _documentTypeRepository.GetAllAsync(dt => 
                    dt.Name.ToLower() == request.Name.ToLower() && 
                    dt.DocumentTypeId != documentTypeId && 
                    !dt.IsDeleted);
                
                if (existingType.Any())
                {
                    return ApiResponse<DocumentTypeDto>.ErrorResponse("Document type with this name already exists", 400);
                }
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name))
                documentType.Name = request.Name;
            
            if (request.Description != null)
                documentType.Description = request.Description;
            
            if (request.IsActive.HasValue)
                documentType.IsActive = request.IsActive.Value;
            
            if (request.AllowedExtensions != null)
                documentType.AllowedExtensions = request.AllowedExtensions;
            
            if (request.MaxFileSizeBytes.HasValue)
                documentType.MaxFileSizeBytes = request.MaxFileSizeBytes;
            
            if (request.RequireFileValidation.HasValue)
                documentType.RequireFileValidation = request.RequireFileValidation.Value;
            
            if (request.Icon != null)
                documentType.Icon = request.Icon;
            
            if (request.Color != null)
                documentType.Color = request.Color;
            
            if (request.DisplayOrder.HasValue)
                documentType.DisplayOrder = request.DisplayOrder.Value;
            
            documentType.UpdatedById = request.UpdatedById;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return await GetDocumentTypeAsync(documentTypeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<DocumentTypeDto>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            // Prevent deletion of system-defined types
            if (documentType.IsSystemDefined)
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete system-defined document types", 403);
            }

            // Check if there are documents using this type
            var documentsUsingType = await _documentRepository.GetAllAsync(d => 
                d.DocumentTypeId == documentTypeId && !d.IsDeleted);
            
            if (documentsUsingType.Any())
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete document type that has associated documents", 400);
            }

            await _documentTypeRepository.DeleteAsync(documentTypeId);
            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> SoftDeleteDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            // Prevent soft deletion of system-defined types
            if (documentType.IsSystemDefined)
            {
                return ApiResponse<bool>.ErrorResponse("Cannot delete system-defined document types", 403);
            }

            documentType.IsDeleted = true;
            documentType.DeletedById = userId;
            documentType.DeletedAt = DateTime.UtcNow;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetAllDocumentTypesAsync(bool? isActive = null)
    {
        try
        {
            Expression<Func<DocumentType, bool>> filter = dt => !dt.IsDeleted;
            
            if (isActive.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsActive == isActive.Value;
            }

            var documentTypes = await _documentTypeRepository.GetAllAsync(filter);
            var documentTypeDtos = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);

                documentTypeDtos.Add(MapToDto(docType, documentCount.Count()));
            }

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(documentTypeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all document types");
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> SearchDocumentTypesAsync(DocumentTypeSearchRequest request)
    {
        try
        {
            Expression<Func<DocumentType, bool>> filter = dt => !dt.IsDeleted;

            if (!string.IsNullOrEmpty(request.Name))
            {
                filter = dt => !dt.IsDeleted && dt.Name.ToLower().Contains(request.Name.ToLower());
            }

            if (request.IsActive.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsActive == request.IsActive.Value;
            }

            if (request.IsSystemDefined.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.IsSystemDefined == request.IsSystemDefined.Value;
            }

            if (request.CreatedFrom.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.CreatedAt >= request.CreatedFrom.Value;
            }

            if (request.CreatedTo.HasValue)
            {
                filter = dt => !dt.IsDeleted && dt.CreatedAt <= request.CreatedTo.Value;
            }

            var documentTypes = await _documentTypeRepository.GetAllAsync(filter);
            var documentTypeDtos = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);

                documentTypeDtos.Add(MapToDto(docType, documentCount.Count()));
            }

            // Apply pagination
            var pagedResults = documentTypeDtos
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(pagedResults);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching document types");
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetActiveDocumentTypesAsync()
    {
        return await GetAllDocumentTypesAsync(true);
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetSystemDocumentTypesAsync()
    {
        try
        {
            var systemTypes = await _documentTypeRepository.GetAllAsync(dt => 
                !dt.IsDeleted && dt.IsSystemDefined && dt.IsActive);
            
            var documentTypeDtos = new List<DocumentTypeDto>();
            foreach (var docType in systemTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);
                documentTypeDtos.Add(MapToDto(docType, documentCount.Count()));
            }

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(documentTypeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system document types");
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetAdminDocumentTypesAsync()
    {
        try
        {
            var adminTypes = await _documentTypeRepository.GetAllAsync(dt => 
                !dt.IsDeleted && !dt.IsSystemDefined && dt.IsActive);
            
            var documentTypeDtos = new List<DocumentTypeDto>();
            foreach (var docType in adminTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);
                documentTypeDtos.Add(MapToDto(docType, documentCount.Count()));
            }

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(documentTypeDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin document types");
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentTypeDto>> GetDocumentTypeWithStatsAsync(Guid documentTypeId)
    {
        return await GetDocumentTypeAsync(documentTypeId);
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetPopularDocumentTypesAsync(int limit = 10)
    {
        try
        {
            var documentTypes = await _documentTypeRepository.GetAllAsync(dt => !dt.IsDeleted && dt.IsActive);
            var documentTypeStats = new List<(DocumentType Type, int Count)>();

            foreach (var docType in documentTypes)
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);
                
                documentTypeStats.Add((docType, documentCount.Count()));
            }

            // Sort by document count and take top limit
            var popularTypes = documentTypeStats
                .OrderByDescending(x => x.Count)
                .Take(limit)
                .Select(x => MapToDto(x.Type, x.Count))
                .ToList();

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(popularTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular document types");
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ValidateDocumentTypeAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            return ApiResponse<bool>.SuccessResponse(documentType != null && !documentType.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> IsDocumentTypeActiveAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            return ApiResponse<bool>.SuccessResponse(documentType != null && !documentType.IsDeleted && documentType.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if document type is active {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<DocumentTypeValidationResponse>> ValidateFileAgainstDocumentTypeAsync(DocumentTypeValidationRequest request)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(request.DocumentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return ApiResponse<DocumentTypeValidationResponse>.ErrorResponse("Document type not found or inactive", 404);
            }

            var validationErrors = new List<string>();

            // Validate file extension
            if (documentType.RequireFileValidation && !string.IsNullOrEmpty(documentType.AllowedExtensions))
            {
                if (!documentType.IsValidFileExtension(request.FileName))
                {
                    validationErrors.Add($"File extension not allowed. Allowed extensions: {documentType.AllowedExtensions}");
                }
            }

            // Validate file size
            if (documentType.RequireFileValidation && documentType.MaxFileSizeBytes.HasValue)
            {
                if (!documentType.IsValidFileSize(request.FileSizeBytes))
                {
                    validationErrors.Add($"File size exceeds maximum allowed size of {documentType.GetMaxFileSizeDisplay()}");
                }
            }

            var response = new DocumentTypeValidationResponse
            {
                IsValid = validationErrors.Count == 0,
                ValidationErrors = validationErrors,
                MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
                AllowedExtensions = documentType.GetAllowedExtensionsList()
            };

            return ApiResponse<DocumentTypeValidationResponse>.SuccessResponse(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file against document type {DocumentTypeId}", request.DocumentTypeId);
            return ApiResponse<DocumentTypeValidationResponse>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ValidateFileExtensionAsync(Guid documentTypeId, string fileName)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found or inactive", 404);
            }

            var isValid = documentType.IsValidFileExtension(fileName);
            return ApiResponse<bool>.SuccessResponse(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file extension for document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ValidateFileSizeAsync(Guid documentTypeId, long fileSizeBytes)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted || !documentType.IsActive)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found or inactive", 404);
            }

            var isValid = documentType.IsValidFileSize(fileSizeBytes);
            return ApiResponse<bool>.SuccessResponse(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating file size for document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ActivateDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            documentType.IsActive = true;
            documentType.UpdatedById = userId;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeactivateDocumentTypeAsync(Guid documentTypeId, Guid userId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            documentType.IsActive = false;
            documentType.UpdatedById = userId;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> ActivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId)
    {
        try
        {
            foreach (var documentTypeId in documentTypeIds)
            {
                await ActivateDocumentTypeAsync(documentTypeId, userId);
            }

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating multiple document types");
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> DeactivateMultipleDocumentTypesAsync(List<Guid> documentTypeIds, Guid userId)
    {
        try
        {
            foreach (var documentTypeId in documentTypeIds)
            {
                await DeactivateDocumentTypeAsync(documentTypeId, userId);
            }

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating multiple document types");
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> IncrementUsageCountAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            documentType.UsageCount++;
            documentType.LastUsedAt = DateTime.UtcNow;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing usage count for document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> UpdateLastUsedAtAsync(Guid documentTypeId)
    {
        try
        {
            var documentType = await _documentTypeRepository.GetByIdAsync(documentTypeId);
            if (documentType == null || documentType.IsDeleted)
            {
                return ApiResponse<bool>.ErrorResponse("Document type not found", 404);
            }

            documentType.LastUsedAt = DateTime.UtcNow;
            documentType.UpdatedAt = DateTime.UtcNow;

            await _documentTypeRepository.SaveChangesAsync();

            return ApiResponse<bool>.SuccessResponse(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used at for document type {DocumentTypeId}", documentTypeId);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<bool>> CreateSystemDocumentTypeAsync(CreateDocumentTypeRequest request)
    {
        try
        {
            // Force system-defined flag
            request.IsSystemDefined = true;
            request.CreatedById = Guid.Empty; // System user

            var result = await CreateDocumentTypeAsync(request);
            return ApiResponse<bool>.SuccessResponse(result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating system document type {Name}", request.Name);
            return ApiResponse<bool>.ErrorResponse("Internal server error", 500);
        }
    }

    public async Task<ApiResponse<List<DocumentTypeDto>>> GetDocumentTypesForEntityAsync(string entityType, bool? isActive = null)
    {
        try
        {
            // Get document types based on entity type
            var documentTypes = await _documentTypeRepository.GetAllAsync(dt => 
                !dt.IsDeleted && dt.IsActive && 
                (isActive == null || dt.IsActive == isActive.Value));

            var filteredTypes = new List<DocumentTypeDto>();

            foreach (var docType in documentTypes.OrderBy(dt => dt.DisplayOrder).ThenBy(dt => dt.Name))
            {
                var documentCount = await _documentRepository.GetAllAsync(d => 
                    d.DocumentTypeId == docType.DocumentTypeId && !d.IsDeleted);

                filteredTypes.Add(MapToDto(docType, documentCount.Count()));
            }

            return ApiResponse<List<DocumentTypeDto>>.SuccessResponse(filteredTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document types for entity {EntityType}", entityType);
            return ApiResponse<List<DocumentTypeDto>>.ErrorResponse("Internal server error", 500);
        }
    }

    private DocumentTypeDto MapToDto(DocumentType documentType, int documentCount = 0)
    {
        return new DocumentTypeDto
        {
            DocumentTypeId = documentType.DocumentTypeId,
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
            CreatedById = documentType.CreatedById,
            CreatedAt = documentType.CreatedAt,
            UpdatedAt = documentType.UpdatedAt,
            DeletedAt = documentType.DeletedAt,
            DocumentCount = documentCount,
            MaxFileSizeDisplay = documentType.GetMaxFileSizeDisplay(),
            AllowedExtensionsList = documentType.GetAllowedExtensionsList()
        };
    }
} 