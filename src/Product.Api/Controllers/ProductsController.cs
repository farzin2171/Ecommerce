using Microsoft.AspNetCore.Mvc;
using Product.Application.DTOs;
using Product.Application.Interfaces;

namespace Product.Api.Controllers;

[ApiController]
[Route("api/products")]
[Produces("application/json")]
public class ProductsController(IProductService service) : ControllerBase
{
    private readonly IProductService _service = service;

    /// <summary>Creates a new product.</summary>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Validation errors — required fields missing or constraints violated.</response>
    /// <response code="409">A product with the same SKU already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), 201)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 409)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var response = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>Returns a paginated list of products.</summary>
    /// <param name="pageNumber">1-based page index (default 1, min 1).</param>
    /// <param name="pageSize">Number of items per page (default 20, min 1, max 100).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Paginated product list with total count.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return ValidationProblem();

        var result = await _service.GetPagedAsync(pageNumber, pageSize, ct);
        return Ok(result);
    }

    /// <summary>Returns a single product by its identifier.</summary>
    /// <param name="id">The product GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Product found.</response>
    /// <response code="404">No product with that ID exists.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var response = await _service.GetByIdAsync(id, ct);
        return Ok(response);
    }

    /// <summary>Fully replaces all fields of an existing product.</summary>
    /// <param name="id">The product GUID.</param>
    /// <param name="request">New field values — all fields required.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Product updated successfully.</response>
    /// <response code="400">Validation errors.</response>
    /// <response code="404">No product with that ID exists.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        await _service.UpdateAsync(id, request, ct);
        return NoContent();
    }

    /// <summary>Permanently deletes a product.</summary>
    /// <param name="id">The product GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="404">No product with that ID exists.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
