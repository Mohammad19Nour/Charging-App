using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class NewPaymentDto
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? Notes{ get; set; }
  [Required]  public double AddedValue { get; set; }
    
   [Required] public IFormFile ImageFile { get; set; }
    
    
}

public class NewCompanyPaymentDto : NewPaymentDto
{ 
    public string? Username { get; set; }
}
public class NewOfficePaymentDto : NewPaymentDto
{ 
    public string? Username { get; set; }
}
