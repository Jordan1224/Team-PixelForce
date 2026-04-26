using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var gameManager = new PlatformerGameManager();
            gameManager.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}
