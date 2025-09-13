using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarungElja.Data;
using WarungElja.Models;

namespace WarungElja.Controllers
{
    public class CashierController : Controller
    {
        private readonly AppDbContext _context;

        public CashierController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.ProductDetails
                .Include(p => p.ProductStocks)
                .Where(p => p.Status == 1 && p.ProductStocks.Any(ps => ps.Stock > 0))
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Sell(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productDetails = await _context.ProductDetails
                .Include(p => p.ProductStocks)
                .FirstOrDefaultAsync(m => m.IdProductDetails == id && m.Status == 1 && m.ProductStocks.Any(ps => ps.Stock > 0));

            if (productDetails == null)
            {
                return NotFound();
            }

            var viewModel = new CashierViewModel
            {
                IdProductDetails = productDetails.IdProductDetails,
                ProductName = productDetails.ProductName,
                Price = productDetails.Price,
                CurrentStock = productDetails.ProductStocks.Any() ? productDetails.ProductStocks.First().Stock : 0
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(int id, CashierViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var productStock = await _context.ProductStock
                    .FirstOrDefaultAsync(ps => ps.IdProductDetails == id);

                if (productStock != null)
                {
                    if (productStock.Stock >= viewModel.SellQuantity)
                    {
                        productStock.Stock -= viewModel.SellQuantity;
                        
                        // Record the sale in SalesRecords
                        var salesRecord = new SalesRecord
                        {
                            ProductDetailsId = id,
                            Price = viewModel.Price,
                            QuantitySold = viewModel.SellQuantity,
                            SaleDate = DateTime.Now
                        };
                        
                        _context.SalesRecords.Add(salesRecord);
                        _context.ProductStock.Update(productStock);

                        if (productStock.Stock == 0)
                        {
                            var productDetails = await _context.ProductDetails.FindAsync(id);
                            if (productDetails != null)
                            {
                                productDetails.Status = 0;
                                _context.ProductDetails.Update(productDetails);
                            }
                        }
                        
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"Successfully sold {viewModel.SellQuantity} units of {viewModel.ProductName}";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("SellQuantity", "Not enough stock available.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Product stock information not found.");
                }
            }

            var productDetailsEntity = await _context.ProductDetails.FindAsync(id);
            if (productDetailsEntity != null)
            {
                viewModel.ProductName = productDetailsEntity.ProductName;
                viewModel.Price = productDetailsEntity.Price;
                
                var productStock = await _context.ProductStock
                    .FirstOrDefaultAsync(ps => ps.IdProductDetails == id);
                viewModel.CurrentStock = productStock?.Stock ?? 0;
            }

            return View(viewModel);
        }
    }
}