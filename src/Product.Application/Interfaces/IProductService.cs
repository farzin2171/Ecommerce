using Product.Application.DTOs;

namespace Product.Application.Interfaces;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<PagedResult<ProductResponse>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<ProductResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
