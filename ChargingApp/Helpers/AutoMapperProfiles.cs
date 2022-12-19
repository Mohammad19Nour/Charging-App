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

        CreateMap<Category, CategoryDto>();
        CreateMap<Category, CategoryWithProductsDto>();
        CreateMap<RechargeMethod, RechargeMethodDto>()
            .ForMember(dest=>dest.Agents,opt=>
                opt.MapFrom(src=>src.ChangerAndCompanies))
            .ForMember(dest=>dest.MethodId,opt=>
                opt.MapFrom(src=>src.Id));
        CreateMap<ChangerAndCompany, AgentDto>()
            .ForMember(dest=>dest.AgentId,opt=>
                opt.MapFrom(src=>src.Id));

        
        CreateMap<Photo, string>().ConvertUsing(p => p.Url ?? "No Photo");

        CreateMap<Product, ProductDto>();
       
        CreateMap<Quantity,int>().ConvertUsing(q=>q.Value);
       // CreateMap<JsonPatchDocument<ProductToUpdateDto>, JsonPatchDocument<Product>>();
       // CreateMap<Operation<ProductToUpdateDto>, Operation<Product>>();
        CreateMap<Payment, PaymentDto>();
        CreateMap<CategoryUpdateDto, Category>();
        CreateMap<Order, OrderDto>() 
            .ForMember(dest => dest.ProductName, opt =>
            opt.MapFrom(src => src.Product.EnglishName));
        ;
        CreateMap<Order, NormalOrderDto>().ForMember(dest => dest.ProductName, opt =>
            opt.MapFrom(src => src.Product.EnglishName));
           
    }
}