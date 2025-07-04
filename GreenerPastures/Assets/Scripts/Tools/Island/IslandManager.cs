using UnityEngine;

public class IslandManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles all floating islands and the structures on them

    public IslandData[] islands;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {

    }

    /// <summary>
    /// Returns the island data array
    /// </summary>
    /// <returns>island data array</returns>
    public IslandData[] GetIslandData()
    {
        return islands;
    }

    /// <summary>
    /// Sets the island data array
    /// </summary>
    /// <param name="islandData">island data array</param>
    public void SetIslandData( IslandData[] islandData )
    {
        islands = islandData;
        if (!ConfigureIslands())
            Debug.LogWarning("--- IslandManager [SetIslandData] : unable to configure islands. will ignore.");
    }

    bool ConfigureIslands()
    {
        bool retBool = false;

        if (islands == null || islands.Length == 0)
            return false;

        // configure islands
        for (int i = 0; i < islands.Length; i++)
        {
            // spawn island
            GameObject islandObj = GameObject.Instantiate((GameObject)Resources.Load("Test Island"));
            // name island
            islandObj.name = "Island " + islands[i].name;
            // position island
            Vector3 pos = Vector3.zero;
            pos.x = islands[i].location.x;
            pos.y = islands[i].location.y;
            pos.z = islands[i].location.z;
            islandObj.transform.position = pos;
            // scale island
            Vector3 lScale = Vector3.one;
            if (islands[i].location.w == 0f)
                islands[i].location.w = 1f; // auto-fix zero scale
            lScale *= islands[i].location.w;
            islandObj.transform.localScale = lScale;
            // parent under Environment/Islands folder object
            GameObject structureFolderObject = GameObject.Find("Islands");
            if (structureFolderObject != null)
                islandObj.transform.parent = structureFolderObject.transform;
            // configure teleport nodes
            if (!ConfigureTPortNodes(islands[i], islandObj))
                Debug.LogWarning("--- IslandManager [ConfigureIslands] : failed to configure teleport nodes on island '" + islandObj.name + "'. will ignore.");
            // configure structures
            if (!ConfigureStructures(islands[i], islandObj))
                Debug.LogWarning("--- IslandManager [ConfigureIslands] : failed to configure structures on island '" + islandObj.name + "'. will ignore.");
        }
        retBool = true; // REVIEW: should acquire failed state of configuration rountines?

        return retBool;
    }

    bool ConfigureTPortNodes(IslandData island, GameObject islandObj)
    {
        bool retBool = false;

        if (island.tports == null || island.tports.Length == 0)
            return retBool;

        // configure teleport nodes
        for (int i = 0; i < island.tports.Length; i++)
        {
            // create teleport node
            GameObject tportNode = GameObject.Instantiate((GameObject)Resources.Load("Teleport Node"));
            // name node & set tag
            tportNode.name = "Teleport Node " + island.tports[i].tag + "[" + island.tports[i].tPortIndex + "]";
            TeleportManager tm = tportNode.GetComponent<TeleportManager>();
            tm.teleporterTag = island.tports[i].tag;
            // REVIEW: need to hold index data in teleport manager?
            // configure to parent island
            tm.islandObj = islandObj;
            tm.islandRadius = island.location.w * 7f;
            // position node
            Vector3 pos = Vector3.zero;
            pos.x = island.tports[i].location.x;
            pos.y = island.tports[i].location.y;
            pos.z = island.tports[i].location.z;
            pos += islandObj.transform.position;
            tportNode.transform.position = pos;
            // parent node to island
            tportNode.transform.parent = islandObj.transform;
            // NOTE: camera manager trigger mechanics handled by teleport manager
            tm.cameraMode = island.tports[i].cameraMode;
            pos = Vector3.zero;
            pos.x = island.tports[i].cameraPosition.x;
            pos.y = island.tports[i].cameraPosition.y;
            pos.z = island.tports[i].cameraPosition.z;
            tm.cameraPanModePosition = pos;
        }
        retBool = true;

        return retBool;
    }

    bool ConfigureStructures(IslandData island, GameObject islandObj)
    {
        bool retBool = false;

        if (island.structures == null || island.structures.Length == 0)
            return retBool;

        for (int i = 0; i < island.structures.Length; i++)
        {
            StructureData sData = island.structures[i];
            // structure type determines resources load name
            string prefabName = "";
            switch (sData.type)
            {
                // TEMP prefab names to test
                case StructureType.Default:
                    // we should never be here
                    break;
                case StructureType.WizardTower:
                    prefabName = "Tower Stand-In";
                    break;
                case StructureType.WizardInterior:
                    prefabName = "Test Tower Interior";
                    break;
                case StructureType.MarketShop:
                    prefabName = "Market Shop";
                    break;
                case StructureType.CompostBin:
                    prefabName = "Compost Bin";
                    break;
                default:
                    break;
            }
            // load structure prefab
            GameObject structure = GameObject.Instantiate((GameObject)Resources.Load(prefabName));
            structure.name = "Structure " + sData.name;
            // position structure
            Vector3 pos = Vector3.zero;
            pos.x = sData.location.x;
            pos.y = sData.location.y;
            pos.z = sData.location.z;
            pos += islandObj.transform.position;
            structure.transform.position = pos;
            // parent to island
            structure.transform.parent = islandObj.transform;
        }
        retBool = true;

        return retBool;
    }
}
