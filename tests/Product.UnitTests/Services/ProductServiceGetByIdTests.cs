using Product.Application.Exceptions;
using Product.Application.Services;
using Product.Domain.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.UnitTests.Services;

public class ProductServiceGetByIdTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _sut;

    public ProductServiceGetByIdTests()
    {
        _sut = new ProductService(_repo);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        var id = Guid.NewGuid();
        _repo.Products.Add(new ProductEntity
        {
            Id = id,
            Name = "Test Product",
            Sku = "SKU-TEST",
            Price = 19.99m,
            StockQuantity = 5,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });

        var response = await _sut.GetByIdAsync(id);

        Assert.Equal(id, response.Id);
        Assert.Equal("Test Product", response.Name);
        Assert.Equal("SKU-TEST", response.Sku);
        Assert.Equal(19.99m, response.Price);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsProductNotFoundException()
    {
        var unknownId = Guid.NewGuid();

        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.GetByIdAsync(unknownId));
    }
}
