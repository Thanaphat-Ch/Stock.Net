using Microsoft.AspNetCore.Mvc;

namespace MyApiProject.Features.Products;

public class PosController : Controller
{
    private readonly IProductService _service;
    public PosController(IProductService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var products = await _service.GetAllAsync();
        return View(products);
    }
}