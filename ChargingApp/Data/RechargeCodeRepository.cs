﻿using System.Security.Cryptography;
using System.Text;
using ChargingApp.Entity;
using ChargingApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChargingApp.Data;

public class RechargeCodeRepository : IRechargeCodeRepository
{
    private const int Size = 8;

    private readonly char[] Chars =
        "yz8ABCabOPQRSTcd0efg1hijk2lmnHIJ3KLMNUopq9rst5uvw7x4DEF6GVWXYZ".ToCharArray();

    private readonly DataContext _context;

    public RechargeCodeRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<List<string>?> GenerateCodesWithValue(int numberOfCodes, int valueOfCode)
    {
        var c = 0;
        var codeList = new List<string>();

        for (int i = 1; i <= numberOfCodes; i++)
        {
            var code = GenerateCode();
            if (await GetCodeAsync(code) != null)
            {
                c++;
                if (c < numberOfCodes * 5)
                {
                    i--;
                    continue;
                }
            }

            _context.RechargeCodes.Add(new RechargeCode { Code = code, Value = valueOfCode });
            codeList.Add(code);
        }

        if (await _context.SaveChangesAsync() <= 0) return null;
        return codeList;
    }

    public async Task<RechargeCode?> GetCodeAsync(string code)
    {
        var tmpCode = await _context.RechargeCodes.FirstOrDefaultAsync(x => x.Code == code);

        return tmpCode;
    }

    public async Task<List<RechargeCode>> GetCodesForUserAsync(int userId)
    {
        return await _context.RechargeCodes
            .Where(x => x.User != null)
            .Where(x => x.User.Id == userId).ToListAsync();
    }

    public void Update(RechargeCode code)
    {
        _context.RechargeCodes.Update(code);
    }

    private string GenerateCode()
    {
        var data = new byte[4 * Size];
        using (var crypto = RandomNumberGenerator.Create())
        {
            crypto.GetBytes(data);
        }

        var result = new StringBuilder(Size);
        for (var i = 0; i < Size; i++)
        {
            var rnd = BitConverter.ToUInt32(data, i * 4);
            var idx = rnd % Chars.Length;

            result.Append(Chars[idx]);
        }

        return result.ToString();
    }
}