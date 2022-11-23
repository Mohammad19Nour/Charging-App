namespace Charging_App.Services;

public static class GenerateRandomeCode
{
    private static Random _random = new Random();
    private static Dictionary<string, Dictionary<string, string>> map = new Dictionary<string, Dictionary<string, string>>();

    public static string GenerateCode(string userId, string token)
    {
        int length = 7;

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789asdfghjklpoiuytrewqzxcvbnm";
        string s = "";
        for (int i = 1; i <= length; i++)
        {
            int id = _random.Next();
            id %= 62;
            s += chars[id];
        }
        map.Add(s, new Dictionary<string, string> { { userId, token } });
    return s;
    }

    public static bool CheckIfExist(string code)
    {
        return true;
    }
}