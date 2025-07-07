using UnityEngine;

public class ItemSpawnManager : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles loose item spawning for dropped items
    // -- also handles specific affects for dropped items when settled --
    // (fertilizer on uprooted plot for improving soil quality)
    // (seed on tilled plot for planting)
    // (stalk or plant on uprooted plot for trans-planting)

    public AnimationCurve dropCurve;

    private struct DroppedItem
    {
        public GameObject dropItem;
        public Vector3 spawnPoint;
        public Vector3 dropTarget;
        public float dropTimer;
    }
    private DroppedItem[] drops = new DroppedItem[0];

    const float DROPTIME = 1f;
    const float VERTICALORIGIN = 0.25f;
    const float TARGETDETECTRADIUS = 0.381f;


    void Start()
    {
        // validate
        if (dropCurve == null || dropCurve.length < 2)
        {
            Debug.LogError("--- ItemSpawnManager [Start] : drop curve undefined. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        CheckDrops();
    }

    void AddDrop( GameObject item, Vector3 start, Vector3 end )
    {
        DroppedItem[] tmp = new DroppedItem[drops.Length + 1];

        // detect the closest island center and range (use tport nodes)
        Vector3 iCenter = Vector3.zero;
        float iRadius = 0f;
        TeleportManager[] tports = GameObject.FindObjectsByType<TeleportManager>(FindObjectsSortMode.None);
        if (tports != null && tports.Length > 0)
        {
            float closest = 9999f;
            int found = -1;
            for (int i = 0; i < tports.Length; i++)
            {
                float dist = Vector3.Distance(tports[i].transform.position, end);
                if (dist < closest)
                {
                    closest = dist;
                    found = i;
                }
            }
            if ( found > -1 )
            {
                iCenter = tports[found].islandObj.transform.position;
                iRadius = tports[found].islandRadius;
            }
        }

        bool doBounce = false;

        // check not off island
        if (Vector3.Distance(end, iCenter) >= iRadius)
            doBounce = true;

        // probe end point for collision with check box
        if (Physics.CheckBox(end + (Vector3.up * 0.15f), (Vector3.one * 0.12f), Quaternion.identity, 1, QueryTriggerInteraction.Ignore))
            doBounce = true;

        if (doBounce)
        {
            // 'bounce' by resetting landing point
            Vector3 startGround = start;
            startGround.y = end.y;
            end -= (end - startGround);
        }

        for (int i=0;i<drops.Length;i++)
        {
            tmp[i] = drops[i];
        }
        tmp[drops.Length].dropItem = item;
        tmp[drops.Length].spawnPoint = start;
        tmp[drops.Length].dropTarget = end;
        tmp[drops.Length].dropTimer = DROPTIME;

        drops = tmp;
    }

    void RemoveDrop( int index )
    {
        DroppedItem[] tmp = new DroppedItem[drops.Length - 1];

        int count = 0;
        for (int i=0;i<drops.Length;i++)
        {
            if (i != index)
            {
                tmp[count] = drops[i];
                count++;
            }
        }

        drops = tmp;
    }

    void CheckDrops()
    {
        for (int i=0; i<drops.Length; i++)
        {
            // run drop timer
            if (drops[i].dropTimer > 0f)
            {
                drops[i].dropTimer -= Time.deltaTime;
                if (drops[i].dropTimer < 0f)
                {
                    drops[i].dropTimer = 0f;
                    Vector3 land = drops[i].dropTarget + (Vector3.up * VERTICALORIGIN);
                    if (drops[i].dropItem != null)
                        drops[i].dropItem.transform.position = land;

                    // drop item affects
                    LooseItemManager looseD = drops[i].dropItem.GetComponent<LooseItemManager>();
                    // fertilizer dropped in uprooted plot
                    if (looseD.looseItem.inv.items[0].type == ItemType.Fertilizer)
                    {
                        if (CheckFertilizerDrop(i))
                            looseD.looseItem.deleteMe = true;
                    }
                    // seed dropped in empty tilled plot
                    if (looseD.looseItem.inv.items[0].type == ItemType.Seed)
                    {
                        if (CheckSeedDrop(i, looseD.looseItem.inv.items[0].plant))
                            looseD.looseItem.deleteMe = true;
                    }
                    // stalk or plant dropped in uprooted plot
                    if (looseD.looseItem.inv.items[0].type == ItemType.Stalk ||
                        looseD.looseItem.inv.items[0].type == ItemType.Plant)
                    {
                        if (CheckPlantDrop(i, looseD.looseItem.inv.items[0]))
                            looseD.looseItem.deleteMe = true;
                    }

                    RemoveDrop(i);
                    continue;
                }
                if (drops[i].dropItem == null)
                {
                    // this happens if player takes it while in animation
                    //Debug.LogWarning("--- ItemSpawnManager [Update] : drop item ["+i+"] lost. will ignore.");
                    drops[i].dropTimer = 0f;
                    continue;
                }
                else
                {
                    Vector3 pos = Vector3.Lerp(drops[i].spawnPoint, drops[i].dropTarget, 1f - (drops[i].dropTimer / DROPTIME));
                    pos.y = drops[i].dropTarget.y + dropCurve.Evaluate(1f - (drops[i].dropTimer / DROPTIME)) + VERTICALORIGIN;
                    drops[i].dropItem.transform.position = pos;
                }
            }
        }
    }

    /// <summary>
    /// Creates new loose item data and spawns loose item object
    /// </summary>
    /// <param name="type">item type</param>
    /// <param name="spawnAt">spawn location</param>
    /// <param name="dropTo">drop target location</param>
    public void SpawnNewItem( ItemType type, Vector3 spawnAt, Vector3 dropTo )
    {
        SpawnItem(InventorySystem.CreateItem(type), spawnAt, dropTo);
    }

    /// <summary>
    /// Spawns a loose item object from given loose item data
    /// </summary>
    /// <param name="item">loose item data</param>
    /// <param name="location">position in world to put item</param>
    public void SpawnItem( LooseItemData item, Vector3 spawnLocation, Vector3 dropLocation )
    {
        GameObject looseItem = GameObject.Instantiate((GameObject)Resources.Load("Loose Item"));
        looseItem.transform.position = spawnLocation + (Vector3.up * VERTICALORIGIN);
        looseItem.name = "Item "+item.inv.items[0].type.ToString();
        LooseItemManager lim = looseItem.GetComponent<LooseItemManager>();
        if (lim == null)
            Debug.LogWarning("--- ItemSpawnManager [SpawnItem] : loose item prefab does not contain loose item manager component. will ignore.");
        lim.looseItem = item;
        lim.looseItem.flipped = (dropLocation.x <= spawnLocation.x);

        lim.frames = new Texture2D[1];
        // if plant, attempt find specific plant type first (get default type item if failed)
        // TODO: clean this up
        ArtData d = new ArtData();
        if (item.inv.items[0].plant != PlantType.Default)
            d = GameObject.FindAnyObjectByType<ArtLibraryManager>().GetArtData(item.inv.items[0].type, item.inv.items[0].plant);
        if ( d.type == ItemType.Default )
            d = GameObject.FindAnyObjectByType<ArtLibraryManager>().GetArtData(item.inv.items[0].type);
        lim.frames[0] = GameObject.FindAnyObjectByType<ArtLibraryManager>().itemImages[d.artIndexBase];

        // handle drop animation
        AddDrop(looseItem.gameObject, spawnLocation, dropLocation);
    }

    bool CheckFertilizerDrop( int index )
    {
        bool retBool = false;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i=0; i<plots.Length; i++)
        {
            float dist = Vector3.Distance(drops[index].dropTarget, plots[i].gameObject.transform.position);
            if (dist < TARGETDETECTRADIUS && plots[i].data.condition == PlotCondition.Uprooted)
            {
                // soil quality improved ~.5 (0-1, gaussian random distribution)
                plots[i].data.soil = Mathf.Clamp01(plots[i].data.soil + RandomSystem.GaussianRandom01());
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    // REVIEW: should we _not_ have a dedicated PLANTING player control, and
    // instead just use this mechanism of dropping a seed loose item on a tilled plot?
    // (if so, how do we probably need a visual/audio signal to the player it is planted)
    bool CheckSeedDrop( int index, PlantType type )
    {
        bool retBool = false;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            float dist = Vector3.Distance(drops[index].dropTarget, plots[i].gameObject.transform.position);
            if (dist < TARGETDETECTRADIUS && plots[i].data.condition == PlotCondition.Tilled)
            {
                // create plant using the seed plant data
                // REVIEW: how to configure proper plant prefab and art
                GameObject plantObj = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                plantObj.transform.parent = plots[i].gameObject.transform;
                plantObj.transform.position = plots[i].gameObject.transform.position;
                plots[i].plant = plantObj;
                PlantData plant = PlantSystem.InitializePlant(type);
                plots[i].data.plant = plant;
                // configure plant from seed to size = 0f (growth) and quality = 0f
                plots[i].data.plant.growth = 0f;
                plots[i].data.plant.quality = 0f;
                plots[i].data.condition = PlotCondition.Growing;
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    bool CheckPlantDrop( int index, ItemData iData )
    {
        bool retBool = false;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            float dist = Vector3.Distance(drops[index].dropTarget, plots[i].gameObject.transform.position);
            if (dist < TARGETDETECTRADIUS && plots[i].data.condition == PlotCondition.Uprooted)
            {
                if (!plots[i].CanAcceptPlantDrop())
                    break; // pause timer still running from uprooting this plot
                // create plant using the seed plant data
                GameObject plantObj = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                plantObj.transform.parent = plots[i].gameObject.transform;
                plantObj.transform.position = plots[i].gameObject.transform.position;
                // configure proper plant prefab and art
                Texture2D t;
                if (iData.type == ItemType.Stalk)
                    t = (Texture2D)Resources.Load("ProtoPlant_Stalk");
                else
                    t = (Texture2D)Resources.Load("ProtoPlant04");
                plantObj.GetComponentInChildren<Renderer>().material.mainTexture = t;
                plots[i].plant = plantObj;
                PlantData plant = PlantSystem.InitializePlant(iData.plant);
                plots[i].data.plant = plant;
                // configure individual plant properties from item data
                plots[i].data.plant.growth = Mathf.Clamp01(0.01f+iData.size); // some growth to trigger grow art
                plots[i].data.plant.health = iData.health;
                plots[i].data.plant.quality = iData.quality;
                plots[i].data.plant.isHarvested = (iData.type == ItemType.Stalk || (iData.type == ItemType.Plant && plant.canReFruit));
                plots[i].gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>().material.mainTexture = 
                    (Texture2D)Resources.Load("ProtoPlot_Tilled");
                // NOTE: we have skipped improving soil quality
                plots[i].data.condition = PlotCondition.Growing;
                retBool = true;
                break;
            }
        }

        return retBool;
    }
}
