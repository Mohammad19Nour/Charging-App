using ChargingApp.DTOs;
using ChargingApp.Entity;

namespace ChargingApp.Interfaces;

public interface IDebitRepository
{
    public void AddDebit(DebitHistory debit);
    public Task<List<DebitDto>> GetDebits(DateQueryDto dto , string? userEmail);
}