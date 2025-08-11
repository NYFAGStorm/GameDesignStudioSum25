using UnityEngine;

public class IslandManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles all floating islands and the structures on them

    public IslandData[] islands;

    private float propTimer;
    private Renderer[] propRenderers = new Renderer[0];

    private bool suspendCheckProps; // during greener pastures routine

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

    public void SetCheckProps( bool check )
    {
        suspendCheckProps = !check;
    }

    void CheckProps()
    {
        if (suspendCheckProps)
            return;

        // use ambient intensity to adjust color of props
        float aIntensity = RenderSettings.ambientIntensity;
        Color c = Color.white;
        c *= Mathf.Clamp01(0.381f + (0.618f * aIntensity));
        c.a = 1f;
        for (int i = 0; i < propRenderers.Length; i++)
        {
            // only change color on props without light
            if (propRenderers[i].gameObject.transform.parent.GetComponentInChildren<Light>() == null)
                propRenderers[i].material.color = c;
            // if this prop has a 3-color layered shader, change colors
            // ...find player who owns the island, see their options, get colors
            if (propRenderers[i].material.shader.name == "Unlit/Three Layer Composite")
            {
                GameObject islandObj = propRenderers[i].gameObject.transform.parent.parent.gameObject;
                if (islandObj.name == "Deco")
                {
                    // this is an interior prop, go up two more parents
                    islandObj = islandObj.transform.parent.parent.gameObject;
                }
                Vector3 pos = islandObj.transform.position;
                GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                if (ggm != null)
                {
                    PlayerData[] playerData = ggm.game.players;
                    for (int n = 0; n < playerData.Length; n++)
                    {
                        // playerData[n].playerIsland

                        if (GameSystem.GetVector(ggm.game.islands[playerData[n].playerIsland].location) == pos)
                        {
                            Color m = PlayerSystem.GetPlayerColor(playerData[n].options.mainColor);
                            m *= Mathf.Clamp01(0.381f + (0.618f * aIntensity));
                            m.a = 1f;
                            Color s = PlayerSystem.GetPlayerColor(playerData[n].options.secondaryColor);
                            s *= Mathf.Clamp01(0.381f + (0.618f * aIntensity));
                            s.a = 1f;
                            Color a = PlayerSystem.GetPlayerColor(playerData[n].options.accentColor);
                            a *= Mathf.Clamp01(0.381f + (0.618f * aIntensity));
                            a.a = 1f;
                            propRenderers[i].material.SetColor("_Color", m);
                            propRenderers[i].material.SetColor("_AltCol", s);
                            propRenderers[i].material.SetColor("_AccentCol", a);
                            break;
                        }
                    }
                }
            }
        }

        for (int i = 0; i < islands.Length; i++)
        {
            if (islands[i].effects.Length > 0)
            {
                // check for island effect for magic vfx spells
                for (int n = 0; n < islands[i].effects.Length; n++)
                {
                    if (islands[i].effects[n] == IslandEffect.SpellStarbloomBurst ||
                        islands[i].effects[n] == IslandEffect.SpellFogOfWar)
                    {
                        // REVIEW: confirm particular island position?
                        // check casts for matching spell and get position
                        CastManager cm = GameObject.FindFirstObjectByType<CastManager>();
                        if (cm != null)
                        {
                            GameObject lightingObject = GameObject.Find("Lighting");
                            for (int t = 0; t < cm.casts.Length; t++)
                            {
                                if (cm.casts[t].type == SpellType.StarbloomBurst)
                                {
                                    GameObject burstVFX = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Starbloom Burst"));
                                    burstVFX.name = "VFX Spell Starbloom Burst";
                                    Vector3 pos = Vector3.zero;
                                    pos.x = cm.casts[t].posX;
                                    pos.y = cm.casts[t].posY;
                                    pos.z = cm.casts[t].posZ;
                                    burstVFX.transform.position = pos;
                                    burstVFX.transform.parent = lightingObject.transform;
                                    Destroy(burstVFX, 8f);
                                }
                                if (cm.casts[t].type == SpellType.FogOfWar)
                                {
                                    GameObject burstVFX = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Fog Of War"));
                                    burstVFX.name = "VFX Spell Fog Of War";
                                    Vector3 pos = Vector3.zero;
                                    pos.x = cm.casts[t].posX;
                                    pos.y = cm.casts[t].posY;
                                    pos.z = cm.casts[t].posZ;
                                    burstVFX.transform.position = pos;
                                    burstVFX.transform.parent = lightingObject.transform;
                                    Destroy(burstVFX, 8f);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public void InvokeSplaturnSpell( int islandIndex, int structureIndex )
    {
        // validate
        if (islandIndex < 0 || islandIndex > islands.Length-1)
        {
            Debug.LogError("--- IslandManager [InvokeSplaturnSpell] : invalid island index. aborting.");
            return;
        }
        if (structureIndex < 0 || structureIndex > islands[islandIndex].structures.Length - 1)
        {
            Debug.LogError("--- IslandManager [InvokeSplaturnSpell] : invalid structure index for island index #"+islandIndex+". aborting.");
            return;
        }
        if (!islands[islandIndex].structures[structureIndex].name.Contains("wiz tower"))
            Debug.LogWarning("--- IslandManager [InvokeSplaturnSpell] : structure '"+ islands[islandIndex].structures[structureIndex].name +"' on island index # " +islandIndex+" does not contain 'wiz tower'. will ignore.");

        // find player of island, find player options main color
        Color playerMainColor = new Color(0.9f, 0.8f, 0.618f);
        PlayerColor pColor = PlayerColor.Default;
        GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm != null)
        {
            PlayerData[] playerData = ggm.game.players;
            for (int n = 0; n < playerData.Length; n++)
            {
                if (playerData[n].playerIsland == islandIndex)
                {
                    playerMainColor = PlayerSystem.GetPlayerColor(playerData[n].options.mainColor);
                    pColor = playerData[n].options.mainColor;
                    break;
                }
            }
        }
        // find structure that is "Structure wiz tower" on island
        GameObject islandFolder = GameObject.Find("Islands");
        GameObject islandObj = null;
        for (int i = 0; i < islandFolder.transform.childCount; i++)
        {
            if (islandFolder.transform.GetChild(i).gameObject.transform.position == GameSystem.GetVector(islands[islandIndex].location))
                islandObj = islandFolder.transform.GetChild(i).gameObject;
        }
        if (islandObj == null)
        {
            Debug.LogError("--- IslandManager [InvokeSplaturnSpell] : no island found at " + GameSystem.GetVector(islands[islandIndex].location) + ". aborting.");
            return;
        }
        GameObject towerObject = null;
        for (int i = 0; i < islandObj.transform.childCount; i++)
        {
            if (islandObj.transform.GetChild(i).gameObject.transform.position == GameSystem.GetVector(islands[islandIndex].structures[structureIndex].location))
                towerObject = islandObj.transform.GetChild(i).gameObject;
        }
        if (towerObject == null)
        {
            Debug.LogError("--- IslandManager [InvokeSplaturnSpell] : no tower found at " + GameSystem.GetVector(islands[islandIndex].structures[structureIndex].location) + ". aborting.");
            return;
        }
        DoSplaturnColoration(towerObject, (StructureEffect)pColor);
        // apply this tower color to the structure as an effect
        islands[islandIndex].structures[structureIndex] = IslandSystem.AddStructureEffect(islands[islandIndex].structures[structureIndex], (StructureEffect)pColor);
    }

    void DoSplaturnColoration( GameObject towerObject, StructureEffect splaturnColor )
    {
        // can look for all Util_White and replace with player color based on effect
        Renderer[] rends = towerObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < rends.Length; i++)
        {
            if (rends[i].material.name == "Util_White (Instance)")
                rends[i].material.SetColor("_Color", PlayerSystem.GetPlayerColor((PlayerColor)splaturnColor));
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
        propTimer = 0.618f;
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
            tm.silentTeleport = island.tports[i].silent;
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
                case StructureType.HermitTower:
                    prefabName = "Hermit Tower";
                    break;
                case StructureType.WizardTower:
                    prefabName = "Wizard Tower";
                    break;
                case StructureType.SorcererTower:
                    prefabName = "Sorcerer Tower";
                    break;
                case StructureType.WizardInterior:
                    prefabName = "Test Tower Interior";
                    break;
                case StructureType.MarketShop:
                    prefabName = "Market Shop";
                    break;
                case StructureType.MarketShopInterior:
                    prefabName = "Market Interior";
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
            // STRUCTURE EFFECT : MAGIC SPELL : SPLATURN
            if (sData.effects.Length > 0)
            {
                for (int n = 0; n < sData.effects.Length; n++)
                {
                    if (sData.effects[n] <= StructureEffect.TowerColorO)
                    {
                        DoSplaturnColoration(structure, sData.effects[n]);
                        break;
                    }
                }
            }
        }
        retBool = true;

        return retBool;
    }

    /// <summary>
    /// Force island manager to re-acquire props in island data, and re-form prop renderers (periodic checks)
    /// </summary>
    /// <param name="iData">island data</param>
    /// <param name="iObject">island object</param>
    public void ForceReConfigurePropRenderers( IslandData iData, GameObject iObject )
    {
        GameObject interiorObj = iObject.transform.Find("Structure tower interior").gameObject;
        for ( int i = 0; i < iData.props.Length; i++ )
        {
            // outdoor prop renderers
            if (iData.props[i].type < PropType.IntCandleA)
            {
                GameObject prop = null;
                if (iObject.transform.Find("Prop "+iData.props[i].name) != null)
                    prop = iObject.transform.Find("Prop " + iData.props[i].name).gameObject;
                else
                {
                    Debug.LogWarning("--- IslandManger [ForceReConfigurePropRenderers] : no object found of name 'Prop " + iData.props[i].name + "'. will ignore.");
                    continue;
                }
                // store prop reneders for color adjustment
                Renderer[] rends = prop.GetComponentsInChildren<Renderer>();
                if (rends != null && rends.Length > 0)
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
            else
            {
                // indoor prop renderers
                GameObject tapestryObj = null;
                GameObject decoObj = null;
                if (iData.props[i].type == PropType.IntTapestryA || iData.props[i].type == PropType.IntTapestryB)
                    decoObj = interiorObj.transform.Find("Deco").gameObject;
                if (decoObj != null)
                {
                    if (iData.props[i].type == PropType.IntTapestryA)
                        tapestryObj = decoObj.transform.Find("Tapestry A").gameObject;
                    if (iData.props[i].type == PropType.IntTapestryB)
                        tapestryObj = decoObj.transform.Find("Tapestry B").gameObject;
                }
                // store prop reneders for color adjustment
                if (tapestryObj != null)
                {
                    Renderer[] rends = tapestryObj.GetComponentsInChildren<Renderer>();
                    if (rends != null && rends.Length > 0)
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
            }
        }
    }

    bool ConfigureProps(IslandData island, GameObject islandObj)
    {
        bool retBool = false;

        if (island.props == null || island.props.Length == 0)
            return retBool;

        for (int i = 0; i < island.props.Length; i++)
        {
            // outdoor props load prefab
            PropData pData = island.props[i];
            if (pData.type < PropType.IntCandleA)
            {
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
                if (rends != null && rends.Length > 0)
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
            else
            {
                // indoor props 
                GameObject interiorTower = GameObject.Find("Structure tower interior");
                GameObject decoObj = null;
                if (interiorTower == null)
                {
                    Debug.LogError("--- IslandManager [ConfigureProps] : interior prop config failed. cannot find tower interior. aborting.");
                    return retBool;
                }
                decoObj = interiorTower.transform.Find("Deco").gameObject;
                GameObject tapestryObj = null;
                switch (pData.type)
                {
                    case PropType.Default:
                        // we should never be here
                        break;
                    case PropType.IntCandleA:
                        decoObj.transform.Find("Candle Prop A").gameObject.SetActive(true);
                        break;
                    case PropType.IntCandleB:
                        decoObj.transform.Find("Candle Prop B").gameObject.SetActive(true);
                        break;
                    case PropType.IntFireplace:
                        decoObj.transform.Find("Fireplace Prop").gameObject.SetActive(true);
                        break;
                    case PropType.IntBookshelf:
                        decoObj.transform.Find("Bookshelf").gameObject.SetActive(true);
                        break;
                    case PropType.IntWritingDesk:
                        decoObj.transform.Find("Writing Desk").gameObject.SetActive(true);
                        break;
                    case PropType.IntTapestryA:
                        decoObj.transform.Find("Tapestry A").gameObject.SetActive(true);
                        tapestryObj = decoObj.transform.Find("Tapestry A").gameObject;
                        break;
                    case PropType.IntTapestryB:
                        decoObj.transform.Find("Tapestry B").gameObject.SetActive(true);
                        tapestryObj = decoObj.transform.Find("Tapestry B").gameObject;
                        break;
                    default:
                        break;
                }
                // store prop reneders for color adjustment
                if (tapestryObj != null)
                {
                    Renderer[] rends = tapestryObj.GetComponentsInChildren<Renderer>();
                    if (rends != null && rends.Length > 0)
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
            }
        }
        retBool = true;

        return retBool;
    }
}
