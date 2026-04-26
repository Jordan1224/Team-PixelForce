using System;
using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// ASCII sprite definitions for console rendering.
/// </summary>
public static class AsciiSprites
{
    // Player sprites
    public static class Player
    {
        public const string Idle = "🟦";      // Standing
        public const string Run = "🟩";       // Moving
        public const string Jump = "▲";       // Jumping
        public const string Fall = "▼";       // Falling
        public const string Hit = "◆";        // Damage
        public const string Dead = "✕";       // Dead
    }

    // Enemy sprites
    public static class Slime
    {
        public const string Idle = "◉";
        public const string Patrol = "◎";
        public const string Chase = "●";
        public const string Attack = "◬";
        public const string Dead = "○";
    }

    // Environment
    public static class Environment
    {
        public const string Solid = "█";              // Wall/solid
        public const string Platform = "▬";          // Platform
        public const string OneWay = "╌";            // One-way platform
        public const string Hazard = "⚡";            // Spike
        public const string Goal = "⭐";              // Goal
        public const string Coin = "◆";              // Collectible
        public const string Empty = " ";             // Empty space
    }

    // Borders
    public static class Border
    {
        public const string Corner_TopLeft = "╔";
        public const string Corner_TopRight = "╗";
        public const string Corner_BottomLeft = "╚";
        public const string Corner_BottomRight = "╝";
        public const string Horizontal = "═";
        public const string Vertical = "║";
    }
}

/// <summary>
/// Advanced rendering system with ASCII sprites.
/// </summary>
public class AdvancedRenderingSystem : IUpdatable
{
    private Level _level;
    private PlayerCharacter _player;
    private List<EnemyBase> _enemies;
    private CollectibleSystem _collectibles;
    
    private int _screenWidth = 100;
    private int _screenHeight = 28;
    private char[,] _screenBuffer;
    private float _cameraX = 0f;

    public AdvancedRenderingSystem(Level level, PlayerCharacter player, List<EnemyBase> enemies, CollectibleSystem collectibles)
    {
        _level = level;
        _player = player;
        _enemies = enemies;
        _collectibles = collectibles;
        _screenBuffer = new char[_screenHeight, _screenWidth];
    }

    public void Tick(float deltaTime)
    {
        // Update camera to follow player
        UpdateCamera();

        // Render frame
        ClearBuffer();
        DrawLevel();
        DrawCollectibles();
        DrawEnemies();
        DrawPlayer();
        DrawBorder();
        DrawHUD();
        PresentBuffer();
    }

    private void UpdateCamera()
    {
        // Center camera on player
        _cameraX = _player.Transform.Position.X - (_screenWidth / 4);
        _cameraX = System.Math.Max(0, System.Math.Min(_cameraX, _level.Width - _screenWidth));
    }

    private void ClearBuffer()
    {
        for (int y = 0; y < _screenHeight; y++)
        {
            for (int x = 0; x < _screenWidth; x++)
            {
                _screenBuffer[y, x] = ' ';
            }
        }
    }

    private void DrawLevel()
    {
        int startX = (int)_cameraX;
        int endX = System.Math.Min(startX + _screenWidth, _level.Width);

        for (int x = startX; x < endX; x++)
        {
            for (int y = 0; y < _level.Height && y < _screenHeight - 4; y++)
            {
                var tile = _level.GetTile(x, y);
                int screenX = x - startX;
                int screenY = y + 1;

                if (screenX >= 0 && screenX < _screenWidth && screenY >= 0 && screenY < _screenHeight - 4)
                {
                    string sprite = tile switch
                    {
                        TileType.Solid => "█",
                        TileType.Platform => "▬",
                        TileType.OneWayPlatform => "╌",
                        TileType.Hazard => "⚡",
                        TileType.Goal => "⭐",
                        TileType.SpawnPoint => "S",
                        _ => " "
                    };

                    if (sprite != " ")
                        _screenBuffer[screenY, screenX] = sprite[0];
                }
            }
        }
    }

    private void DrawCollectibles()
    {
        int startX = (int)_cameraX;

        // Render active collectibles
        // (This would be better with a list of collectibles, but simplified here)
    }

    private void DrawEnemies()
    {
        int startX = (int)_cameraX;

        foreach (var enemy in _enemies)
        {
            if (!enemy.IsActive) continue;

            int screenX = (int)(enemy.Transform.Position.X - _cameraX);
            int screenY = (int)enemy.Transform.Position.Y + 1;

            if (screenX >= 0 && screenX < _screenWidth && screenY >= 0 && screenY < _screenHeight - 4)
            {
                string sprite = AsciiSprites.Slime.Chase;
                _screenBuffer[screenY, screenX] = sprite[0];
            }
        }
    }

    private void DrawPlayer()
    {
        int screenX = (int)(_player.Transform.Position.X - _cameraX);
        int screenY = (int)_player.Transform.Position.Y + 1;

        if (screenX >= 0 && screenX < _screenWidth && screenY >= 0 && screenY < _screenHeight - 4)
        {
            string sprite = AsciiSprites.Player.Idle;
            _screenBuffer[screenY, screenX] = sprite[0];
        }
    }

    private void DrawBorder()
    {
        // Top border
        for (int x = 0; x < _screenWidth; x++)
        {
            _screenBuffer[0, x] = '═';
        }

        // Bottom border (HUD area)
        for (int x = 0; x < _screenWidth; x++)
        {
            _screenBuffer[_screenHeight - 4, x] = '═';
        }
    }

    private void DrawHUD()
    {
        int hudY = _screenHeight - 3;

        string line1 = $"❤ Health: {_player.Health}/100  |  🪙 Coins: {_collectibles.TotalCollected}";
        string line2 = $"Pos: ({_player.Transform.Position.X:F1}, {_player.Transform.Position.Y:F1})  |  Vel: ({_player.Velocity.X:F1}, {_player.Velocity.Y:F1})";
        string line3 = "A/D: Move | W/SPACE: Jump | P: Pause | ESC: Quit";

        DrawString(line1, 1, hudY);
        DrawString(line2, 1, hudY + 1);
        DrawString(line3, 1, hudY + 2);
    }

    private void DrawString(string text, int x, int y)
    {
        for (int i = 0; i < text.Length && x + i < _screenWidth && y < _screenHeight; i++)
        {
            _screenBuffer[y, x + i] = text[i];
        }
    }

    private void PresentBuffer()
    {
        Console.Clear();

        for (int y = 0; y < _screenHeight; y++)
        {
            for (int x = 0; x < _screenWidth; x++)
            {
                Console.Write(_screenBuffer[y, x]);
            }
            Console.WriteLine();
        }
    }

    public void Clear()
    {
        _screenBuffer = null;
    }
}
