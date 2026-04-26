using UnityEngine;

/// <summary>
/// Physics component for Unity GameObjects.
/// Manages velocity, acceleration, and physics updates using Rigidbody2D.
/// </summary>
public class PhysicsComponent : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    
    [SerializeField] private float friction = 0.95f;
    [SerializeField] private float mass = 1f;
    [SerializeField] private bool useGravity = true;
    
    private Vector2 _velocity;
    private Vector2 _acceleration;

    public Vector2 Velocity 
    { 
        get => _rigidbody.velocity;
        set => _rigidbody.velocity = value;
    }

    public float Mass => mass;
    public float Friction => friction;
    public bool UseGravity => useGravity;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
            _rigidbody.gravityScale = useGravity ? 1f : 0f;
            _rigidbody.mass = mass;
        }
    }

    public void ApplyForce(Vector2 force)
    {
        _acceleration += force / mass;
    }

    public void SetVelocity(Vector2 newVelocity)
    {
        _rigidbody.velocity = newVelocity;
    }

    public void Stop()
    {
        _rigidbody.velocity = Vector2.zero;
        _acceleration = Vector2.zero;
    }
}