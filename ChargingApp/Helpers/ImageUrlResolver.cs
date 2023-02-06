using System.Reflection;
using AutoMapper;
using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class ImageUrlResolver : IValueResolver<Product,ProductDto, string?>
{
    private readonly IConfiguration _config;

    public ImageUrlResolver(IConfiguration config)
    {
        _config = config;
    }

    public string? Resolve(Product source, ProductDto destination, string? destMember, ResolutionContext context)
    {
        return source.Photo == null ? "No Photo" : source.Photo.Url;
    }
}