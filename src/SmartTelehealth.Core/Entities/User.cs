using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class User : IdentityUser<Guid>
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    public new string? PhoneNumber { get; set; }
    
    public DateTime DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    [MaxLength(500)]
    public string? Address { get; set; }
    
    [MaxLength(100)]
    public string? City { get; set; }
    
    [MaxLength(50)]
    public string? State { get; set; }
    
    [MaxLength(20)]
    public string? ZipCode { get; set; }
    
    [MaxLength(100)]
    public string? Country { get; set; }
    
    [MaxLength(100)]
    public string? EmergencyContactName { get; set; }
    
    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }
    
    // Additional properties for service compatibility
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // JWT Authentication
    [MaxLength(500)]
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiry { get; set; }
    
    // Soft delete
    public bool IsDeleted { get; set; } = false;
    
    // Stripe Integration
    [MaxLength(100)]
    public string? StripeCustomerId { get; set; }
    
    [MaxLength(100)]
    public string? LicenseNumber { get; set; }
    
    public string UserType { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public new string? Phone { get => PhoneNumber; set => PhoneNumber = value; }
    
    // Password reset properties
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
    
    // User preferences
    public string? NotificationPreferences { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZonePreference { get; set; }
    
    // User Role - Foreign Key Reference
    public Guid UserRoleId { get; set; }
    public virtual UserRole UserRole { get; set; } = null!;
    
    // Navigation properties
    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<HealthAssessment> HealthAssessments { get; set; } = new List<HealthAssessment>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();
    public virtual ICollection<AppointmentParticipant> AppointmentParticipants { get; set; } = new List<AppointmentParticipant>();
    public virtual ICollection<AppointmentPaymentLog> PaymentLogs { get; set; } = new List<AppointmentPaymentLog>();
    public virtual ICollection<AppointmentDocument> UploadedDocuments { get; set; } = new List<AppointmentDocument>();
    public virtual ICollection<AppointmentEvent> AppointmentEvents { get; set; } = new List<AppointmentEvent>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Computed property for role name
    public string RoleName => UserRole?.Name ?? "Unknown";
} 