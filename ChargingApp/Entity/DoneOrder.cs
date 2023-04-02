namespace ChargingApp.Entity;

public class DoneOrder : BaseEntity
{
    public string Email { get; set; }
    public string? ProductEnglishName { get; set; }
    public string? ProductArabicName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
    public decimal TotalQuantity { get; set; } = 1; // total that user buy
    public string Photo { get; set; } = "NO Photo";
    public string? PlayerId { get; set; }
    public string PlayerName { get; set; }

    public bool CanChooseQuantity { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; } = 1; // total in system to calc benefit reports
    public string PaymentGateway { get; set; } = "";
    public string OrderType { get; set; } = "Normal";
    public string Notes { get; set; }= "Pending";
    
    public int Status { get; set; } = 0;//0-pending 1-succeed 2-rejected 3-wrong
    public int StatusIfCanceled { get; set; } = 0;
    /*0 not canceled
    1 canceled but not confirmed by admin
    2 canceled and accepted by admin
    3 canceled but rejected by admin
    */
}