using System;
using System.Numerics;

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
            Console.WriteLine($"[{Id}] Enemy took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            OnDamageTaken?.Invoke(damage);
        };

        _health.OnDestroyed += () =>
        {
            Console.WriteLine($"[{Id}] Enemy defeated!");
            IsActive = false;
            OnDestroyed?.Invoke();
        };
    }

    public override void Initialize()
    {
        Console.WriteLine($"Enemy [{Id}] spawned at {Transform.Position}");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        // Simple patrol AI
        ApplyForce(new Vector2(_moveDirection * _patrolSpeed, 0));

        // Bounce at boundaries
        if (Transform.Position.X < -50 || Transform.Position.X > 50)
        {
            _moveDirection *= -1;
        }

        _physics.Update(Transform, deltaTime);
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
        Console.WriteLine($"Enemy [{Id}] removed");
    }
}