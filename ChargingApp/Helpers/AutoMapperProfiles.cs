using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        var status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong","Received","Cancelled" };
        var statusForCancel = new List<string>
            { "Not canceled", "Waiting", "Cancellation Accepted", "Cancellation Rejected" };

        CreateMap<ProductToUpdateDto , Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ProductWithQuantityToUpdateDto , Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

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
                opt.MapFrom(x=>x.Photo == null?"No photo" : x.Photo.Url));
        CreateMap<Photo, string>().ConvertUsing(p => (p.Url == null) ? "No Photo" : "http://mohammad09nour-001-site1.etempurl.com" + p.Url);
        
        CreateMap<Quantity, int>().ConvertUsing(q => q.Value);

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

        CreateMap<Order, SellsDto>()
            .ForMember(dest => dest.UserEmail, opt =>
                opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));
        
        CreateMap<AppUser, UserInfoDto>().ForMember(dest => dest.AccountType, opt =>
            opt.MapFrom(src => src.VIPLevel == 0 ? "Normal" : ("VIP " + src.VIPLevel)));

        CreateMap<CategoryUpdateDto, Category>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<UpdateUserInfoDto, AppUser>().ForAllMembers
        (opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Order, OrderDto>()
           .ForMember(dest => dest.StatusIfCanceled, opt =>
                opt.MapFrom(src => statusForCancel[src.StatusIfCanceled]))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));

        CreateMap<Order, OrderAdminDto>();
        CreateMap<Order, NormalOrderDto>()
            .ForMember(dest => dest.StatusIfCanceled, opt =>
                opt.MapFrom(src => "Not allowed"))
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
            .ForMember(dest => dest.Status, opt =>
               opt.MapFrom(src => status[src.Status]))
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(src =>src.Photo == null ? "No Photo" : "http://mohammad09nour-001-site1.etempurl.com" + src.Photo.Url));


        CreateMap<Payment, PaymentAdminDto>()
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom(src => src.User.Email));

        CreateMap<NewOurAgentDto, OurAgent>();
        CreateMap<OurAgent, OurAgentsDto>();
        CreateMap<UpdateOurAgentDto, OurAgent>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null)
            );

        CreateMap<DebitHistory , DebitDto>()
            .ForMember(dest=>dest.Username,opt=>
            opt.MapFrom(src=>src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest=>dest.UserEmail,opt=>
                opt.MapFrom(src=>src.User.Email));
    }
    //"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"
}