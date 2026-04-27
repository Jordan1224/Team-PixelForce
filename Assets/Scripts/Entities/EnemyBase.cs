using System;
using UnityEngine;

public abstract class EnemyBase : GameEntity, IDamageable, ICollidable
{
    protected HealthComponent _health;
    protected Rigidbody2D _rb;
    protected Collider2D _collider;

    public int Health => _health != null ? _health.Health : 0;

    protected virtual void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    // Forward IDamageable event to HealthComponent
    public event Action OnDestroyed
    {
        add    { _health.OnDestroyed += value; }
        remove { _health.OnDestroyed -= value; }
    }

    public void TakeDamage(int amount)
    {
        _health?.TakeDamage(amount);
    }

    // ICollidable
    public Collider2D GetCollider() => _collider;
    public Vector2 GetPosition() => transform.position;
    public virtual void OnCollide(ICollidable other) { }
}
