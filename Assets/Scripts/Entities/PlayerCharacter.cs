using System;
using System.Numerics;

/// <summary>
/// Player character entity with physics, health, movement, and combat.
/// </summary>
public class PlayerCharacter : GameEntity, IPhysicsBody, IDamageable, ICollidable
{
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private MovementController _movementController;
    private CollisionController _collisionController;
    private AnimationController _animationController;

    private float _attackCooldown = 0f;
    private float _attackDuration = 0.5f;
    private bool _isAttacking = false;

    public Vector2 Velocity
    {
        get => _physics.Velocity;
        set => _physics.Velocity = value;
    }

    public int Health => _health.Health;
    public System.Drawing.Rectangle Bounds => new System.Drawing.Rectangle(
        (int)Transform.Position.X - 2,
        (int)Transform.Position.Y - 3,
        4,
        6
    );

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;

    public PlayerCharacter(string id) : base(id)
    {
        _physics = new PhysicsComponent { Friction = 0.9f, UseGravity = true };
        _health = new HealthComponent(maxHealth: 100);
        _movementController = new MovementController();
        _collisionController = new CollisionController();
        _animationController = new AnimationController();

        _health.OnDamageTaken += (damage) =>
        {
            Console.WriteLine($"[{Id}] Took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            _animationController.PlayAnimation("hit", loop: false);
            OnDamageTaken?.Invoke(damage);
        };

        _health.OnDestroyed += () =>
        {
            Console.WriteLine($"[{Id}] Player defeated!");
            IsActive = false;
            _animationController.PlayAnimation("dead", loop: false);
            OnDestroyed?.Invoke();
        };
    }

    public override void Initialize()
    {
        Console.WriteLine($"PlayerCharacter [{Id}] initialized at {Transform.Position}");
        _animationController.PlayAnimation("idle");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        HandleInput();
        UpdateAttack(deltaTime);
        _movementController.Update(_physics, _collisionController.IsGrounded);
        _collisionController.Update(Transform, _physics.Velocity);
        _physics.Update(Transform, deltaTime, new Vector2(0, 9.8f));
        _animationController.Update(deltaTime);

        UpdateAnimationState();
    }

    private void HandleInput()
    {
        // This will be set by InputSystem in real implementation
        var moveInput = Vector2.Zero;
        var jumpInput = false;

        // For now, this is handled elsewhere
        _movementController.SetInput(moveInput, jumpInput);
    }

    private void UpdateAttack(float deltaTime)
    {
        if (_attackCooldown > 0)
            _attackCooldown -= deltaTime;

        if (_isAttacking)
        {
            if (_animationController.IsFinished())
            {
                _isAttacking = false;
                _animationController.PlayAnimation("idle");
            }
        }
    }

    private void UpdateAnimationState()
    {
        if (_isAttacking) return;

        if (_collisionController.IsGrounded)
        {
            if (System.Math.Abs(_physics.Velocity.X) > 0.1f)
            {
                _animationController.PlayAnimation("run");
            }
            else
            {
                _animationController.PlayAnimation("idle");
            }
        }
        else
        {
            _animationController.PlayAnimation("jump");
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

    public void Attack()
    {
        if (_attackCooldown > 0) return;

        _isAttacking = true;
        _attackCooldown = _attackDuration;
        _animationController.PlayAnimation("attack", loop: false);

        Console.WriteLine($"[{Id}] Attacks!");
        // Damage enemies in range
    }

    public void OnCollision(ICollidable other)
    {
        if (other is EnemyBase enemy && _isAttacking)
        {
            enemy.TakeDamage(10);
        }
        else if (other is EnemyBase && !_isAttacking)
        {
            TakeDamage(5);
        }
    }

    public override void Shutdown()
    {
        Console.WriteLine($"PlayerCharacter [{Id}] shutdown");
    }
}