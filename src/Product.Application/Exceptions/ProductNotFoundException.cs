namespace Product.Application.Exceptions;

public class ProductNotFoundException(Guid id)
    : Exception($"Product with ID '{id}' was not found.");
