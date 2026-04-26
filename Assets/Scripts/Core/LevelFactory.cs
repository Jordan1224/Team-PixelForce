/// <summary>
/// Level factory that creates predefined levels.
/// </summary>
public static class LevelFactory
{
    public static Level CreateLevel1()
    {
        var level = new Level(80, 24);

        // ===== BORDERS & WALLS =====
        // Left wall
        for (int y = 0; y < 24; y++)
            level.SetTile(0, y, TileType.Solid);

        // Right wall
        for (int y = 0; y < 24; y++)
            level.SetTile(79, y, TileType.Solid);

        // Bottom floor
        for (int x = 0; x < 80; x++)
            level.SetTile(x, 23, TileType.Solid);

        // ===== LEVEL DESIGN (Mario-style) =====
        
        // Starting platform
        for (int x = 2; x < 10; x++)
            level.SetTile(x, 21, TileType.Solid);

        // Ascending platforms
        for (int x = 12; x < 16; x++)
            level.SetTile(x, 19, TileType.Solid);

        for (int x = 18; x < 22; x++)
            level.SetTile(x, 17, TileType.Solid);

        for (int x = 24; x < 28; x++)
            level.SetTile(x, 15, TileType.Solid);

        // Mid-level platform
        for (int x = 30; x < 40; x++)
            level.SetTile(x, 16, TileType.Solid);

        // Descending platforms
        for (int x = 42; x < 46; x++)
            level.SetTile(x, 18, TileType.Solid);

        for (int x = 48; x < 52; x++)
            level.SetTile(x, 20, TileType.Solid);

        // Hazard spike section
        level.SetTile(54, 22, TileType.Hazard);
        level.SetTile(55, 22, TileType.Hazard);
        level.SetTile(56, 22, TileType.Hazard);

        // Final platform before goal
        for (int x = 60; x < 70; x++)
            level.SetTile(x, 19, TileType.Solid);

        // Goal area
        level.SetTile(65, 17, TileType.Goal);

        // Spawn point
        level.SetTile(4, 20, TileType.SpawnPoint);

        return level;
    }

    public static Level CreateLevel2()
    {
        var level = new Level(90, 26);

        // ===== BORDERS & WALLS =====
        for (int y = 0; y < 26; y++)
            level.SetTile(0, y, TileType.Solid);

        for (int y = 0; y < 26; y++)
            level.SetTile(89, y, TileType.Solid);

        for (int x = 0; x < 90; x++)
            level.SetTile(x, 25, TileType.Solid);

        // ===== LEVEL DESIGN =====
        
        // Start area
        for (int x = 2; x < 12; x++)
            level.SetTile(x, 23, TileType.Solid);

        // Jumping challenge - series of mid-air platforms
        for (int x = 15; x < 18; x++)
            level.SetTile(x, 20, TileType.Solid);

        for (int x = 21; x < 24; x++)
            level.SetTile(x, 18, TileType.Solid);

        for (int x = 27; x < 30; x++)
            level.SetTile(x, 16, TileType.Solid);

        for (int x = 33; x < 36; x++)
            level.SetTile(x, 14, TileType.Solid);

        // Wide platform with slime enemies
        for (int x = 40; x < 55; x++)
            level.SetTile(x, 17, TileType.Solid);

        // Hazard area
        for (int x = 58; x < 61; x++)
            level.SetTile(x, 24, TileType.Hazard);

        // One-way platform (can drop through)
        for (int x = 65; x < 72; x++)
            level.SetTile(x, 19, TileType.OneWayPlatform);

        // Final ascent
        for (int x = 75; x < 80; x++)
            level.SetTile(x, 15, TileType.Solid);

        // Goal
        level.SetTile(85, 13, TileType.Goal);

        // Spawn
        level.SetTile(4, 22, TileType.SpawnPoint);

        return level;
    }
}
