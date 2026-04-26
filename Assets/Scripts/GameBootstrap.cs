using System;

/// <summary>
/// Entry point for the PixelForce platformer game.
/// </summary>
class GameBootstrap
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════╗");
        Console.WriteLine("║     PIXELFORCE - PLATFORMER    ║");
        Console.WriteLine("║   A Tile-Based Adventure Game  ║");
        Console.WriteLine("╚════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("Controls:");
        Console.WriteLine("  A/D   - Move left/right");
        Console.WriteLine("  W/Space - Jump");
        Console.WriteLine("  P     - Pause");
        Console.WriteLine();
        Console.WriteLine("Objective:");
        Console.WriteLine("  Reach the goal (⭐) in each level");
        Console.WriteLine("  Avoid hazards and enemies");
        Console.WriteLine("  Collect coins for points");
        Console.WriteLine();
        Console.WriteLine("Press any key to start...");
        Console.ReadKey();

        try
        {
            var gameManager = new PlatformerGameManager();
            gameManager.Run();
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine("Game Error:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            Console.ReadKey();
        }
    }
}
