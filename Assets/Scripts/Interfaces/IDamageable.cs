/// <summary>
/// Represents an object that can take damage.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// Current health points.
    /// </summary>
    int Health { get; }

    /// <summary>
    /// Called when this object takes damage.
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    void TakeDamage(int amount);

    /// <summary>
    /// Event raised when this object is destroyed (health <= 0).
    /// </summary>
    event System.Action OnDestroyed;
}