// REVIEW: necessary namespaces

public static class PlantSystem
{
    /// <summary>
    /// Creates a new plant
    /// </summary>
    /// <returns>initialized plant data</returns>
    public static PlantData InitializePlant( PlantType type )
    {
        PlantData retPlant = new PlantData();

        // initialize
        retPlant.plantName = type.ToString();
        retPlant.type = type;
        retPlant.rarity = PlantRarity.Common;
        retPlant.growthRate = 1f;
        retPlant.health = 1f;
        retPlant.springVitality = 1f;
        retPlant.summerVitality = 1f;
        retPlant.fallVitality = 1f;
        retPlant.winterVitality = 1f;
        retPlant.harvestAmount = 1;
        retPlant.seedPotential = 0.5f;
        retPlant.plantEffects = new PlantEffect[0];

        retPlant = ConfigurePlantByType(retPlant, type);

        return retPlant;
    }

    static PlantData ConfigurePlantByType( PlantData data, PlantType type )
    {
        PlantData retData = data;

        switch (type)
        {
            case PlantType.Default:
                // we should never be here
                break;
            // COMMON PLANTS
            case PlantType.Corn:
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Tomato:
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Carrot:
                break;
            case PlantType.Poppy:
                retData.growthRate = .5f;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Rose:
                retData.growthRate = .5f;
                retData.winterVitality = 0.75f;
                break;
            case PlantType.Sunflower:
                retData.growthRate = .25f;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.25f;
                break;
            case PlantType.Moonflower:
                retData.growthRate = .25f;
                retData.isDarkPlant = true;
                break;
            case PlantType.Apple:
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Orange:
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Lemon:
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            // UNCOMMON PLANTS
            // RARE PLANTS
            // SPECIAL PLANTS
            // UNIQUE PLANTS
            default:
                UnityEngine.Debug.LogWarning("--- PlantSystem [ConfigurePlantByType] : type "+type.ToString()+" not found. will ignore.");
                break;
        }

        return retData;
    }
}
