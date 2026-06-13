using Product.Application.Services;
using Product.Domain.Entities;
using Product.Domain.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.UnitTests.Services;

public class ProductServiceGetPagedTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _sut;

    public ProductServiceGetPagedTests()
    {
        _sut = new ProductService(_repo);
    }

    [Fact]
    public async Task GetPagedAsync_NonEmptyCatalog_ReturnsCorrectPage()
    {
        for (var i = 1; i <= 5; i++)
        {
            _repo.Products.Add(new ProductEntity
            {
                Id = Guid.NewGuid(),
                Name = $"Product {i}",
                Sku = $"SKU-{i:000}",
                Price = i * 10m,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 2);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task GetPagedAsync_EmptyCatalog_ReturnsEmptyItemsWithZeroTotalCount()
    {
        var result = await _sut.GetPagedAsync(pageNumber: 1, pageSize: 20);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
