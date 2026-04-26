using System;

/// <summary>
/// Health system for damageable entities.
/// Can be composed into entities or inherited.
/// </summary>
public class HealthComponent : IDamageable
{
    private int _health;
    
    public int MaxHealth { get; }
    public int Health => _health;

    public event Action OnDestroyed;
    public event Action<int> OnDamageTaken;
    public event Action<int> OnHealed;

    public HealthComponent(int maxHealth)
    {
        MaxHealth = maxHealth;
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

        if (_health > MaxHealth)
            _health = MaxHealth;

        int healed = _health - oldHealth;
        if (healed > 0)
            OnHealed?.Invoke(healed);
    }

    public void Reset()
    {
        _health = MaxHealth;
    }
}