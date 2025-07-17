using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

#region User Roles Master Table
public class UserRole : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
#endregion

#region Appointment Status Master Table
public class AppointmentStatus : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Payment Status Master Table
public class PaymentStatus : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
}
#endregion

#region Refund Status Master Table
public class RefundStatus : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
}
#endregion

#region Participant Status Master Table
public class ParticipantStatus : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
}
#endregion

#region Participant Role Master Table
public class ParticipantRole : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentParticipant> Participants { get; set; } = new List<AppointmentParticipant>();
}
#endregion

#region Invitation Status Master Table
public class InvitationStatus : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentInvitation> Invitations { get; set; } = new List<AppointmentInvitation>();
}
#endregion

#region Appointment Type Master Table
public class AppointmentType : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Consultation Mode Master Table
public class ConsultationMode : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Color { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
#endregion

#region Document Type Master Table
public class DocumentType : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentDocument> Documents { get; set; } = new List<AppointmentDocument>();
}
#endregion

#region Reminder Type Master Table
public class ReminderType : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
}
#endregion

#region Reminder Timing Master Table
public class ReminderTiming : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    public int MinutesBeforeAppointment { get; set; } = 0;
    
    // Navigation properties
    public virtual ICollection<AppointmentReminder> Reminders { get; set; } = new List<AppointmentReminder>();
}
#endregion

#region Event Type Master Table
public class EventType : BaseEntity
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public int SortOrder { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Icon { get; set; } // For UI display
    
    // Navigation properties
    public virtual ICollection<AppointmentEvent> Events { get; set; } = new List<AppointmentEvent>();
}
#endregion 