using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface ISliderRepository
{
    public void AddPhoto(SliderPhoto photo);
    public void DeletePhoto(SliderPhoto photo);
    public Task<List<SliderPhotoDto>> GetSliderPhotosAsync();
    public Task<SliderPhoto?> GetPhotoNoTrackingByIdAsync(int photoId);
}