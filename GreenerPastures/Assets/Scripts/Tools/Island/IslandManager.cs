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
        ConfigureIslands();
    }

    void ConfigureIslands()
    {
        if (islands == null || islands.Length == 0)
            return;

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
            // parent under Environment/Structure folder object
            GameObject structureFolderObject = GameObject.Find("Structure");
            if (structureFolderObject != null)
                islandObj.transform.parent = structureFolderObject.transform;
            // configure teleport nodes
            ConfigureTPortNodes(islands[i], islandObj);
            // configure structures
            ConfigureStructures(islands[i], islandObj);
        }
    }

    void ConfigureTPortNodes(IslandData island, GameObject islandObj)
    {
        // validate same length of tportNodes and tportTags
        if (island.tportNodes.Length != island.tportTags.Length)
        {
            Debug.LogError("--- IslandManager [ConfigureTPortNodes] : mismatch number between node positions and tags. aborting.");
            return;
        }

        // configure teleport nodes
        for (int i = 0; i < island.tportNodes.Length; i++)
        {
            // create teleport node
            GameObject tportNode = GameObject.Instantiate((GameObject)Resources.Load("Teleport Node"));                //
            // name node & set tag
            tportNode.name = "Teleport Node " + island.tportTags[i];
            TeleportManager tm = tportNode.GetComponent<TeleportManager>();
            tm.teleporterTag = island.tportTags[i];
            // configure to parent island
            tm.islandObj = islandObj;
            tm.islandRadius = island.location.w * 7f;
            // TODO: configure associated camera trigger
            // REVIEW: refactor camera trigger mechanism?
            // position node
            Vector3 pos = Vector3.zero;
            pos.x = island.tportNodes[i].x;
            pos.y = island.tportNodes[i].y;
            pos.z = island.tportNodes[i].z;
            pos += islandObj.transform.position;
            tportNode.transform.position = pos;
            // parent node to island
            tportNode.transform.parent = islandObj.transform;
        }
    }

    void ConfigureStructures(IslandData island, GameObject islandObj)
    {
        // TODO: ...
        for (int i = 0; i < island.structures.Length; i++)
        {
            // structure type determines resources load name
            // load structure prefab
            //GameObject structure = GameObject.Instantiate((GameObject)Resources.Load());
        }
    }
}
