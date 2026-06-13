# Feature Specification: Product API — CRUD Microservice

**Feature Branch**: `001-product-api-crud`  
**Created**: 2026-06-12  
**Status**: Draft  
**Input**: User description: "@plans/Product.Api-Spec.md"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create a Product (Priority: P1)

A catalog manager submits a new product with its name, pricing, stock quantity, and unique product code. The system records the product and confirms it was created, returning the new product's identifier so the caller can reference it later.

**Why this priority**: Creating products is the foundational operation — without it, no other operation has data to work with.

**Independent Test**: Send a valid product payload; verify the response includes a new identifier and that the product can subsequently be retrieved.

**Acceptance Scenarios**:

1. **Given** a valid product payload (name, price, SKU, stock quantity), **When** the create endpoint is called, **Then** the product is persisted and the response contains the new product identifier along with a "Created" status.
2. **Given** a payload missing a required field (e.g., name), **When** the create endpoint is called, **Then** the system returns a validation error describing the missing field.
3. **Given** a product code (SKU) already used by another product, **When** the create endpoint is called, **Then** the system returns a conflict error and the duplicate is not saved.

---

### User Story 2 - Browse the Product Catalog (Priority: P2)

A client application fetches a paginated list of products to display in a catalog view. The caller specifies which page and how many items per page it wants.

**Why this priority**: Reading/listing products is the most frequent operation in any catalog and enables all downstream catalog display use cases.

**Independent Test**: Populate the catalog with several products, then request a paged list and verify the correct subset and total count are returned.

**Acceptance Scenarios**:

1. **Given** multiple products exist, **When** the list endpoint is requested with a page number and page size, **Then** the response contains the correct subset of products plus the total count.
2. **Given** an empty catalog, **When** the list endpoint is requested, **Then** the response returns an empty list with a total count of zero.

---

### User Story 3 - Retrieve a Single Product (Priority: P3)

A client fetches the full details of one product by its unique identifier.

**Why this priority**: Individual product detail views are a core consumer pattern for catalog APIs.

**Independent Test**: Create a product, then fetch it by its identifier and verify all fields match.

**Acceptance Scenarios**:

1. **Given** a product exists, **When** its identifier is supplied to the detail endpoint, **Then** the full product record is returned.
2. **Given** no product with the supplied identifier exists, **When** the detail endpoint is called, **Then** the system returns a "not found" response.

---

### User Story 4 - Update a Product (Priority: P4)

A catalog manager submits updated values for an existing product's fields. All fields are replaced in a single operation.

**Why this priority**: Product information changes over time (price adjustments, description updates, stock corrections); full-update semantics cover the widest variety of changes.

**Independent Test**: Create a product, update it with new values, then retrieve it and verify all fields reflect the updated values.

**Acceptance Scenarios**:

1. **Given** an existing product, **When** valid updated values are submitted, **Then** the product record is replaced with the new values and the system confirms success.
2. **Given** an update payload with invalid data (e.g., negative price), **When** the update endpoint is called, **Then** the system returns a validation error.
3. **Given** an identifier that does not match any product, **When** the update endpoint is called, **Then** the system returns a "not found" response.

---

### User Story 5 - Delete a Product (Priority: P5)

A catalog manager removes a product from the catalog by its unique identifier.

**Why this priority**: Inventory lifecycle requires permanent removal of discontinued products.

**Independent Test**: Create a product, delete it, then verify it can no longer be retrieved.

**Acceptance Scenarios**:

1. **Given** an existing product, **When** the delete endpoint is called with its identifier, **Then** the product is permanently removed and the system confirms success.
2. **Given** an identifier that does not match any product, **When** the delete endpoint is called, **Then** the system returns a "not found" response.

---

### Edge Cases

- What happens when the product code (SKU) contains special characters?
- How does the system respond to a page number beyond the last page (returns empty list, not an error)?
- What happens when price is exactly zero (valid, per business rule >= 0)?
- What happens when stock quantity is zero (valid; product still listed as active)?
- How does the system handle extremely large page sizes?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a REST API for full CRUD operations on a product catalog.
- **FR-002**: System MUST allow creation of a product with the following required fields: name (max 200 characters), price (non-negative), and product code (unique, max 50 characters). Description, stock quantity, and active status are optional on create.
- **FR-003**: System MUST persist all product fields: name, description, price, product code, stock quantity, active flag, creation timestamp, and last-update timestamp.
- **FR-004**: System MUST return a paginated list of products when requested, including total record count, current page number, page size, and the product items.
- **FR-005**: System MUST retrieve a single product record by its unique system-assigned identifier.
- **FR-006**: System MUST support full replacement of all product fields via a single update operation.
- **FR-007**: System MUST permanently delete a product record by its identifier.
- **FR-008**: System MUST reject creation of a product whose product code duplicates an existing product, returning a conflict response.
- **FR-009**: System MUST validate all input and return structured, human-readable error messages for any validation failure.
- **FR-010**: System MUST return a "not found" response for any operation referencing a non-existent product identifier.
- **FR-011**: System MUST provide interactive API documentation listing all endpoints, their inputs, and their expected responses.

### Key Entities

- **Product**: Represents a catalog item available for sale. Key attributes: unique system identifier, name, optional description, price, unique product code (SKU), stock quantity, active status, creation date/time, last-update date/time.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All five catalog operations (create, list, retrieve, update, delete) complete successfully end-to-end against a running data store.
- **SC-002**: Submitting a duplicate product code during creation returns a conflict response 100% of the time.
- **SC-003**: Submitting invalid or missing required fields returns a structured validation error 100% of the time.
- **SC-004**: Referencing a non-existent product identifier returns a "not found" response 100% of the time.
- **SC-005**: The interactive API documentation lists all five endpoints with correct input and output descriptions.
- **SC-006**: A paginated list request returns the correct subset of products and the accurate total count for any combination of page number and page size.
- **SC-007**: No catalog management logic is visible outside the designated service layer (clean layering verified by code review).

## Assumptions

- No authentication or authorization is required for this iteration; all endpoints are publicly accessible.
- No cross-service communication, messaging, or eventing is in scope.
- The service targets a local development environment; production deployment, scaling, and infrastructure are out of scope.
- Deletion is permanent (no soft-delete or audit trail required in this iteration).
- Pagination uses simple offset-based page number and page size parameters; cursor-based pagination is out of scope.
- All timestamps are stored and returned in UTC.
- Price precision is two decimal places; no currency conversion is required.
