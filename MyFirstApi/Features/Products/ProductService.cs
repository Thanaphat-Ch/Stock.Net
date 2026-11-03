using Microsoft.EntityFrameworkCore;
using MyApiProject.Models;

namespace MyApiProject.Features.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        return await _db.Products2
            .AsNoTracking() 
            .Select(p => new ProductResponseDto(p.Id, p.Name, p.Price, p.Stock))
            .ToListAsync();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var p = await _db.Products2.FindAsync(id);
        return p is null ? null : new ProductResponseDto(p.Id, p.Name, p.Price, p.Stock);
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Price = dto.Price,
            Stock = dto.Stock
        };

        _db.Products2.Add(product);
        await _db.SaveChangesAsync();
        return new ProductResponseDto(product.Id, product.Name, product.Price, product.Stock);
    }

    public async Task<bool> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await _db.Products2.FindAsync(id);
        if (product is null) return false;

        product.Name = dto.Name;
        product.Price = dto.Price;
        product.Stock = dto.Stock;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _db.Products2.FindAsync(id);
        if (product is null) return false;

        _db.Products2.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }
}