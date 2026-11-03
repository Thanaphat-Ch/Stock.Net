using System.ComponentModel.DataAnnotations;

namespace MyApiProject.Features.Products;

public class CreateProductDto
{
    [Required(ErrorMessage = "กรุณาระบุชื่อสินค้า")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "ราคาต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Range(0, 9999, ErrorMessage = "สต็อกต้องไม่ติดลบ")]
    public int Stock { get; set; }
}

public class UpdateProductDto : CreateProductDto { }

public record ProductResponseDto(int Id, string Name, decimal Price, int Stock);

public class ProductUpsertDto
{
    public int Id { get; set; } = 0;
    [Required(ErrorMessage = "กรุณาระบุชื่อสินค้า")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "ราคาต้องมากกว่า 0")]
    public decimal Price { get; set; }

    [Range(0, 9999, ErrorMessage = "สต็อกต้องไม่ติดลบ")]
    public int Stock { get; set; }
}