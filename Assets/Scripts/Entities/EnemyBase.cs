using System;
using System.Numerics;

/// <summary>
/// Base class for all enemies. Combines physics, health, and AI.
/// </summary>
public abstract class EnemyBase : GameEntity, IPhysicsBody, IDamageable, ICollidable
{
    protected PhysicsComponent _physics;
    protected HealthComponent _health;
    protected DetectionComponent _detection;
    protected StateMachine _stateMachine;
    protected AnimationController _animationController;

    protected float _baseSpeed = 2f;

    public Vector2 Velocity
    {
        get => _physics.Velocity;
        set => _physics.Velocity = value;
    }

    public int Health => _health.Health;
    public System.Drawing.Rectangle Bounds => new System.Drawing.Rectangle(
        (int)Transform.Position.X - 1,
        (int)Transform.Position.Y - 2,
        2,
        4
    );

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;

    public EnemyBase(string id, Vector2 startPosition) : base(id)
    {
        Transform.Position = startPosition;
        _physics = new PhysicsComponent { Friction = 0.92f, UseGravity = true };
        _health = new HealthComponent(maxHealth: 30);
        _detection = new DetectionComponent();
        _stateMachine = new StateMachine();
        _animationController = new AnimationController();

        _health.OnDamageTaken += (damage) =>
        {
            Console.WriteLine($"[{Id}] Enemy took {damage} damage! Health: {_health.Health}/{_health.MaxHealth}");
            OnDamageTaken?.Invoke(damage);
        };

        _health.OnDestroyed += () =>
        {
            Console.WriteLine($"[{Id}] Enemy defeated!");
            IsActive = false;
            _stateMachine.ChangeState(AIState.Dead);
            _animationController.PlayAnimation("dead", loop: false);
            OnDestroyed?.Invoke();
        };

        SetupStateMachine();
    }

    protected virtual void SetupStateMachine()
    {
        // Override in derived classes
    }

    public override void Initialize()
    {
        Console.WriteLine($"Enemy [{Id}] spawned at {Transform.Position}");
        _stateMachine.ChangeState(AIState.Idle);
        _animationController.PlayAnimation("idle");
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        _stateMachine.Update();
        _physics.Update(Transform, deltaTime, new Vector2(0, 9.8f));
        _animationController.Update(deltaTime);
    }

    /// <summary>
    /// Called by AISystem to update AI logic.
    /// </summary>
    public virtual void UpdateAI(PlayerCharacter player)
    {
        _detection.Update(Transform, player);
    }

    public void ApplyForce(Vector2 force)
    {
        _physics.ApplyForce(force);
    }

    public void TakeDamage(int amount)
    {
        _health.TakeDamage(amount);
    }

    public abstract void OnCollision(ICollidable other);

    public override void Shutdown()
    {
        Console.WriteLine($"Enemy [{Id}] removed");
    }
}

/// <summary>
/// Enemy that patrols back and forth.
/// </summary>
public class PatrolEnemy : EnemyBase
{
    private float _patrolDirection = 1f;
    private float _patrolRange = 20f;
    private Vector2 _startPosition;

    public PatrolEnemy(string id, Vector2 startPosition) : base(id, startPosition)
    {
        _startPosition = startPosition;
    }

    protected override void SetupStateMachine()
    {
        _stateMachine.AddState(AIState.Idle, () =>
        {
            _animationController.PlayAnimation("idle");
        });

        _stateMachine.AddState(AIState.Patrol, () =>
        {
            ApplyForce(new Vector2(_patrolDirection * _baseSpeed, 0));
            _animationController.PlayAnimation("run");

            // Check patrol boundaries
            if (System.Math.Abs(Transform.Position.X - _startPosition.X) > _patrolRange)
            {
                _patrolDirection *= -1;
            }
        });

        _stateMachine.AddState(AIState.Chase, () =>
        {
            if (_detection.DetectedPlayer != null)
            {
                var dirToPlayer = System.Math.Sign(_detection.DetectedPlayer.Transform.Position.X - Transform.Position.X);
                ApplyForce(new Vector2(dirToPlayer * _baseSpeed * 1.5f, 0));
                _animationController.PlayAnimation("run");
            }
        });

        _stateMachine.AddState(AIState.Dead, () =>
        {
            _physics.Stop();
        });
    }

    public override void UpdateAI(PlayerCharacter player)
    {
        base.UpdateAI(player);

        if (!IsActive) return;

        if (_detection.HasLineOfSight)
        {
            _stateMachine.ChangeState(AIState.Chase);
        }
        else
        {
            _stateMachine.ChangeState(AIState.Patrol);
        }
    }

    public override void OnCollision(ICollidable other)
    {
        if (other is PlayerCharacter player)
        {
            player.TakeDamage(5);
        }
    }
}

/// <summary>
/// Enemy that chases the player aggressively.
/// </summary>
public class ChaserEnemy : EnemyBase
{
    private float _chaseSpeed = 3.5f;
    private float _attackCooldown = 0f;
    private float _attackInterval = 1f;

    public ChaserEnemy(string id, Vector2 startPosition) : base(id, startPosition)
    {
        _baseSpeed = _chaseSpeed;
    }

    protected override void SetupStateMachine()
    {
        _stateMachine.AddState(AIState.Idle, () =>
        {
            _physics.Velocity = new Vector2(0, _physics.Velocity.Y);
            _animationController.PlayAnimation("idle");
        });

        _stateMachine.AddState(AIState.Chase, () =>
        {
            if (_detection.DetectedPlayer != null)
            {
                var dirToPlayer = System.Math.Sign(_detection.DetectedPlayer.Transform.Position.X - Transform.Position.X);
                ApplyForce(new Vector2(dirToPlayer * _chaseSpeed, 0));
                _animationController.PlayAnimation("run");
            }
        });

        _stateMachine.AddState(AIState.Attack, () =>
        {
            _animationController.PlayAnimation("attack", loop: false);
        });

        _stateMachine.AddState(AIState.Dead, () =>
        {
            _physics.Stop();
        });
    }

    public override void Tick(float deltaTime)
    {
        if (_attackCooldown > 0)
            _attackCooldown -= deltaTime;

        base.Tick(deltaTime);
    }

    public override void UpdateAI(PlayerCharacter player)
    {
        base.UpdateAI(player);

        if (!IsActive) return;

        if (_detection.HasLineOfSight)
        {
            var distanceToPlayer = System.Numerics.Vector2.Distance(Transform.Position, player.Transform.Position);
            
            if (distanceToPlayer < 3f)
            {
                _stateMachine.ChangeState(AIState.Attack);
            }
            else
            {
                _stateMachine.ChangeState(AIState.Chase);
            }
        }
        else
        {
            _stateMachine.ChangeState(AIState.Idle);
        }
    }

    public override void OnCollision(ICollidable other)
    {
        if (other is PlayerCharacter player && _attackCooldown <= 0)
        {
            player.TakeDamage(8);
            _attackCooldown = _attackInterval;
        }
    }
}