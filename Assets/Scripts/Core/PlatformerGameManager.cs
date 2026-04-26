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
        // Patrol slimes
        var slime1 = new PatrolEnemy("Slime-Patrol-1", new Vector2(15, 18));
        _enemies.Add(slime1);
        slime1.Initialize();
        _collisionSystem.Register(slime1);

        var slime2 = new PatrolEnemy("Slime-Patrol-2", new Vector2(35, 14));
        _enemies.Add(slime2);
        slime2.Initialize();
        _collisionSystem.Register(slime2);

        var slime3 = new PatrolEnemy("Slime-Patrol-3", new Vector2(48, 16));
        _enemies.Add(slime3);
        slime3.Initialize();
        _collisionSystem.Register(slime3);

        // Chaser slimes (more aggressive)
        var chaserSlime = new ChaserEnemy("Slime-Chaser-1", new Vector2(55, 20));
        _enemies.Add(chaserSlime);
        chaserSlime.Initialize();
        _collisionSystem.Register(chaserSlime);
    }

    private void SpawnCollectibles()
    {
        // Spawn coins along the platform path
        var coinPositions = new Vector2[] {
            new Vector2(14, 18),    // After first jump
            new Vector2(20, 16),    // Mid-jump challenge
            new Vector2(26, 14),    // Higher platform
            new Vector2(35, 14),    // Wide platform
            new Vector2(47, 15),    // Middle area
            new Vector2(65, 16),    // Before goal
            new Vector2(75, 13)     // Near goal
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
