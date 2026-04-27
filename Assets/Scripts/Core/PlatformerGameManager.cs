using System;
using System.Collections.Generic;
using UnityEngine;
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
    private AdvancedRenderingSystem _renderingSystem;
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
        GameObject playerObj = new GameObject("Player");
        PlayerCharacter _player = playerObj.AddComponent<PlayerCharacter>();
        _player.Initialize("Hero");

        Vector2 spawn = _currentLevel.FindSpawnPoint();
        _player.transform.position = new Vector3(spawn.x, spawn.y, _player.transform.position.z);

        _player.OnGoalReached += OnLevelComplete;
        _player.OnDestroyed += OnPlayerDeath;

        // Create collision system for this level
        _collisionSystem = new CollisionSystem(_currentLevel);
        _collisionSystem.Register(_player);

        // Spawn enemies
        SpawnEnemies();

        // Spawn collectibles
        SpawnCollectibles();

        // Create rendering system for this level
        _renderingSystem = new AdvancedRenderingSystem(_currentLevel, _player, _enemies, _collectibles);
    }

    private void SpawnEnemies()
    {
        // Patrol slimes
        var slime1 = new GameObject("Slime-Patrol-1");
        slime1.transform.position = new Vector3(15, 18, 0);
        var enemy1 = slime1.AddComponent<Enemy>();
        _enemies.Add(enemy1);
        enemy1.Initialize();
        _collisionSystem.Register(enemy1);

        var slime2 = new GameObject("Slime-Patrol-2");
        slime2.transform.position = new Vector3(35, 14, 0);
        var enemy2 = slime2.AddComponent<Enemy>();
        _enemies.Add(enemy2);
        enemy2.Initialize();
        _collisionSystem.Register(enemy2);

        var slime3 = new GameObject("Slime-Patrol-3");
        slime3.transform.position = new Vector3(48, 16, 0);
        var enemy3 = slime3.AddComponent<Enemy>();
        _enemies.Add(enemy3);
        enemy3.Initialize();
        _collisionSystem.Register(enemy3);

        // Chaser slimes (more aggressive)
        var chaserSlime = new GameObject("Slime-Chaser-1");
        chaserSlime.transform.position = new Vector3(55, 20, 0);
        var chaserEnemy = chaserSlime.AddComponent<Enemy>();
        _enemies.Add(chaserEnemy);
        chaserEnemy.Initialize();
        _collisionSystem.Register(chaserEnemy);
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
        Console.WriteLine("\n=== LEVEL COMPLETE ===");
        Console.ReadKey();
        LoadLevel(_currentLevelIndex + 1);
        _stateManager.SetState(GameStateType.Playing);
    }

    private void OnPlayerDeath()
    {
        _stateManager.SetState(GameStateType.GameOver);
        Console.WriteLine("\n=== GAME OVER ===");
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
                if (enemy is Enemy e)
                    e.UpdateAI(_player);
            }
        }

        // Check collisions
        _collisionSystem.Tick(deltaTime);

        // Check collectibles
        _collectibles.CheckCollisions(_player.GetPosition());

        // Render frame
        _renderingSystem.Tick(deltaTime);
    }

    private void HandleInput()
    {
        var command = _inputSystem.PollInput();
        
        Vector2 moveInput = Vector2.zero;
        bool jumpDown = false;
        bool jumpPressed = false;

        switch (command)
        {
            case InputCommand.MoveLeft:
                moveInput = new Vector2(-1, moveInput.y);
                break;
            case InputCommand.MoveRight:
                moveInput = new Vector2(1, moveInput.y);
                break;
            case InputCommand.Jump:
                jumpDown = true;
                jumpPressed = true;
                break;
            case InputCommand.Pause:
                _stateManager.SetState(_stateManager.IsPaused ? GameStateType.Playing : GameStateType.Paused);
                break;
        }

        // Pass input to player's movement controller
        if (_player.GetMovementController() != null)
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


