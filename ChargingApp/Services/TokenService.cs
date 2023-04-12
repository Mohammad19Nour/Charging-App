using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace ChargingApp.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<AppUser> _userManager;
    private SymmetricSecurityKey _key;
 
    public TokenService(UserManager<AppUser> userManager , IConfiguration config)
    {
        _userManager = userManager;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
    }

    public async Task<string> CreateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Email, user.Email),
       
        };

       
        var creds = new SigningCredentials(_key , SecurityAlgorithms.HmacSha384Signature);
 
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddYears(2),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}