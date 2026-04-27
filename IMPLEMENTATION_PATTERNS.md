# PixelForce - Implementation Patterns & Code Examples

## Overview
This document provides practical code examples showing how to implement common patterns in the PixelForce architecture.

---

## 1. Creating a New Entity

### Pattern: Extend GameEntity

```csharp
public class MyCustomEntity : GameEntity, ICollidable, IPhysicsBody
{
    // Unity components
    private Rigidbody2D _rb;
    private Collider2D _collider;
    
    // Game components
    private PhysicsComponent _physics;
    private HealthComponent _health;
    
    // Events
    public event Action OnCustomEvent;
    
    // Initialization
    protected override void Start()
    {
        base.Start();  // Call parent
        
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _physics = GetComponent<PhysicsComponent>();
        _health = GetComponent<HealthComponent>();
        
        // Subscribe to events
        if (_health != null)
        {
            _health.OnDamageTaken += HandleDamage;
            _health.OnDestroyed += HandleDestroyed;
        }
    }
    
    public override void Initialize()
    {
        Debug.Log($"{gameObject.name} initialized");
    }
    
    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;
        
        // Update logic here
        UpdateBehavior(deltaTime);
    }
    
    public override void Shutdown()
    {
        Debug.Log($"{gameObject.name} shutdown");
        
        // Unsubscribe from events
        if (_health != null)
        {
            _health.OnDamageTaken -= HandleDamage;
            _health.OnDestroyed -= HandleDestroyed;
        }
    }
    
    // ICollidable
    public Collider2D GetCollider() => _collider;
    public Vector2 GetPosition() => transform.position;
    public void OnCollide(ICollidable other)
    {
        Debug.Log($"Collided with {other}");
    }
    
    // IPhysicsBody
    public Vector2 Velocity
    {
        get => _physics?.Velocity ?? Vector2.zero;
        set { if (_physics != null) _physics.Velocity = value; }
    }
    
    public void ApplyForce(Vector2 force)
    {
        if (_physics != null)
            _physics.ApplyForce(force);
    }
    
    // Private methods
    private void UpdateBehavior(float deltaTime)
    {
        // Implement entity-specific behavior
    }
    
    private void HandleDamage(int damage)
    {
        Debug.Log($"Took {damage} damage!");
    }
    
    private void HandleDestroyed()
    {
        gameObject.SetActive(false);
    }
}
```

---

## 2. Creating a New System

### Pattern: Implement IUpdatable

```csharp
public class MyCustomSystem : IUpdatable
{
    // Registry
    private List<IMyCustomInterface> _registeredObjects = new();
    
    // Configuration
    private float _updateInterval = 0.1f;
    private float _timeSinceUpdate = 0f;
    
    // Dependencies
    private Level _level;
    private GameStateManager _gameState;
    
    public MyCustomSystem(Level level, GameStateManager gameState)
    {
        _level = level;
        _gameState = gameState;
    }
    
    // Registration
    public void Register(IMyCustomInterface obj)
    {
        if (!_registeredObjects.Contains(obj))
            _registeredObjects.Add(obj);
    }
    
    public void Unregister(IMyCustomInterface obj)
    {
        _registeredObjects.Remove(obj);
    }
    
    // IUpdatable
    public void Tick(float deltaTime)
    {
        if (!_gameState.IsPlaying) return;
        
        // Throttled update
        _timeSinceUpdate += deltaTime;
        if (_timeSinceUpdate >= _updateInterval)
        {
            _timeSinceUpdate -= _updateInterval;
            UpdateSystem();
        }
    }
    
    // Core logic
    private void UpdateSystem()
    {
        foreach (var obj in _registeredObjects)
        {
            if (obj.IsActive)
            {
                ProcessObject(obj);
            }
        }
    }
    
    private void ProcessObject(IMyCustomInterface obj)
    {
        // Process object according to system rules
    }
    
    // Cleanup
    public void Clear()
    {
        _registeredObjects.Clear();
    }
}

// Corresponding interface
public interface IMyCustomInterface
{
    bool IsActive { get; }
    void OnSystemUpdate();
}
```

---

## 3. Implementing an Interface

### Pattern: Multiple Interface Implementation

