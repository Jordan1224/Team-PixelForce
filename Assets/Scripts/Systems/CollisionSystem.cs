using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Advanced collision detection and resolution.
/// </summary>
public class CollisionSystem : IUpdatable
{
    private Level _level;
    private List<ICollidable> _collidables = new List<ICollidable>();
    private const float COLLISION_CHECK_DISTANCE = 2f;

    public CollisionSystem(Level level)
    {
        _level = level;
    }

    public void Register(ICollidable collidable)
    {
        if (!_collidables.Contains(collidable))
            _collidables.Add(collidable);
    }

    public void Unregister(ICollidable collidable)
    {
        _collidables.Remove(collidable);
    }

    public void Tick(float deltaTime)
    {
        // Entity-to-entity collisions
        for (int i = 0; i < _collidables.Count; i++)
        {
            for (int j = i + 1; j < _collidables.Count; j++)
            {
                CheckEntityCollision(_collidables[i], _collidables[j]);
            }
        }

        // Entity-to-level collisions
        foreach (var collidable in _collidables)
        {
            CheckLevelCollisions(collidable);
        }
    }

    private void CheckEntityCollision(ICollidable a, ICollidable b)
    {
        var boundsA = GetBounds(a);
        var boundsB = GetBounds(b);

        if (BoundsIntersect(boundsA, boundsB))
        {
            a.OnCollide(b);
            b.OnCollide(a);
        }
    }

    private Bounds GetBounds(ICollidable collidable)
    {
        var collider = collidable.GetCollider();
        if (collider != null)
            return collider.bounds;
        var pos = collidable.GetPosition();
        return new Bounds(pos, Vector3.one);
    }

    private bool BoundsIntersect(Bounds a, Bounds b)
    {
        return a.Intersects(b);
    }

    private void CheckLevelCollisions(ICollidable entity)
    {
        if (entity is PlayerCharacter player)
        {
            var bounds = GetBounds(player);
            var playerPos = player.GetPosition();

            bool isGrounded = false;

            // Check all tiles in level
            for (int x = 0; x < _level.Width; x++)
            {
                for (int y = 0; y < _level.Height; y++)
                {
                    var tileType = _level.GetTile(x, y);
                    if (tileType == TileType.Empty) continue;

                    var tileBoundsRect = _level.GetTileBounds(x, y);
                    var tileBounds = RectToBounds(tileBoundsRect);

                    if (tileType == TileType.Solid || tileType == TileType.Platform)
                    {
                        // Check if player is above tile (landing)
                        if (BoundsIntersect(bounds, tileBounds) && player.Velocity.y >= 0)
                        {
                            isGrounded = true;
                            player.transform.position = new Vector3(playerPos.x, tileBounds.max.y + bounds.extents.y, player.transform.position.z);
                        }
                    }
                    else if (tileType == TileType.Hazard)
                    {
                        if (BoundsIntersect(bounds, tileBounds))
                        {
                            player.TakeDamage(100); // Instant death
                        }
                    }
                    else if (tileType == TileType.Goal)
                    {
                        if (BoundsIntersect(bounds, tileBounds))
                        {
                            player.ReachGoal();
                        }
                    }
                }
            }

            player.SetGrounded(isGrounded);
        }
    }

    private Bounds RectToBounds(System.Drawing.Rectangle rect)
    {
        var center = new Vector3(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f, 0);
        var size = new Vector3(rect.Width, rect.Height, 1);
        return new Bounds(center, size);
    }

    public void Clear()
    {
        _collidables.Clear();
    }
}
