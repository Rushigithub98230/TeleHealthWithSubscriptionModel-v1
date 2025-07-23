using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Application.DTOs
{
    public class CreateNotificationDto
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string Message { get; set; } = string.Empty;
        
        public string? Type { get; set; }
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ScheduledAt { get; set; }
    }
} 