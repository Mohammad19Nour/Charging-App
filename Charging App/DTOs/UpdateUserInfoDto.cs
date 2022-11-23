using System.ComponentModel.DataAnnotations;

namespace Charging_App.DTOs;

public class UpdateUserInfoDto
{
     public string? UserName { get; set; }
     public string? Country { get; set; }
     public string? City { get; set; }
}