/// <summary>
/// Represents any object that needs to update during the game loop.
/// </summary>
public interface IUpdatable
{
    /// <summary>
    /// Called once per frame to update object state.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last frame in seconds</param>
    void Tick(float deltaTime);
}