namespace ChargingApp.DTOs;


public class NewAgentDto
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
}

public class AgentDto : NewAgentDto
{
    public int AgentId { get; set; }
}