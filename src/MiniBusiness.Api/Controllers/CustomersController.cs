using MediatR;
using Microsoft.AspNetCore.Mvc;
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

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ISender _sender;

    public CustomersController(ISender sender)
    {
        _sender = sender ??  throw new ArgumentNullException(nameof(sender));
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerDto>> Create(
        [FromBody] CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = customer.Id },
            customer);
    }
    
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
    
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CustomerDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var customers = await _sender.Send(
            new GetCustomersQuery(page, pageSize),
            cancellationToken);

        return Ok(customers);
    }
    
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new ReactivateCustomerCommand(id), cancellationToken);
        return NoContent();
    }
}