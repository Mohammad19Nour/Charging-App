using AutoMapper;
using AutoMapper.Execution;
using ChargingApp.Entity;

namespace ChargingApp.Helpers;

public class PhotoProjectionResolver : ITypeConverter<Photo, String>
{
    public string Convert(Photo source, string destination, ResolutionContext context)
    {
        // Your implementation here to convert the Photo object to a string URL
        return "some-url-to-photo";
    }
}