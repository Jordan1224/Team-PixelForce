using System;
using UnityEngine;

/// <summary>
/// Example enemy entity that demonstrates AI, physics, and health.
/// </summary>
public class Enemy : EnemyBase
{
    private PhysicsComponent _physics;
    private float _patrolSpeed = 2f;
    private float _moveDirection = 1f;

    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector2 initialDirection = Vector2.left;

    private Vector2 _direction;

    private void FixedUpdate()
    {
        if (_rb != null)
            _rb.linearVelocity = _direction * moveSpeed;
    }

    public void FlipDirection()
    {
        _direction = -_direction;
    }

    public override void OnCollide(ICollidable other)
    {
        // Example: damage the player
        if (other is PlayerCharacter player)
            player.TakeDamage(1);
    }

    public Vector2 Velocity
    {
        get => _physics?.Velocity ?? Vector2.zero;
        set { if (_physics != null) _physics.Velocity = value; }
    }

    public virtual void UpdateAI(PlayerCharacter player)
    {
        // Patrol movement
        _rb.linearVelocity = new Vector2(_moveDirection * _patrolSpeed, 0);

        // Bounce at boundaries
        if (transform.position.x < -50 || transform.position.x > 50)
        {
            _moveDirection *= -1;
        }
    }

    protected override void Awake()
    {
        base.Awake();

        // 1. Position (Unity uses transform)
        // If you need to set startPosition, do it through Initialize(), not Awake().
        // transform.position = startPosition;  // Only if passed in externally

        // 2. Physics (Unity uses Rigidbody2D, not custom PhysicsComponent)
        if (_rb != null)
            _rb.linearDamping = 0.98f;   // Equivalent to friction

        // 3. HealthComponent (Unity does NOT use constructors)
        // HealthComponent must already be on the GameObject.
        // Set maxHealth in the Inspector instead of passing it in.
        if (_health != null)
        {
            _health.OnDamageTaken += (damage) =>
            {
                Debug.Log($"[{name}] Enemy took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            };

            _health.OnDestroyed += () =>
            {
                Debug.Log($"[{name}] Enemy defeated!");
                gameObject.SetActive(false);
            };
        }
    }


    public override void Initialize()
    {
        Debug.Log($"Enemy [{Id}] spawned at {transform.position}");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        // Simple patrol AI
        _rb.linearVelocity = new Vector2(_moveDirection * _patrolSpeed, 0);

        // Bounce at boundaries
        if (transform.position.x < -50 || transform.position.x > 50)
        {
            _moveDirection *= -1;
        }
    }

    public void ApplyForce(Vector2 force)
    {
        _physics.ApplyForce(force);
    }

    public new void TakeDamage(int amount)
    {
        _health.TakeDamage(amount);
    }

    public override void Shutdown()
    {
        Debug.Log($"Enemy [{Id}] removed");
    }
}