using AutoMapper;
using Charging_App.DTOs;
using Charging_App.Entity;

namespace Charging_App.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, UserDto>();
        CreateMap<UpdateUserInfoDto, AppUser>();
        CreateMap<RegisterDto, AppUser>();
    }
}