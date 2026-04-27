using System;
using UnityEngine;

/// <summary>
/// Example enemy entity that demonstrates AI, physics, and health.
/// </summary>
public class Enemy : GameEntity, IPhysicsBody, IDamageable
{
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private float _patrolSpeed = 2f;
    private float _moveDirection = 1f;

    public Vector2 Velocity
    {
        get => _physics.Velocity;
        set => _physics.Velocity = value;
    }

    public int Health => _health.Health;
    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;

    public Enemy(string id, Vector2 startPosition) : base(id)
    {
        Transform.Position = startPosition;
        _physics = new PhysicsComponent { Friction = 0.98f };
        _health = new HealthComponent(maxHealth: 30);

        _health.OnDamageTaken += (damage) =>
        {
            Debug.Log($"[{Id}] Enemy took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            OnDamageTaken?.Invoke(damage);
        };

        _health.OnDestroyed += () =>
        {
            Debug.Log($"[{Id}] Enemy defeated!");
            IsActive = false;
            OnDestroyed?.Invoke();
        };
    }

    public override void Initialize()
    {
        Debug.Log($"Enemy [{Id}] spawned at {transform.position}");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        // Simple patrol AI
        ApplyForce(new Vector2(_moveDirection * _patrolSpeed, 0));

        // Bounce at boundaries
        if (transform.position.x < -50 || transform.position.x > 50)
        {
            _moveDirection *= -1;
        }

        _physics.Update(transform, deltaTime);
    }

    public void ApplyForce(Vector2 force)
    {
        _physics.ApplyForce(force);
    }

    public void TakeDamage(int amount)
    {
        _health.TakeDamage(amount);
    }

    public override void Shutdown()
    {
        Debug.Log($"Enemy [{Id}] removed");
    }
}