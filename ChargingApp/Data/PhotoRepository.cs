using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class PhotoRepository : IPhotoRepository
{
    private readonly DataContext _context;

    public PhotoRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Photo?> AddPhotoAsync(IFormFile file)
    {
        try
        {
            var fileName = file.FileName;

            fileName =  DateTime.Now.ToString("yyyyMMddHHmmss_") + Path.GetFileName(fileName);

            var uploadPath = Path.Combine("wwwroot/images/", fileName);

            var stream = new FileStream(uploadPath, FileMode.Create);

            await file.CopyToAsync(stream);
            await stream.DisposeAsync();
            
            var imageLink = "/images/" + fileName;
            var photo = new Photo
            {
                Url = imageLink
            };

            photo =  (await _context.Photos.AddAsync(photo)).Entity;
            
            return photo;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    public async Task<bool> DeletePhotoByIdAsync(int photoId)
    {
        var photo = await _context.Photos.FirstOrDefaultAsync(x => x.Id == photoId);

        if (photo == null) return false;

        _context.Photos.Remove(photo);
       
        return true;
    }
}