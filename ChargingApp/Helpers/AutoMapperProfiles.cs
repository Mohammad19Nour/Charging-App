using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace ChargingApp.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        var status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong" };
        var statusForCancel = new List<string>
            { "Not canceled", "Waiting", "Cancelation Accepted", "Cancelation Rejected" };
      
        CreateMap<VIPLevel, VipLevelDto>();
        CreateMap<SliderPhoto, SliderPhotoDto>();
        CreateMap<AppUser, UserDto>();
        CreateMap<UpdateUserInfoDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<NewAgentDto, ChangerAndCompany>();
        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryWithProductsDto>();
        CreateMap<RechargeMethod, RechargeMethodDto>()
            .ForMember(dest => dest.Agents, opt =>
                opt.MapFrom(src => src.ChangerAndCompanies))
            .ForMember(dest => dest.MethodId, opt =>
                opt.MapFrom(src => src.Id));
        CreateMap<ChangerAndCompany, AgentDto>()
            .ForMember(dest => dest.AgentId, opt =>
                opt.MapFrom(src => src.Id));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(src => src.Photo == null ? "No Photo" : "https://localhost:7217" + src.Photo.Url));
        CreateMap<Photo, string>().ConvertUsing(p => (p.Url == null) ? "No Photo" : "https://localhost:7217" + p.Url);
        CreateMap<Quantity, int>().ConvertUsing(q => q.Value);
        // CreateMap<JsonPatchDocument<ProductToUpdateDto>, JsonPatchDocument<Product>>();
        // CreateMap<Operation<ProductToUpdateDto>, Operation<Product>>();

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));
        
        CreateMap<Payment, CompanyPaymentDto>()
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));

        CreateMap<Payment, OfficePaymentDto>().ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]))
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo));

        CreateMap<AppUser, UserInfoDto>().ForMember(dest => dest.AccountType, opt =>
            opt.MapFrom(src => src.VIPLevel == 0 ? "Normal" : ("VIP " + src.VIPLevel)));

        CreateMap<CategoryUpdateDto, Category>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UpdateUserInfoDto, AppUser>().ForAllMembers
        (opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.ProductEnglishName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.ProductArabicName, opt =>
                opt.MapFrom(src => src.Product.ArabicName))
            .ForMember(dest => dest.StatusIfCanceled, opt =>
                opt.MapFrom(src => statusForCancel[src.StatusIfCanceled]))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));

        CreateMap<Order, OrderAdminDto>().ForMember(dest => dest.EnglishName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.ArabicName, opt =>
                opt.MapFrom(src => src.Product.ArabicName))
            ;
        CreateMap<Order, NormalOrderDto>()
            .ForMember(dest => dest.StatusIfCanceled, opt =>
                opt.MapFrom(src => "Not allowed"))
            .ForMember(dest => dest.ProductEnglishName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.ProductArabicName, opt =>
                opt.MapFrom(src => src.Product.ArabicName))
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));
        
        CreateMap<DateTime, DateTime>()
            .ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));

        CreateMap<Currency, CurrencyDto>();
        CreateMap<Order, PendingOrderDto>()
            .ForMember(dest => dest.UserName, opt =>
                opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.ProductEnglishName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.ProductArabicName, opt =>
                opt.MapFrom(src => src.Product.ArabicName))
            ;
        CreateMap<Payment, PaymentAdminDto>()
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom(src => src.User.Email));

        CreateMap<NewOurAgentDto, OurAgent>();
        CreateMap<OurAgent, OurAgentsDto>();
        CreateMap<UpdateOurAgentDto, OurAgent>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null)
            );
    }
    //"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"
}