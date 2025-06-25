// REVIEW: necessary namespaces

public class IslandSystem
{
    /// <summary>
    /// Creates new island data with given name and location
    /// </summary>
    /// <param name="islandName">island name</param>
    /// <param name="islandLocation">position data in world space</param>
    /// <returns>initialized island data</returns>
    public static IslandData InitializeIsland( string islandName, PositionData islandLocation )
    {
        IslandData retIsland = new IslandData();

        retIsland.name = islandName;
        retIsland.location = islandLocation;
        retIsland.tportNodes = new PositionData[0];
        retIsland.tportTags = new string[0];
        retIsland.structures = new StructureData[0];
        retIsland.effects = new IslandEffect[0];

        return retIsland;
    }

    /// <summary>
    /// Creates new island structure data with given name, type and location
    /// </summary>
    /// <param name="structureName">structure name</param>
    /// <param name="structureType">structure type</param>
    /// <param name="structureLocation">structure location (relative to island center)</param>
    /// <returns>initialized structure data</returns>
    public static StructureData InitialzieStructure( string structureName, StructureType structureType,  PositionData structureLocation )
    {
        StructureData retStructure = new StructureData();

        retStructure.name = structureName;
        retStructure.type = structureType;
        retStructure.location = structureLocation;
        retStructure.effects = new StructureEffect[0];

        return retStructure;
    }

    /// <summary>
    /// Adds a given structure to a given island
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="structure">structure data</param>
    /// <returns>island data with structure added, allows multiples of structure type</returns>
    public static IslandData AddStructureToIsland( IslandData island, StructureData structure )
    {
        IslandData retIsland = island;

        // add structure
        StructureData[] tmp = new StructureData[retIsland.structures.Length+1];
        for (int i=0; i< retIsland.structures.Length; i++)
        {
            tmp[i] = retIsland.structures[i];
        }
        tmp[retIsland.structures.Length] = structure;
        retIsland.structures = tmp;

        return retIsland;
    }

    /// <summary>
    /// Removes a given structure from a given island
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="structure">structure data</param>
    /// <returns>island data with structure removed, if specific structure existed</returns>
    public static IslandData RemoveIslandStructure( IslandData island, StructureData structure )
    {
        IslandData retIsland = island;

        // validate structure exists
        bool found = false;
        for (int i=0; i < retIsland.structures.Length; i++)
        {
            if (retIsland.structures[i] == structure)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retIsland;
        // remove structure
        StructureData[] tmp = new StructureData[retIsland.structures.Length - 1];
        int count = 0;
        for (int i = 0; i < retIsland.structures.Length; i++)
        {
            if (retIsland.structures[i] != structure)
            {
                tmp[count] = retIsland.structures[i];
                count++;
            }
        }
        retIsland.structures = tmp;

        return retIsland;
    }

    /// <summary>
    /// Returns true if given island data includes given island effect type
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="effect">island effect type</param>
    /// <returns>true if given island data includes given island effect type, false if not</returns>
    public static bool IslandHasEffect( IslandData island, IslandEffect effect )
    {
        bool retBool = false;

        for (int i = 0; i < island.effects.Length; i++)
        {
            if (island.effects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Adds a given island effect type to the given island data
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="effect">island effect type</param>
    /// <returns>island data with effect added, if it didn't already exist</returns>
    public static IslandData AddIslandEffect( IslandData island, IslandEffect effect )
    {
        IslandData retIsland = island;

        // validate effect does not exist
        bool found = false;
        for (int i = 0; i < retIsland.effects.Length; i++)
        {
            if (retIsland.effects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retIsland;
        // add effect
        IslandEffect[] tmp = new IslandEffect[retIsland.effects.Length+1];
        for (int i = 0; i < retIsland.effects.Length; i++)
        {
            tmp[i] = retIsland.effects[i];
        }
        tmp[retIsland.effects.Length] = effect;
        retIsland.effects = tmp;

        return retIsland;
    }

    /// <summary>
    /// Removes an island effect from a given island, if it existed
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="effect">effect type</param>
    /// <returns>island data with effect removed, if it existed</returns>
    public static IslandData RemoveIslandEffect( IslandData island, IslandEffect effect )
    {
        IslandData retIsland = island;

        // validate effect exists
        bool found = false;
        for (int i = 0; i < retIsland.effects.Length; i++)
        {
            if (retIsland.effects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retIsland;
        // remove effect
        IslandEffect[] tmp = new IslandEffect[retIsland.effects.Length - 1];
        int count = 0;
        for (int i = 0; i < retIsland.effects.Length; i++)
        {
            if (retIsland.effects[i] != effect)
            {
                tmp[count] = retIsland.effects[i];
                count++;
            }
        }
        retIsland.effects = tmp;

        return retIsland;
    }

    /// <summary>
    /// Returns true if given structure data includes given structure effect type
    /// </summary>
    /// <param name="structure">structure data</param>
    /// <param name="effect">structure effect type</param>
    /// <returns>true if structure data includes given effect, false if not</returns>
    public static bool StructureHasEffect( StructureData structure, StructureEffect effect )
    {
        bool retBool = false;

        for (int i = 0; i < structure.effects.Length; i++)
        {
            if (structure.effects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Adds a given structure effect type to given structure data, if it doesn't exist
    /// </summary>
    /// <param name="structure">structure data</param>
    /// <param name="effect">structure effect type</param>
    /// <returns>structure data with effect added, if it didn't already exist</returns>
    public static StructureData AddStructureEffect( StructureData structure, StructureEffect effect )
    {
        StructureData retStructure = structure;

        // validate effect does not exist
        bool found = false;
        for (int i = 0; i < retStructure.effects.Length; i++)
        {
            if (retStructure.effects[i] != effect)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retStructure;
        // add effect
        StructureEffect[] tmp = new StructureEffect[retStructure.effects.Length+1];
        for (int i = 0; i < retStructure.effects.Length; i++)
        {
            tmp[i] = retStructure.effects[i];
        }
        tmp[retStructure.effects.Length] = effect;
        retStructure.effects = tmp;

        return retStructure;
    }

    /// <summary>
    /// Removes a given structure effect type from given structure data, if it exists
    /// </summary>
    /// <param name="structure">structure data</param>
    /// <param name="effect">strucutre effect type</param>
    /// <returns>structure data with effect removed, if it existed</returns>
    public static StructureData RemoveStructureEffect(StructureData structure, StructureEffect effect)
    {
        StructureData retStructure = structure;

        // validate effect exists
        bool found = false;
        for (int i = 0; i < retStructure.effects.Length; i++)
        {
            if (retStructure.effects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retStructure;
        // remove effect
        StructureEffect[] tmp = new StructureEffect[retStructure.effects.Length - 1];
        int count = 0;
        for (int i = 0; i < retStructure.effects.Length; i++)
        {
            if (retStructure.effects[i] != effect)
            {
                tmp[count] = retStructure.effects[i];
                count++;
            }
        }
        retStructure.effects = tmp;

        return retStructure;
    }
}
