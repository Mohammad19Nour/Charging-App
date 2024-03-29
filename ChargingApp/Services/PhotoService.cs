﻿using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace ChargingApp.Services;

public class PhotoService : IPhotoService
{
    public PhotoService()
    {
    }

    public async Task<(bool, string, string)> AddPhotoAsync(IFormFile file)
    {
        try
        {
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var fileName = file.FileName.Where(c => c != ' ').Aggregate("", (current, c) => current + c);

            fileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + Path.GetFileName(fileName);

            var uploadPath = Path.Combine("wwwroot/images/", fileName);

            var stream = new FileStream(uploadPath, FileMode.Create);

            await file.CopyToAsync(stream);
            await stream.DisposeAsync();

            var imageLink = "/images/" + fileName;
            return (true, imageLink, "done");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return (false, "", "Failed tp upload photo");
        }
    }

    public async Task<bool> DeletePhotoAsync(string name)
    {
        // var deleteParams = new DeletionParams(publicId);
        //var result = await _cloudinary.DestroyAsync(deleteParams);

        // name = name[8..];
        if (File.Exists("wwwroot" + name)) //check file exist or not  
        {
            File.Delete("wwwroot" + name);
            return true;
        }

        return false;
    }
}