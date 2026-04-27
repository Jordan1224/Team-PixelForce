# PixelForce - Technical Design Patterns Documentation

## Overview

The PixelForce platformer game employs a multi-pattern architecture designed for maintainability, scalability, and separation of concerns. This document details the design patterns used throughout the codebase and their implementations.

---

## 1. **Component Pattern** (MonoBehaviour Architecture)

### Description
The project uses Unity's native MonoBehaviour component system as the foundational architecture. All game entities inherit from `GameEntity`, which extends `MonoBehaviour`, allowing them to leverage Unity's lifecycle events and physics engine.

### Implementation

**Base Class: `GameEntity`**
```csharp
public abstract class GameEntity : MonoBehaviour
{
    public string Id => entityId;
    public bool IsActive => isActive;
    
    protected virtual void Start() => Initialize();
    protected virtual void OnDestroy() => Shutdown();
    
    public virtual void Initialize() { }
    public virtual void Shutdown() { }
    public abstract void Tick(float deltaTime);
}
```

### Key Components
- **PhysicsComponent**: Wraps Rigidbody2D for physics simulation
- **HealthComponent**: Manages entity health, damage, and destruction events
- **AdvancedMovementController**: Handles platformer-specific movement (coyote time, variable jump height)

### Benefits
- Native Unity integration (Rigidbody2D, Collider2D, Transform)
- Automatic lifecycle management
- Inspector-based configuration
- Easy debugging in Unity Editor

### Used By
- `PlayerCharacter`
- `Enemy` / `EnemyBase`
- `Player`

---

## 2. **Interface Segregation Pattern**

### Description
Multiple focused interfaces define specific capabilities without forcing unnecessary dependencies. Each interface represents a single responsibility.

### Interfaces Implemented

#### **IUpdatable**
Represents objects that need per-frame updates.
```csharp
public interface IUpdatable
{
    void Tick(float deltaTime);
}
```
**Used By:** GameEntity, CollisionSystem, PhysicsSystem, AISystem, AdvancedRenderingSystem, UISystem

#### **ICollidable**
Defines collision capabilities.
```csharp
public interface ICollidable
{
    Collider2D GetCollider();
    Vector2 GetPosition();
    void OnCollide(ICollidable other);
}
```
**Used By:** PlayerCharacter, EnemyBase

#### **IPhysicsBody**
Represents physics-enabled objects.
```csharp
public interface IPhysicsBody
{
    Vector2 Velocity { get; set; }
    void ApplyForce(Vector2 force);
}
```
**Used By:** PlayerCharacter, Enemy

#### **IDamageable**
Represents objects that can take damage.
```csharp
public interface IDamageable
{
    int Health { get; }
    void TakeDamage(int amount);
    event Action OnDestroyed;
    event Action<int> OnDamageTaken;
}
```
**Used By:** PlayerCharacter, EnemyBase

#### **IGameComponent**
Defines basic component lifecycle (rarely used directly).
```csharp
public interface IGameComponent
{
    string Id { get; }
    bool IsActive { get; }
    void Initialize();
    void Shutdown();
}
```

### Benefits
- Single Responsibility Principle
- Multiple interface implementation
- Loose coupling between systems
- Easy to test and extend

---

## 3. **State Machine Pattern**

### Description
Used for AI behavior management, allowing entities to transition between discrete states with clean state-specific logic.

### Implementation: `StateMachine`
```csharp
public class StateMachine
{
    private Dictionary<AIState, Action> _states;
    private AIState _currentState;
    
    public void AddState(AIState state, Action onUpdate)
    public void ChangeState(AIState newState)
    public void Update()
}
```

### AI States
```csharp
public enum AIState
{
    Idle,      // Stationary
    Patrol,    // Movement pattern
    Chase,     // Following player
    Attack,    // Combat
    Stun,      // Disabled temporarily
}
```

### Benefits
- Clean behavior organization
- Easy state transitions
- Clear state responsibilities
- Extensible for new states

### Current Implementation
The `Enemy` class has an `UpdateAI()` method that implements patrol behavior as a simplified state machine without formal state objects.

---

## 4. **Manager/Coordinator Pattern**

### Description
Dedicated manager classes handle specific game subsystems, coordinating between components and maintaining central registries.

