/// <summary>
/// Game state enumeration.
/// </summary>
public enum GameState
{
    Menu,
    Loading,
    Playing,
    Paused,
    GameOver,
    Victory
}

/// <summary>
/// High-level input commands.
/// </summary>
public enum InputCommand
{
    MoveLeft,
    MoveRight,
    Jump,
    Attack,
    Interact,
    Pause,
    None
}

/// <summary>
/// Enemy AI states.
/// </summary>
public enum AIState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Knockback,
    Dead
}