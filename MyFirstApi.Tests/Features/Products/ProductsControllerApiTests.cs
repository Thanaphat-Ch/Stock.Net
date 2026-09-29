using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyApiProject.Features.Products;
using MyApiProject.Features.ProductsApi;
using Xunit;

namespace MyFirstApi.Tests.Features.ProductApi;

public class ProductsApiControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductsApiController _controller;

    public ProductsApiControllerTests()
    {
        // 1. จำลอง (Mock) Interface ไม่ให้ผูกกับ Service ตัวจริง
        _productServiceMock = new Mock<IProductService>();

        // 2. ส่ง Mock เข้าไปใน Controller ผ่าน DI Constructor
        _controller = new ProductsApiController(_productServiceMock.Object);
    }

    [Fact]
    public async Task GetById_WhenProductExists_ReturnsOkResultWithProduct()
    {
        // Arrange (เตรียมข้อมูลจำลอง)
        int targetId = 1;
        var mockProduct = new ProductResponseDto(targetId, "Keyboard", 1500m, 10);

        // กำหนดเงื่อนไขว่า ถ้า Controller เรียก GetByIdAsync(1) ให้ส่ง mockProduct กลับมา
        _productServiceMock
            .Setup(service => service.GetByIdAsync(targetId))
            .ReturnsAsync(mockProduct);

        // Act (ลงมือยิงคำสั่ง)
        var result = await _controller.GetById(targetId);

        // Assert (ตรวจสอบผลลัพธ์)
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedProduct = okResult.Value.Should().BeOfType<ProductResponseDto>().Subject;
        
        returnedProduct.Id.Should().Be(targetId);
        returnedProduct.Name.Should().Be("Keyboard");
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        int targetId = 999;

        // จำลองสถานการณ์ว่าหาของไม่เจอ (คืนค่า null)
        _productServiceMock
            .Setup(service => service.GetByIdAsync(targetId))
            .ReturnsAsync((ProductResponseDto?)null);

        // Act
        var result = await _controller.GetById(targetId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WhenValidDtoProvided_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "Mouse", Price = 500m, Stock = 5 };
        var createdResponse = new ProductResponseDto(10, "Mouse", 500m, 5);

        _productServiceMock
            .Setup(service => service.CreateAsync(createDto))
            .ReturnsAsync(createdResponse);

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
        createdResult.Value.Should().BeEquivalentTo(createdResponse);
    }
}