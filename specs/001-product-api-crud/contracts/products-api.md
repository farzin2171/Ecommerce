# API Contract: Products

**Service**: Product.Api  
**Base Route**: `/api/products`  
**Content-Type**: `application/json`  
**Date**: 2026-06-12

---

## Endpoints

### GET /api/products

Retrieve a paginated list of all products.

**Query Parameters**

| Parameter    | Type | Default | Constraints     |
|-------------|------|---------|-----------------|
| `pageNumber` | int  | 1       | >= 1            |
| `pageSize`   | int  | 20      | >= 1, <= 100    |

**Success Response — 200 OK**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Wireless Keyboard",
      "description": "Compact wireless keyboard with backlight",
      "price": 49.99,
      "sku": "KB-WL-001",
      "stockQuantity": 150,
      "isActive": true,
      "createdAtUtc": "2026-06-01T10:00:00Z",
      "updatedAtUtc": null
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 20
}
```

**Error Responses**

| Status | When |
|--------|------|
| 400    | Invalid query parameter values |

---

### GET /api/products/{id}

Retrieve a single product by its unique identifier.

**Path Parameters**

| Parameter | Type | Description           |
|-----------|------|-----------------------|
| `id`      | UUID | Product's identifier  |

**Success Response — 200 OK**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Wireless Keyboard",
  "description": "Compact wireless keyboard with backlight",
  "price": 49.99,
  "sku": "KB-WL-001",
  "stockQuantity": 150,
  "isActive": true,
  "createdAtUtc": "2026-06-01T10:00:00Z",
  "updatedAtUtc": null
}
```

**Error Responses**

| Status | When                       |
|--------|----------------------------|
| 404    | Product not found          |

---

### POST /api/products

Create a new product.

**Request Body**

```json
{
  "name": "Wireless Keyboard",
  "description": "Compact wireless keyboard with backlight",
  "price": 49.99,
  "sku": "KB-WL-001",
  "stockQuantity": 150,
  "isActive": true
}
```

**Field Constraints**

| Field           | Required | Constraints                      |
|-----------------|----------|----------------------------------|
| `name`          | Yes      | Non-empty, max 200 characters    |
| `description`   | No       | Max 2000 characters              |
| `price`         | Yes      | >= 0, two decimal places         |
| `sku`           | Yes      | Non-empty, unique, max 50 chars  |
| `stockQuantity` | No       | >= 0 (default: 0)                |
| `isActive`      | No       | Boolean (default: true)          |

**Success Response — 201 Created**

```
Location: /api/products/3fa85f64-5717-4562-b3fc-2c963f66afa6

Body: ProductResponse (same shape as GET /api/products/{id})
```

**Error Responses**

| Status | When                                          |
|--------|-----------------------------------------------|
| 400    | Validation failure (missing/invalid fields)   |
| 409    | SKU already exists for another product        |

**400 Example (ValidationProblemDetails)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["The Name field is required."],
    "Price": ["Price must be greater than or equal to 0."]
  }
}
```

**409 Example (ProblemDetails)**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.10",
  "title": "Conflict",
  "status": 409,
  "detail": "A product with SKU 'KB-WL-001' already exists."
}
```

---

### PUT /api/products/{id}

Fully replace an existing product's fields.

**Path Parameters**

| Parameter | Type | Description           |
|-----------|------|-----------------------|
| `id`      | UUID | Product's identifier  |

**Request Body** — same field constraints as POST, all fields required.

```json
{
  "name": "Wireless Keyboard Pro",
  "description": "Updated description",
  "price": 59.99,
  "sku": "KB-WL-001",
  "stockQuantity": 120,
  "isActive": true
}
```

**Success Response — 204 No Content**

```
(empty body)
```

**Error Responses**

| Status | When                                          |
|--------|-----------------------------------------------|
| 400    | Validation failure                            |
| 404    | Product not found                             |

---

### DELETE /api/products/{id}

Permanently delete a product.

**Path Parameters**

| Parameter | Type | Description           |
|-----------|------|-----------------------|
| `id`      | UUID | Product's identifier  |

**Success Response — 204 No Content**

```
(empty body)
```

**Error Responses**

| Status | When              |
|--------|-------------------|
| 404    | Product not found |

---

## Common Error Shape

All error responses use RFC 7807 `ProblemDetails`:

```json
{
  "type": "string (URI reference)",
  "title": "string",
  "status": 404,
  "detail": "string (human-readable explanation)"
}
```

Validation errors use `ValidationProblemDetails` which extends the above with an `errors` dictionary.
