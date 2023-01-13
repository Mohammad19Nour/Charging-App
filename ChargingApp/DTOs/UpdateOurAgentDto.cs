using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class UpdateOurAgentDto
{
    [Required] public int Id { get; set; }
    public string? ArabicName { get; set; }
    public string? EnglishName { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
}