/// <summary>
/// Base interface for all game objects/entities.
/// </summary>
public interface IGameComponent
{
    /// <summary>
    /// Unique identifier for this component.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Whether this component is currently active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Initialize this component.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Cleanup when component is destroyed.
    /// </summary>
    void Shutdown();
}