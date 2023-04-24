using System.Reflection;
using AutoMapper;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class PhotoConverter<T> : ITypeConverter<T, object> where T : class
{
    private readonly IConfiguration _config;

    public PhotoConverter(IConfiguration config)
    {
        _config = config;
    }

    public object Convert(T source, object destination, ResolutionContext context)
    {
        PropertyInfo photoProperty = destination.GetType().GetProperty("Photo");

        if (photoProperty == null)
        {
            return "No Photo";
        }

        var photoValue = photoProperty.GetValue(destination, null) as Photo;

        if (photoValue == null || photoValue.Url == null)
        {
            return "No Photo";
        }

        return _config["ApiUrl"] + photoValue.Url;
    }
}