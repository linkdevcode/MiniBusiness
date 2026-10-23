using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MiniBusiness.Api.Requests;
using MiniBusiness.Application.Common;
using MiniBusiness.Application.Customers.Commands.CreateCustomer;
using MiniBusiness.Application.Customers.Commands.DeactivateCustomer;
using MiniBusiness.Application.Customers.Commands.ReactivateCustomer;
using MiniBusiness.Application.Customers.Commands.UpdateCustomer;
using MiniBusiness.Application.Customers.DTOs;
using MiniBusiness.Application.Customers.Queries.GetCustomerById;
using MiniBusiness.Application.Customers.Queries.GetCustomers;

namespace MiniBusiness.Api.Controllers;

/// <summary>
/// Customer management endpoints (API v1).
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/customers")]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="command">Customer name and email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created customer, with Location header pointing to GetById.</returns>
    /// <response code="201">Customer created successfully.</response>
    /// <response code="400">Validation failed (invalid name/email).</response>
    /// <response code="409">Email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(
            command,
            cancellationToken);

        // Match route group format 'v'VVV → /api/v1/... (not v1.0)
        var apiVersion = HttpContext.RequestedApiVersion?.ToString("VVV") ?? "1";

        return CreatedAtAction(
            nameof(GetById),
            new { version = apiVersion, id = customer.Id },
            customer);
    }
    
    /// <summary>
    /// Gets a customer by id.
    /// </summary>
    /// <param name="id">Customer id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The customer if found.</returns>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer does not exist.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(
            new GetCustomerByIdQuery(id),
            cancellationToken);

        if (customer is null)
            return NotFound();

        return Ok(customer);
    }
    
    /// <summary>
    /// Lists customers with pagination.
    /// </summary>
    /// <remarks>
    /// Rate limited by client IP (policy <c>fixed-by-ip</c>).
    /// Page size is capped by <c>Pagination:MaxPageSize</c> (default 100).
    /// </remarks>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged list of customers.</returns>
    /// <response code="200">Page returned successfully.</response>
    /// <response code="400">Invalid page or pageSize.</response>
    /// <response code="429">Too many requests from this IP.</response>
    [EnableRateLimiting("fixed-by-ip")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var customers = await _sender.Send(
            new GetCustomersQuery(page, pageSize),
            cancellationToken);

        return Ok(customers);
    }
    
    /// <summary>
    /// Updates an existing customer's name and email.
    /// </summary>
    /// <param name="id">Customer id.</param>
    /// <param name="request">New name and email.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated customer.</returns>
    /// <response code="200">Customer updated.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Customer does not exist.</response>
    /// <response code="409">Email already used by another customer.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerDto>> Update(
        Guid id,
        [FromBody] UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCustomerCommand(
            id,
            request.Name,
            request.Email);

        var customer = await _sender.Send(
            command,
            cancellationToken);

        return Ok(customer);
    }
    
    /// <summary>
    /// Deactivates a customer (soft disable).
    /// </summary>
    /// <param name="id">Customer id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Customer deactivated.</response>
    /// <response code="400">Customer is already inactive.</response>
    /// <response code="404">Customer does not exist.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reactivates a previously deactivated customer.
    /// </summary>
    /// <param name="id">Customer id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Customer reactivated.</response>
    /// <response code="400">Customer is already active.</response>
    /// <response code="404">Customer does not exist.</response>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ReactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}