### Key Managers

#### **GameStateManager**
Manages overall game state transitions.
```csharp
public enum GameStateType { MainMenu, Playing, Paused, LevelComplete, GameOver }

public class GameStateManager
{
    private GameStateType _currentState;
    public void SetState(GameStateType newState)
}
```

#### **PlatformerGameManager**
Orchestrates the platformer gameplay loop.
- Manages level loading/unloading
- Spawns enemies and collectibles
- Coordinates between systems
- Handles input forwarding

```csharp
public class PlatformerGameManager : IUpdatable
{
    private Level _currentLevel;
    private PlayerCharacter _player;
    private List<EnemyBase> _enemies;
    private CollisionSystem _collisionSystem;
    private AISystem _aiSystem;
    // ... coordinates all gameplay
}
```

#### **InputSystem**
Centralizes input handling, converting raw input to commands.
```csharp
public enum InputCommand { MoveLeft, MoveRight, Jump, Pause, Quit }

public class InputSystem
{
    public InputCommand PollInput() { }
}
```

### Benefits
- Centralized control flow
- Easy system coordination
- Single point of modification
- Simpler debugging

---

## 5. **System Architecture (Data-Driven Design)**

### Description
Separate systems handle distinct aspects of gameplay, registered and updated together but maintaining independence.

### Core Systems

#### **CollisionSystem** (IUpdatable)
Manages collision detection and resolution.
- Maintains registry of collidable objects
- Performs entity-to-entity collision checks
- Performs entity-to-level collision detection
- Triggers collision events via ICollidable.OnCollide()

```csharp
public class CollisionSystem : IUpdatable
{
    private List<ICollidable> _collidables;
    public void Register(ICollidable collidable)
    public void Tick(float deltaTime) => CheckCollisions()
}
```

#### **PhysicsSystem** (IUpdatable)
Manages physics bodies and gravity.
- Maintains registry of physics bodies
- Delegates velocity updates to Unity's Rigidbody2D
- Tracks gravity state

```csharp
public class PhysicsSystem : IUpdatable
{
    private List<IPhysicsBody> _bodies;
    private Vector2 _gravity;
    public void Tick(float deltaTime)
}
```

#### **AISystem** (IUpdatable)
Coordinates enemy AI updates.
- Registers enemies
- Updates AI at fixed intervals (throttled for performance)
- Delegates to individual enemy UpdateAI() implementations

```csharp
public class AISystem : IUpdatable
{
    private List<EnemyBase> _enemies;
    public void Tick(float deltaTime)
}
```

#### **AdvancedRenderingSystem** (IUpdatable)
Handles game rendering (currently ASCII console-based).
- Maintains screen buffer
- Draws tiles, entities, collectibles
- Renders HUD

### Benefits
- **Separation of Concerns:** Each system has one responsibility
- **Testability:** Systems can be tested independently
- **Reusability:** Systems can be used in different game types
- **Performance:** Systems can be optimized independently
- **Maintainability:** Clear organization and dependencies

---

## 6. **Factory Pattern**

### Description
Encapsulates object creation logic, making it reusable and centralizable.

### Implementations

#### **LevelFactory**
Creates predefined game levels.
```csharp
public static class LevelFactory
{
    public static Level CreateLevel1() { }
    public static Level CreateLevel2() { }
}
```

**Features:**
- Tile-by-tile level design
- Platforms, hazards, goals, spawn points
- Encapsulates complex level setup

#### **VectorPool**
Object pool for Vector2 instances (optimization).
```csharp
public class VectorPool : ObjectPool<Vector2>
```

### Benefits
- Centralized creation logic
- Easy level modifications
- Supports procedural generation if needed
- Reduces code duplication

---

## 7. **Object Pool Pattern**

### Description
Pre-allocates and reuses objects instead of creating/destroying them, reducing garbage collection overhead.

### Implementation: `ObjectPool<T>`
```csharp
public class ObjectPool<T> where T : new()
{
    private Stack<T> _available;
    private HashSet<T> _inUse;
    
    public T Get() { }
    public void Return(T item) { }
}
```

**Generic parameters:**
- Initial capacity
- Custom factory function
- Callbacks for Get/Return

