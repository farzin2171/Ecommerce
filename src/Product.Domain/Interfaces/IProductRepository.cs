namespace Product.Domain.Interfaces;

public interface IProductRepository
{
    Task<Entities.Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Entities.Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default);
    Task AddAsync(Entities.Product product, CancellationToken ct = default);
    void Update(Entities.Product product);
    void Remove(Entities.Product product);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
