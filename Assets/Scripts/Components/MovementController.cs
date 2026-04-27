using UnityEngine;


/// <summary>
/// Handles player movement acceleration, deceleration, and jump logic.
/// </summary>
public class MovementController
{
    public float MaxSpeed { get; set; } = 5f;
    public float Acceleration { get; set; } = 20f;
    public float JumpForce { get; set; } = 10f;
    public float Friction { get; set; } = 0.85f;

    private Vector2 _moveInput = Vector2.zero;
    private bool _jumpRequested = false;

    public void SetInput(Vector2 moveDirection, bool jump)
    {
        _moveInput = moveDirection;
        _jumpRequested = jump;
    }

    public void Update(PhysicsComponent physics, bool isGrounded)
    {
        // Apply movement acceleration
        if (_moveInput != Vector2.zero)
        {
            var normalized = Vector2.Normalize(_moveInput);
            var acceleration = normalized * Acceleration;
            physics.ApplyForce(acceleration);

            // Cap horizontal speed
            if (System.Math.Abs(physics.Velocity.x) > MaxSpeed)
            {
                physics.Velocity = new Vector2(
                    System.Math.Sign(physics.Velocity.x) * MaxSpeed,
                    physics.Velocity.y
                );
            }
        }
        else
        {
            // Apply friction when no input
            physics.Velocity = new Vector2(physics.Velocity.x * Friction, physics.Velocity.y);
        }

        // Jump
        if (_jumpRequested && isGrounded)
        {
            physics.SetVelocity(new Vector2(physics.Velocity.x, -JumpForce));
            _jumpRequested = false;
        }

        _moveInput = Vector2.zero;
    }
}

/// <summary>
/// Handles collision detection and grounding for player.
/// </summary>
public class CollisionController
{
    public bool IsGrounded { get; private set; }
    public float GroundCheckDistance { get; set; } = 0.5f;

    public void Update(Transform transform, Vector2 velocity)
    {
        // Simple ground check: if moving down and was grounded before, stay grounded
        IsGrounded = velocity.y >= 0;

        // In a real game, you'd raycast downward to check for solid ground
        // For now, we use simple threshold
        if (velocity.y >= -0.1f && velocity.y <= 0.1f)
        {
            IsGrounded = true;
        }
    }

    public void CheckCollisions()
    {
        // This would be called after physics update to check solid collisions
        // Simplified for this implementation
    }
}