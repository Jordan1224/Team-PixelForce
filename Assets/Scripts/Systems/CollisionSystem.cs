using System.Numerics;
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
        var boundsA = a.Bounds;
        var boundsB = b.Bounds;

        if (boundsA.IntersectsWith(boundsB))
        {
            a.OnCollision(b);
            b.OnCollision(a);
        }
    }

    private void CheckLevelCollisions(ICollidable entity)
    {
        if (entity is PlayerCharacter player)
        {
            var bounds = player.Bounds;
            var tilesInBounds = _level.GetTilesInBounds(bounds);

            bool isGrounded = false;

            foreach (var (x, y, tileType) in tilesInBounds)
            {
                var tileBounds = _level.GetTileBounds(x, y);

                if (tileType == TileType.Solid || tileType == TileType.Platform)
                {
                    // Check if player is above tile (landing)
                    if (bounds.Bottom >= tileBounds.Top && bounds.Bottom <= tileBounds.Top + 10 &&
                        player.Velocity.Y >= 0)
                    {
                        isGrounded = true;
                        player.Transform.Position = new Vector2(
                            player.Transform.Position.X,
                            tileBounds.Top - (bounds.Height / 2)
                        );
                    }
                    // Check collision from sides
                    else if (bounds.IntersectsWith(tileBounds))
                    {
                        // Resolve collision (simple push out)
                        PushOutOfTile(player, tileBounds);
                    }
                }
                else if (tileType == TileType.Hazard)
                {
                    if (bounds.IntersectsWith(tileBounds))
                    {
                        player.TakeDamage(100); // Instant death
                    }
                }
                else if (tileType == TileType.Goal)
                {
                    if (bounds.IntersectsWith(tileBounds))
                    {
                        player.ReachGoal();
                    }
                }
            }

            player.SetGrounded(isGrounded);
        }
    }

    private void PushOutOfTile(PlayerCharacter player, System.Drawing.Rectangle tileBounds)
    {
        var playerPos = player.Transform.Position;
        var playerBounds = player.Bounds;

        // Determine push direction based on overlap
        float overlapLeft = playerBounds.Right - tileBounds.Left;
        float overlapRight = tileBounds.Right - playerBounds.Left;
        float overlapTop = playerBounds.Bottom - tileBounds.Top;
        float overlapBottom = tileBounds.Bottom - playerBounds.Top;

        float minOverlap = System.Math.Min(System.Math.Min(overlapLeft, overlapRight), System.Math.Min(overlapTop, overlapBottom));

        if (minOverlap == overlapLeft)
        {
            player.Transform.Position = new Vector2(tileBounds.Left - playerBounds.Width / 2 - 0.1f, playerPos.Y);
        }
        else if (minOverlap == overlapRight)
        {
            player.Transform.Position = new Vector2(tileBounds.Right + playerBounds.Width / 2 + 0.1f, playerPos.Y);
        }
    }

    public void Clear()
    {
        _collidables.Clear();
    }
}
