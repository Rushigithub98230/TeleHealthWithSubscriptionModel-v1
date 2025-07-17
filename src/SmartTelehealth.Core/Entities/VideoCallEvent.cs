using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class VideoCallEvent
    {
        public Guid Id { get; set; }
        public Guid VideoCallId { get; set; }
        public Guid? UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public VideoCallEventType Type { get; set; }
        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
        public string? Metadata { get; set; } // JSON data for additional event details
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual VideoCall VideoCall { get; set; } = null!;
        public virtual User? User { get; set; }
        public virtual Provider? Provider { get; set; }
    }

    public enum VideoCallEventType
    {
        Started,
        Ended,
        ParticipantJoined,
        ParticipantLeft,
        RecordingStarted,
        RecordingStopped,
        CallInitiated,
        CallDisconnected,
        CallRejected,
        VideoEnabled,
        VideoDisabled,
        AudioEnabled,
        AudioDisabled,
        ScreenSharingStarted,
        ScreenSharingStopped,
        QualityChanged
    }
} 