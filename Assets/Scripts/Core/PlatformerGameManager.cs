using System;
using System.Collections.Generic;
using System.Numerics;
using System.Diagnostics;

/// <summary>
/// Main game orchestrator for platformer gameplay.
/// </summary>
public class PlatformerGameManager : IUpdatable
{
    private Level _currentLevel;
    private Level[] _levels;
    private int _currentLevelIndex = 0;

    private PlayerCharacter _player;
    private List<EnemyBase> _enemies = new List<EnemyBase>();
    private CollectibleSystem _collectibles;
    private CollisionSystem _collisionSystem;
    private InputSystem _inputSystem;
    private UISystem _uiSystem;
    private GameStateManager _stateManager;

    private bool _levelComplete = false;
    private Stopwatch _gameTimer;
    private float _timeSinceGameOver = 0f;

    public PlatformerGameManager()
    {
        // Initialize levels
        _levels = new[] {
            LevelFactory.CreateLevel1(),
            LevelFactory.CreateLevel2()
        };

        _uiSystem = new UISystem();
        _inputSystem = new InputSystem();
        _stateManager = new GameStateManager();
        _collectibles = new CollectibleSystem();
    }

    public void Initialize()
    {
        LoadLevel(0);
        _stateManager.SetState(GameStateType.Playing);
        _gameTimer = Stopwatch.StartNew();
    }

    private void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Console.WriteLine("All levels complete!");
            _stateManager.SetState(GameStateType.GameOver);
            return;
        }

        _currentLevelIndex = levelIndex;
        _currentLevel = _levels[levelIndex];
        _levelComplete = false;

        // Clear previous entities
        _enemies.Clear();
        _collectibles.Clear();

        // Create player
        _player = new PlayerCharacter("Hero");
        _player.Transform.Position = _currentLevel.FindSpawnPoint();
        _player.OnGoalReached += OnLevelComplete;
        _player.OnDestroyed += OnPlayerDeath;

        // Create collision system for this level
        _collisionSystem = new CollisionSystem(_currentLevel);
        _collisionSystem.Register(_player);

        // Spawn enemies
        SpawnEnemies();

        // Spawn collectibles
        SpawnCollectibles();

        _uiSystem.SetLevel(_currentLevelIndex + 1, _levels.Length);
    }

    private void SpawnEnemies()
    {
        // Patrol enemies
        var patrolEnemy1 = new PatrolEnemy("PatrolEnemy1", new Vector2(10, 14));
        _enemies.Add(patrolEnemy1);
        patrolEnemy1.Initialize();
        _collisionSystem.Register(patrolEnemy1);

        var patrolEnemy2 = new PatrolEnemy("PatrolEnemy2", new Vector2(25, 12));
        _enemies.Add(patrolEnemy2);
        patrolEnemy2.Initialize();
        _collisionSystem.Register(patrolEnemy2);

        // Chaser enemy
        var chaser = new ChaserEnemy("ChaserEnemy1", new Vector2(38, 14));
        _enemies.Add(chaser);
        chaser.Initialize();
        _collisionSystem.Register(chaser);
    }

    private void SpawnCollectibles()
    {
        // Spawn coins at various locations
        var coinPositions = new Vector2[] {
            new Vector2(5, 15),
            new Vector2(15, 12),
            new Vector2(28, 12),
            new Vector2(45, 15)
        };

        foreach (var pos in coinPositions)
        {
            var coin = new Collectible(pos);
            _collectibles.Add(coin);
        }
    }

    private void OnLevelComplete()
    {
        _levelComplete = true;
        _stateManager.SetState(GameStateType.LevelComplete);
        _uiSystem.RenderGameOver(true);
        Console.ReadKey();
        LoadLevel(_currentLevelIndex + 1);
        _stateManager.SetState(GameStateType.Playing);
    }

    private void OnPlayerDeath()
    {
        _stateManager.SetState(GameStateType.GameOver);
        _uiSystem.RenderGameOver(false);
        Console.ReadKey();
        LoadLevel(_currentLevelIndex); // Restart current level
        _stateManager.SetState(GameStateType.Playing);
    }

    public void Tick(float deltaTime)
    {
        if (!_stateManager.IsPlaying) return;

        // Handle input
        HandleInput();

        // Update player
        _player.Tick(deltaTime);

        // Update enemies
        foreach (var enemy in _enemies)
        {
            if (enemy.IsActive)
            {
                enemy.Tick(deltaTime);
                enemy.UpdateAI(_player);
            }
        }

        // Check collisions
        _collisionSystem.Tick(deltaTime);

        // Check collectibles
        _collectibles.CheckCollisions(_player.Bounds);

        // Render UI
        _uiSystem.Render(_player, _collectibles, _stateManager.CurrentState.ToString());
    }

    private void HandleInput()
    {
        var command = _inputSystem.PollInput();
        
        Vector2 moveInput = Vector2.Zero;
        bool jumpDown = false;
        bool jumpPressed = false;

        switch (command)
        {
            case InputCommand.MoveLeft:
                moveInput.X = -1;
                break;
            case InputCommand.MoveRight:
                moveInput.X = 1;
                break;
            case InputCommand.Jump:
                jumpDown = true;
                jumpPressed = true;
                break;
            case InputCommand.Pause:
                _stateManager.SetState(_stateManager.IsPaused ? GameStateType.Playing : GameStateType.Paused);
                break;
        }

        // This needs to be passed to player movement controller
        // For now, we'll set it directly
        _player.GetMovementController().SetInput(moveInput, jumpDown, jumpPressed);
    }

    public void Run()
    {
        Initialize();

        Stopwatch frameTimer = Stopwatch.StartNew();
        float targetFrameTime = 1f / 60f;

        while (true)
        {
            var deltaTime = (float)frameTimer.Elapsed.TotalSeconds;
            frameTimer.Restart();

            if (deltaTime > 0.1f) deltaTime = 0.1f;

            Tick(deltaTime);

            // Frame timing
            var sleepTime = (int)((targetFrameTime - deltaTime) * 1000);
            if (sleepTime > 0)
                System.Threading.Thread.Sleep(sleepTime);
        }
    }
}

// Helper extension to PlayerCharacter
public static class PlayerCharacterExtensions
{
    private static Dictionary<PlayerCharacter, AdvancedMovementController> _movementMap = 
        new Dictionary<PlayerCharacter, AdvancedMovementController>();

    public static void SetMovementController(this PlayerCharacter player, AdvancedMovementController controller)
    {
        _movementMap[player] = controller;
    }

    public static AdvancedMovementController GetMovementController(this PlayerCharacter player)
    {
        if (_movementMap.TryGetValue(player, out var controller))
            return controller;
        return null;
    }
}
