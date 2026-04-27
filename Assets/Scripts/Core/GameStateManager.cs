using System;
using UnityEngine;
/// <summary>
/// Game state management.
/// </summary>
public enum GameStateType
{
    MainMenu,
    Playing,
    Paused,
    LevelComplete,
    GameOver
}

public class GameStateManager
{
    private GameStateType _currentState = GameStateType.MainMenu;
    public GameStateType CurrentState => _currentState;

    public void SetState(GameStateType newState)
    {
        if (_currentState != newState)
        {
            _currentState = newState;
            Console.WriteLine($"[State] Changed to: {newState}");
        }
    }

    public bool IsPlaying => _currentState == GameStateType.Playing;
    public bool IsPaused => _currentState == GameStateType.Paused;
    public bool IsGameOver => _currentState == GameStateType.GameOver;
}
