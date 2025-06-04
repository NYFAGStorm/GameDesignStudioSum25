// REVIEW: necessary namespaces

public static class WorldSystem
{
    /// <summary>
    /// Creates new world data
    /// </summary>
    /// <returns>intialized world data</returns>
    public static WorldData InitializeWorld()
    {
        WorldData retWorld = new WorldData();

        // initialize
        retWorld.worldTimeOfDay = 9f; // 9am
        retWorld.worldDayOfMonth = 1;
        retWorld.worldMonth = WorldMonth.Mar;
        retWorld.worldSeason = WorldSeason.Spring;
        // REVIEW: leave for functions to set
        retWorld.annualProgress = ((9f/24f)+60f)/360f; // Mar.1 @ 9am
        retWorld.baseTemperature = 68f; // F or C?
        retWorld.dawnTime = 5f; // 5am
        retWorld.duskTime = 17f; // 5pm

        return retWorld;
    }

    /// <summary>
    /// Calculates annual progress, temperature and dawn/dusk times based on world date and time
    /// </summary>
    /// <param name="world">world data</param>
    /// <returns>world data with revised progress, temperature, dawn and dusk times</returns>
    public static WorldData CalculateAnnualProgress( WorldData world )
    {
        WorldData retWorld = world;

        retWorld.annualProgress = ((world.worldTimeOfDay / 24f) + ((int)world.worldMonth + 1) * 30f) / 360f;
        // set base temperature
        retWorld.baseTemperature = 68f; // F or C?
        // set dawn and dusk times
        retWorld.dawnTime = 5f; // 5am
        retWorld.duskTime = 17f; // 5pm

        return retWorld;
    }
}
