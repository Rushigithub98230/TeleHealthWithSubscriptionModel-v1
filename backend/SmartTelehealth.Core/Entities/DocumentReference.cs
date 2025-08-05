using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class DocumentReference : BaseEntity
{
    // Primary identification
    public Guid DocumentId { get; set; }
    public virtual Document Document { get; set; } = null!;
    
    // Reference information
    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty; // e.g., "Appointment", "User", "Chat"
    
    public Guid EntityId { get; set; } // ID of the referenced entity
    
    [MaxLength(100)]
    public string? ReferenceType { get; set; } // e.g., "profile_picture", "medical_report", "chat_attachment"
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // Access control
    public bool IsPublic { get; set; } = false;
    
    public DateTime? ExpiresAt { get; set; }
    
    // Audit
    public Guid CreatedById { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
} 