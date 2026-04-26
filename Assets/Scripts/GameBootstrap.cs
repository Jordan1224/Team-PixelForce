using System;

/// <summary>
/// Entry point for the PixelForce platformer game.
/// </summary>
class GameBootstrap
{
    static void Main(string[] args)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║      PIXELFORCE - PLATFORMER GAME      ║");
        Console.WriteLine("║   Inspired by Classic Mario Platformers║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("                 CONTROLS");
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  A / D         - Move left / right");
        Console.WriteLine("  W / SPACE     - Jump (hold for higher jump)");
        Console.WriteLine("  P             - Pause");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("                 OBJECTIVE");
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  🎯 Reach the GOAL (⭐) in each level");
        Console.WriteLine("  🪙 Collect COINS for points");
        Console.WriteLine("  👾 Avoid SLIMES (patrol & chase enemies)");
        Console.WriteLine("  ⚡ Avoid HAZARDS (spikes = instant death)");
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("              GAME FEATURES");
        Console.WriteLine("═══════════════════════════════════════════");
        Console.WriteLine("  ✓ Mario-style platformer levels");
        Console.WriteLine("  ✓ Bordered worlds with walls & flooring");
        Console.WriteLine("  ✓ Multiple mid-air platforms");
        Console.WriteLine("  ✓ Intelligent slime enemies");
        Console.WriteLine("  ✓ Variable jump height (coyote time)");
        Console.WriteLine("  ✓ Progressive level difficulty");
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
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║           GAME ERROR                   ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();
            Console.WriteLine("Message: " + ex.Message);
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
