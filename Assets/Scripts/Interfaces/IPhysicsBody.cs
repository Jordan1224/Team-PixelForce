/// <summary>
/// Represents an object with physics simulation (velocity, forces).
/// </summary>
public interface IPhysicsBody
{
    /// <summary>
    /// Current velocity of the body.
    /// </summary>
    System.Numerics.Vector2 Velocity { get; set; }

    /// <summary>
    /// Applies a force to the body (affects velocity/acceleration).
    /// </summary>
    /// <param name="force">The force vector to apply</param>
    void ApplyForce(System.Numerics.Vector2 force);
}