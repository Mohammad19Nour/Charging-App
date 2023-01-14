using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace ChargingApp.Services;

public class PhotoService :IPhotoService
{
    /* private readonly Cloudinary _cloudinary;
    
    public PhotoService(IOptions<CloudinarySettings>config)
    {
        var acc = new Account(
            config.Value.CloudName,
            config.Value.ApiKey,
            config.Value.ApiSecret
            );
        _cloudinary = new Cloudinary(acc);
    }*/

   public PhotoService()
   {
   }

    public async Task<(bool , string ,string)> AddPhotoAsync(IFormFile file)
    {
        try
        {
            var fileName = file.FileName;

            fileName =  DateTime.Now.ToString("yyyyMMddHHmmss_") + Path.GetFileName(fileName);

            var uploadPath = Path.Combine("wwwroot/images/", fileName);

            var stream = new FileStream(uploadPath, FileMode.Create);

            await file.CopyToAsync(stream);
            await stream.DisposeAsync();
            
            var imageLink ="/images/" + fileName;
            return (true,imageLink , "done");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false,"", "Failed tp upload photo");
        }
      /*  var uploadResult = new ImageUploadResult();

        if (file.Length > 0)
        { 
            using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.Name , stream),
                Transformation = new Transformation().Height(500).Width(500).Crop("fill").Gravity("face")
            };

            try
            {

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
                throw new Exception("Failed to upload photo.. check your internet");
            }
        }

        return uploadResult;*/
    }
    public async Task<bool> DeletePhotoAsync(string name)
    {
       // var deleteParams = new DeletionParams(publicId);
        //var result = await _cloudinary.DestroyAsync(deleteParams);

       // name = name[8..];
        if (File.Exists("wwwroot" + name))//check file exsit or not  
        {  
            File.Delete( "wwwroot" +name);
            return true;
        }  
        Console.WriteLine(name);
        return false;
    }
}