// REVIEW: necessary namespaces

public static class PlantSystem
{
    /// <summary>
    /// Creates a new plant
    /// </summary>
    /// <returns>initialized plant data</returns>
    public static PlantData InitializePlant()
    {
        PlantData retPlant = new PlantData();

        // initialize
        retPlant.plantName = "No Plant";
        retPlant.plantEffects = new PlantEffect[0];

        return retPlant;
    }
}
