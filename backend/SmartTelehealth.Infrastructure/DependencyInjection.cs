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

        // Register Repositories (temporarily removed for focused testing)
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IProviderPayoutRepository, ProviderPayoutRepository>();

        // Register Services
        services.AddScoped<LocalFileStorageService>();
        services.AddScoped<FileStorageFactory>();
        services.AddScoped<IFileStorageService>(provider => provider.GetRequiredService<FileStorageFactory>().CreateFileStorageService());
        
        // Register Document Services
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentTypeService, DocumentTypeService>();
        services.AddScoped<DocumentTypeSeedService>();

        // Cloud Storage Services (temporarily removed for focused testing)
        // services.AddScoped<AzureBlobStorageService>();
        // services.AddScoped<AwsS3StorageService>();
        // services.AddAzureClients(builder => { builder.AddBlobServiceClient(configuration.GetConnectionString("AzureBlobStorage")); });
        // services.AddAWSService<IAmazonS3>(configuration.GetAWSOptions());

        return services;
    }
} 