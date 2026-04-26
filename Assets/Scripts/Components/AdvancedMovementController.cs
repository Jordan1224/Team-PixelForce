using System.Numerics;

/// <summary>
/// Advanced player movement with coyote time and variable jump height.
/// </summary>
public class AdvancedMovementController
{
    // Movement
    public float MaxSpeed { get; set; } = 8f;
    public float Acceleration { get; set; } = 25f;
    public float GroundFriction { get; set; } = 0.85f;
    public float AirFriction { get; set; } = 0.95f;

    // Jumping
    public float JumpForce { get; set; } = 12f;
    public float MaxJumpHoldTime { get; set; } = 0.15f;
    public float CoyoteTime { get; set; } = 0.1f; // Frames after leaving ground to allow jump
    public float GravityScale { get; set; } = 1f;

    // State
    private Vector2 _moveInput = Vector2.Zero;
    private bool _jumpInputDown = false;
    private bool _jumpInputPressed = false;
    private float _coyoteCounter = 0f;
    private float _jumpHoldTimer = 0f;
    private bool _isJumping = false;

    public bool CanJump => _coyoteCounter > 0;
    public bool IsJumping => _isJumping;
    public bool IsGrounded => _coyoteCounter >= CoyoteTime;

    public void SetInput(Vector2 moveDirection, bool jumpDown, bool jumpPressed)
    {
        _moveInput = moveDirection;
        _jumpInputDown = jumpDown;
        _jumpInputPressed = jumpPressed;
    }

    public void Update(PhysicsComponent physics, bool actuallyGrounded, float deltaTime)
    {
        // Update coyote counter
        if (actuallyGrounded)
        {
            _coyoteCounter = CoyoteTime;
        }
        else
        {
            _coyoteCounter -= deltaTime;
        }

        // Handle jump input
        if (_jumpInputPressed && CanJump && !_isJumping)
        {
            _isJumping = true;
            _jumpHoldTimer = 0;
            physics.Velocity = new Vector2(physics.Velocity.X, 0); // Reset vertical velocity
        }

        // Extend jump height while holding button
        if (_jumpInputDown && _isJumping)
        {
            _jumpHoldTimer += deltaTime;
            if (_jumpHoldTimer <= MaxJumpHoldTime)
            {
                // Apply continuous upward force
                physics.ApplyForce(new Vector2(0, -JumpForce * 5f)); // Extra force while holding
            }
        }
        else if (_isJumping)
        {
            _isJumping = false;
        }

        // Apply initial jump impulse
        if (_isJumping && _jumpHoldTimer == 0)
        {
            physics.Velocity = new Vector2(physics.Velocity.X, -JumpForce);
        }

        // Apply horizontal movement acceleration
        if (_moveInput.X != 0)
        {
            var targetVelocity = _moveInput.X * MaxSpeed;
            var acceleration = Acceleration;
            var newVelX = physics.Velocity.X;

            if (System.Math.Abs(physics.Velocity.X) < MaxSpeed)
            {
                newVelX += _moveInput.X * acceleration * deltaTime;
            }

            newVelX = System.Math.Clamp(newVelX, -MaxSpeed, MaxSpeed);
            physics.Velocity = new Vector2(newVelX, physics.Velocity.Y);
        }
        else
        {
            // Apply friction
            var friction = actuallyGrounded ? GroundFriction : AirFriction;
            physics.Velocity = new Vector2(physics.Velocity.X * friction, physics.Velocity.Y);
        }

        _jumpInputPressed = false;
    }

    public void OnGroundContact()
    {
        _coyoteCounter = CoyoteTime;
        _isJumping = false;
        _jumpHoldTimer = 0;
    }
}
