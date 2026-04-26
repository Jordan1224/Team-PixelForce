using System;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Manages all game entities, systems, and game state.
/// Responsible for initialization, updating, and cleanup.
/// </summary>
public class GameManager
{
    private List<IUpdatable> _updatables = new List<IUpdatable>();
    private List<IGameComponent> _components = new List<IGameComponent>();
    private bool _isRunning = false;
    private float _targetFrameTime = 1f / 60f; // 60 FPS target

    // Game systems
    private PhysicsSystem _physicsSystem;
    private AISystem _aiSystem;
    private InputSystem _inputSystem;
    private RenderingSystem _renderingSystem;

    // Game state
    private GameState _gameState = GameState.Menu;
    public GameState CurrentState => _gameState;

    public event Action<GameState> OnStateChanged;

    public GameManager()
    {
        _physicsSystem = new PhysicsSystem();
        _aiSystem = new AISystem();
        _inputSystem = new InputSystem();
        _renderingSystem = new RenderingSystem();

        // Register systems as updatables
        _updatables.Add(_physicsSystem);
        _updatables.Add(_aiSystem);
        _updatables.Add(_renderingSystem);
    }

    /// <summary>
    /// Register a component to be managed.
    /// </summary>
    public void RegisterComponent(IGameComponent component)
    {
        if (_components.Contains(component)) return;

        _components.Add(component);
        component.Initialize();

        if (component is IUpdatable updatable)
        {
            _updatables.Add(updatable);
        }

        if (component is IPhysicsBody physicsBody)
        {
            _physicsSystem.RegisterBody(physicsBody);
        }

        if (component is EnemyBase enemy)
        {
            _aiSystem.RegisterEnemy(enemy);
        }

        if (component is PlayerCharacter player)
        {
            _aiSystem.SetPlayer(player);
        }
    }

    /// <summary>
    /// Unregister a component from management.
    /// </summary>
    public void UnregisterComponent(IGameComponent component)
    {
        if (!_components.Contains(component)) return;

        component.Shutdown();
        _components.Remove(component);

        if (component is IUpdatable updatable)
        {
            _updatables.Remove(updatable);
        }

        if (component is IPhysicsBody physicsBody)
        {
            _physicsSystem.UnregisterBody(physicsBody);
        }

        if (component is EnemyBase enemy)
        {
            _aiSystem.UnregisterEnemy(enemy);
        }
    }

    /// <summary>
    /// Change game state.
    /// </summary>
    public void SetState(GameState newState)
    {
        if (_gameState == newState) return;

        _gameState = newState;
        OnStateChanged?.Invoke(newState);

        Console.WriteLine($"Game state changed to: {newState}");
    }

    /// <summary>
    /// Main game loop.
    /// </summary>
    public void Run()
    {
        _isRunning = true;
        SetState(GameState.Playing);
        
        Stopwatch stopwatch = Stopwatch.StartNew();
        float deltaTime = 0f;

        while (_isRunning)
        {
            deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
            stopwatch.Restart();

            // Cap delta time to prevent large jumps
            if (deltaTime > 0.1f)
                deltaTime = 0.1f;

            // Handle pause
            var command = _inputSystem.PollInput();
            if (command == InputCommand.Pause)
            {
                SetState(_gameState == GameState.Paused ? GameState.Playing : GameState.Paused);
            }

            if (command == InputCommand.None && Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).KeyChar;
                if (key == (char)27) // ESC
                {
                    Stop();
                }
            }

            // Update all updatable components if playing
            if (_gameState == GameState.Playing)
            {
                foreach (var updatable in _updatables)
                {
                    updatable.Tick(deltaTime);
                }

                // Check win/lose conditions
                CheckGameEndConditions();
            }

            // Frame timing
            System.Threading.Thread.Sleep(Math.Max(0, (int)((_targetFrameTime - deltaTime) * 1000)));
        }

        Shutdown();
    }

    private void CheckGameEndConditions()
    {
        // Check if player is dead
        var player = FindComponent<PlayerCharacter>();
        if (player != null && !player.IsActive)
        {
            SetState(GameState.GameOver);
            Stop();
        }

        // Check if all enemies are dead
        var enemies = FindAllComponents<EnemyBase>();
        bool allEnemiesDead = true;
        foreach (var enemy in enemies)
        {
            if (enemy.IsActive)
            {
                allEnemiesDead = false;
                break;
            }
        }

        if (allEnemiesDead && enemies.Count > 0)
        {
            SetState(GameState.Victory);
            Stop();
        }
    }

    /// <summary>
    /// Stop the game loop.
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
    }

    /// <summary>
    /// Clean up all components.
    /// </summary>
    private void Shutdown()
    {
        foreach (var component in _components)
        {
            component.Shutdown();
        }
        _components.Clear();
        _updatables.Clear();
        
        _physicsSystem.Clear();
        _aiSystem.Clear();
        _inputSystem.Clear();
        _renderingSystem.Clear();
        
        Console.WriteLine("Game shutdown complete.");
    }

    /// <summary>
    /// Get systems for direct access if needed.
    /// </summary>
    public PhysicsSystem GetPhysicsSystem() => _physicsSystem;
    public AISystem GetAISystem() => _aiSystem;
    public InputSystem GetInputSystem() => _inputSystem;
    public RenderingSystem GetRenderingSystem() => _renderingSystem;

    public IReadOnlyList<IGameComponent> GetAllComponents() => _components.AsReadOnly();

    private T FindComponent<T>() where T : class
    {
        foreach (var component in _components)
        {
            if (component is T typed)
                return typed;
        }
        return null;
    }

    private List<T> FindAllComponents<T>() where T : class
    {
        var result = new List<T>();
        foreach (var component in _components)
        {
            if (component is T typed)
                result.Add(typed);
        }
        return result;
    }
}