using ChargingApp.Data;
using ChargingApp.Helpers;
using ChargingApp.Interfaces;
using ChargingApp.Services;
using ChargingApp.SignalR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Extentions;

public static class ApplicationServiceExtensions
{
    public static void AddApplicationServices(this IServiceCollection services
        , IConfiguration config)
    {
        services.AddHostedService<BackgroundTask>();
        services.Configure<FormOptions>(opt =>
            opt.ValueCountLimit = int.MaxValue);
        
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<PresenceTracker>().AddSignalR();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IApiService , ApiService>();
        services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
        services.AddTransient<IEmailHelper, EmailSenderService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddDbContext<DataContext>(options =>
        {
            var connectionString =
                "Data source=chargingapp.db";
            var servConnectionString =
                "Data Source=SQL8001.site4now.net;Initial Catalog=db_a91f76_pop;User Id=db_a91f76_pop_admin;Password=Mohamed09914";

            options.UseSqlite(connectionString);
        });
        
       /* services.AddDataProtection()              
            .PersistKeysToFileSystem(new DirectoryInfo(@"h:\root\home\mohammad09nour-001\www\site1\directory\"))
            .UseCryptographicAlgorithms(
                new AuthenticatedEncryptorConfiguration
                {
                    EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                    ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
                });*/
    }
}