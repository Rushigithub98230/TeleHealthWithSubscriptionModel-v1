using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartTelehealth.Core.Entities;

namespace SmartTelehealth.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    // Master Tables DbSets
    public DbSet<MasterBillingCycle> MasterBillingCycles { get; set; }
    public DbSet<MasterCurrency> MasterCurrencies { get; set; }
    public DbSet<MasterPrivilegeType> MasterPrivilegeTypes { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<AppointmentStatus> AppointmentStatuses { get; set; }
    public DbSet<PaymentStatus> PaymentStatuses { get; set; }
    public DbSet<RefundStatus> RefundStatuses { get; set; }
    public DbSet<ParticipantStatus> ParticipantStatuses { get; set; }
    public DbSet<ParticipantRole> ParticipantRoles { get; set; }
    public DbSet<InvitationStatus> InvitationStatuses { get; set; }
    public DbSet<AppointmentType> AppointmentTypes { get; set; }
    public DbSet<ConsultationMode> ConsultationModes { get; set; }
    public DbSet<DocumentType> DocumentTypes { get; set; }
    public DbSet<ReminderType> ReminderTypes { get; set; }
    public DbSet<ReminderTiming> ReminderTimings { get; set; }
    public DbSet<EventType> EventTypes { get; set; }
    
    // DbSets
    public DbSet<Provider> Providers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<HealthAssessment> HealthAssessments { get; set; }
    public DbSet<Consultation> Consultations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatRoomParticipant> ChatRoomParticipants { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageReadReceipt> MessageReadReceipts { get; set; }
    public DbSet<MedicationDelivery> MedicationDeliveries { get; set; }
    public DbSet<DeliveryTracking> DeliveryTracking { get; set; }
    public DbSet<BillingRecord> BillingRecords { get; set; }
    public DbSet<BillingAdjustment> BillingAdjustments { get; set; }
    public DbSet<ProviderCategory> ProviderCategories { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    
    // Appointment entities
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<AppointmentParticipant> AppointmentParticipants { get; set; }
    public DbSet<AppointmentInvitation> AppointmentInvitations { get; set; }
    public DbSet<AppointmentPaymentLog> AppointmentPaymentLogs { get; set; }
    public DbSet<AppointmentDocument> AppointmentDocuments { get; set; }
    public DbSet<AppointmentReminder> AppointmentReminders { get; set; }
    public DbSet<AppointmentEvent> AppointmentEvents { get; set; }
        
    // Video Call entities
    public DbSet<VideoCall> VideoCalls { get; set; }
    public DbSet<VideoCallParticipant> VideoCallParticipants { get; set; }
    public DbSet<VideoCallEvent> VideoCallEvents { get; set; }
    
    public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
    public DbSet<Privilege> Privileges { get; set; }
    public DbSet<SubscriptionPlanPrivilege> SubscriptionPlanPrivileges { get; set; }
    public DbSet<UserSubscriptionPrivilegeUsage> UserSubscriptionPrivilegeUsages { get; set; }
    
    public DbSet<CategoryQuestion> CategoryQuestions { get; set; }
    public DbSet<CategoryQuestionAnswer> CategoryQuestionAnswers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure master tables
        ConfigureMasterTables(builder);
        
        // Configure entity relationships and constraints
        ConfigureUser(builder);
        ConfigureProvider(builder);
        ConfigureCategory(builder);
        ConfigureSubscriptionPlan(builder);
        ConfigureSubscription(builder);
        ConfigureHealthAssessment(builder);
        ConfigureConsultation(builder);
        ConfigureMessage(builder);
        ConfigureChatRoom(builder);
        ConfigureChatRoomParticipant(builder);
        ConfigureMessageReaction(builder);
        ConfigureMessageReadReceipt(builder);
        ConfigureMedicationDelivery(builder);
        ConfigureBillingRecord(builder);
        ConfigureProviderCategory(builder);
        ConfigureNotification(builder);
        ConfigureAuditLog(builder);
        ConfigureAppointment(builder);
        ConfigureAppointmentParticipant(builder);
        ConfigureAppointmentInvitation(builder);
        ConfigureAppointmentPaymentLog(builder);
        ConfigureAppointmentDocument(builder);
        ConfigureAppointmentReminder(builder);
        ConfigureAppointmentEvent(builder);
        ConfigureVideoCall(builder);
        ConfigureVideoCallParticipant(builder);
        ConfigureVideoCallEvent(builder);
        ConfigureSubscriptionPayment(builder);
    }
    
    private void ConfigureMasterTables(ModelBuilder builder)
    {
        // UserRole
        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        
        // AppointmentStatus
        builder.Entity<AppointmentStatus>(entity =>
        {
            entity.ToTable("AppointmentStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });
        
        // PaymentStatus
        builder.Entity<PaymentStatus>(entity =>
        {
            entity.ToTable("PaymentStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // RefundStatus
        builder.Entity<RefundStatus>(entity =>
        {
            entity.ToTable("RefundStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ParticipantStatus
        builder.Entity<ParticipantStatus>(entity =>
        {
            entity.ToTable("ParticipantStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ParticipantRole
        builder.Entity<ParticipantRole>(entity =>
        {
            entity.ToTable("ParticipantRoles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // InvitationStatus
        builder.Entity<InvitationStatus>(entity =>
        {
            entity.ToTable("InvitationStatuses");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // AppointmentType
        builder.Entity<AppointmentType>(entity =>
        {
            entity.ToTable("AppointmentTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // ConsultationMode
        builder.Entity<ConsultationMode>(entity =>
        {
            entity.ToTable("ConsultationModes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Color).HasMaxLength(50);
        });
        
        // DocumentType
        builder.Entity<DocumentType>(entity =>
        {
            entity.ToTable("DocumentTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });
        
        // ReminderType
        builder.Entity<ReminderType>(entity =>
        {
            entity.ToTable("ReminderTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });
        
        // ReminderTiming
        builder.Entity<ReminderTiming>(entity =>
        {
            entity.ToTable("ReminderTimings");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.MinutesBeforeAppointment).HasDefaultValue(0);
        });
        
        // EventType
        builder.Entity<EventType>(entity =>
        {
            entity.ToTable("EventTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Icon).HasMaxLength(50);
        });

        // MasterBillingCycle
        builder.Entity<MasterBillingCycle>(entity =>
        {
            entity.ToTable("MasterBillingCycles");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        // MasterCurrency
        builder.Entity<MasterCurrency>(entity =>
        {
            entity.ToTable("MasterCurrencies");
            entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Symbol).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
        // MasterPrivilegeType
        builder.Entity<MasterPrivilegeType>(entity =>
        {
            entity.ToTable("MasterPrivilegeTypes");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
        });
    }
    
    private void ConfigureUser(ModelBuilder builder)
    {
        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DateOfBirth).IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            // UserRole relationship
            entity.HasOne(e => e.UserRole)
                .WithMany(e => e.Users)
                .HasForeignKey(e => e.UserRoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureProvider(ModelBuilder builder)
    {
        builder.Entity<Provider>(entity =>
        {
            entity.ToTable("Providers");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(100);
            entity.Property(e => e.State).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Specialty).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
        });
    }
    
    private void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
            entity.Property(e => e.OneTimeConsultationFee).HasPrecision(18, 2);
            entity.Property(e => e.RequiresHealthAssessment).HasDefaultValue(true);
            entity.Property(e => e.AllowsMedicationDelivery).HasDefaultValue(true);
            entity.Property(e => e.AllowsFollowUpMessaging).HasDefaultValue(true);
        });
    }
    
    private void ConfigureSubscriptionPlan(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.DisplayOrder);
            // Remove old price fields and category reference
            // entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2); // Removed
            // entity.Property(e => e.QuarterlyPrice).HasPrecision(18, 2); // Removed
            // entity.Property(e => e.AnnualPrice).HasPrecision(18, 2); // Removed
            // entity.HasOne(e => e.Category).WithMany(e => e.SubscriptionPlans).HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict); // Removed

            entity.HasOne(e => e.BillingCycle)
                .WithMany()
                .HasForeignKey(e => e.BillingCycleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureSubscription(ModelBuilder builder)
    {
        builder.Entity<Subscription>(entity =>
        {
            entity.ToTable("Subscriptions");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CurrentPrice).HasPrecision(18, 2);
            entity.Property(e => e.AutoRenew).HasDefaultValue(true);
            // Remove BillingFrequency
            // entity.Property(e => e.BillingFrequency).HasConversion<string>(); // Removed

            entity.HasOne(e => e.User)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SubscriptionPlan)
                .WithMany(e => e.Subscriptions)
                .HasForeignKey(e => e.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.BillingCycle)
                .WithMany()
                .HasForeignKey(e => e.BillingCycleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
    
    private void ConfigureHealthAssessment(ModelBuilder builder)
    {
        builder.Entity<HealthAssessment>(entity =>
        {
            entity.ToTable("HealthAssessments");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IsEligibleForTreatment).HasDefaultValue(false);
            entity.Property(e => e.RequiresFollowUp).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.HealthAssessments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.HealthAssessments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureConsultation(ModelBuilder builder)
    {
        builder.Entity<Consultation>(entity =>
        {
            entity.ToTable("Consultations");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Fee).HasPrecision(18, 2);
            entity.Property(e => e.RequiresFollowUp).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.HealthAssessment)
                .WithMany(e => e.Consultations)
                .HasForeignKey(e => e.HealthAssessmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessage(ModelBuilder builder)
    {
        builder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Content).IsRequired().HasMaxLength(4000);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.EncryptionKey).HasMaxLength(255);
            
            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ChatRoom)
                .WithMany(cr => cr.Messages)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.ReplyToMessage)
                .WithMany(e => e.Replies)
                .HasForeignKey(e => e.ReplyToMessageId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasMany(e => e.Reactions)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.ReadReceipts)
                .WithOne(rr => rr.Message)
                .HasForeignKey(rr => rr.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        builder.Entity<MessageAttachment>(entity =>
        {
            entity.ToTable("MessageAttachments");
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsImage).HasDefaultValue(false);
            entity.Property(e => e.IsDocument).HasDefaultValue(false);
            entity.Property(e => e.IsVideo).HasDefaultValue(false);
            entity.Property(e => e.IsAudio).HasDefaultValue(false);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureChatRoom(ModelBuilder builder)
    {
        builder.Entity<ChatRoom>(entity =>
        {
            entity.ToTable("ChatRooms");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IsEncrypted).HasDefaultValue(true);
            entity.Property(e => e.AllowFileSharing).HasDefaultValue(true);
            entity.Property(e => e.AllowVoiceCalls).HasDefaultValue(true);
            entity.Property(e => e.AllowVideoCalls).HasDefaultValue(true);
            
            entity.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureChatRoomParticipant(ModelBuilder builder)
    {
        builder.Entity<ChatRoomParticipant>(entity =>
        {
            entity.ToTable("ChatRoomParticipants");
            entity.Property(e => e.Role).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CanSendMessages).HasDefaultValue(true);
            entity.Property(e => e.CanSendFiles).HasDefaultValue(true);
            entity.Property(e => e.CanInviteOthers).HasDefaultValue(false);
            entity.Property(e => e.CanModerate).HasDefaultValue(false);
            
            entity.HasOne(e => e.ChatRoom)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.ChatRoomId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessageReaction(ModelBuilder builder)
    {
        builder.Entity<MessageReaction>(entity =>
        {
            entity.ToTable("MessageReactions");
            entity.Property(e => e.Emoji).IsRequired().HasMaxLength(10);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.Reactions)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMessageReadReceipt(ModelBuilder builder)
    {
        builder.Entity<MessageReadReceipt>(entity =>
        {
            entity.ToTable("MessageReadReceipts");
            entity.Property(e => e.DeviceInfo).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            
            entity.HasOne(e => e.Message)
                .WithMany(e => e.ReadReceipts)
                .HasForeignKey(e => e.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
    
    private void ConfigureMedicationDelivery(ModelBuilder builder)
    {
        builder.Entity<MedicationDelivery>(entity =>
        {
            entity.ToTable("MedicationDeliveries");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.DeliveryAddress).IsRequired().HasMaxLength(500);
            entity.Property(e => e.ShippingCost).HasPrecision(18, 2);
            entity.Property(e => e.RequiresSignature).HasDefaultValue(false);
            entity.Property(e => e.IsRefrigerated).HasDefaultValue(false);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.MedicationDeliveries)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany(e => e.MedicationDeliveries)
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<DeliveryTracking>(entity =>
        {
            entity.ToTable("DeliveryTracking");
            entity.Property(e => e.EventType).HasConversion<string>();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            
            entity.HasOne(e => e.MedicationDelivery)
                .WithMany(e => e.TrackingEvents)
                .HasForeignKey(e => e.MedicationDeliveryId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureBillingRecord(ModelBuilder builder)
    {
        builder.Entity<BillingRecord>(entity =>
        {
            entity.ToTable("BillingRecords");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.IsRecurring).HasDefaultValue(false);
            entity.Property(e => e.AccruedAmount).HasPrecision(18, 2);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany(e => e.BillingRecords)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.MedicationDelivery)
                .WithMany()
                .HasForeignKey(e => e.MedicationDeliveryId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        builder.Entity<BillingAdjustment>(entity =>
        {
            entity.ToTable("BillingAdjustments");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Percentage).HasPrecision(5, 2);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsPercentage).HasDefaultValue(false);
            entity.Property(e => e.IsApproved).HasDefaultValue(true);
            
            entity.HasOne(e => e.BillingRecord)
                .WithMany(e => e.Adjustments)
                .HasForeignKey(e => e.BillingRecordId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
    
    private void ConfigureProviderCategory(ModelBuilder builder)
    {
        builder.Entity<ProviderCategory>(entity =>
        {
            entity.ToTable("ProviderCategories");
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.ConsultationFee).HasPrecision(18, 2);
            
            entity.HasOne(e => e.Provider)
                .WithMany(e => e.ProviderCategories)
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Category)
                .WithMany(e => e.ProviderCategories)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Composite unique constraint
            entity.HasIndex(e => new { e.ProviderId, e.CategoryId }).IsUnique();
        });
    }

    private void ConfigureNotification(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }

    private void ConfigureAuditLog(ModelBuilder builder)
    {
        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserEmail).HasMaxLength(100);
            entity.Property(e => e.UserRole).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.OldValues).HasMaxLength(1000);
            entity.Property(e => e.NewValues).HasMaxLength(1000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }

    private void ConfigureVideoCall(ModelBuilder builder)
    {
        builder.Entity<VideoCall>(entity =>
        {
            entity.ToTable("VideoCalls");
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.Token).HasMaxLength(500);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureVideoCallParticipant(ModelBuilder builder)
    {
        builder.Entity<VideoCallParticipant>(entity =>
        {
            entity.ToTable("VideoCallParticipants");
            entity.Property(e => e.IsInitiator).HasDefaultValue(false);
            entity.Property(e => e.IsVideoEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsAudioEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsScreenSharingEnabled).HasDefaultValue(false);
            entity.Property(e => e.DeviceInfo).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(100);
            
            entity.HasOne(e => e.VideoCall)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.VideoCallId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureVideoCallEvent(ModelBuilder builder)
    {
        builder.Entity<VideoCallEvent>(entity =>
        {
            entity.ToTable("VideoCallEvents");
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Metadata).HasMaxLength(1000);
            
            entity.HasOne(e => e.VideoCall)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.VideoCallId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureAppointment(ModelBuilder builder)
    {
        builder.Entity<Appointment>(entity =>
        {
            entity.ToTable("Appointments");
            entity.Property(e => e.Fee).HasPrecision(18, 2);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.StripeSessionId).HasMaxLength(255);
            entity.Property(e => e.OpenTokSessionId).HasMaxLength(255);
            entity.Property(e => e.MeetingUrl).HasMaxLength(500);
            entity.Property(e => e.MeetingId).HasMaxLength(100);
            entity.Property(e => e.RecordingId).HasMaxLength(100);
            entity.Property(e => e.RecordingUrl).HasMaxLength(500);
            entity.Property(e => e.ReasonForVisit).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Symptoms).HasMaxLength(1000);
            entity.Property(e => e.PatientNotes).HasMaxLength(1000);
            entity.Property(e => e.Diagnosis).HasMaxLength(1000);
            entity.Property(e => e.Prescription).HasMaxLength(1000);
            entity.Property(e => e.ProviderNotes).HasMaxLength(1000);
            entity.Property(e => e.FollowUpInstructions).HasMaxLength(1000);
            entity.Property(e => e.IsPaymentCaptured).HasDefaultValue(false);
            entity.Property(e => e.IsRefunded).HasDefaultValue(false);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.IsVideoCallStarted).HasDefaultValue(false);
            entity.Property(e => e.IsVideoCallEnded).HasDefaultValue(false);
            entity.Property(e => e.IsRecordingEnabled).HasDefaultValue(true);
            entity.Property(e => e.IsPatientNotified).HasDefaultValue(false);
            entity.Property(e => e.IsProviderNotified).HasDefaultValue(false);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);
            entity.Property(e => e.AppointmentStatusId).IsRequired(); // Now Guid
            entity.HasOne(e => e.AppointmentStatus)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.AppointmentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Foreign key relationships
            entity.HasOne(e => e.Patient)
                .WithMany(e => e.PatientAppointments)
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Subscription)
                .WithMany()
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Consultation)
                .WithMany()
                .HasForeignKey(e => e.ConsultationId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationships
            entity.HasOne(e => e.AppointmentType)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.AppointmentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ConsultationMode)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.ConsultationModeId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.PaymentStatus)
                .WithMany(e => e.Appointments)
                .HasForeignKey(e => e.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentParticipant(ModelBuilder builder)
    {
        builder.Entity<AppointmentParticipant>(entity =>
        {
            entity.ToTable("AppointmentParticipants");
            entity.Property(e => e.ExternalEmail).HasMaxLength(256);
            entity.Property(e => e.ExternalPhone).HasMaxLength(32);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Participants)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.AppointmentParticipants)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Master table relationships
            entity.HasOne(e => e.ParticipantRole)
                .WithMany(r => r.Participants)
                .HasForeignKey(e => e.ParticipantRoleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.ParticipantStatus)
                .WithMany(s => s.Participants)
                .HasForeignKey(e => e.ParticipantStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentInvitation(ModelBuilder builder)
    {
        builder.Entity<AppointmentInvitation>(entity =>
        {
            entity.ToTable("AppointmentInvitations");
            entity.Property(e => e.InvitedEmail).HasMaxLength(256);
            entity.Property(e => e.InvitedPhone).HasMaxLength(32);
            entity.Property(e => e.Message).HasMaxLength(500);
            
            entity.HasOne(e => e.Appointment)
                .WithMany()
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.InvitedByUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.InvitedUser)
                .WithMany()
                .HasForeignKey(e => e.InvitedUserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.InvitationStatus)
                .WithMany(e => e.Invitations)
                .HasForeignKey(e => e.InvitationStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentPaymentLog(ModelBuilder builder)
    {
        builder.Entity<AppointmentPaymentLog>(entity =>
        {
            entity.ToTable("AppointmentPaymentLogs");
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.PaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.SessionId).HasMaxLength(255);
            entity.Property(e => e.RefundId).HasMaxLength(255);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.RefundReason).HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Master table relationships
            entity.HasOne(e => e.PaymentStatus)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.PaymentStatusId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.RefundStatus)
                .WithMany(e => e.PaymentLogs)
                .HasForeignKey(e => e.RefundStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentDocument(ModelBuilder builder)
    {
        builder.Entity<AppointmentDocument>(entity =>
        {
            entity.ToTable("AppointmentDocuments");
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.DocumentTypeId).IsRequired(); // Now Guid
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.UploadedBy)
                .WithMany(e => e.UploadedDocuments)
                .HasForeignKey(e => e.UploadedById)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationship
            entity.HasOne(e => e.DocumentType)
                .WithMany(e => e.Documents)
                .HasForeignKey(e => e.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentReminder(ModelBuilder builder)
    {
        builder.Entity<AppointmentReminder>(entity =>
        {
            entity.ToTable("AppointmentReminders");
            entity.Property(e => e.ScheduledAt).IsRequired();
            entity.Property(e => e.SentAt).IsRequired(false);
            entity.Property(e => e.IsSent).HasDefaultValue(false);
            entity.Property(e => e.IsDelivered).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.Property(e => e.RecipientEmail).HasMaxLength(100);
            entity.Property(e => e.RecipientPhone).HasMaxLength(20);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Master table relationships
            entity.HasOne(e => e.ReminderType)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.ReminderTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ReminderTiming)
                .WithMany(e => e.Reminders)
                .HasForeignKey(e => e.ReminderTimingId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureAppointmentEvent(ModelBuilder builder)
    {
        builder.Entity<AppointmentEvent>(entity =>
        {
            entity.ToTable("AppointmentEvents");
            entity.Property(e => e.OccurredAt).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasMaxLength(500);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            
            entity.HasOne(e => e.Appointment)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.User)
                .WithMany(e => e.AppointmentEvents)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
                
            entity.HasOne(e => e.Provider)
                .WithMany()
                .HasForeignKey(e => e.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);
                
            // Master table relationship
            entity.HasOne(e => e.EventType)
                .WithMany(e => e.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureSubscriptionPayment(ModelBuilder builder)
    {
        builder.Entity<SubscriptionPayment>(entity =>
        {
            entity.ToTable("SubscriptionPayments");
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            // ... existing property and relationship configuration ...
        });
    }
} 