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


        CreateMap<Photo, string>().ConvertUsing(p => p.Url ?? "No Photo");

        CreateMap<Product, ProductDto>();

        CreateMap<Quantity, int>().ConvertUsing(q => q.Value);
        // CreateMap<JsonPatchDocument<ProductToUpdateDto>, JsonPatchDocument<Product>>();
        // CreateMap<Operation<ProductToUpdateDto>, Operation<Product>>();
        
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src =>
                    (!src.Checked ? "Pending" : (src.Checked && !src.Succeed) ? "Rejected" : "Succeed")));
        
        CreateMap<Payment, CompanyPaymentDto>().ForMember(dest => dest.Status, opt =>
            opt.MapFrom(src =>
                (!src.Checked ? "Pending" : (src.Checked && !src.Succeed) ? "Rejected" : "Succeed")));

        CreateMap<Payment, OfficePaymentDto>().ForMember(dest => dest.Status, opt =>
            opt.MapFrom(src =>
                (!src.Checked ? "Pending" : (src.Checked && !src.Succeed) ? "Rejected" : "Succeed")));

        CreateMap<AppUser, UserInfoDto>().ForMember(dest => dest.AccountType, opt =>
            opt.MapFrom(src => src.VIPLevel == 0 ? "Normal" : ("VIP " + src.VIPLevel)));

        CreateMap<CategoryUpdateDto, Category>();
        
        CreateMap<UpdateUserInfoDto, AppUser>().ForAllMembers
        (opt =>
            opt.Condition((src, dest, srcMember) => srcMember != null));
        
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.ProductName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src =>
                    (!src.Checked ? "Pending" : (src.Checked && !src.Succeed) ? "Rejected" : "Succeed")))
           ;// .ForMember(dest => dest.CreatedAt, opt =>
              //  opt.MapFrom(src => src.CreatedAt.ToString("g")));
 
        CreateMap<Order, NormalOrderDto>()
            .ForMember(dest => dest.ProductName, opt =>
                opt.MapFrom(src => src.Product.EnglishName))
            .ForMember(dest => dest.Status, opt =>
                opt.MapFrom(src =>
                    (!src.Checked ? "Pending" : (src.Checked && !src.Succeed) ? "Rejected" : "Succeed")))
            ;//.ForMember(dest => dest.CreatedAt, opt =>
               // opt.MapFrom(src => src.CreatedAt.ToString("g")));
         CreateMap<DateTime, DateTime>()
            .ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    }
    //"yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"
}