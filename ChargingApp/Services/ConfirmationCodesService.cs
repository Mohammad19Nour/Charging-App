using System.Security.Cryptography;
using System.Text;

namespace ChargingApp.Services;

public class ConfirmationCodesService
{
    private static readonly Dictionary<string, (int userId,string token)> Map = new();
    private const int Size = 8;
    private static readonly char[] Chars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

    public static string GenerateCode(int userId, string token)
    {
        RemoveUserCodes(userId);
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

        Map.Add(result.ToString(), (userId, token));
        return result.ToString();
    }


    public static bool CheckIfExist(string code)
    {
        return Map.Keys.Any(x => x == code);
    }

    public static void RemoveUserCodes(int userId)
    {
        var list = Map.Where(x => x.Value.userId == userId).Select(x => x.Key).ToList();
        
        foreach (var c in list)
        {
            Map.Remove(c);
        }
    }
    
    public static (int userId,string token)? GetUserIdAndToken(string code)
    {
        if (!CheckIfExist(code)) return null;

        return Map[code];
    }
}