using ChargingApp.Data;
using ChargingApp.Entity;
using ChargingApp.Errors;
using ChargingApp.Extentions;
using ChargingApp.Middleware;
using ChargingApp.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
    
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>
{
    builder
        .AllowAnyOrigin()
         .WithOrigins("https://app.quickstoreqs.com")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed((_) => true);
}));

builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API", Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value != null && e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        var errorResponse = new ApiValidationResponse
        {
            Error = errors
        };
        return new BadRequestObjectResult(errorResponse);
    };
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("v1/swagger.json", "MyAPI V1");
    });
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseStatusCodePagesWithReExecute("/errors/{0}");

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(x => x
    .AllowAnyOrigin()
    .WithOrigins("https://app.quickstoreqs.com")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .SetIsOriginAllowed((_) => true));

app.UseAuthentication();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("/hubs");


using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    
    var directory = Path.Combine(Directory.GetCurrentDirectory(), "directory");

    if (!Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
    var context = services.GetRequiredService<DataContext>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await Seed.SeedRoles(roleManager);
    await Seed.SeedUsers(userManager, roleManager);
  //  await Seed.SeedCategories(context);
    await Seed.SeedVipLevels(context);
  //  await Seed.SeedProducts(context);
    await Seed.SeedPayments(context);
    await Seed.SeedPaymentMethods(context);
  //  await Seed.SeedCompanies(context);
    await Seed.SeedCurrency(context);
  //  await Seed.SeedOurAgents(context);
    await Seed.SeedSites(context); 
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

await app.RunAsync();