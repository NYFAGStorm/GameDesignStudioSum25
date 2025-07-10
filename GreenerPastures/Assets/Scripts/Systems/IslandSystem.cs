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
        retIsland.location = islandLocation; // w = island scale
        retIsland.tports = new TPortNodeConfig[0];
        retIsland.structures = new StructureData[0];
        retIsland.props = new PropData[0];
        retIsland.effects = new IslandEffect[0];

        return retIsland;
    }

    /// <summary>
    /// Creates new teleport node data based on given tag and position
    /// </summary>
    /// <param name="tPortTag">teleport node tag</param>
    /// <param name="tPortIndex">teleport node index (0 or 1 for paired nodes)</param>
    /// <param name="tPortLocation">teleport location</param>
    /// <returns>initialized teleport node data</returns>
    public static TPortNodeConfig InitializeTeleportNode( string tPortTag, int tPortIndex, PositionData tPortLocation )
    {
        TPortNodeConfig retTport = new TPortNodeConfig();

        retTport.tag = tPortTag;
        retTport.tPortIndex = tPortIndex;
        retTport.location = tPortLocation;
        retTport.cameraMode = CameraManager.CameraMode.Follow; // default config
        retTport.cameraPosition = new PositionData();

        return retTport;
    }

    /// <summary>
    /// Adds given teleport node data to given island, if it doesn't already exist
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="tportNode">teleport node data</param>
    /// <returns>island data with teleport node added, if it didn't already exist</returns>
    public static IslandData AddTeleportNodeToIsland( IslandData island, TPortNodeConfig tportNode )
    {
        IslandData retIsland = island;

        // validate does not exist (match tag and index)
        bool found = false;
        for (int i = 0; i < retIsland.tports.Length; i++)
        {
            if (retIsland.tports[i].tag == tportNode.tag &&
                retIsland.tports[i].tPortIndex == tportNode.tPortIndex)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retIsland;
        TPortNodeConfig[] tmp = new TPortNodeConfig[retIsland.tports.Length + 1];
        for (int i = 0; i < retIsland.tports.Length; i++)
        {
            tmp[i] = retIsland.tports[i];
        }
        tmp[retIsland.tports.Length] = tportNode;
        retIsland.tports = tmp;

        return retIsland;
    }

    /// <summary>
    /// Removes given teleport node data from given island data, if it exists
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="tportNode">teleport node data</param>
    /// <returns>island data with teleport node removed, if it existed</returns>
    public static IslandData RemoveTeleportNodeFromIsland( IslandData island, TPortNodeConfig tportNode )
    {
        IslandData retIsland = island;

        // validate does exist (match tag and index)
        bool found = false;
        for (int i = 0; i < retIsland.tports.Length; i++)
        {
            if (retIsland.tports[i].tag == tportNode.tag &&
                retIsland.tports[i].tPortIndex == tportNode.tPortIndex)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retIsland;
        TPortNodeConfig[] tmp = new TPortNodeConfig[retIsland.tports.Length - 1];
        int count = 0;
        for (int i = 0; i < retIsland.tports.Length; i++)
        {
            if (retIsland.tports[i].tag != tportNode.tag ||
                retIsland.tports[i].tPortIndex != tportNode.tPortIndex)
            {
                tmp[count] = retIsland.tports[i];
                count++;
            }
        }
        retIsland.tports = tmp;

        return retIsland;
    }

    /// <summary>
    /// Creates new island structure data with given name, type and location
    /// </summary>
    /// <param name="structureName">structure name</param>
    /// <param name="structureType">structure type</param>
    /// <param name="structureLocation">structure location (relative to island center)</param>
    /// <returns>initialized structure data</returns>
    public static StructureData InitializeStructure( string structureName, StructureType structureType, PositionData structureLocation )
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
    /// Creates new island prop data with given name, type and location relative to island center
    /// </summary>
    /// <param name="propName">prop name</param>
    /// <param name="propType">prop type</param>
    /// <param name="propLocation">prop location</param>
    /// <returns>initialized prop data</returns>
    public static PropData InitializeProp( string propName, PropType propType, PositionData propLocation )
    {
        PropData retProp = new PropData();

        retProp.name = propName;
        retProp.type = propType;
        retProp.location = propLocation;
        retProp.effects = new PropEffect[0];

        return retProp;
    }

    /// <summary>
    /// Adds a given prop to a given island
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="prop">prop data</param>
    /// <returns>island data with prop added</returns>
    public static IslandData AddPropToIsland( IslandData island, PropData prop )
    {
        IslandData retIsland = island;

        // add prop
        PropData[] tmp = new PropData[retIsland.props.Length + 1];
        for (int i = 0; i < retIsland.props.Length; i++)
        {
            tmp[i] = retIsland.props[i];
        }
        tmp[retIsland.props.Length] = prop;
        retIsland.props = tmp;

        return retIsland;
    }

    /// <summary>
    /// Removes a given prop from a given island
    /// </summary>
    /// <param name="island">island data</param>
    /// <param name="prop">prop data</param>
    /// <returns>island data with prop removed, if it existed</returns>
    public static IslandData RemovePropFromIsland( IslandData island, PropData prop )
    {
        IslandData retIsland = island;

        // validate prop exists
        bool found = false;
        for (int i = 0; i < retIsland.props.Length; i++)
        {
            if (retIsland.props[i] == prop)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retIsland;
        // remove prop
        PropData[] tmp = new PropData[retIsland.props.Length - 1];
        int count = 0;
        for (int i = 0; i < retIsland.props.Length; i++)
        {
            if (retIsland.props[i] != prop)
            {
                tmp[count] = retIsland.props[i];
                count++;
            }
        }
        retIsland.props = tmp;

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

    /// <summary>
    /// Returns true if given prop includes given prop effect type
    /// </summary>
    /// <param name="prop">prop data</param>
    /// <param name="effect">prop effect type</param>
    /// <returns>true if prop effect exists on this prop, false if not</returns>
    public static bool PropHasEffect( PropData prop, PropEffect effect )
    {
        bool retBool = false;

        for (int i = 0; i < prop.effects.Length; i++)
        {
            if (prop.effects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Adds a given prop effect type to a given prop, if it doesn't already exist
    /// </summary>
    /// <param name="prop">prop data</param>
    /// <param name="effect">prop effect type</param>
    /// <returns>prop data with effect added, if it didn't already exist</returns>
    public static PropData AddPropEffect( PropData prop, PropEffect effect )
    {
        PropData retProp = prop;

        // validate effect does not exist
        bool found = false;
        for (int i = 0; i < retProp.effects.Length; i++)
        {
            if (retProp.effects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retProp;
        // add effect
        PropEffect[] tmp = new PropEffect[retProp.effects.Length + 1];
        for (int i = 0; i < retProp.effects.Length; i++)
        {
            tmp[i] = retProp.effects[i];
        }
        tmp[retProp.effects.Length] = effect;
        retProp.effects = tmp;

        return retProp;
    }

    /// <summary>
    /// Removes a given prop effect type from a given prop, if it exists on it
    /// </summary>
    /// <param name="prop">prop data</param>
    /// <param name="effect">prop effect type</param>
    /// <returns>prop data with prop effect removed, if it existed</returns>
    public static PropData RemovePropEffect( PropData prop, PropEffect effect )
    {
        PropData retProp = prop;

        // validate effect exists
        bool found = false;
        for (int i = 0; i < retProp.effects.Length; i++)
        {
            if (retProp.effects[i] == effect)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retProp;
        // remove effect
        PropEffect[] tmp = new PropEffect[retProp.effects.Length + 1];
        int count = 0;
        for (int i = 0; i < retProp.effects.Length; i++)
        {
            if (retProp.effects[i] != effect)
            {
                tmp[count] = retProp.effects[i];
                count++;
            }
        }
        retProp.effects = tmp;

        return retProp;
    }
}
