using Product.Application.Exceptions;
using Product.Application.Services;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.UnitTests.Services;

public class ProductServiceDeleteTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _sut;

    public ProductServiceDeleteTests()
    {
        _sut = new ProductService(_repo);
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_RemovesItFromRepository()
    {
        var id = Guid.NewGuid();
        _repo.Products.Add(new ProductEntity
        {
            Id = id,
            Name = "To Delete",
            Sku = "DEL-001",
            Price = 1m,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _sut.DeleteAsync(id);

        Assert.Empty(_repo.Products);
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_ThrowsProductNotFoundException()
    {
        await Assert.ThrowsAsync<ProductNotFoundException>(() => _sut.DeleteAsync(Guid.NewGuid()));
    }
}
