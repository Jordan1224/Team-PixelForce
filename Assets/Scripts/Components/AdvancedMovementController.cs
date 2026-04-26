using UnityEngine;

/// <summary>
/// Advanced movement controller with coyote time and variable jump height.
/// Integrates with Rigidbody2D for platformer mechanics.
/// </summary>
public class AdvancedMovementController : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private PhysicsComponent _physicsComponent;
    
    // Coyote time: allows jump shortly after leaving ground
    private float _coyoteCounter = 0f;
    [SerializeField] private float _coyoteTime = 0.1f;
    
    // Jump hold: variable jump height based on button hold duration
    private float _jumpHoldTimer = 0f;
    [SerializeField] private float _maxJumpHoldTime = 0.15f;
    [SerializeField] private float _jumpForce = 8f;
    
    // Movement
    private Vector2 _moveInput;
    private bool _jumpDown;
    private bool _jumpPressed;
    
    [SerializeField] private float _maxSpeed = 8f;
    [SerializeField] private float _groundFriction = 0.85f;
    [SerializeField] private float _airFriction = 0.95f;
    
    private bool _isGrounded = false;
    
    public float GravityScale { get; set; } = 1f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _physicsComponent = GetComponent<PhysicsComponent>();
        
        if (_rigidbody == null)
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
    }

    public void SetInput(Vector2 moveInput, bool jumpDown, bool jumpPressed)
    {
        _moveInput = moveInput;
        _jumpDown = jumpDown;
        _jumpPressed = jumpPressed;
    }

    public void UpdateMovement(bool isGrounded, float deltaTime)
    {
        _isGrounded = isGrounded;

        // Update coyote counter
        if (_isGrounded)
        {
            _coyoteCounter = _coyoteTime;
        }
        else
        {
            _coyoteCounter -= deltaTime;
        }

        // Handle jump input
        if (_jumpPressed && _coyoteCounter > 0)
        {
            _jumpHoldTimer = _maxJumpHoldTime;
            _coyoteCounter = 0; // Consume coyote time
        }

        // Apply jump force while holding
        if (_jumpDown && _jumpHoldTimer > 0)
        {
            _rigidbody.velocity += Vector2.up * _jumpForce * deltaTime;
            _jumpHoldTimer -= deltaTime;
        }

        // Apply horizontal movement
        Vector2 currentVelocity = _rigidbody.velocity;
        
        if (Mathf.Abs(_moveInput.x) > 0.1f)
        {
            currentVelocity.x = _moveInput.x * _maxSpeed;
        }
        else
        {
            // Decelerate
            float friction = _isGrounded ? _groundFriction : _airFriction;
            currentVelocity.x *= friction;
        }

        // Clamp speed
        if (currentVelocity.magnitude > _maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * _maxSpeed;
        }

        _rigidbody.velocity = currentVelocity;
    }

    public void OnGroundContact()
    {
        _jumpHoldTimer = 0;
    }
}
