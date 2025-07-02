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
        retPlant.adjustedGrowthRate = 1f;
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

    /// <summary>
    /// Returns true if given plant data includes given effect
    /// </summary>
    /// <param name="plant">plant data</param>
    /// <param name="effect">plant effect</param>
    /// <returns>true if plant effect exists, false if not</returns>
    public static bool PlantHasEffect( PlantData plant, PlantEffect effect )
    {
        bool retBool = false;

        for (int i = 0; i < plant.plantEffects.Length; i++)
        {
            if (plant.plantEffects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Configures plant data given initialized plant data and a plant type
    /// </summary>
    /// <param name="data">plant data</param>
    /// <param name="type">plant type</param>
    /// <returns>configured plant data</returns>
    static PlantData ConfigurePlantByType( PlantData data, PlantType type )
    {
        PlantData retData = data;

        switch (type)
        {
            case PlantType.Default:
                // we should never be here
                /*
                retData.growthRate = 1f;
                retData.isDarkPlant = false;
                retData.canReFruit = false;
                retData.springVitality = 1f;
                retData.summerVitality = 1f;
                retData.fallVitality = 1f;
                retData.winterVitality = 1f;
                retData.harvestAmount = 1;
                retData.seedPotential = 0.5f;
                */
                break;
            // COMMON PLANTS
            case PlantType.Corn:
                retData.growthRate = 1.25f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Tomato:
                retData.growthRate = 1.25f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Carrot:
                retData.growthRate = 1.25f;
                break;
            case PlantType.Poppy:
                retData.growthRate = .5f;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                break;
            case PlantType.Rose:
                retData.growthRate = .5f;
                retData.canReFruit = true;
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
                retData.harvestAmount = 3;
                break;
            case PlantType.Orange:
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                retData.harvestAmount = 3;
                break;
            case PlantType.Lemon:
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.springVitality = 0.75f;
                retData.fallVitality = 0.75f;
                retData.winterVitality = 0.5f;
                retData.harvestAmount = 3;
                break;
            // UNCOMMON PLANTS
            case PlantType.Lotus:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .75f;
                retData.canReFruit = true;
                retData.fallVitality = .5f;
                retData.winterVitality = .5f;
                break;
            case PlantType.Marigold:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .75f;
                retData.canReFruit = true;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                break;
            case PlantType.Magnolia:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .5f;
                retData.isDarkPlant = true;
                retData.summerVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.6f;
                break;
            case PlantType.Myosotis:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .2f;
                retData.isDarkPlant = true;
                retData.harvestAmount = 2;
                retData.seedPotential = 0.8f;
                break;
            case PlantType.Chrystalia:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .8f;
                retData.canReFruit = true;
                retData.springVitality = .8f;
                retData.summerVitality = .5f;
                retData.winterVitality = .8f;
                retData.seedPotential = 0.7f;
                break;
            case PlantType.Pumpkin:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .75f;
                retData.isDarkPlant = true;
                retData.canReFruit = true;
                retData.springVitality = .75f;
                retData.summerVitality = .75f;
                retData.fallVitality = 1f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.7f;
                break;
            case PlantType.Underbloom:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = 1f;
                retData.isDarkPlant = true;
                retData.canReFruit = true;
                retData.springVitality = .5f;
                retData.summerVitality = .5f;
                retData.fallVitality = .75f;
                break;
            case PlantType.WaterLily:
                retData.plantName = "Water Lily";
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .5f;
                retData.canReFruit = true;
                retData.summerVitality = .75f;
                retData.fallVitality = .5f;
                retData.winterVitality = .75f;
                break;
            case PlantType.Snowgrace:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = 1f;
                retData.isDarkPlant = true;
                retData.springVitality = .5f;
                retData.summerVitality = .25f;
                retData.fallVitality = .5f;
                retData.harvestAmount = 3;
                break;
            case PlantType.Popcorn:
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .75f;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                retData.harvestAmount = 3;
                retData.seedPotential = 0.7f;
                break;
            case PlantType.EclipseFlower:
                retData.plantName = "Eclipse Flower";
                retData.rarity = PlantRarity.Uncommon;
                retData.growthRate = .75f;
                retData.isDarkPlant = false;
                retData.canReFruit = true;
                retData.springVitality = .5f;
                retData.fallVitality = .5f;
                retData.plantEffects = new PlantEffect[1];
                retData.plantEffects[0] = PlantEffect.DayNightPlant;
                break;
            // RARE PLANTS
            case PlantType.GoldenApple:
                retData.plantName = "Golden Apple";
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .75f;
                retData.canReFruit = true;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                break;
            case PlantType.Hollowbloom:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .8f;
                retData.isDarkPlant = true;
                retData.canReFruit = true;
                retData.springVitality = .75f;
                retData.winterVitality = .75f;
                break;
            case PlantType.Mandrake:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .25f;
                retData.summerVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.8f;
                break;
            case PlantType.FrostLily:
                retData.plantName = "Frost Lily";
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .72f;
                retData.isDarkPlant = true;
                retData.canReFruit = true;
                retData.springVitality = .5f;
                retData.summerVitality = .25f;
                retData.fallVitality = .75f;
                break;
            case PlantType.Banana:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .5f;
                retData.canReFruit = true;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                retData.harvestAmount = 3;
                break;
            case PlantType.Coconut:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .75f;
                retData.canReFruit = true;
                retData.fallVitality = .5f;
                retData.winterVitality = .5f;
                retData.harvestAmount = 2;
                break;
            case PlantType.Mysteria:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .75f;
                retData.isDarkPlant = false;
                retData.canReFruit = true;
                retData.springVitality = .75f;
                retData.summerVitality = .75f;
                retData.fallVitality = .75f;
                retData.winterVitality = .75f;
                retData.plantEffects = new PlantEffect[1];
                retData.plantEffects[0] = PlantEffect.DayNightPlant;
                break;
            case PlantType.Nightshade:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .4f;
                retData.isDarkPlant = true;
                retData.springVitality = .75f;
                retData.summerVitality = 1f;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.65f;
                break;
            case PlantType.CrystalRose:
                retData.plantName = "Crystal Rose";
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .65f;
                retData.springVitality = .8f;
                retData.summerVitality = .7f;
                retData.fallVitality = .6f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.7f;
                break;
            case PlantType.Yarrow:
                retData.rarity = PlantRarity.Rare;
                retData.growthRate = .8f;
                retData.springVitality = 1f;
                retData.summerVitality = .75f;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.65f;
                break;
            // SPECIAL PLANTS
            case PlantType.Dragonroot:
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .35f;
                retData.springVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.6f;
                break;
            case PlantType.WinterRose:
                retData.plantName = "Winter Rose";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .35f;
                retData.isDarkPlant = true;
                retData.canReFruit = true;
                retData.springVitality = .75f;
                retData.summerVitality = .25f;
                retData.fallVitality = .5f;
                break;
            case PlantType.FleurDeLis:
                retData.plantName = "Fleur-De-Lis";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .3f;
                retData.springVitality = .75f;
                retData.fallVitality = .75f;
                retData.winterVitality = .5f;
                retData.harvestAmount = 2;
                retData.seedPotential = 0.8f;
                break;
            case PlantType.Tropicus:
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .25f;
                retData.canReFruit = true;
                retData.springVitality = .9f;
                retData.summerVitality = 1f;
                retData.fallVitality = .7f;
                retData.winterVitality = .5f;
                retData.harvestAmount = 3;
                break;
            case PlantType.MourningNyx:
                retData.plantName = "Mourning Nyx";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .2f;
                retData.isDarkPlant = true;
                retData.springVitality = .25f;
                retData.summerVitality = .25f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.6f;
                break;
            case PlantType.BlastApple:
                retData.plantName = "Blast Apple";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .2f;
                retData.canReFruit = true;
                retData.summerVitality = .75f;
                retData.fallVitality = .5f;
                retData.winterVitality = .25f;
                retData.seedPotential = 0.6f;
                break;
            case PlantType.PixiePlumeria:
                retData.plantName = "Pixie Plumeria";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .2f;
                retData.summerVitality = .75f;
                retData.winterVitality = .5f;
                retData.seedPotential = 0.65f;
                break;
            case PlantType.FaeFoxglove:
                retData.plantName = "Fae Foxglove";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .2f;
                retData.isDarkPlant = true;
                retData.summerVitality = .9f;
                retData.winterVitality = .3f;
                retData.seedPotential = 0.65f;
                break;
            case PlantType.DruidsLotus:
                retData.plantName = "Druid's Lotus";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .3f;
                retData.canReFruit = true;
                retData.springVitality = .75f;
                retData.summerVitality = .75f;
                retData.fallVitality = .75f;
                retData.winterVitality = .75f;
                retData.seedPotential = 0.8f;
                break;
            case PlantType.SplatBerry:
                retData.plantName = "Splat Berry";
                retData.rarity = PlantRarity.Special;
                retData.growthRate = .35f;
                retData.canReFruit = true;
                retData.springVitality = .8f;
                retData.summerVitality = .65f;
                retData.fallVitality = .8f;
                retData.winterVitality = .65f;
                break;
            // UNIQUE PLANTS
            default:
                UnityEngine.Debug.LogWarning("--- PlantSystem [ConfigurePlantByType] : type "+type.ToString()+" not found. will ignore.");
                break;
        }

        return retData;
    }

    public static PlantData GetGraftResult( PlantData stalk, PlantData fruit )
    {
        PlantData retPlant = stalk;

        switch (stalk.type)
        {
            case PlantType.Corn:
                // poppy fruit = popcorn type
                if (fruit.type == PlantType.Poppy)
                    retPlant = InitializePlant(PlantType.Popcorn);
                break;
            case PlantType.Sunflower:
                // moonflower fruit = eclipse flower type
                if (fruit.type == PlantType.Moonflower)
                    retPlant = InitializePlant(PlantType.EclipseFlower);
                break;
            case PlantType.Apple:
                // marigold fruit = golden apple type
                if (fruit.type == PlantType.Marigold)
                    retPlant = InitializePlant(PlantType.GoldenApple);
                break;
            case PlantType.Underbloom:
                // pumpkin fruit = hollowbloom type
                if (fruit.type == PlantType.Pumpkin)
                    retPlant = InitializePlant(PlantType.Hollowbloom);
                break;
            case PlantType.WaterLily:
                // snowgrace fruit = frost lily
                if (fruit.type == PlantType.Snowgrace)
                    retPlant = InitializePlant(PlantType.FrostLily);
                break;
            case PlantType.Marigold:
                // magnolia fruit or myosotis fruit = mysteria type
                if (fruit.type == PlantType.Magnolia || fruit.type == PlantType.Myosotis)
                    retPlant = InitializePlant(PlantType.Mysteria);
                break;
            case PlantType.Rose:
                // chrystallia fruit = crystal rose
                if (fruit.type == PlantType.Chrystalia)
                    retPlant = InitializePlant(PlantType.CrystalRose);
                break;
            case PlantType.Mandrake:
                // any 'rare' rarity fruit = dragonroot type
                if (fruit.rarity == PlantRarity.Rare)
                    retPlant = InitializePlant(PlantType.Dragonroot);
                break;
            case PlantType.FrostLily:
                // crystal rose fruit = winter rose type
                if (fruit.type == PlantType.CrystalRose)
                    retPlant = InitializePlant(PlantType.WinterRose);
                break;
            case PlantType.Orange:
                // banana fruit = tropicus type
                if (fruit.type == PlantType.Banana)
                    retPlant = InitializePlant(PlantType.Tropicus);
                break;
            case PlantType.Coconut:
                // lemon fruit = tropicus type
                if (fruit.type == PlantType.Lemon)
                    retPlant = InitializePlant(PlantType.Tropicus);
                break;
            case PlantType.EclipseFlower:
                // hollowbloom fruit = mourning nyx type
                if (fruit.type == PlantType.Hollowbloom)
                    retPlant = InitializePlant(PlantType.MourningNyx);
                break;
            case PlantType.Popcorn:
                // golden apple fruit = blast apple type
                if (fruit.type == PlantType.GoldenApple)
                    retPlant = InitializePlant(PlantType.BlastApple);
                break;
            case PlantType.Mysteria:
                // lotus fruit = druid's lotus type
                if (fruit.type == PlantType.Lotus)
                    retPlant = InitializePlant(PlantType.DruidsLotus);
                break;
            case PlantType.Tomato:
                // pumpkin fruit = splat berry type
                if (fruit.type == PlantType.Pumpkin)
                    retPlant = InitializePlant(PlantType.SplatBerry);
                break;
        }

        retPlant.growth = fruit.growth;
        retPlant.vitality = fruit.vitality;
        retPlant.health = fruit.health;
        retPlant.quality = fruit.quality;
        retPlant.isHarvested = false;

        return retPlant;
    }
}
