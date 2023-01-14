using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IPhotoRepository
{
    public Task<Photo?> AddPhotoAsync(IFormFile file);
    public Task<bool> DeletePhotoByIdAsync(int photoId);
}