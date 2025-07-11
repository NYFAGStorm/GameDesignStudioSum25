using UnityEngine;

public class IslandManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles all floating islands and the structures on them

    public IslandData[] islands;

    private float propTimer;
    private Renderer[] propRenderers = new Renderer[0];

    const float PROPCHECKTIME = 5f;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            propTimer = PROPCHECKTIME;
        }
    }

    void Update()
    {
        // run prop check timer
        if (propTimer > 0f)
        {
            propTimer -= Time.deltaTime;
            if (propTimer < 0f)
            {
                propTimer = PROPCHECKTIME;
                CheckProps();
            }
        }
    }

    void CheckProps()
    {
        // use ambient intensity to adjust color of props
        float aIntensity = RenderSettings.ambientIntensity;
        Color c = Color.white;
        c *= Mathf.Clamp01(0.381f + (0.618f * aIntensity));
        c.a = 1f;
        for (int i = 0; i < propRenderers.Length; i++)
        {
            propRenderers[i].material.color = c;
        }
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
            if (!ConfigureTPortNodes(islands[i], islandObj) && islands[i].tports != null && islands[i].tports.Length > 0)
                Debug.LogWarning("--- IslandManager [ConfigureIslands] : failed to configure teleport nodes on island '" + islandObj.name + "'. will ignore.");
            // configure structures
            if (!ConfigureStructures(islands[i], islandObj) && islands[i].structures != null && islands[i].structures.Length > 0)
                Debug.LogWarning("--- IslandManager [ConfigureIslands] : failed to configure structures on island '" + islandObj.name + "'. will ignore.");
            // configure props
            if (!ConfigureProps(islands[i], islandObj) && islands[i].props != null && islands[i].props.Length > 0)
                Debug.LogWarning("--- IslandManager [ConfigureIslands] : failed to configure props on island '" + islandObj.name + "'. will ignore.");
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
                default:
                    break;
            }
            // invalid prefab type
            if (prefabName == "")
                return retBool;
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

    bool ConfigureProps(IslandData island, GameObject islandObj)
    {
        bool retBool = false;

        if (island.props == null || island.props.Length == 0)
            return retBool;

        for (int i = 0; i < island.props.Length; i++)
        {
            PropData pData = island.props[i];
            // prop type determines resources load name
            string prefabName = "";
            switch (pData.type)
            {
                case PropType.Default:
                    // we should never be here
                    break;
                case PropType.RockA:
                    prefabName = "Rock A";
                    break;
                case PropType.RockB:
                    prefabName = "Rock B";
                    break;
                case PropType.RockC:
                    prefabName = "Rock C";
                    break;
                case PropType.BushA:
                    prefabName = "Bush A";
                    break;
                case PropType.BushB:
                    prefabName = "Bush B";
                    break;
                case PropType.BushC:
                    prefabName = "Bush C";
                    break;
                case PropType.CompostBin:
                    prefabName = "Compost Bin";
                    break;
                case PropType.Mailbox:
                    prefabName = "Mail Box";
                    break;
                case PropType.LampPostA:
                    prefabName = "Lamp Post A";
                    break;
                case PropType.LampPostB:
                    prefabName = "Lamp Post B";
                    break;
                case PropType.BannerA:
                    prefabName = "Banner A";
                    break;
                case PropType.BannerB:
                    prefabName = "Banner B";
                    break;
                default:
                    break;
            }            
            // invalid prefab type
            if (prefabName == "")
                return retBool;
            // load prop prefab
            GameObject prop = GameObject.Instantiate((GameObject)Resources.Load(prefabName));
            prop.name = "Prop " + pData.name;
            // position prop
            Vector3 pos = Vector3.zero;
            pos.x = pData.location.x;
            pos.y = pData.location.y;
            pos.z = pData.location.z;
            pos += islandObj.transform.position;
            prop.transform.position = pos;
            // parent to island
            prop.transform.parent = islandObj.transform;
            // store prop reneders for color adjustment
            Renderer[] rends = prop.GetComponentsInChildren<Renderer>();
            if (rends != null  && rends.Length > 0)
            {
                Renderer[] tmp = new Renderer[propRenderers.Length + rends.Length];
                for (int n = 0; n < propRenderers.Length; n++)
                {
                    tmp[n] = propRenderers[n];
                }
                for (int n = 0; n < rends.Length; n++)
                {
                    tmp[propRenderers.Length + n] = rends[n];
                }
                propRenderers = tmp;
            }
        }
        retBool = true;

        return retBool;
    }
}
