using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class AutoMapperProfiles : Profile
{
    private const string BaseUrl = "http://chargingapp-001-site1.atempurl.com";

    public AutoMapperProfiles()
    {
        var status = new List<string> { "Pending", "Succeed", "Rejected", "Wrong", "Received", "Cancelled" };
        var statusForCancel = new List<string>
            { "Not canceled", "Waiting", "Cancellation Accepted", "Cancellation Rejected" };

        CreateMap<UpdatePaymentGatewayDto, PaymentGateway>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Order, DoneOrderDto>().ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]))
            .ForMember(dest => dest.UserName, opt =>
                opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]))
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(src =>
                    src.Photo == null ? "No Photo" : BaseUrl + src.Photo.Url))
            .ForMember(dest => dest.Quantity, opt =>
                opt.MapFrom(src =>
                    src.TotalQuantity));
        ;
        CreateMap<AppUser, AdminDto>();
        CreateMap<ProductToUpdateDto, Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<ProductWithQuantityToUpdateDto, Product>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<VIPLevel, VipLevelDto>()
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(x => BaseUrl + x.Photo.Url));
        CreateMap<VIPLevel, AdminVipLevelDto>().ForMember(dest => dest.Photo, opt =>
            opt.MapFrom(x => BaseUrl + x.Photo.Url));
        ;
        CreateMap<SliderPhoto, SliderPhotoDto>()
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(x => BaseUrl + x.Photo.Url));

        CreateMap<AppUser, UserDto>();
        CreateMap<UpdateUserInfoDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
        CreateMap<NewAgentDto, ChangerAndCompany>();
        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryWithProductsDto>()
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(src => BaseUrl + src.Photo.Url));
        CreateMap<PaymentGateway, PaymentGatewayDto>();
        CreateMap<RechargeMethod, RechargeMethodDto>()
            .ForMember(dest => dest.Agents, opt =>
                opt.MapFrom(src => src.ChangerAndCompanies))
            .ForMember(dest => dest.MethodId, opt =>
                opt.MapFrom(src => src.Id));
        CreateMap<ChangerAndCompany, AgentDto>()
            .ForMember(dest => dest.AgentId, opt =>
                opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(src => BaseUrl + src.Photo.Url));

        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Photo, opt =>
                opt.MapFrom(x => x.Photo == null ? "No photo" : BaseUrl + x.Photo.Url));

        CreateMap<Photo, string>().ConvertUsing(p =>
            (p.Url == null) ? "No Photo" : BaseUrl + p.Url);

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo == null ? "No photo" : BaseUrl + src.Photo.Url))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));

        CreateMap<Payment, CompanyPaymentDto>()
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo == null ? "No photo" : BaseUrl + src.Photo.Url))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]));

        CreateMap<Payment, OfficePaymentDto>().ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src => status[src.Status]))
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo == null ? "No photo" : BaseUrl + src.Photo.Url));

        CreateMap<Order, SellsDto>()
            .ForMember(dest => dest.UserEmail, opt =>
                opt.MapFrom(src => src.User.Email))
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName));

        CreateMap<AppUser, UserInfoDto>().ForMember(dest => dest.AccountType, opt =>
            opt.MapFrom(src => src.VIPLevel == 0 ? "Normal" : ("VIP " + src.VIPLevel)));
        CreateMap<AppUser, NormalUserInfoDto>().ForMember(dest => dest.AccountType, opt =>
            opt.MapFrom(src => "Normal"));

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
            .ForMember(dest => dest.GatewayArabicName, opt =>
                opt.MapFrom(src => src.PaymentGateway!.ArabicName))
            .ForMember(dest => dest.GatewayEnglishName, opt =>
                opt.MapFrom(src => src.PaymentGateway!.EnglishName))
            .ForMember(dest => dest.StatusIfCanceled, opt =>
                opt.MapFrom(src => "Not allowed"))
            .ForMember(dest => dest.ReceiptNumberUrl, opt =>
                opt.MapFrom(src => src.Photo == null ? "No Photo" : BaseUrl + src.Photo.Url))
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
                opt.MapFrom(src =>
                    src.Photo == null ? "No Photo" : BaseUrl + src.Photo.Url))
            .ForMember(dest => dest.Quantity, opt =>
                opt.MapFrom(src =>
                    src.TotalQuantity));


        CreateMap<Payment, PaymentAdminDto>()
            .ForMember(dest => dest.Email, opt =>
                opt.MapFrom(src => src.User.Email));

        CreateMap<NewOurAgentDto, OurAgent>();
        CreateMap<OurAgent, OurAgentsDto>();
        CreateMap<UpdateOurAgentDto, OurAgent>()
            .ForAllMembers(opt =>
                opt.Condition((src, dest, srcMember) => srcMember != null)
            );

        CreateMap<DebitHistory, DebitDto>()
            .ForMember(dest => dest.Username, opt =>
                opt.MapFrom(src => src.User.FirstName + " " + src.User.LastName))
            .ForMember(dest => dest.UserEmail, opt =>
                opt.MapFrom(src => src.User.Email));
    }
    //"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"
}