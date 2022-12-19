using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class UpdateUserInfoDto
{
     public string? FirstName { get; set; }
     public string? LastName { get; set; }
     public string? Country { get; set; }
     public string? City { get; set; }
}