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
                
            return View(salesReports);
        }
    }
}