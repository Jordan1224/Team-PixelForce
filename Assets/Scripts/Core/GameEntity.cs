using System;
using System.Numerics;

/// <summary>
/// Base class for all game entities.
/// Provides common behavior for components that need position, update loop, and lifecycle.
/// </summary>
public abstract class GameEntity : IGameComponent, IUpdatable
{
    public string Id { get; protected set; }
    public bool IsActive { get; set; }
    public Transform Transform { get; protected set; }

    protected GameEntity(string id)
    {
        Id = id;
        IsActive = true;
        Transform = new Transform();
    }

    public virtual void Initialize()
    {
        // Override in derived classes
    }

    public virtual void Shutdown()
    {
        // Override in derived classes
    }

    public abstract void Tick(float deltaTime);
}