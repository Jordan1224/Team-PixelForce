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
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"=== PixelForce Level {_currentLevel}/{_totalLevels} ===".PadRight(Console.WindowWidth));
            Console.WriteLine($"Health: {player.Health}/100  |  Coins: {collectibles.TotalCollected}  |  State: {currentState}".PadRight(Console.WindowWidth));
            Console.WriteLine($"Pos: ({player.Transform.Position.X:F1}, {player.Transform.Position.Y:F1})  |  Vel: ({player.Velocity.X:F1}, {player.Velocity.Y:F1})".PadRight(Console.WindowWidth));
            Console.WriteLine();
        }
    }

    public void RenderGameOver(bool won)
    {
        Console.Clear();
        if (won)
        {
            Console.WriteLine("╔════════════════════════╗");
            Console.WriteLine("║     LEVEL COMPLETE!    ║");
            Console.WriteLine("╚════════════════════════╝");
        }
        else
        {
            Console.WriteLine("╔════════════════════════╗");
            Console.WriteLine("║      GAME OVER!        ║");
            Console.WriteLine("╚════════════════════════╝");
        }
        Console.WriteLine("\nPress any key to continue...");
    }
}
