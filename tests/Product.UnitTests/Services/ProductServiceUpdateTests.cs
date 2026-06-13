using Product.Application.DTOs;
using Product.Application.Exceptions;
using Product.Application.Services;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.UnitTests.Services;

public class ProductServiceUpdateTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _sut;

    public ProductServiceUpdateTests()
    {
        _sut = new ProductService(_repo);
    }

    [Fact]
    public async Task UpdateAsync_ValidRequest_MutatesEntityAndSetsUpdatedAtUtc()
    {
        var id = Guid.NewGuid();
        var original = new ProductEntity
        {
            Id = id,
            Name = "Old Name",
            Sku = "SKU-OLD",
            Price = 5m,
            StockQuantity = 1,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _repo.Products.Add(original);

        var before = DateTime.UtcNow;
        await _sut.UpdateAsync(id, new UpdateProductRequest
        {
            Name = "New Name",
            Sku = "SKU-NEW",
            Price = 99m,
            StockQuantity = 50,
            IsActive = false,
            Description = "Updated description"
        });
        var after = DateTime.UtcNow;

        Assert.Equal("New Name", original.Name);
        Assert.Equal("SKU-NEW", original.Sku);
        Assert.Equal(99m, original.Price);
        Assert.Equal(50, original.StockQuantity);
        Assert.False(original.IsActive);
        Assert.Equal("Updated description", original.Description);
        Assert.NotNull(original.UpdatedAtUtc);
        Assert.InRange(original.UpdatedAtUtc!.Value, before, after);
    }

    [Fact]
    public async Task UpdateAsync_UnknownId_ThrowsProductNotFoundException()
    {
        await Assert.ThrowsAsync<ProductNotFoundException>(() =>
            _sut.UpdateAsync(Guid.NewGuid(), new UpdateProductRequest
            {
                Name = "X",
                Sku = "X",
                Price = 1m,
                StockQuantity = 0,
                IsActive = true
            }));
    }
}
