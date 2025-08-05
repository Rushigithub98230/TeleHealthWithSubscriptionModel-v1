using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class DocumentType : BaseEntity
{
    // Primary identification
    public Guid DocumentTypeId { get; set; } = Guid.NewGuid();
    
    // Basic information
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Prescription", "Blood Report", "License", "Invoice"
    
    [MaxLength(500)]
    public string? Description { get; set; } // Optional additional context
    
    // System vs Admin defined
    public bool IsSystemDefined { get; set; } = false; // Distinguishes system types from admin-created types
    public bool IsActive { get; set; } = true; // Whether the type is available for selection
    public bool IsDeleted { get; set; } = false;
    
    // File validation rules
    [MaxLength(1000)]
    public string? AllowedExtensions { get; set; } // Comma-separated: ".pdf,.jpg,.png"
    public long? MaxFileSizeBytes { get; set; } // Maximum file size in bytes (e.g., 5MB = 5242880)
    public bool RequireFileValidation { get; set; } = true; // Whether to validate files against rules
    
    // UI/UX properties
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    [MaxLength(20)]
    public string? Color { get; set; } // For UI display (hex color)
    public int DisplayOrder { get; set; } = 0; // For sorting in UI
    
    // Usage tracking
    public int UsageCount { get; set; } = 0; // Number of documents using this type
    public DateTime? LastUsedAt { get; set; } // When this type was last used
    
    // Audit fields
    public Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public Guid? UpdatedById { get; set; }
    public virtual User? UpdatedBy { get; set; }
    
    public Guid? DeletedById { get; set; }
    public virtual User? DeletedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
    
    // Helper methods for file validation
    public bool IsValidFileExtension(string fileName)
    {
        if (string.IsNullOrEmpty(AllowedExtensions) || !RequireFileValidation)
            return true;
            
        var fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension))
            return false;
            
        var allowedExtensions = AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim().ToLowerInvariant())
            .ToList();
            
        return allowedExtensions.Contains(fileExtension);
    }
    
    public bool IsValidFileSize(long fileSizeBytes)
    {
        if (!MaxFileSizeBytes.HasValue || !RequireFileValidation)
            return true;
            
        return fileSizeBytes <= MaxFileSizeBytes.Value;
    }
    
    public string GetMaxFileSizeDisplay()
    {
        if (!MaxFileSizeBytes.HasValue)
            return "No limit";
            
        var bytes = MaxFileSizeBytes.Value;
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024} KB";
        if (bytes < 1024 * 1024 * 1024)
            return $"{bytes / (1024 * 1024)} MB";
        return $"{bytes / (1024 * 1024 * 1024)} GB";
    }
    
    public List<string> GetAllowedExtensionsList()
    {
        if (string.IsNullOrEmpty(AllowedExtensions))
            return new List<string>();
            
        return AllowedExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(ext => ext.Trim())
            .ToList();
    }
} 