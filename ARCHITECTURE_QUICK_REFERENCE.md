# PixelForce Architecture - Quick Reference Guide

## Project Structure

```
Assets/Scripts/
├── Core/                          # Core game systems
│   ├── GameEntity.cs             # Base class for all entities
│   ├── GameManager.cs            # Main game orchestrator
│   ├── PlatformerGameManager.cs   # Platformer-specific orchestrator
│   ├── GameStateManager.cs       # Game state machine
│   ├── Level.cs                  # Level tile grid
│   ├── LevelFactory.cs           # Level creation
│   ├── PhysicsComponent.cs       # Physics wrapper (Rigidbody2D)
│   ├── HealthComponent.cs        # Health/damage system
│   ├── Collectible.cs            # Collectible items
│   ├── UISystem.cs               # UI rendering
│   ├── Transform.cs              # Transform data (if used)
│   ├── ObjectPool.cs             # Generic object pool
│   ├── VectorPool.cs             # Vector2 object pool
│   └── Enums.cs                  # Game enumerations
│
├── Systems/                       # Independent game systems
│   ├── CollisionSystem.cs        # Collision detection/response
│   ├── PhysicsSystem.cs          # Physics management
│   ├── AISystem.cs               # AI coordination & state machine
│   ├── InputSystem.cs            # Input handling
│   ├── RenderingSystem.cs        # Basic rendering
│   ├── AdvancedRenderingSystem.cs# ASCII sprite rendering
│   └── [Other systems]
│
├── Entities/                      # Game entities
│   ├── GameEntity.cs             # Base entity class
│   ├── PlayerCharacter.cs        # Player entity
│   ├── EnemyBase.cs              # Base enemy class
│   ├── Enemy.cs                  # Concrete enemy implementation
│   └── Player.cs                 # Alternative player class
│
├── Interfaces/                    # Contracts
│   ├── IUpdatable.cs             # Objects that need Tick()
│   ├── IPhysicsBody.cs           # Physics-enabled objects
│   ├── ICollidable.cs            # Collision-capable objects
│   ├── IDamageable.cs            # Damage-receiving objects
│   ├── IGameComponent.cs         # Game component interface
│   └── [Other interfaces]
│
├── Components/                    # Reusable components (if separated)
│   └── [Component scripts]
│
└── GameBootstrap.cs              # Game initialization
```

---

## Core Classes Overview

### GameEntity (Abstract Base Class)
```csharp
public abstract class GameEntity : MonoBehaviour
{
    // Lifecycle
    public virtual void Initialize()    // Called on Start()
    public virtual void Shutdown()      // Called on OnDestroy()
    public abstract void Tick(float dt) // Per-frame update
    
    // Properties
    public string Id                    // Entity identifier
    public bool IsActive                // Is entity active?
}
```

**Inherited By:** PlayerCharacter, EnemyBase, Player

---

### Key Managers

#### GameStateManager
- Manages: Playing, Paused, GameOver, LevelComplete, MainMenu states
- Usage: `_stateManager.SetState(GameStateType.Playing)`

#### PlatformerGameManager
- Orchestrates entire platformer game
- Manages: Level loading, enemy spawning, system coordination
- Calls: `Tick()` on all systems

#### InputSystem
- Converts raw input to InputCommand enum
- Commands: MoveLeft, MoveRight, Jump, Pause, Quit

---

### Core Systems (All implement IUpdatable)

| System | Responsibility | Key Methods |
|--------|-----------------|------------|
| **CollisionSystem** | Collision detection & response | `Register()`, `Tick()`, `Unregister()` |
| **PhysicsSystem** | Physics body management | `RegisterBody()`, `UnregisterBody()` |
| **AISystem** | Enemy AI coordination | `RegisterEnemy()`, `UnregisterEnemy()` |
| **AdvancedRenderingSystem** | ASCII rendering | `DrawLevel()`, `DrawPlayer()`, `Tick()` |
| **InputSystem** | Input polling | `PollInput()` |

---

## Component Architecture

### PlayerCharacter Components
```
PlayerCharacter (GameEntity)
├── Rigidbody2D (Unity)
├── PhysicsComponent
├── AdvancedMovementController
└── HealthComponent
```

### Enemy Components
```
Enemy (EnemyBase extends GameEntity)
├── Rigidbody2D (Unity)
├── PhysicsComponent (optional)
└── HealthComponent
```

---

## Interface Reference

### IUpdatable
```csharp
void Tick(float deltaTime);
```
**Implemented By:** All systems, GameEntity subclasses

### ICollidable
```csharp
Collider2D GetCollider();
Vector2 GetPosition();
void OnCollide(ICollidable other);
```
**Implemented By:** PlayerCharacter, EnemyBase

### IPhysicsBody
```csharp
Vector2 Velocity { get; set; }
void ApplyForce(Vector2 force);
```
**Implemented By:** PlayerCharacter, Enemy

### IDamageable
```csharp
int Health { get; }
void TakeDamage(int amount);
event Action OnDestroyed;
event Action<int> OnDamageTaken;
```
**Implemented By:** PlayerCharacter, EnemyBase

---

## Common Tasks

### Adding a New Feature

**Step 1: Identify the Pattern**
- Data-driven? → Create a System (implements IUpdatable)
- Entity behavior? → Extend GameEntity or add Component
- Global state? → Add to GameStateManager or Manager class
- Reusable ability? → Create Interface + Component

**Step 2: Implement Following Pattern**
```csharp
// Example: New damage type system
public class DamageTypeSystem : IUpdatable
{
    private List<IDamageable> _targets = new();
    
    public void Register(IDamageable target) => _targets.Add(target);
    public void Tick(float dt) { /* logic */ }
}
```

