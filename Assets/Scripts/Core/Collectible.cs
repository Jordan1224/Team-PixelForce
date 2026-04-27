using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Collectible item in the world.
/// </summary>
public interface ICollectible
{
    Vector2 Position { get; }
    bool IsActive { get; set; }
    void OnCollect();
}

/// <summary>
/// Simple coin/item collectible.
/// </summary>
public class Collectible : ICollectible
{
    public Vector2 Position { get; set; }
    public bool IsActive { get; set; } = true;
    public int Value { get; set; } = 10;

    public event Action OnCollected;

    public Collectible(Vector2 position)
    {
        Position = position;
    }

    public void OnCollect()
    {
        IsActive = false;
        OnCollected?.Invoke();
    }
}

/// <summary>
/// Manages all collectibles in the game.
/// </summary>
public class CollectibleSystem
{
    private List<ICollectible> _collectibles = new List<ICollectible>();
    private int _totalCollected = 0;

    public int TotalCollected => _totalCollected;
    public int RemainingCollectibles => _collectibles.Count(c => c.IsActive);

    public void Add(ICollectible collectible)
    {
        _collectibles.Add(collectible);
    }

    public void Remove(ICollectible collectible)
    {
        _collectibles.Remove(collectible);
    }

    public void CheckCollisions(Vector2 playerPosition, float playerRadius = 1f)
    {
        foreach (var collectible in _collectibles)
        {
            if (!collectible.IsActive) continue;

            // Simple distance-based collision
            float distance = Vector2.Distance(playerPosition, collectible.Position);
            float collisionDistance = playerRadius + 0.5f; // Collectible radius

            if (distance <= collisionDistance)
            {
                collectible.OnCollect();
                _totalCollected++;
            }
        }
    }

    public void Clear()
    {
        _collectibles.Clear();
        _totalCollected = 0;
    }
}
