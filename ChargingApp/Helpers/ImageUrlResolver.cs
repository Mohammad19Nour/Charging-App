using System.Reflection;
using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class ImageUrlResolver : IValueResolver<object , object , string>
{
    private readonly IConfiguration _config;

    public ImageUrlResolver(IConfiguration config)
    {
        _config = config;
    }

    public string Resolve(object source, object destination, string destMember, ResolutionContext context)
    {
        if (source is Product)
        {
            Console.WriteLine("product+\n\n\n\n\n\n");
            var product = (Product)source;
            var productDto = (ProductDto)destination;
            if (product.Photo is null)
                return "nono";
            return "fr" + product.Photo.Url;
        }

        if (source is Category)
        {
            Console.WriteLine("product+\n\n\n\n\n\n");
            return "ggg";
        }
        else
        {
            
        }

        return "l";
    }
}