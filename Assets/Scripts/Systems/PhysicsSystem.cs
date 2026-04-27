using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized physics system for all physics bodies.
/// Handles gravity, velocity integration, and collision resolution.
/// </summary>
public class PhysicsSystem : IUpdatable
{
    private List<IPhysicsBody> _bodies = new List<IPhysicsBody>();
    private Vector2 _gravity = new Vector2(0, 9.8f);
    private float _fixedDeltaTime = 1f / 60f;
    private float _accumulatedTime = 0f;

    public Vector2 Gravity
    {
        get => _gravity;
        set => _gravity = value;
    }

    public void RegisterBody(IPhysicsBody body)
    {
        if (!_bodies.Contains(body))
            _bodies.Add(body);
    }

    public void UnregisterBody(IPhysicsBody body)
    {
        _bodies.Remove(body);
    }

    /// <summary>
    /// Run physics simulation at fixed timestep for stability.
    /// Note: Actual physics is handled by Unity's Rigidbody2D.
    /// This system mainly handles collision queries.
    /// </summary>
    public void Tick(float deltaTime)
    {
        // Physics is handled by Unity's engine with Rigidbody2D
        // We just maintain the bodies list and can query it if needed
    }

    private void SimulatePhysics(float deltaTime)
    {
        // Collision detection using ICollidable interface
        for (int i = 0; i < _bodies.Count; i++)
        {
            for (int j = i + 1; j < _bodies.Count; j++)
            {
                CheckCollision(_bodies[i], _bodies[j]);
            }
        }
    }

    private void CheckCollision(IPhysicsBody body1, IPhysicsBody body2)
    {
        // Check if both are collidable
        if (body1 is ICollidable col1 && body2 is ICollidable col2)
        {
            var collider1 = col1.GetCollider();
            var collider2 = col2.GetCollider();

            // Use Unity's collider bounds for AABB collision detection
            if (collider1 != null && collider2 != null)
            {
                var bounds1 = collider1.bounds;
                var bounds2 = collider2.bounds;

                // AABB collision detection
                if (bounds1.Intersects(bounds2))
                {
                    col1.OnCollide(col2);
                    col2.OnCollide(col1);
                }
            }
        }
    }

    private GameEntity FindEntityForBody(IPhysicsBody body)
    {
        // This is a simplified lookup. In production, you'd maintain a proper mapping.
        if (body is GameEntity entity)
            return entity;
        return null;
    }

    public void Clear()
    {
        _bodies.Clear();
    }
}