using Product.Application.DTOs;
using Product.Application.Exceptions;
using Product.Application.Services;
using Product.Domain.Interfaces;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.UnitTests.Services;

public class ProductServiceCreateTests
{
    private readonly FakeProductRepository _repo = new();
    private readonly ProductService _sut;

    public ProductServiceCreateTests()
    {
        _sut = new ProductService(_repo);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsProductResponse()
    {
        var request = new CreateProductRequest
        {
            Name = "Widget",
            Sku = "WGT-001",
            Price = 9.99m,
            StockQuantity = 10,
            IsActive = true
        };

        var response = await _sut.CreateAsync(request);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal("Widget", response.Name);
        Assert.Equal("WGT-001", response.Sku);
        Assert.Equal(9.99m, response.Price);
        Assert.Equal(10, response.StockQuantity);
        Assert.True(response.IsActive);
        Assert.Single(_repo.Products);
    }

    [Fact]
    public async Task CreateAsync_DuplicateSku_ThrowsDuplicateSkuException()
    {
        _repo.Products.Add(new ProductEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing",
            Sku = "DUP-001",
            Price = 1m,
            CreatedAtUtc = DateTime.UtcNow
        });

        var request = new CreateProductRequest
        {
            Name = "New Widget",
            Sku = "DUP-001",
            Price = 5m
        };

        await Assert.ThrowsAsync<DuplicateSkuException>(() => _sut.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAtUtcToUtcValue()
    {
        var before = DateTime.UtcNow;

        var response = await _sut.CreateAsync(new CreateProductRequest
        {
            Name = "Timestamp Test",
            Sku = "TS-001",
            Price = 1m
        });

        var after = DateTime.UtcNow;
        Assert.Equal(DateTimeKind.Utc, response.CreatedAtUtc.Kind);
        Assert.InRange(response.CreatedAtUtc, before, after);
    }
}

internal sealed class FakeProductRepository : IProductRepository
{
    public List<ProductEntity> Products { get; } = [];

    public Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Products.FirstOrDefault(p => p.Id == id));

    public Task<(IReadOnlyList<ProductEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var items = Products.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult<(IReadOnlyList<ProductEntity>, int)>((items, Products.Count));
    }

    public Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default)
        => Task.FromResult(Products.Any(p => p.Sku == sku));

    public Task AddAsync(ProductEntity product, CancellationToken ct = default)
    {
        Products.Add(product);
        return Task.CompletedTask;
    }

    public void Update(ProductEntity product) { }

    public void Remove(ProductEntity product) => Products.Remove(product);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(0);
}
