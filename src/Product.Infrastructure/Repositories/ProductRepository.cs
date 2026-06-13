using Microsoft.EntityFrameworkCore;
using Product.Domain.Interfaces;
using Product.Infrastructure.Persistence;
using ProductEntity = Product.Domain.Entities.Product;

namespace Product.Infrastructure.Repositories;

public class ProductRepository(ProductDbContext db) : IProductRepository
{
    public async Task<ProductEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Products.FindAsync([id], ct);

    public async Task<(IReadOnlyList<ProductEntity> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Products.AsNoTracking();
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<bool> SkuExistsAsync(string sku, CancellationToken ct = default)
        => await db.Products.AnyAsync(p => p.Sku == sku, ct);

    public async Task AddAsync(ProductEntity product, CancellationToken ct = default)
        => await db.Products.AddAsync(product, ct);

    public void Update(ProductEntity product)
        => db.Products.Update(product);

    public void Remove(ProductEntity product)
        => db.Products.Remove(product);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
}
