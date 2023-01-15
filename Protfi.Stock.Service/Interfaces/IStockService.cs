using System.Text.Json.Nodes;
using Protfi.Common;

namespace Protfi.Stock.Service.Interfaces;

public interface IStockService : IServiceBase
{
    public Task<JsonNode> GetSummery();
    public Task<JsonNode> GetDailyOpenClose(string symbol, DateTime from, DateTime to);
    public Task<JsonNode> GetLastTrade(string symbol);
}