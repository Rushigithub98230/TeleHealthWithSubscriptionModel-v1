using AutoMapper;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Application.DTOs;

namespace SmartTelehealth.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => src.UserRoleId.ToString()));
        
        CreateMap<UserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
            .ForMember(dest => dest.UserRoleId, opt => opt.MapFrom(src => Guid.Parse(src.UserRoleId)));

        // Appointment mappings
        CreateMap<Appointment, AppointmentDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.PatientId.ToString()))
            .ForMember(dest => dest.ProviderId, opt => opt.MapFrom(src => src.ProviderId.ToString()))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId.ToString()))
            .ForMember(dest => dest.SubscriptionId, opt => opt.MapFrom(src => src.SubscriptionId.ToString()))
            .ForMember(dest => dest.ConsultationId, opt => opt.MapFrom(src => src.ConsultationId.ToString()))
            .ForMember(dest => dest.AppointmentTypeId, opt => opt.MapFrom(src => src.AppointmentTypeId))
            .ForMember(dest => dest.ConsultationModeId, opt => opt.MapFrom(src => src.ConsultationModeId));

        // Subscription mappings
        CreateMap<Subscription, SubscriptionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.PlanId, opt => opt.MapFrom(src => src.SubscriptionPlanId.ToString()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.SubscriptionPlan.Price))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.SubscriptionPlan.BillingCycleId))
            .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.SubscriptionPlan.CurrencyId));
        // Add mapping for CreateSubscriptionDto to Subscription
        CreateMap<CreateSubscriptionDto, Subscription>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => Guid.Parse(src.UserId)))
            .ForMember(dest => dest.SubscriptionPlanId, opt => opt.MapFrom(src => Guid.Parse(src.PlanId)))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.BillingCycleId, opt => opt.MapFrom(src => src.BillingCycleId));

        // Chat mappings
        CreateMap<ChatRoom, ChatRoomDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

        CreateMap<Message, MessageDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.ChatRoomId, opt => opt.MapFrom(src => src.ChatRoomId.ToString()))
            .ForMember(dest => dest.SenderId, opt => opt.MapFrom(src => src.SenderId.ToString()));

        // Notification mappings
        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()));
    }
} 