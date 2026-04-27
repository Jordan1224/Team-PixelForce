using System;
using UnityEngine;

/// <summary>
/// Example player entity that demonstrates using physics, health, and input.
/// </summary>
public class Player : GameEntity, IPhysicsBody, IDamageable
{
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private Rigidbody2D _rb;
    private float _speed = 5f;

    public Vector2 Velocity 
    { 
        get => _physics != null ? _physics.Velocity : Vector2.zero;
        set { if (_physics != null) _physics.Velocity = value; }
    }

    public int Health => _health != null ? _health.Health : 0;
    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;

    protected override void Start()
    {
        base.Start();
        _rb = GetComponent<Rigidbody2D>();
        _physics = GetComponent<PhysicsComponent>();
        _health = GetComponent<HealthComponent>();

        if (_health != null)
        {
            _health.OnDamageTaken += (damage) =>
            {
                Debug.Log($"[{Id}] Took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
                OnDamageTaken?.Invoke(damage);
            };

            _health.OnDestroyed += () =>
            {
                Debug.Log($"[{Id}] Defeated!");
                gameObject.SetActive(false);
                OnDestroyed?.Invoke();
            };
        }
    }

    public override void Initialize()
    {
        Debug.Log($"Player [{Id}] initialized at {transform.position}");
    }

    public override void Tick(float deltaTime)
    {
        if (!isActive) return;

        HandleInput(deltaTime);
    }

    private void HandleInput(float deltaTime)
    {
        Vector2 moveDirection = Vector2.zero;

        // Unity Input system
        if (Input.GetKey(KeyCode.W)) moveDirection = new Vector2(moveDirection.x, moveDirection.y - 1);
        if (Input.GetKey(KeyCode.S)) moveDirection = new Vector2(moveDirection.x, moveDirection.y + 1);
        if (Input.GetKey(KeyCode.A)) moveDirection = new Vector2(moveDirection.x - 1, moveDirection.y);
        if (Input.GetKey(KeyCode.D)) moveDirection = new Vector2(moveDirection.x + 1, moveDirection.y);
        if (Input.GetKeyDown(KeyCode.E)) TakeDamage(10);

        if (moveDirection != Vector2.zero)
        {
            moveDirection = Vector2.Normalize(moveDirection);
            ApplyForce(moveDirection * _speed);
        }
    }

    public void ApplyForce(Vector2 force)
    {
        if (_physics != null)
            _physics.ApplyForce(force);
    }

    public void TakeDamage(int amount)
    {
        if (_health != null)
            _health.TakeDamage(amount);
    }

    public override void Shutdown()
    {
        Debug.Log($"Player [{Id}] shutdown");
    }
}