using ChargingApp.Entity;

namespace ChargingApp.DTOs;

public class PaymentAndRechargeMethodDto
{
    public List<RechargeMethodDto> ForRecharge { get; set; }
    public  List<PaymentGatewayDto> ForPaymentAndRecharge { get; set; }
}