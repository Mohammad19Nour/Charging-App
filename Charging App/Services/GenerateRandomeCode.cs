namespace Charging_App.Services;

public static class GenerateRandomeCode
{
    private static Random _random = new Random();
    private static Dictionary<string, Dictionary<int, string>?> map = new
        Dictionary<string, Dictionary<int, string>?>();

    public static string GenerateCode(int userId, string token)
    {
        var length = 7;

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789asdfghjklpoiuytrewqzxcvbnm";
        var s = "";

        while (true)
        {
             s = "";
            for (var i = 1; i <= length; i++)
            {
                var id = _random.Next();
                id %= 62;
                s += chars[id];
            }

            if (map.ContainsKey(s))continue;
            break;
        }

        map.Add(s, new Dictionary<int, string> { { userId, token } });
    return s;
    }

    public static bool CheckIfExist(string code)
    {
        if (map.Keys.FirstOrDefault(x => x == code) == null) return false;
        return true;
    }

    public static void DeleteCode(string code)
    {
        int userId = map[code].Keys.First();

        List<string> list = new List<string>();
        
        foreach (var x in map)
        {
            var c = x.Key;
            var id = x.Value.Keys.First();
            var val = x.Value[id];

            if (id == userId) list.Add(c);
        }

        foreach (var c in list)
        {
            map.Remove(c);
        }
        return;
    }

    public static Dictionary<int, string>? GetUserIdAndToken(string code)
    {
        if (!CheckIfExist(code)) return null;

        return map[code];

    }

}