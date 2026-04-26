using System;
using System.Numerics;

/// <summary>
/// Player character entity with physics, health, movement, and combat.
/// </summary>
public class PlayerCharacter : GameEntity, IPhysicsBody, IDamageable, ICollidable
{
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private AdvancedMovementController _movementController;
    private AnimationController _animationController;

    private float _attackCooldown = 0f;
    private float _attackDuration = 0.5f;
    private bool _isAttacking = false;
    private bool _isGrounded = false;
    private bool _reachedGoal = false;

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

    public bool HasReachedGoal => _reachedGoal;

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;
    public event Action OnGoalReached;

    public PlayerCharacter(string id) : base(id)
    {
        _physics = new PhysicsComponent { Friction = 0.9f, UseGravity = true, Mass = 1f };
        _health = new HealthComponent(maxHealth: 100);
        _movementController = new AdvancedMovementController();
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

    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
        if (grounded)
            _movementController.OnGroundContact();
    }

    public void ReachGoal()
    {
        if (!_reachedGoal)
        {
            _reachedGoal = true;
            Console.WriteLine($"[{Id}] Reached the goal!");
            OnGoalReached?.Invoke();
        }
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        HandleInput();
        UpdateAttack(deltaTime);
        
        // Update movement controller
        _movementController.Update(_physics, _isGrounded, deltaTime);
        
        // Apply gravity
        _physics.Acceleration += new Vector2(0, 9.8f * _movementController.GravityScale);
        
        // Update physics
        _physics.Update(Transform, deltaTime);
        _animationController.Update(deltaTime);

        UpdateAnimationState();
    }

    private void HandleInput()
    {
        // This will be set by InputSystem in real implementation
        var moveInput = Vector2.Zero;
        var jumpDown = false;
        var jumpPressed = false;

        // For now, this is handled elsewhere
        _movementController.SetInput(moveInput, jumpDown, jumpPressed);
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

        if (_isGrounded)
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

    public AdvancedMovementController GetMovementController()
    {
        return _movementController;
    }

    public override void Shutdown()
    {
        Console.WriteLine($"PlayerCharacter [{Id}] shutdown");
    }
}