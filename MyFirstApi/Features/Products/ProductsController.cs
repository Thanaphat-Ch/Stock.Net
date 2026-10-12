using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace MyApiProject.Features.Products;

public class ProductsController : Controller
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        Log.Information("page /product");
        var products = await _service.GetAllAsync();
        return View(products);
    }

    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        if (ModelState.IsValid)
        {
            await _service.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        var products = await _service.GetAllAsync();
        return View(nameof(Index), products);
    }

    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, int amount)
    {
        var product = await _service.GetByIdAsync(id);
        if (product != null)
        {
            var updateDto = new UpdateProductDto
            {
                Name = product.Name,
                Price = product.Price,
                Stock = Math.Max(0, product.Stock + amount)
            };
            await _service.UpdateAsync(id, updateDto);
        }
        return RedirectToAction(nameof(Index));
    }

    // [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}