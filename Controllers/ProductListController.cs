using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarungElja.Data;
using WarungElja.Models;

namespace WarungElja.Controllers
{
    public class ProductListController : Controller
    {
        private readonly AppDbContext _context;

        public ProductListController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.ProductDetails
                .Include(p => p.ProductStocks)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productDetails = await _context.ProductDetails
                .Include(p => p.ProductStocks)
                .FirstOrDefaultAsync(m => m.IdProductDetails == id);
            if (productDetails == null)
            {
                return NotFound();
            }

            return View(productDetails);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductName,Price")] ProductDetails productDetails)
        {
            if (ModelState.IsValid)
            {
                productDetails.Status = 0;
                _context.Add(productDetails);
                await _context.SaveChangesAsync();

                var productStock = new ProductStock
                {
                    IdProductDetails = productDetails.IdProductDetails,
                    Stock = 0,
                };
                _context.ProductStock.Add(productStock);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(productDetails);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productDetails = await _context.ProductDetails.FindAsync(id);
            if (productDetails == null)
            {
                return NotFound();
            }
            return View(productDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProductDetails,ProductName,Price,Status")] ProductDetails productDetails)
        {
            if (id != productDetails.IdProductDetails)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productDetails);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductDetailsExists(productDetails.IdProductDetails))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(productDetails);
        }

        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productDetails = await _context.ProductDetails
                .FirstOrDefaultAsync(m => m.IdProductDetails == id);
            if (productDetails == null)
            {
                return NotFound();
            }

            return View(productDetails);
        }

        [HttpPost, ActionName("Deactivate")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var productDetails = await _context.ProductDetails.FindAsync(id);
            if (productDetails != null)
            {
                productDetails.Status = 0;
                _context.ProductDetails.Update(productDetails);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> StockUpdate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productStock = await _context.ProductStock
                .Include(ps => ps.ProductDetails)
                .FirstOrDefaultAsync(ps => ps.IdProductDetails == id);

            if (productStock == null)
            {
                productStock = new ProductStock
                {
                    IdProductDetails = id.Value,
                    Stock = 0,
                };

                var productDetails = await _context.ProductDetails.FindAsync(id);
                if (productDetails != null)
                {
                    productStock.ProductDetails = productDetails;
                }
            }

            var viewModel = new StockUpdateViewModel
            {
                IdProductDetails = productStock.IdProductDetails,
                ProductName = productStock.ProductDetails?.ProductName,
                CurrentStock = productStock.Stock
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockUpdate(int id, StockUpdateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var existingProductStock = await _context.ProductStock
                    .FirstOrDefaultAsync(ps => ps.IdProductDetails == id);

                if (existingProductStock == null)
                {
                    existingProductStock = new ProductStock
                    {
                        IdProductDetails = id,
                        Stock = viewModel.AddStock, 
                    };
                    _context.ProductStock.Add(existingProductStock);
                    
                    if (viewModel.AddStock > 0)
                    {
                        var productDetailsEntity = await _context.ProductDetails.FindAsync(id);
                        if (productDetailsEntity != null)
                        {
                            productDetailsEntity.Status = 1;
                            _context.ProductDetails.Update(productDetailsEntity);
                        }
                    }
                }
                else
                {
                    existingProductStock.Stock += viewModel.AddStock;
                    _context.ProductStock.Update(existingProductStock);

                    if (existingProductStock.Stock > 0)
                    {
                        var productDetailsEntity = await _context.ProductDetails.FindAsync(id);
                        if (productDetailsEntity != null)
                        {
                            productDetailsEntity.Status = 1;
                            _context.ProductDetails.Update(productDetailsEntity);
                        }
                    }
                    else if (existingProductStock.Stock == 0)
                    {
                        var productDetailsEntity = await _context.ProductDetails.FindAsync(id);
                        if (productDetailsEntity != null)
                        {
                            productDetailsEntity.Status = 0;
                            _context.ProductDetails.Update(productDetailsEntity);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var productDetails = await _context.ProductDetails.FindAsync(id);
            viewModel.ProductName = productDetails?.ProductName;
            
            var currentProductStock = await _context.ProductStock
                .FirstOrDefaultAsync(ps => ps.IdProductDetails == id);
            viewModel.CurrentStock = currentProductStock?.Stock ?? 0;

            return View(viewModel);
        }

        private bool ProductDetailsExists(int id)
        {
            return _context.ProductDetails.Any(e => e.IdProductDetails == id);
        }
    }
}