**Step 3: Register with Manager**
```csharp
// In PlatformerGameManager.Initialize()
_damageSystem = new DamageTypeSystem();
```

### Adding a New Enemy Type
1. Extend `EnemyBase`
2. Override `UpdateAI(PlayerCharacter player)`
3. Prefab configuration in Unity
4. Spawn via `PlatformerGameManager.SpawnEnemies()`

### Creating a New Level
```csharp
public static Level CreateLevel3()
{
    var level = new Level(width, height);
    // Set tiles...
    level.SetTile(x, y, TileType.Solid);
    return level;
}
```

### Listening to Events
```csharp
// Subscribe to health component
_health.OnDamageTaken += (damage) => 
{
    Debug.Log($"Took {damage} damage!");
};

// Subscribe to player goal reached
_player.OnGoalReached += () => 
{
    Debug.Log("Level Complete!");
};
```

---

## Execution Flow

### Per Frame
```
1. InputSystem.PollInput()              → Get player input
2. PlatformerGameManager.Tick(dt)
   a. PlayerCharacter.Tick(dt)          → Process input, physics
   b. Enemy[].Tick(dt) + UpdateAI()     → Enemy behavior
   c. CollisionSystem.Tick(dt)          → Check collisions
   d. PhysicsSystem.Tick(dt)            → Physics updates
   e. AISystem.Tick(dt)                 → AI updates (throttled)
   f. AdvancedRenderingSystem.Tick(dt)  → Draw frame
3. Frame rendered
```

### Level Load
```
1. PlatformerGameManager.LoadLevel(index)
2. Create player GameObject + components
3. Instantiate enemies
4. Create CollisionSystem
5. Spawn collectibles
6. Create RenderingSystem
7. Begin gameplay
```

---

## Common Patterns

### Entity Composition
```csharp
// Don't inherit for behavior, compose
public class PlayerCharacter : GameEntity
{
    private PhysicsComponent _physics;        // Composition
    private HealthComponent _health;          // Composition
    private AdvancedMovementController _move; // Composition
}
```

### System Registration
```csharp
// Systems maintain their own registries
collisionSystem.Register(entity);
physicsSystem.RegisterBody(body);
aiSystem.RegisterEnemy(enemy);
```

### Event Communication
```csharp
// Use events instead of direct references
_health.OnDamageTaken += HandleDamage;
_player.OnGoalReached += OnLevelComplete;
```

### State Transitions
```csharp
// Use GameStateManager for global states
if (_stateManager.IsPlaying)
{
    // Update gameplay
}
```

---

## Performance Tips

1. **Object Pooling** - Use ObjectPool<T> for frequently allocated objects
2. **Throttled Updates** - AISystem updates every 0.1s, not every frame
3. **System Registration** - Only registered objects are processed
4. **Rigidbody2D** - Let Unity handle physics, don't reimplement
5. **Lazy Initialization** - Create systems/entities only when needed

---

## Debugging

### Console Logs
Most systems log important events:
```
[State] Changed to: Playing
[GameEntity] Initialized
[Collision] Player hit by enemy
```

### Unity Inspector
- Entity IDs visible in Inspector
- Component states inspectable
- Rigidbody2D values show in real-time

### Key Debug Points
```csharp
// Check game state
Debug.Log($"State: {_stateManager.CurrentState}");

// Check collision registry
Debug.Log($"Collidables: {_collisionSystem._collidables.Count}");

// Check active enemies
Debug.Log($"Enemies: {_enemies.FindAll(e => e.IsActive).Count}");
```

---

## Class Relationships

```
MonoBehaviour (Unity)
    └── GameEntity (Abstract)
        ├── PlayerCharacter (IPhysicsBody, IDamageable, ICollidable)
        ├── Player (IPhysicsBody, IDamageable)
        └── EnemyBase (IDamageable, ICollidable)
            └── Enemy (with UpdateAI)

IUpdatable (Interface)
    ├── GameEntity
    ├── CollisionSystem
    ├── PhysicsSystem
    ├── AISystem
    ├── AdvancedRenderingSystem
    └── InputSystem

IPhysicsBody (Interface)
    ├── PlayerCharacter
    └── Enemy

ICollidable (Interface)
    ├── PlayerCharacter
    └── EnemyBase

IDamageable (Interface)
    ├── PlayerCharacter
    ├── EnemyBase
    └── HealthComponent
```

---

## File Checklist for New Developers

- [ ] Read DESIGN_PATTERNS.md - Understand patterns
- [ ] Review GameEntity.cs - Understand base class
- [ ] Check PlatformerGameManager.cs - See game flow
- [ ] Look at PlayerCharacter.cs - Example implementation
- [ ] Study Enemy.cs - AI implementation
- [ ] Review interfaces/ - Understand contracts
- [ ] Check CollisionSystem.cs - Understand collision
- [ ] Review Event subscriptions - Understand communication

---

## Common Gotchas

1. **Remember to implement all abstract methods** from GameEntity
2. **Register entities with systems** (Collision, Physics, AI)
3. **Use lowercase for Vector2 components** (x, y) not (X, Y)
4. **Events should be nullable** - use `?.Invoke()`
5. **Unregister from systems** when entity is destroyed
6. **Use Transform lowercase** - `transform.position` not `Transform.Position`

---

## Resources

- **Design Patterns:** See DESIGN_PATTERNS.md
- **Level Design:** See LevelFactory.cs for examples
- **Physics:** PhysicsComponent.cs wraps Rigidbody2D
- **AI:** See Enemy.cs and AISystem.cs
- **Collision:** See CollisionSystem.cs and ICollidable.cs
