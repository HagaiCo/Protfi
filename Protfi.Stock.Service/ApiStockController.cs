using Microsoft.AspNetCore.Mvc;
using Protfi.Stock.Service.Interfaces;

namespace Protfi.Stock.Service;

[ApiController]
[Route("api/stock")]
public class ApiStockController : Controller
{
    private readonly IStockService _stockService;
    public ApiStockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpGet]
    [Route("GetSummery")]
    public async Task<IActionResult> GetSummery()
    {
        var result = await _stockService.GetSummery();
        return Ok(result);
    }
    
    [HttpGet]
    [Route("GetDailyOpenClose/{symbol}/{from:datetime}/{to:datetime}")]
    public async Task<IActionResult> GetDailyOpenClose(string symbol, DateTime from , DateTime to)
    {
        var result = await _stockService.GetDailyOpenClose(symbol, from, to);
        return Ok(result);
    }
    
    [HttpGet]
    [Route("GetLastTrade/{symbol}")]
    public async Task<IActionResult> GetLastTrade(string symbol)
    {
        var result = await _stockService.GetLastTrade(symbol);
        return Ok(result);
    }
}