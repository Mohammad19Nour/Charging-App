using System.ComponentModel.DataAnnotations;

namespace ChargingApp.DTOs;

public class NewPaymentDto
{
    public string CreatedDate { get; set; }
    public string? Notes { get; set; }
    [Required] public decimal AddedValue { get; set; }
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