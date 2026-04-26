using System;
using System.Numerics;

/// <summary>
/// Example player entity that demonstrates using physics, health, and input.
/// </summary>
public class Player : GameEntity, IPhysicsBody, IDamageable
{
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private float _speed = 5f;

    public Vector2 Velocity 
    { 
        get => _physics.Velocity;
        set => _physics.Velocity = value;
    }

    public int Health => _health.Health;
    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;

    public Player(string id) : base(id)
    {
        _physics = new PhysicsComponent();
        _health = new HealthComponent(maxHealth: 100);

        _health.OnDamageTaken += (damage) =>
        {
            Console.WriteLine($"[{Id}] Took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            OnDamageTaken?.Invoke(damage);
        };

        _health.OnDestroyed += () =>
        {
            Console.WriteLine($"[{Id}] Defeated!");
            IsActive = false;
            OnDestroyed?.Invoke();
        };
    }

    public override void Initialize()
    {
        Console.WriteLine($"Player [{Id}] initialized at {Transform.Position}");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        HandleInput(deltaTime);
        _physics.Update(Transform, deltaTime);
    }

    private void HandleInput(float deltaTime)
    {
        Vector2 moveDirection = Vector2.Zero;

        // Simple WASD input
        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey(true).KeyChar;
            switch (char.ToLower(key))
            {
                case 'w': moveDirection.Y -= 1; break;
                case 's': moveDirection.Y += 1; break;
                case 'a': moveDirection.X -= 1; break;
                case 'd': moveDirection.X += 1; break;
                case 'e': TakeDamage(10); break;
            }
        }

        if (moveDirection != Vector2.Zero)
        {
            moveDirection = Vector2.Normalize(moveDirection);
            ApplyForce(moveDirection * _speed);
        }
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
        Console.WriteLine($"Player [{Id}] shutdown");
    }
}