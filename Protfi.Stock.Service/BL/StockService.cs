using System.Text.Json.Nodes;
using Protfi.Common;
using Protfi.Stock.Service.Interfaces;

namespace Protfi.Stock.Service.BL;

public class StockService : ServiceBase, IStockService
{
    private const string StockUrl = "https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks";
    public async Task<JsonNode> GetSummery()
    {
        var dateTimeNow = DateTime.UtcNow.AddDays(-1);
        var todayDate = dateTimeNow.ToString("yyyy-MM-dd");

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{StockUrl}/{todayDate}?adjusted=true&apiKey={Consts.PolygonAppKey}")
        };
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var jsonResult = JsonNode.Parse(body);
        
        return jsonResult ?? throw new InvalidOperationException();
    }
    
    public async Task<JsonNode> GetDailyOpenClose(string symbol, DateTime from, DateTime to)
    {
        var symbolUpper = symbol.ToUpper();

        var fromSpecificFormat = from.ToString("yyyy-MM-dd");
        var toSpecificFormat = to.ToString("yyyy-MM-dd");
        var uri = $"https://quotient.p.rapidapi.com/equity/daily?symbol={symbolUpper}&from={fromSpecificFormat}&to={toSpecificFormat}&adjust=false";
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(uri),
            Headers =
            {
                { "X-RapidAPI-Key", Consts.QuotientAppKey },
                { "X-RapidAPI-Host", "quotient.p.rapidapi.com" },
            },
        };

        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        var jsonResult = JsonNode.Parse(body);
        
        return jsonResult ?? throw new InvalidOperationException();
    }
    
    public async Task<JsonNode> GetLastTrade(string symbol)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"https://api.polygon.io/v2/last/trade/{symbol}?apiKey={Consts.PolygonAppKey}")
        };
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();
        var jsonResult = JsonNode.Parse(body);
        
        return jsonResult ?? throw new InvalidOperationException();
    }
}