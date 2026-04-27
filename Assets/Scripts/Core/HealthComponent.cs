using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int _health;

    public int MaxHealth => maxHealth;
    public int Health => _health;

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;
    public event Action<int> OnHealed;

    private void Awake()
    {
        _health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount < 0) amount = 0;

        _health -= amount;
        OnDamageTaken?.Invoke(amount);

        if (_health <= 0)
        {
            _health = 0;
            OnDestroyed?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount < 0) amount = 0;

        int oldHealth = _health;
        _health += amount;

        if (_health > maxHealth)
            _health = maxHealth;

        int healed = _health - oldHealth;
        if (healed > 0)
            OnHealed?.Invoke(healed);
    }

    public void Reset()
    {
        _health = maxHealth;
    }
}
