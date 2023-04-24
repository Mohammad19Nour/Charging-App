/*using System.Reflection;
using AutoMapper;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class PhotoResolver<S, D> : IValueResolver<S, D, string> where S : class where D : class

{
    private readonly IConfiguration _configuration;
    private readonly string photoUrl;


    public PhotoResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Resolve(S source, D destination, string destMember, ResolutionContext context)
    {
        PropertyInfo photoProperty = source.GetType().GetProperty("Photo");

        if (photoProperty == null)
        {
            return "No Photo";
        }

        var photoValue = photoProperty.GetValue(source, null) as Photo;

        if (photoValue?.Url == null)
        {
            return "No Photo";
        }

        return _configuration["ApiUrl"] + photoValue.Url;
    }
}*/