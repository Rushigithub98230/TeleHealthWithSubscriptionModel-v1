using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class DeliveryTracking : BaseEntity
{
    public enum TrackingEventType
    {
        Created,
        Processing,
        Shipped,
        InTransit,
        OutForDelivery,
        Delivered,
        Failed,
        Returned,
        Exception
    }
    
    // Foreign key
    public Guid MedicationDeliveryId { get; set; }
    public virtual MedicationDelivery MedicationDelivery { get; set; } = null!;
    
    // Tracking details
    public TrackingEventType EventType { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? Location { get; set; }
    
    public DateTime EventTime { get; set; }
    
    [MaxLength(100)]
    public string? TrackingNumber { get; set; }
    
    [MaxLength(100)]
    public string? Carrier { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public bool IsDelivered => EventType == TrackingEventType.Delivered;
    public bool IsFailed => EventType == TrackingEventType.Failed;
    public bool IsReturned => EventType == TrackingEventType.Returned;
} 