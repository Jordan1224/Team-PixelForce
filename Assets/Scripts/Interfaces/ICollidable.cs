/// <summary>
/// Represents an object that can collide with other objects.
/// </summary>
public interface ICollidable
{
    /// <summary>
    /// Bounding box for collision detection.
    /// </summary>
    System.Drawing.Rectangle Bounds { get; }

    /// <summary>
    /// Called when this object collides with another.
    /// </summary>
    /// <param name="other">The other collidable object</param>
    void OnCollision(ICollidable other);
}