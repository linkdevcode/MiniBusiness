using MiniBusiness.Domain.ValueObjects;

namespace MiniBusiness.Domain.Events;

public record CustomerCreatedEvent(Guid CustomerId, string CustomerEmail);