/// <summary>
/// Level factory that creates predefined levels.
/// </summary>
public static class LevelFactory
{
    public static Level CreateLevel1()
    {
        var level = new Level(50, 20);

        // Create base platform
        for (int x = 0; x < 50; x++)
        {
            level.SetTile(x, 19, TileType.Solid);
        }

        // Stairs and platforms
        for (int x = 2; x < 8; x++)
        {
            level.SetTile(x, 17 - (x - 2), TileType.Solid);
        }

        // Gap
        for (int x = 12; x < 15; x++)
        {
            level.SetTile(x, 15, TileType.Solid);
        }

        // Platform series
        for (int x = 20; x < 25; x++)
        {
            level.SetTile(x, 14, TileType.Solid);
        }

        // Enemy area
        for (int x = 28; x < 32; x++)
        {
            level.SetTile(x, 16, TileType.Solid);
        }

        // Hazard spike
        level.SetTile(35, 18, TileType.Hazard);

        // Goal area
        for (int x = 40; x < 45; x++)
        {
            level.SetTile(x, 17, TileType.Solid);
        }
        level.SetTile(42, 16, TileType.Goal);

        // Spawn point
        level.SetTile(1, 17, TileType.SpawnPoint);

        return level;
    }

    public static Level CreateLevel2()
    {
        var level = new Level(60, 22);

        // Base
        for (int x = 0; x < 60; x++)
            level.SetTile(x, 21, TileType.Solid);

        // Platforms
        for (int x = 5; x < 10; x++)
            level.SetTile(x, 18, TileType.Solid);

        for (int x = 15; x < 20; x++)
            level.SetTile(x, 16, TileType.Solid);

        for (int x = 25; x < 30; x++)
            level.SetTile(x, 14, TileType.Solid);

        // Hazard area
        level.SetTile(32, 20, TileType.Hazard);
        level.SetTile(33, 20, TileType.Hazard);

        // Goal
        for (int x = 50; x < 56; x++)
            level.SetTile(x, 18, TileType.Solid);
        level.SetTile(53, 17, TileType.Goal);

        level.SetTile(1, 20, TileType.SpawnPoint);

        return level;
    }
}
