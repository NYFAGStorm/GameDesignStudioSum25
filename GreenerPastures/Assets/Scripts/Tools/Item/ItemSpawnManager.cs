using UnityEngine;

public class ItemSpawnManager : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles loose item spawning for dropped items
    // -- also handles specific affects for dropped items when settled --
    // (fertilizer on uprooted plot for improving soil quality)
    // (seed on tilled plot for planting)

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
                    drops[i].dropItem.transform.position = land;

                    // drop item affects
                    LooseItemManager looseD = drops[i].dropItem.GetComponent<LooseItemManager>();
                    // fertilizer dropped in uprooted plot
                    if (looseD.looseItem.inv.items[0].type == ItemType.Fertilizer)
                    {
                        if (CheckFertilizerDrop(i))
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
                    pos.y = dropCurve.Evaluate(1f - (drops[i].dropTimer / DROPTIME)) + VERTICALORIGIN;
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
        ArtData d = GameObject.FindAnyObjectByType<ArtLibraryManager>().GetArtData(item.inv.items[0].type);
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
            if (dist < 0.25f && plots[i].data.condition == PlotCondition.Uprooted)
            {
                // soil quality improved ~.5 (0-1, gaussian random distribution)
                plots[i].data.soil = Mathf.Clamp01(plots[i].data.soil + RandomSystem.GaussianRandom01());
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    bool CheckSeedDrop( int index )
    {
        bool retBool = false;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            float dist = Vector3.Distance(drops[index].dropTarget, plots[i].gameObject.transform.position);
            if (dist < 0.25f && plots[i].data.condition == PlotCondition.Tilled)
            {
                // soil quality improved ~.5 (0-1, gaussian random distribution)
                //plots[i].data.soil = Mathf.Clamp01(plots[i].data.soil + RandomSystem.GaussianRandom01());
                // TODO: plant using the seed plant data
                
                retBool = true;
                break;
            }
        }

        return retBool;
    }
}