### Usage
- **VectorPool:** Pre-allocates Vector2 instances
- **Collectibles:** Could use object pool for coin reuse

### Benefits
- Reduced garbage collection
- Predictable memory usage
- Better performance for frequently-created objects
- Frame time stability

---

## 8. **Observer Pattern** (Events)

### Description
Objects communicate through loosely-coupled event subscriptions rather than direct references.

### Event Examples

#### **HealthComponent Events**
```csharp
public event Action OnDestroyed;
public event Action<int> OnDamageTaken;
public event Action<int> OnHealed;
```

#### **PlayerCharacter Events**
```csharp
public event Action OnDestroyed;
public event Action<int> OnDamageTaken;
public event Action OnGoalReached;
```

#### **Collectible Events**
```csharp
public event Action OnCollected;
```

### Usage Pattern
```csharp
// Subscription
_health.OnDamageTaken += (damage) => 
{
    Debug.Log($"Took {damage} damage!");
};

// Publishing
OnDamageTaken?.Invoke(10);
```

### Benefits
- Decoupled communication
- Easy to add/remove listeners
- Multiple listeners per event
- Cleaner code organization

---

## 9. **Composition Over Inheritance Pattern**

### Description
Rather than deep inheritance hierarchies, components are composed together to create complex behaviors.

### Example: PlayerCharacter
Instead of:
```csharp
// BAD: Deep hierarchy
class Creature : Entity { }
class Humanoid : Creature { }
class Combatant : Humanoid { }
class Player : Combatant { }
```

We use composition:
```csharp
// GOOD: Composition
public class PlayerCharacter : GameEntity
{
    private Rigidbody2D _rb;
    private PhysicsComponent _physics;
    private AdvancedMovementController _movementController;
    private HealthComponent _health;
}
```

### Benefits
- Flexibility in behavior combinations
- Avoids fragile base class problem
- Easy to test components independently
- Better code reuse

---

## 10. **Adapter Pattern** (Implicit)

### Description
Components adapt Unity's native systems (Rigidbody2D, Transform) to game-specific interfaces.

### Example: `PhysicsComponent`
Adapts Rigidbody2D to IPhysicsBody interface:
```csharp
public class PhysicsComponent : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    
    public Vector2 Velocity 
    { 
        get => _rigidbody.linearVelocity;
        set => _rigidbody.linearVelocity = value;
    }
    
    public void ApplyForce(Vector2 force) { }
    public void SetVelocity(Vector2 newVelocity) { }
}
```

### Benefits
- Consistent interface across codebase
- Unity engine abstraction
- Easy to swap implementations
- Testable without Unity dependencies

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                   GameBootstrap                         │
│              (Initializes all systems)                  │
└──────────────────────┬──────────────────────────────────┘
                       │
        ┌──────────────┼──────────────┐
        │              │              │
    ┌───▼────┐   ┌────▼────┐  ┌─────▼────┐
    │GameState│   │Platform │  │  Level   │
    │Manager  │   │ Manager │  │ Factory  │
    └─────────┘   └────┬────┘  └──────────┘
                       │
        ┌──────────────┼──────────────┬─────────┐
        │              │              │         │
    ┌───▼────┐  ┌─────▼────┐ ┌──────▼───┐ ┌──▼──────┐
    │Collision   │Physics   │ │   AI     │ │Rendering│
    │  System    │ System   │ │ System   │ │ System  │
    └────┬────┘  └────┬────┘ └────┬─────┘ └────┬────┘
         │            │            │             │
    ┌────▼────────────▼────────────▼─────────────▼────┐
    │                                                  │
    │         Game Entities (MonoBehaviours)          │
    │  ┌──────────────┐  ┌──────────────────┐        │
    │  │PlayerCharacter│  │   Enemy / NPC    │        │
    │  │  Components: │  │   Components:    │        │
    │  │• Physics     │  │  • Physics       │        │
    │  │• Health      │  │  • Health        │        │
    │  │• Movement    │  │  • AI Logic      │        │
    │  │• Collision   │  │  • Collision     │        │
    │  └──────────────┘  └──────────────────┘        │
    └─────────────────────────────────────────────────┘
