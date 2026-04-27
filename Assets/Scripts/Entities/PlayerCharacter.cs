using System;
using UnityEngine;

/// <summary>
/// Player character entity with physics, health, movement, and combat.
/// </summary>

public class PlayerCharacter : GameEntity, IPhysicsBody, IDamageable, ICollectible
{
    private Rigidbody2D _rigidbody;
    private PhysicsComponent _physics;
    private AdvancedMovementController _movementController;
    private HealthComponent _health;
    public void Initialize(string name)
    {
        this.characterName = name;
        Debug.Log("Initialized player: " + name);
    }

    public Collider2D GetCollider()
    {
        return GetComponent<Collider2D>();
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void OnCollide(ICollidable other)
    {
        Debug.Log("Player collided with: " + other);
    }

    [SerializeField] private string characterName;

    [SerializeField] private float _attackCooldown = 0f;
    [SerializeField] private float _attackDuration = 0.5f;
    private bool _isAttacking = false;
    private bool _isGrounded = false;
    private bool _reachedGoal = false;

    public int Health => _health != null ? _health.CurrentHealth : 0;
    public bool HasReachedGoal => _reachedGoal;

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;
    public event Action OnGoalReached;

    protected override void Start()
    {
        base.Start();
        
        _rigidbody = GetComponent<Rigidbody2D>();
        _physics = GetComponent<PhysicsComponent>();
        _movementController = GetComponent<AdvancedMovementController>();
        _health = GetComponent<HealthComponent>();

        if (_health != null)
        {
            _health.OnDamageTaken += HandleDamage;
            _health.OnDestroyed += HandleDeath;
        }
    }

    public override void Initialize()
    {
        Debug.Log($"PlayerCharacter [{Id}] initialized at {transform.position}");
    }

    public void SetGrounded(bool grounded)
    {
        _isGrounded = grounded;
        if (grounded && _movementController != null)
            _movementController.OnGroundContact();
    }

    public void ReachGoal()
    {
        if (!_reachedGoal)
        {
            _reachedGoal = true;
            Debug.Log($"[{Id}] Reached the goal!");
            OnGoalReached?.Invoke();
        }
    }

    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;

        if (_movementController != null)
            _movementController.UpdateMovement(_isGrounded, deltaTime);

        UpdateAttack(deltaTime);
    }

    private void UpdateAttack(float deltaTime)
    {
        if (_attackCooldown > 0)
            _attackCooldown -= deltaTime;

        if (_isAttacking)
        {
            _attackDuration -= deltaTime;
            if (_attackDuration <= 0)
            {
                _isAttacking = false;
                _attackDuration = 0.5f;
            }
        }
    }

    private void HandleDamage(int damage)
    {
        Debug.Log($"[{Id}] Took {damage} damage!");
        OnDamageTaken?.Invoke(damage);
    }

    private void HandleDeath()
    {
        Debug.Log($"[{Id}] Player defeated!");
        isActive = false;
        OnDestroyed?.Invoke();
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

    public void Attack()
    {
        if (_attackCooldown > 0) return;

        _isAttacking = true;
        _attackCooldown = _attackDuration;
        Debug.Log($"[{Id}] Attacks!");
    }

    public AdvancedMovementController GetMovementController()
    {
        return _movementController;
    }

    public override void Shutdown()
    {
        Debug.Log($"PlayerCharacter [{Id}] shutdown");
    }
}