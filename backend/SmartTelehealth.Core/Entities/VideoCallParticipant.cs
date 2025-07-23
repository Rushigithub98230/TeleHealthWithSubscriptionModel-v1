using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class VideoCallParticipant
    {
        public Guid Id { get; set; }
        public Guid VideoCallId { get; set; }
        public Guid UserId { get; set; }
        public Guid? ProviderId { get; set; }
        public bool IsInitiator { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public int DurationSeconds { get; set; } = 0;
        public bool IsVideoEnabled { get; set; } = true;
        public bool IsAudioEnabled { get; set; } = true;
        public bool IsScreenSharingEnabled { get; set; } = false;
        public int? AudioQuality { get; set; } // 1-5 scale
        public int? VideoQuality { get; set; } // 1-5 scale
        public int? NetworkQuality { get; set; } // 1-5 scale
        public string? DeviceInfo { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public virtual VideoCall VideoCall { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual Provider? Provider { get; set; }
    }
} 