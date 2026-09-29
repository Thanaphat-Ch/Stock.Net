using Microsoft.AspNetCore.Mvc;
using MyApiProject.Features.Products;

namespace MyApiProject.Features.ProductsApi;

[ApiController]
[Route("api/products")]
public class ProductsApiController : ControllerBase
{
    private readonly IProductService _productService;
    public ProductsApiController(IProductService productService) => _productService = productService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _productService.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _productService.GetByIdAsync(id);
        if (item is null) return NotFound(new { message = "ไม่พบสินค้า" });
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var created = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var success = await _productService.UpdateAsync(id, dto);
        if (!success) return NotFound(new { message = "ไม่พบสินค้าที่จะแก้ไข" });
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _productService.DeleteAsync(id);
        if (!success) return NotFound(new { message = "ไม่พบสินค้าที่จะลบ" });
        return NoContent();
    }
}