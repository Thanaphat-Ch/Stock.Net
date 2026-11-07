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
        var products = await _service.GetAllAsync();
        return View(products);
    }

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Create(CreateProductDto dto)
    //{
    //   if (ModelState.IsValid)
    //   {
    //       await _service.CreateAsync(dto);
    //       return RedirectToAction(nameof(Index));
    //   }

    //   var products = await _service.GetAllAsync();
    //   return View(nameof(Index), products);
    //}

    [Authorize(Roles = "Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustStock(int id, int amount)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null) return NotFound();
        int newStock = Math.Max(0, product.Stock + amount);

        var updateDto = new UpdateProductDto
        {
            Name = product.Name,
            Price = product.Price,
            Stock = newStock
        };

        await _service.UpdateAsync(id, updateDto);
        return RedirectToAction(nameof(Index));
    }
    
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Edit(int id, UpdateProductDto dto)
    //{
    //   if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

    //   await _service.UpdateAsync(id, dto);
    //   return RedirectToAction(nameof(Index));
    //}

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Staff")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ProductUpsertDto dto)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));

        if (dto.Id == 0)
        {
            var createDto = new CreateProductDto
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            await _service.CreateAsync(createDto);
        }
        else
        {
            var updateDto = new UpdateProductDto
            {
                Name = dto.Name,
                Price = dto.Price,
                Stock = dto.Stock
            };

            await _service.UpdateAsync(dto.Id, updateDto);
        }
        return RedirectToAction(nameof(Index));
    }
}