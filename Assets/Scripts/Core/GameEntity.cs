using System;
using UnityEngine;

/// <summary>
/// Base class for all game entities in Unity.
/// Provides common behavior for GameObjects in the scene.
/// </summary>
public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] protected string entityId;
    protected bool isActive = true;

    public string Id => entityId;
    public bool IsActive => isActive;

    protected virtual void Start()
    {
        Initialize();
    }

    protected virtual void OnDestroy()
    {
        Shutdown();
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
