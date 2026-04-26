using System.Numerics;

/// <summary>
/// Basic physics implementation for game objects.
/// Can be composed into entities or inherited.
/// </summary>
public class PhysicsComponent : IPhysicsBody
{
    public Vector2 Velocity { get; set; }
    public Vector2 Acceleration { get; set; }
    
    /// <summary>
    /// Friction multiplier (0.95 = 5% slowdown per frame).
    /// </summary>
    public float Friction { get; set; } = 0.95f;

    /// <summary>
    /// Whether gravity affects this body.
    /// </summary>
    public bool UseGravity { get; set; } = true;

    /// <summary>
    /// Whether this body is currently grounded.
    /// </summary>
    public bool IsGrounded { get; set; }

    /// <summary>
    /// Mass of this body (for force calculations).
    /// </summary>
    public float Mass { get; set; } = 1f;

    /// <summary>
    /// Applies an instantaneous force to this body.
    /// </summary>
    public void ApplyForce(Vector2 force)
    {
        Acceleration += force / Mass;
    }

    /// <summary>
    /// Update position based on velocity and acceleration.
    /// </summary>
    public void Update(Transform transform, float deltaTime, Vector2 gravity = default)
    {
        // Apply gravity
        if (UseGravity && gravity != Vector2.Zero)
        {
            Acceleration += gravity;
        }

        // Apply acceleration to velocity
        Velocity += Acceleration * deltaTime;

        // Apply friction
        Velocity *= Friction;

        // Update position
        transform.Translate(Velocity * deltaTime);

        // Reset acceleration each frame
        Acceleration = Vector2.Zero;
    }

    /// <summary>
    /// Instantly set velocity (for knockback, etc.)
    /// </summary>
    public void SetVelocity(Vector2 newVelocity)
    {
        Velocity = newVelocity;
    }

    /// <summary>
    /// Stop all movement.
    /// </summary>
    public void Stop()
    {
        Velocity = Vector2.Zero;
        Acceleration = Vector2.Zero;
    }
}