namespace Product.Application.Exceptions;

public class DuplicateSkuException(string sku)
    : Exception($"A product with SKU '{sku}' already exists.");
