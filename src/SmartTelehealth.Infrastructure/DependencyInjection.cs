using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartTelehealth.Application.Interfaces;
using SmartTelehealth.Core.Interfaces;
using SmartTelehealth.Infrastructure.Data;
using SmartTelehealth.Infrastructure.Repositories;
using SmartTelehealth.Infrastructure.Services;
using SmartTelehealth.Application.Services;
using Azure.Storage.Blobs;

namespace SmartTelehealth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IAppointmentInvitationRepository, AppointmentInvitationRepository>();
        services.AddScoped<IAppointmentParticipantRepository, AppointmentParticipantRepository>();
        services.AddScoped<IAppointmentPaymentLogRepository, AppointmentPaymentLogRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<ISubscriptionPaymentRepository, SubscriptionPaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
        services.AddScoped<IChatRoomParticipantRepository, ChatRoomParticipantRepository>();
        services.AddScoped<IMessageReactionRepository, MessageReactionRepository>();
        services.AddScoped<IPrivilegeRepository, PrivilegeRepository>();
        services.AddScoped<ISubscriptionPlanPrivilegeRepository, SubscriptionPlanPrivilegeRepository>();
        services.AddScoped<IUserSubscriptionPrivilegeUsageRepository, UserSubscriptionPrivilegeUsageRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<IParticipantRoleRepository, ParticipantRoleRepository>();
        services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
        services.AddScoped<IBillingRepository, BillingRepository>();
        services.AddScoped<IOpenTokService, OpenTokService>();
        services.AddScoped<INotificationService, SmartTelehealth.Infrastructure.Services.NotificationService>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IMedicationShipmentRepository, MedicationShipmentRepository>();
        services.AddScoped<IConsultationRepository, ConsultationRepository>();
        services.AddScoped<IHealthAssessmentRepository, HealthAssessmentRepository>();
        services.AddScoped<ICloudChatStorageService, CloudChatStorageService>();
        services.AddScoped<PrivilegeService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IInfermedicaService, InfermedicaService>();
        // Register a stub for IPharmacyIntegrationRepository if not implemented
        services.AddScoped<IPharmacyIntegrationRepository, PharmacyIntegrationRepositoryStub>();

        // Services
        services.AddScoped<IBillingService, SmartTelehealth.Infrastructure.Services.BillingService>();
        services.AddScoped<IChatStorageFactory, ChatStorageFactory>();
        services.AddScoped<ICloudChatStorageService, CloudChatStorageService>();
        services.AddScoped<ILocalChatStorageService, LocalChatStorageService>();

        // File Storage Services
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<AzureBlobStorageService>();
        services.AddScoped<AwsS3StorageService>();
        services.AddScoped<FileStorageFactory>();
        
        // Register the factory-based file storage service
        services.AddScoped<IFileStorageService>(provider =>
        {
            var factory = provider.GetRequiredService<FileStorageFactory>();
            return factory.CreateFileStorageService();
        });

        // Register BlobServiceClient using configuration
        services.AddSingleton(x =>
        {
            var config = x.GetRequiredService<IConfiguration>();
            var connStr = config.GetConnectionString("AzureBlobStorage");
            return new BlobServiceClient(connStr);
        });

        return services;
    }
} 