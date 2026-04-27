using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines a tile in the world.
/// </summary>
public enum TileType
{
    Empty,
    Solid,
    Platform,
    OneWayPlatform,
    Hazard,
    Goal,
    SpawnPoint
}

/// <summary>
/// Tile-based level system.
/// </summary>
public class Level
{
    private TileType[,] _tiles;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float TileSize { get; set; } = 1f;

    public Level(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new TileType[height, width];
    }

    public void SetTile(int x, int y, TileType type)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            _tiles[y, x] = type;
    }

    public TileType GetTile(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            return _tiles[y, x];
        return TileType.Empty;
    }

    public System.Drawing.Rectangle GetTileBounds(int x, int y)
    {
        return new System.Drawing.Rectangle(
            (int)(x * TileSize),
            (int)(y * TileSize),
            (int)TileSize,
            (int)TileSize
        );
    }

    /// <summary>
    /// Get all solid tiles in a given bounds area.
    /// </summary>
    public List<(int x, int y, TileType type)> GetTilesInBounds(System.Drawing.Rectangle bounds)
    {
        var result = new List<(int, int, TileType)>();
        
        int startX = (int)(bounds.X / TileSize);
        int startY = (int)(bounds.Y / TileSize);
        int endX = (int)((bounds.X + bounds.Width) / TileSize) + 1;
        int endY = (int)((bounds.Y + bounds.Height) / TileSize) + 1;

        for (int y = startY; y <= endY; y++)
        {
            for (int x = startX; x <= endX; x++)
            {
                var tile = GetTile(x, y);
                if (tile != TileType.Empty)
                    result.Add((x, y, tile));
            }
        }

        return result;
    }

    public Vector2 FindSpawnPoint()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (_tiles[y, x] == TileType.SpawnPoint)
                    return new Vector2(x * TileSize + TileSize / 2, y * TileSize);
            }
        }
        return Vector2.zero;
    }

    public Vector2 FindGoal()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (_tiles[y, x] == TileType.Goal)
                    return new Vector2(x * TileSize + TileSize / 2, y * TileSize);
            }
        }
        return Vector2.zero;
    }
}
