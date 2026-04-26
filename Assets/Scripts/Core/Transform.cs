using System;
using System.Numerics;

/// <summary>
/// Represents position, rotation, and scale in the game world.
/// </summary>
public class Transform
{
    /// <summary>
    /// World position.
    /// </summary>
    public Vector2 Position { get; set; }

    /// <summary>
    /// Rotation in degrees (0-360).
    /// </summary>
    public float Rotation { get; set; }

    /// <summary>
    /// Scale multiplier (1.0 = normal size).
    /// </summary>
    public Vector2 Scale { get; set; }

    public Transform(Vector2 position = default, float rotation = 0f, Vector2? scale = null)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale ?? Vector2.One;
    }

    /// <summary>
    /// Get direction vector based on rotation.
    /// </summary>
    public Vector2 GetDirection()
    {
        float radians = Rotation * (float)Math.PI / 180f;
        return new Vector2((float)Math.Cos(radians), (float)Math.Sin(radians));
    }

    /// <summary>
    /// Move relative to current position.
    /// </summary>
    public void Translate(Vector2 offset)
    {
        Position += offset;
    }

    /// <summary>
    /// Rotate by additional degrees.
    /// </summary>
    public void Rotate(float degrees)
    {
        Rotation += degrees;
        if (Rotation >= 360f) Rotation -= 360f;
        if (Rotation < 0f) Rotation += 360f;
    }
}