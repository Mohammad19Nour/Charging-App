namespace ChargingApp.DTOs;

public class RechargeMethodDto
{
    public int MethodId { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public List<AgentDto>? Agents { get; set; } 
}