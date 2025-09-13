using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using WarungElja.Data;
using WarungElja.Models;

namespace WarungElja.Controllers
{
    [Authorize]
    public class SalesReportController : Controller
    {
        private readonly AppDbContext _context;

        public SalesReportController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var salesReports = await _context.SalesRecords
                .Include(s => s.ProductDetails)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
                                
            var totalSales = salesReports.Count;
            var totalRevenue = salesReports.Sum(s => s.Price * s.QuantitySold);
            var todaySales = salesReports.Where(s => s.SaleDate.Date == DateTime.Today).ToList();
            var todayRevenue = todaySales.Sum(s => s.Price * s.QuantitySold);
            
            var topProducts = salesReports
                .GroupBy(s => new { s.ProductDetailsId, s.ProductDetails.ProductName })
                .Select(g => new {
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(s => s.QuantitySold),
                    TotalRevenue = g.Sum(s => s.Price * s.QuantitySold)
                })
                .OrderByDescending(g => g.TotalQuantity)
                .Take(5)
                .ToList();

            var salesTrend = salesReports
                .GroupBy(s => s.SaleDate.Date)
                .Select(g => new {
                    Date = g.Key.ToString("MMM dd"),
                    TotalSales = g.Sum(s => s.QuantitySold),
                    TotalRevenue = g.Sum(s => s.Price * s.QuantitySold)
                })
                .OrderBy(g => g.Date)
                .Take(7)
                .ToList();

            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TodaySales = todaySales.Count;
            ViewBag.TodayRevenue = todayRevenue;
            ViewBag.TopProducts = topProducts;
            ViewBag.SalesTrend = salesTrend;

            return View(salesReports);
        }
    }
}