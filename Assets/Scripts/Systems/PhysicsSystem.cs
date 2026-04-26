using System;
using System.Collections.Generic;
using System.Numerics;

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
    /// </summary>
    public void Tick(float deltaTime)
    {
        _accumulatedTime += deltaTime;

        // Fixed timestep physics loop (prevents instability at high framerates)
        while (_accumulatedTime >= _fixedDeltaTime)
        {
            SimulatePhysics(_fixedDeltaTime);
            _accumulatedTime -= _fixedDeltaTime;
        }
    }

    private void SimulatePhysics(float deltaTime)
    {
        // Update all physics bodies
        foreach (var body in _bodies)
        {
            if (body is PhysicsComponent physics)
            {
                // Find the entity this body belongs to
                var entity = FindEntityForBody(body);
                if (entity != null && entity.IsActive)
                {
                    physics.Update(entity.Transform, deltaTime, _gravity);
                }
            }
        }

        // Resolve collisions (simplified broadphase)
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
            var bounds1 = col1.Bounds;
            var bounds2 = col2.Bounds;

            // AABB collision detection
            if (bounds1.X < bounds2.X + bounds2.Width &&
                bounds1.X + bounds1.Width > bounds2.X &&
                bounds1.Y < bounds2.Y + bounds2.Height &&
                bounds1.Y + bounds1.Height > bounds2.Y)
            {
                col1.OnCollision(col2);
                col2.OnCollision(col1);
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