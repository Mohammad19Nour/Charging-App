﻿namespace ChargingApp.DTOs;


public class NewAgentDto
{
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public IFormFile ImageFile { get; set; }
}

public class AgentDto
{
    
    public int AgentId { get; set; }
    public string ArabicName { get; set; }
    public string EnglishName { get; set; }
    public string Photo { get; set; }
}