```csharp
public class Projectile : GameEntity, ICollidable, IPhysicsBody
{
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private float _lifetime = 5f;
    private float _elapsed = 0f;
    
    // ICollidable Implementation
    public Collider2D GetCollider() => _collider;
    
    public Vector2 GetPosition() => transform.position;
    
    public void OnCollide(ICollidable other)
    {
        if (other is IDamageable damageable)
        {
            damageable.TakeDamage(10);
            gameObject.SetActive(false);  // Projectile destroyed
        }
    }
    
    // IPhysicsBody Implementation
    public Vector2 Velocity
    {
        get => _rb.linearVelocity;
        set => _rb.linearVelocity = value;
    }
    
    public void ApplyForce(Vector2 force)
    {
        _rb.AddForce(force);
    }
    
    // GameEntity Implementation
    public override void Initialize()
    {
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
    }
    
    public override void Tick(float deltaTime)
    {
        if (!IsActive) return;
        
        _elapsed += deltaTime;
        if (_elapsed >= _lifetime)
        {
            gameObject.SetActive(false);
        }
    }
    
    public override void Shutdown()
    {
        // Cleanup
    }
}
```

---

## 4. Event-Based Communication

### Pattern: Publish-Subscribe

```csharp
// PUBLISHER - Raises events
public class HealthComponent : MonoBehaviour, IDamageable
{
    public event Action<int> OnDamageTaken;
    public event Action OnDestroyed;
    
    private int _health = 100;
    
    public void TakeDamage(int amount)
    {
        _health -= amount;
        OnDamageTaken?.Invoke(amount);  // Publish
        
        if (_health <= 0)
        {
            OnDestroyed?.Invoke();  // Publish
        }
    }
}

// SUBSCRIBER 1 - UI System
public class UIHealthDisplay : MonoBehaviour
{
    private HealthComponent _health;
    private Text _healthText;
    
    private void Start()
    {
        _health = GetComponent<HealthComponent>();
        
        // Subscribe to event
        _health.OnDamageTaken += UpdateHealthDisplay;
        _health.OnDestroyed += ShowGameOver;
    }
    
    private void UpdateHealthDisplay(int damage)
    {
        _healthText.text = $"Health: {_health.Health}";
    }
    
    private void ShowGameOver()
    {
        _healthText.text = "GAME OVER";
    }
    
    private void OnDestroy()
    {
        // Unsubscribe (important!)
        _health.OnDamageTaken -= UpdateHealthDisplay;
        _health.OnDestroyed -= ShowGameOver;
    }
}

// SUBSCRIBER 2 - Audio System
public class AudioSystem : MonoBehaviour
{
    private void Start()
    {
        var health = GetComponent<HealthComponent>();
        
        // Subscribe to event
        health.OnDamageTaken += PlayDamageSound;
        health.OnDestroyed += PlayDeathSound;
    }
    
    private void PlayDamageSound(int damage)
    {
        AudioSource.PlayClipAtPoint(damageClip, transform.position);
    }
    
    private void PlayDeathSound()
    {
        AudioSource.PlayClipAtPoint(deathClip, transform.position);
    }
}
```

---

## 5. System Registration Pattern

### Pattern: Registering with Manager

```csharp
public class PlatformerGameManager : IUpdatable
{
    private CollisionSystem _collisionSystem;
    private PhysicsSystem _physicsSystem;
    private AISystem _aiSystem;
    
    private PlayerCharacter _player;
    private List<EnemyBase> _enemies;
    
    public void Initialize()
    {
        // Create systems
        _collisionSystem = new CollisionSystem(_currentLevel);
        _physicsSystem = new PhysicsSystem();
        _aiSystem = new AISystem();
        
        // Create player
        var playerObj = new GameObject("Player");
        _player = playerObj.AddComponent<PlayerCharacter>();
        
        // Register player with systems
        _collisionSystem.Register(_player);
        _physicsSystem.RegisterBody(_player);
        
        // Create enemies
        SpawnEnemies();
    }
    
    private void SpawnEnemies()
    {
        // Create enemy 1
        var enemyObj1 = new GameObject("Enemy1");
        var enemy1 = enemyObj1.AddComponent<Enemy>();
        
        // Register with systems
        _collisionSystem.Register(enemy1);
        _physicsSystem.RegisterBody(enemy1);
        _aiSystem.RegisterEnemy(enemy1);
        
        _enemies.Add(enemy1);
    }
    
    public void Tick(float deltaTime)
    {
        // Update all systems
        _collisionSystem.Tick(deltaTime);
        _physicsSystem.Tick(deltaTime);
        _aiSystem.Tick(deltaTime);
    }
}
```

---

## 6. State Machine Pattern

### Pattern: AI State Management

