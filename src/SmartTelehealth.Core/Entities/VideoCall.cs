using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class VideoCall
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RecordingUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual Appointment Appointment { get; set; } = null!;
        public virtual ICollection<VideoCallParticipant> Participants { get; set; } = new List<VideoCallParticipant>();
        public virtual ICollection<VideoCallEvent> Events { get; set; } = new List<VideoCallEvent>();
    }
} 