namespace MiniBusiness.Domain.Entities;

public class Entity
{
    public Guid Id { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }
}