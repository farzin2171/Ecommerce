using Product.Application.DTOs;
using Product.Application.Exceptions;
using Product.Application.Interfaces;
using Product.Domain.Interfaces;

namespace Product.Application.Services;

public class ProductService(IProductRepository repository) : IProductService
{
    private readonly IProductRepository _repository = repository;

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        if (await _repository.SkuExistsAsync(request.Sku, ct))
            throw new DuplicateSkuException(request.Sku);

        var product = new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Sku = request.Sku,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _repository.AddAsync(product, ct);
        await _repository.SaveChangesAsync(ct);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new ProductNotFoundException(id);
        return MapToResponse(product);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new ProductNotFoundException(id);

        _repository.Remove(product);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _repository.GetByIdAsync(id, ct)
            ?? throw new ProductNotFoundException(id);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Sku = request.Sku;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        _repository.Update(product);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<ProductResponse>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(pageNumber, pageSize, ct);
        return new PagedResult<ProductResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static ProductResponse MapToResponse(Domain.Entities.Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        Sku = product.Sku,
        StockQuantity = product.StockQuantity,
        IsActive = product.IsActive,
        CreatedAtUtc = product.CreatedAtUtc,
        UpdatedAtUtc = product.UpdatedAtUtc
    };
}