```

---

## Data Flow: Game Loop

```
Frame Update
    │
    ├─→ InputSystem.PollInput()
    │      └─→ Store in movement state
    │
    ├─→ PlatformerGameManager.Tick()
    │      ├─→ GameStateManager check
    │      ├─→ PlayerCharacter.Tick()
    │      │     ├─→ AdvancedMovementController.UpdateMovement()
    │      │     └─→ Update attack state
    │      │
    │      ├─→ Enemy[].Tick() + UpdateAI()
    │      │     └─→ Simple patrol AI
    │      │
    │      ├─→ CollisionSystem.Tick()
    │      │     ├─→ Entity-to-entity checks
    │      │     └─→ Entity-to-level checks
    │      │
    │      ├─→ CollectibleSystem.CheckCollisions()
    │      │     └─→ Update collected count
    │      │
    │      └─→ AdvancedRenderingSystem.Tick()
    │           ├─→ Clear buffer
    │           ├─→ Draw level
    │           ├─→ Draw entities
    │           ├─→ Draw HUD
    │           └─→ Present to screen
    │
    └─→ Frame Complete
```

---

## Key Design Decisions

### 1. **MonoBehaviour Base Architecture**
- Leverages Unity's native systems
- Easier inspector integration
- Better performance with physics
- Familiar to Unity developers

### 2. **System-Based Organization**
- Clear separation of concerns
- Each system independent and testable
- Easy to add/remove/modify systems
- Scalable architecture

### 3. **Interface-Based Communication**
- Loose coupling between components
- Easy to swap implementations
- Flexible composition patterns

### 4. **Event-Driven Communication**
- Systems communicate through events
- No hard references between unrelated systems
- Easy to extend behavior

### 5. **Manager Coordination**
- Central orchestration point
- Easy to understand game flow
- Single point of modification for global behavior

---

## Extensibility Examples

### Adding a New Enemy Type
1. Extend `EnemyBase`
2. Implement `Initialize()`, `Tick()`, `UpdateAI()`
3. Register with `CollisionSystem` and `AISystem`

### Adding a New Collectible Type
1. Implement `ICollectible`
2. Add to `CollectibleSystem`
3. Update `CheckCollisions()` behavior if needed

### Adding a New Game System
1. Implement `IUpdatable`
2. Register in `PlatformerGameManager`
3. Call `Tick()` in game loop

### Adding New Damage Types
1. Extend `HealthComponent`
2. Implement damage mitigation logic
3. Update event handlers

---

## Performance Considerations

### Object Pooling
- Vector2 instances reused via `VectorPool`
- Reduces garbage collection

### Throttled AI Updates
- AISystem updates at fixed intervals (0.1s)
- Not every frame, reduces CPU usage

### System Registration
- Only registered objects processed
- Dynamic registration/unregistration
- No waste on inactive objects

### Fixed Timestep Physics
- PhysicsSystem uses fixed delta time
- Stable physics simulation
- Predictable frame timing

---

## Testing Strategy

### Unit Testable Components
- **PhysicsComponent:** Can test velocity/force application
- **HealthComponent:** Can test damage/healing logic
- **GameStateManager:** Can test state transitions
- **CollisionSystem:** Can test collision detection math

### Integration Testable Systems
- **PlayerCharacter:** Movement + Physics
- **Enemy + AISystem:** AI behavior
- **CollisionSystem + Entities:** Collision response

### End-to-End Testing
- **Full game loop:** PlatformerGameManager
- **Level transitions:** Level loading/unloading
- **Game state:** From menu to gameplay to completion

---

## Summary

The PixelForce architecture employs industry-standard design patterns to create a maintainable, extensible, and performant platformer game. Key patterns include:

1. **Component Pattern** - MonoBehaviour-based entity composition
2. **Interface Segregation** - Multiple focused interfaces
3. **State Machine** - AI behavior management
4. **Manager Pattern** - System coordination
5. **System Architecture** - Separated concerns
6. **Factory Pattern** - Level and object creation
7. **Object Pool Pattern** - Performance optimization
8. **Observer Pattern** - Event-driven communication
9. **Composition Over Inheritance** - Flexible behavior
10. **Adapter Pattern** - Unity integration

This design supports:
- ✅ Easy feature addition
- ✅ Clear code organization
- ✅ Good performance
- ✅ Testability
- ✅ Scalability
- ✅ Team collaboration