```csharp
public class AdvancedEnemy : EnemyBase
{
    private StateMachine _stateMachine;
    private PlayerCharacter _detectedPlayer;
    private float _attackRange = 5f;
    private float _attackTimer = 0f;
    private float _attackCooldown = 1f;
    
    protected override void Start()
    {
        base.Start();
        
        // Setup state machine
        _stateMachine = new StateMachine();
        
        _stateMachine.AddState(AIState.Idle, StateIdle);
        _stateMachine.AddState(AIState.Patrol, StatePatrol);
        _stateMachine.AddState(AIState.Chase, StateChase);
        _stateMachine.AddState(AIState.Attack, StateAttack);
        
        _stateMachine.ChangeState(AIState.Patrol);
    }
    
    public override void UpdateAI(PlayerCharacter player)
    {
        // Detect player
        if (player != null && Vector2.Distance(transform.position, player.transform.position) < 15f)
        {
            _detectedPlayer = player;
        }
        else
        {
            _detectedPlayer = null;
        }
        
        // Update state machine
        _stateMachine.Update();
    }
    
    // States
    private void StateIdle()
    {
        if (_detectedPlayer != null)
        {
            _stateMachine.ChangeState(AIState.Chase);
        }
        else
        {
            _stateMachine.ChangeState(AIState.Patrol);
        }
    }
    
    private void StatePatrol()
    {
        // Move left/right
        _rb.linearVelocity = new Vector2(2f, _rb.linearVelocity.y);
        
        if (_detectedPlayer != null)
        {
            _stateMachine.ChangeState(AIState.Chase);
        }
    }
    
    private void StateChase()
    {
        float distance = Vector2.Distance(transform.position, _detectedPlayer.transform.position);
        
        if (distance < _attackRange)
        {
            _stateMachine.ChangeState(AIState.Attack);
        }
        else
        {
            // Move toward player
            float direction = _detectedPlayer.transform.position.x > transform.position.x ? 1f : -1f;
            _rb.linearVelocity = new Vector2(direction * 3f, _rb.linearVelocity.y);
        }
        
        if (_detectedPlayer == null)
        {
            _stateMachine.ChangeState(AIState.Patrol);
        }
    }
    
    private void StateAttack()
    {
        _attackTimer += Time.deltaTime;
        
        if (_attackTimer >= _attackCooldown)
        {
            // Attack player
            _detectedPlayer.TakeDamage(5);
            _attackTimer = 0f;
        }
        
        if (_detectedPlayer == null)
        {
            _stateMachine.ChangeState(AIState.Patrol);
        }
    }
}
```

---

## 7. Object Pooling Pattern

### Pattern: Generic Pool Usage

```csharp
// Using ObjectPool for Vector2
public class VectorPool : ObjectPool<Vector2>
{
    public VectorPool(int initialCapacity = 100)
        : base(initialCapacity, 
               factory: () => Vector2.zero,
               onGet: (v) => { /* reset */ },
               onReturn: (v) => { /* cleanup */ })
    {
    }
}

// Using in gameplay
public class CollisionDetection
{
    private VectorPool _vectorPool;
    
    public CollisionDetection()
    {
        _vectorPool = new VectorPool();
    }
    
    public Vector2 GetCollisionNormal(ICollidable a, ICollidable b)
    {
        var normal = _vectorPool.Get();  // Get from pool
        
        // Calculate collision normal
        normal = (b.GetPosition() - a.GetPosition()).normalized;
        
        // ... use normal ...
        
        _vectorPool.Return(normal);  // Return to pool
        
        return normal;
    }
}
```

---

## 8. Composition Pattern

### Pattern: Building Complex Behavior

```csharp
// Entity with multiple components
public class FullyEquippedEnemy : EnemyBase
{
    // Components attached
    private PhysicsComponent _physics;
    private HealthComponent _health;
    private Collider2D _collider;
    
    // Behaviors
    private PatrolBehavior _patrolBehavior;
    private ChaseBehavior _chaseBehavior;
    private AttackBehavior _attackBehavior;
    
    protected override void Start()
    {
        base.Start();
        
        // Get components
        _physics = GetComponent<PhysicsComponent>();
        _health = GetComponent<HealthComponent>();
        _collider = GetComponent<Collider2D>();
        
        // Create behaviors
        _patrolBehavior = new PatrolBehavior(this, _physics);
        _chaseBehavior = new ChaseBehavior(this, _physics);
        _attackBehavior = new AttackBehavior(this, _health);
        
        // Subscribe to events
        _health.OnDamageTaken += OnTakeDamage;
    }
    
    public override void UpdateAI(PlayerCharacter player)
    {
        // Behaviors compose to create complex AI
        if (CanSeePlayer(player))
        {
            _chaseBehavior.Execute(player);
            
            if (IsInAttackRange(player))
            {
                _attackBehavior.Execute(player);
            }
        }
        else
        {
            _patrolBehavior.Execute();
        }
    }
    
    private bool CanSeePlayer(PlayerCharacter player) => 
        Vector2.Distance(transform.position, player.transform.position) < 20f;
    
    private bool IsInAttackRange(PlayerCharacter player) => 
        Vector2.Distance(transform.position, player.transform.position) < 2f;
    
    private void OnTakeDamage(int damage)
    {
        Debug.Log($"Enemy took {damage} damage!");
    }
}

// Reusable behaviors
public class PatrolBehavior
{
    private GameEntity _entity;
    private PhysicsComponent _physics;
    private float _patrolSpeed = 2f;
    
    public PatrolBehavior(GameEntity entity, PhysicsComponent physics)
    {
        _entity = entity;
        _physics = physics;
    }
    
    public void Execute()
    {
        _physics.Velocity = new Vector2(_patrolSpeed, _physics.Velocity.y);
    }
}
```

