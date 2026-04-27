using UnityEngine;

public interface ICollidable
{
    Collider2D GetCollider();
    Vector2 GetPosition();
    void OnCollide(ICollidable other);
}
