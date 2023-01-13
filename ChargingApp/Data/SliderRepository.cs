using AutoMapper;
using AutoMapper.QueryableExtensions;
using ChargingApp.DTOs;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class SliderRepository : ISliderRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public SliderRepository(DataContext context , IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddPhoto(SliderPhoto photo)
    {
        _context.SliderPhotos.Add(photo);
    }

    public void DeletePhoto(SliderPhoto photo)
    {
        _context.SliderPhotos.Remove(photo);
    }

    public async Task<List<SliderPhotoDto>?> GetSliderPhotosAsync()
    {
        return await _context.SliderPhotos
            .Include(p => p.Photo)
            .ProjectTo<SliderPhotoDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<SliderPhoto?> GetPhotoNoTrackingByIdAsync(int photoId)
    {
        return await _context.SliderPhotos
            .Include(x=>x.Photo)
            .Where(x => x.PhotoId == photoId)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}