---

## 9. Factory Pattern

### Pattern: Creating Complex Objects

```csharp
public static class EntityFactory
{
    public static PlayerCharacter CreatePlayer(Vector3 position)
    {
        var playerObj = new GameObject("Player");
        playerObj.transform.position = position;
        
        // Add required components
        var player = playerObj.AddComponent<PlayerCharacter>();
        var rb = playerObj.AddComponent<Rigidbody2D>();
        var collider = playerObj.AddComponent<BoxCollider2D>();
        var physics = playerObj.AddComponent<PhysicsComponent>();
        var health = playerObj.AddComponent<HealthComponent>();
        var movement = playerObj.AddComponent<AdvancedMovementController>();
        
        // Configure components
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        collider.size = new Vector2(0.5f, 1f);
        
        return player;
    }
    
    public static Enemy CreateEnemy(Vector3 position, EnemyType type)
    {
        var enemyObj = new GameObject($"Enemy_{type}");
        enemyObj.transform.position = position;
        
        // Add components
        var enemy = enemyObj.AddComponent<Enemy>();
        var rb = enemyObj.AddComponent<Rigidbody2D>();
        var collider = enemyObj.AddComponent<BoxCollider2D>();
        var health = enemyObj.AddComponent<HealthComponent>();
        
        // Configure based on type
        switch (type)
        {
            case EnemyType.Slime:
                health.maxHealth = 20;
                break;
            case EnemyType.Goblin:
                health.maxHealth = 30;
                break;
            case EnemyType.Ogre:
                health.maxHealth = 50;
                break;
        }
        
        return enemy;
    }
}

public enum EnemyType { Slime, Goblin, Ogre }

// Usage
var player = EntityFactory.CreatePlayer(Vector3.zero);
var slime = EntityFactory.CreateEnemy(Vector3.one, EnemyType.Slime);
```

---

## 10. Adapter Pattern

### Pattern: Adapting Unity Systems

```csharp
// Adapter: Makes Rigidbody2D compatible with IPhysicsBody interface
public class PhysicsComponent : MonoBehaviour, IPhysicsBody
{
    private Rigidbody2D _rigidbody;
    
    [SerializeField] private float friction = 0.95f;
    [SerializeField] private float mass = 1f;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody2D>();
        }
        
        _rigidbody.mass = mass;
    }
    
    // IPhysicsBody interface
    public Vector2 Velocity
    {
        get => _rigidbody.linearVelocity;
        set => _rigidbody.linearVelocity = value;
    }
    
    public void ApplyForce(Vector2 force)
    {
        _rigidbody.AddForce(force, ForceMode2D.Force);
    }
    
    public void SetVelocity(Vector2 newVelocity)
    {
        _rigidbody.linearVelocity = newVelocity;
    }
    
    public void Stop()
    {
        _rigidbody.linearVelocity = Vector2.zero;
    }
}

// Adapter: Makes Collider2D compatible with ICollidable interface
public class ColliderAdapter : MonoBehaviour, ICollidable
{
    private Collider2D _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }
    
    public Collider2D GetCollider() => _collider;
    
    public Vector2 GetPosition() => transform.position;
    
    public void OnCollide(ICollidable other)
    {
        // Handle collision
    }
}
```

---

## Summary

These patterns provide reusable solutions for common development challenges in PixelForce:

| Pattern | Use Case |
|---------|----------|
| **Entity Extension** | Creating new game objects |
| **System Implementation** | Creating new game systems |
| **Interface Implementation** | Adding capabilities to entities |
| **Event-Based Communication** | Decoupled system interaction |
| **System Registration** | Manager coordination |
| **State Machine** | AI behavior management |
| **Object Pooling** | Performance optimization |
| **Composition** | Building complex behaviors |
| **Factory** | Complex object creation |
| **Adapter** | Unity system integration |

Use these patterns as templates when extending the engine.
