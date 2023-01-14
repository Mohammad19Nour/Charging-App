using CloudinaryDotNet.Actions;

namespace ChargingApp.Interfaces;

public interface IPhotoService
{
    Task< (bool Success , string Url,string Message)> AddPhotoAsync(IFormFile file);
    Task<bool> DeletePhotoAsync(string publicId);
}