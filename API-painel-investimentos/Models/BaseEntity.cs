namespace API_painel_investimentos.Models;

/// <summary>
/// Represents the base class for entities, providing common properties and functionality  for uniquely identifying
/// entities and tracking their creation and modification timestamps.
/// </summary>
/// <remarks>This class is intended to be inherited by other entity classes to ensure consistent  implementation
/// of entity identification and timestamp tracking. It includes the following features: <list type="bullet">
/// <item><description>A unique identifier (<see cref="Id"/>) for the entity.</description></item>
/// <item><description>Automatic tracking of the entity's creation time (<see cref="CreatedAt"/>).</description></item>
/// <item><description>Optional tracking of the entity's last update time (<see
/// cref="UpdatedAt"/>).</description></item> </list> The class also provides equality comparison based on the <see
/// cref="Id"/> property,  ensuring that entities with the same identifier are considered equal.</remarks>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class with a unique identifier and the current UTC
    /// timestamp.
    /// </summary>
    /// <remarks>The <see cref="Id"/> property is automatically set to a new GUID, and the <see
    /// cref="CreatedAt"/> property is set to the current UTC time.</remarks>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <remarks>This method compares the runtime type and the <c>Id</c> property of the current instance and
    /// the specified object.</remarks>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified object is of the same type and has the same identifier as the current
    /// instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    /// <summary>
    /// Returns a hash code for the current object.
    /// </summary>
    /// <remarks>The hash code is derived from the value of the <see cref="Id"/> property.  This ensures that
    /// objects with the same <see cref="Id"/> produce the same hash code.</remarks>
    /// <returns>An integer that represents the hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Updates the <see cref="UpdatedAt"/> property to the current UTC date and time.
    /// </summary>
    /// <remarks>This method sets the <see cref="UpdatedAt"/> property to <see cref="DateTime.UtcNow"/>, 
    /// indicating the most recent update timestamp. It is typically used to track the last modification time.</remarks>
    public void UpdateTimestamps()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
