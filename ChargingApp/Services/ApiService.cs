using System.Net;
using System.Text;
using System.Text.Json;
using ChargingApp.Interfaces;

namespace ChargingApp.Services;

public class ApiService : IApiService
{
    private const string baseUrl = "https://api.fast-store.co/client/api";
    private const string token = "7515d6dd5ae4de7b4e7de94c55aea5a2ff17fa37f45da962";

    public async Task<bool> CheckProductByIdIfExistAsync(int id)
    {
        var list = await GetAllProductsAsync();
        return list != null && list.Any(x => x.id == id);
    }

    public async Task<(bool Success, string Message, int OrderId)> SendOrderAsync(int productId, double qty,
        string playerId)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("api-token",
            token);
        var response = await httpClient.GetAsync(baseUrl +
                                                 "/newOrder/" + productId + "/params?playerID=" + playerId + "&qty=" +
                                                 qty);

        var content = await response.Content.ReadAsByteArrayAsync();
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);

        if (response.IsSuccessStatusCode)
        {
            var data = jsonElement.GetProperty("data");
            var id = data.GetProperty("order_id").ToString();
            var orderId = int.Parse(id);
            return (true, "Waiting", orderId);
        }

        var str = jsonElement.GetProperty("msg").ToString();
        var message = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(str));

        return (false, message, 1);
    }

    public async Task<(bool Succeed, string Status )> CheckOrderStatusAsync(int orderId)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-token",
                token);
            var response = await httpClient.GetAsync(baseUrl + "/check?orders=[" + orderId + "]");

            var content = await response.Content.ReadAsByteArrayAsync();

            var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);
            var dataList = jsonElement.GetProperty("data")
                .EnumerateArray().ToList();
            if (response.IsSuccessStatusCode)
            {
                return dataList.Count == 0
                    ? (false, "order not found")
                    : (true, dataList[0].GetProperty("status").ToString());
            }

            return (false, "order not found");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<ProductResponse>?> GetAllProductsAsync()
    {
        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        httpClient.DefaultRequestHeaders.Add("api-token",
            token);
        var response = await httpClient.GetAsync(baseUrl + "/products");
        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<ProductResponse>>(content);
    }

    public async Task<(bool Success, string Message)> CancelOrderByIdAsync(int apiOrderId)
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("api-token",
                token);
            var response = await httpClient.GetAsync(baseUrl +
                                                     "/order/cancel?id=" + apiOrderId);

           // Console.WriteLine(response.StatusCode+"\n\n\n\n000\n\n"+apiOrderId);
            return response.StatusCode != HttpStatusCode.OK
                ? (false, "Can't cancel this order now")
                : (true, "Done");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public class ProductResponse
    {
        public int id { get; set; }
        public double price { get; set; }
        public string name { get; set; }
    }
}