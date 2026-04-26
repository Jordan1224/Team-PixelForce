using System;
using System.Collections.Generic;

/// <summary>
/// Maps raw keyboard input to high-level commands.
/// Decouples input handling from game logic.
/// </summary>
public class InputSystem
{
    private Dictionary<char, InputCommand> _keyMap;
    private InputCommand _currentCommand = InputCommand.None;

    public InputCommand CurrentCommand => _currentCommand;

    public InputSystem()
    {
        _keyMap = new Dictionary<char, InputCommand>
        {
            { 'a', InputCommand.MoveLeft },
            { 'A', InputCommand.MoveLeft },
            { 'd', InputCommand.MoveRight },
            { 'D', InputCommand.MoveRight },
            { 'w', InputCommand.Jump },
            { 'W', InputCommand.Jump },
            { ' ', InputCommand.Jump },
            { 'j', InputCommand.Attack },
            { 'J', InputCommand.Attack },
            { 'e', InputCommand.Interact },
            { 'E', InputCommand.Interact },
            { 'p', InputCommand.Pause },
            { 'P', InputCommand.Pause },
        };
    }

    /// <summary>
    /// Poll current input and return command.
    /// </summary>
    public InputCommand PollInput()
    {
        _currentCommand = InputCommand.None;

        if (Console.KeyAvailable)
        {
            try
            {
                var keyInfo = Console.ReadKey(true);
                if (_keyMap.TryGetValue(keyInfo.KeyChar, out var command))
                {
                    _currentCommand = command;
                }
            }
            catch { }
        }

        return _currentCommand;
    }

    /// <summary>
    /// Customize key mapping.
    /// </summary>
    public void MapKey(char key, InputCommand command)
    {
        _keyMap[key] = command;
    }

    /// <summary>
    /// Remove key mapping.
    /// </summary>
    public void UnmapKey(char key)
    {
        _keyMap.Remove(key);
    }

    public void Clear()
    {
        _keyMap.Clear();
    }
}