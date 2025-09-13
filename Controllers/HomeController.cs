using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WarungElja.Models;
using WarungElja.Data;
using Microsoft.EntityFrameworkCore;

namespace WarungElja.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalProducts = await _context.ProductDetails.CountAsync();
        var activeProducts = await _context.ProductDetails.CountAsync(p => p.Status == 1);
        var totalStock = await _context.ProductStock.SumAsync(ps => (int?)ps.Stock) ?? 0;
        var todaySales = await _context.SalesRecords
            .Where(sr => sr.SaleDate.Date == DateTime.Today)
            .SumAsync(sr => (int?)sr.QuantitySold) ?? 0;
        
        var recentSales = await _context.SalesRecords
            .Include(sr => sr.ProductDetails)
            .OrderByDescending(sr => sr.SaleDate)
            .Take(5)
            .ToListAsync();

        ViewBag.TotalProducts = totalProducts;
        ViewBag.ActiveProducts = activeProducts;
        ViewBag.TotalStock = totalStock;
        ViewBag.TodaySales = todaySales;
        ViewBag.RecentSales = recentSales;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
