using System;
using UnityEngine;

/// <summary>
/// Game UI state display.
/// </summary>
public class UISystem
{
    private int _currentLevel = 1;
    private int _totalLevels = 3;
    private bool _showDebugInfo = true;

    public void SetLevel(int current, int total)
    {
        _currentLevel = current;
        _totalLevels = total;
    }

    public void Render(PlayerCharacter player, CollectibleSystem collectibles, string currentState)
    {
        if (_showDebugInfo)
        {
            var pos = player.transform.position;
            var vel = player.Velocity;
            Debug.Log($"=== PixelForce Level {_currentLevel}/{_totalLevels} ===");
            Debug.Log($"Health: {player.Health}/100  |  Coins: {collectibles.TotalCollected}  |  State: {currentState}");
            Debug.Log($"Pos: ({pos.x:F1}, {pos.y:F1})  |  Vel: ({vel.x:F1}, {vel.y:F1})");
        }
    }

    public void RenderGameOver(bool won)
    {
        if (won)
        {
            Debug.Log("╔════════════════════════╗");
            Debug.Log("║     LEVEL COMPLETE!    ║");
            Debug.Log("╚════════════════════════╝");
        }
        else
        {
            Debug.Log("╔════════════════════════╗");
            Debug.Log("║      GAME OVER!        ║");
            Debug.Log("╚════════════════════════╝");
        }
    }
}
