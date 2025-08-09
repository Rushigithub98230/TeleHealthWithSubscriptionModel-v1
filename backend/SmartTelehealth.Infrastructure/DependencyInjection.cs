using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;

namespace SmartTelehealth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Configuration (temporarily removed for focused testing)
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IProviderPayoutRepository, ProviderPayoutRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IHealthAssessmentRepository, HealthAssessmentRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentParticipantRepository, AppointmentParticipantRepository>();
        services.AddScoped<IAppointmentInvitationRepository, AppointmentInvitationRepository>();
        services.AddScoped<IAppointmentPaymentLogRepository, AppointmentPaymentLogRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IBillingAdjustmentRepository, BillingAdjustmentRepository>();
        services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
        services.AddScoped<ISubscriptionStatusHistoryRepository, SubscriptionStatusHistoryRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
        services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
        services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IMedicationDeliveryRepository, MedicationDeliveryRepository>();
        services.AddScoped<IMedicationShipmentRepository, MedicationShipmentRepository>();
        services.AddScoped<IPharmacyIntegrationRepository, PharmacyIntegrationRepositoryStub>();
        services.AddScoped<IParticipantRoleRepository, ParticipantRoleRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageReactionRepository, MessageReactionRepository>();
        services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IChatRoomParticipantRepository, ChatRoomParticipantRepository>();
        services.AddScoped<IVideoCallRepository, VideoCallRepository>();
        services.AddScoped<IProviderFeeRepository, ProviderFeeRepository>();
        services.AddScoped<IProviderOnboardingRepository, ProviderOnboardingRepository>();


        // Register Services
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<FileStorageFactory>();
        services.AddScoped<IFileStorageService>(provider => provider.GetRequiredService<FileStorageFactory>().CreateFileStorageService());
        
        // Register Document Services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentTypeService, DocumentTypeService>();
        services.AddScoped<DocumentTypeSeedService>();
        
        // Register JWT Service
        services.AddScoped<IJwtService, JwtService>();
        
        // Register Notification Service
        services.AddScoped<INotificationService, NotificationService>();
        
        // Register Questionnaire Repository
        services.AddScoped<IQuestionnaireRepository, QuestionnaireRepository>();
        
        // Register OpenTok Service
        services.AddScoped<IOpenTokService, OpenTokService>();
        
        // Register Stripe Service (placeholder - needs implementation)
        services.AddScoped<IStripeService, StripeService>();

        // Register Automated Billing Service as a hosted service
        services.AddHostedService<AutomatedBillingService>();

        // Register Application AutomatedBillingService as a scoped service
        services.AddScoped<SmartTelehealth.Application.Services.AutomatedBillingService>();

        // Cloud Storage Services (temporarily removed for focused testing)
        // services.AddScoped<AzureBlobStorageService>();
        // services.AddScoped<AwsS3StorageService>();
        // services.AddAzureClients(builder => { builder.AddBlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")); });
        // services.AddAWSService<IAmazonS3>(configuration.GetAWSOptions());

        return services;
    }
} 