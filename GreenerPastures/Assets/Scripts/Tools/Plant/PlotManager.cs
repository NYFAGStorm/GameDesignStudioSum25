using UnityEngine;

public class PlotManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plot of land that is able to hold a single plant

    public enum CurrentAction
    {
        None,
        Working,
        Planting,
        Watering,
        Harvesting,
        Uprooting
    }

    public PlotData data;
    public GameObject plant;

    private TimeManager tim;

    private PlayerControlManager currentPlayer;
    private Renderer cursor;
    private bool cursorActive;
    private float cursorTimer;
    private float plotTimer;
    private float uprootedTimer;

    private CurrentAction action;
    private bool actionDirty; // complete, not yet cleared
    private bool actionClear; // no action pressed
    private bool actionProgressDisplay;
    private float actionProgress;
    private float actionLimit;
    private string actionLabel;
    private float actionCompleteTimer;

    private float harvestDisplayTimer;
    private float harvestQualityValue;

    const float CURSORPULSEDURATION = 0.5f;
    // temp use time manager multiplier
    const float WATERDRAINRATE = 0.25f;
    const float WATERDRAINWITHPLANTRATE = 0.1f;
    const float SOILDEGRADERATE = 0.381f;
    const float PLOTCHECKINTERVAL = 1f;
    // action hold time windows to complete
    const float WORKLANDWINDOW = 2f;
    const float WATERWINDOW = 1f;
    const float PLANTWINDOW = .5f;
    const float HARVESTWINDOW = 1.5f;
    const float UPROOTWINDOW = 2.5f;
    const float ACTIONCOMPLETEDURATION = 0.5f;
    const float HARVESTDISPLAYDURATION = 1f;
    const float UPROOTEDPLOTPAUSE = 1.5f; // disallow dropped items after uprooted


    void Start()
    {
        // validate
        tim = GameObject.FindAnyObjectByType<TimeManager>();
        if ( tim == null )
        {
            Debug.LogError("--- PlotManager [Start] : no time manager found in scene. aborting.");
            enabled = false;
        }
        cursor = gameObject.transform.Find("Cursor").GetComponent<Renderer>();
        if ( cursor == null )
        {
            Debug.LogError("--- PlotManager [Start] : no cursor child found. aborting.");
            enabled = false;
        }
        // inititalize
        if (enabled)
        {
            data = FarmSystem.InitializePlot();
            cursor.enabled = false;
            plotTimer = 0.1f;
        }
    }

    void Update()
    {
        CheckCursor();

        // run plot timer
        if ( plotTimer > 0f )
        {
            plotTimer -= Time.deltaTime;
            if ( plotTimer < 0f )
            {
                plotTimer = PLOTCHECKINTERVAL;
                // set sun
                data.sun = Mathf.Clamp01(Mathf.Sin((tim.dayProgress - .25f) * 2f * Mathf.PI));
                // water drain
                data.water = Mathf.Clamp01(data.water - (WATERDRAINRATE * Time.deltaTime * PLOTCHECKINTERVAL));
                // water drain faster if plant, based on plant growth
                if (plant != null)
                    data.water = Mathf.Clamp01(data.water - (data.plant.growth * WATERDRAINWITHPLANTRATE * Time.deltaTime * PLOTCHECKINTERVAL));
                // soil degrade, if plant exists
                if (plant != null)
                    data.soil = Mathf.Clamp01(data.soil - (data.plant.growth * data.plant.vitality * SOILDEGRADERATE * Time.deltaTime * PLOTCHECKINTERVAL));
                // PLOT EFFECTS:
                if (FarmSystem.PlotHasEffect(data, PlotEffect.SummonWaterI))
                    data.water = 1f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.EclipseI))
                    data.sun = Mathf.Clamp( (data.sun * 0.5f), 0f, 0.5f );
            }
        }

        // update plot hazards
        // REVIEW: what hazards?
        UpdatePlotHazards();

        // run action complete timer
        if ( actionCompleteTimer > 0f )
        {
            actionCompleteTimer -= Time.deltaTime;
            if ( actionCompleteTimer < 0f )
            {
                actionCompleteTimer = 0f;
                actionLabel = "";
                actionProgressDisplay = false;
            }
        }

        // run harvest display timer
        if ( harvestDisplayTimer > 0f )
        {
            harvestDisplayTimer -= Time.deltaTime;
            if ( harvestDisplayTimer < 0f )
                harvestDisplayTimer = 0f;
        }

        // run uprooted timer
        if ( uprootedTimer > 0f )
        {
            uprootedTimer -= Time.deltaTime;
            if ( uprootedTimer < 0f )
                uprootedTimer = 0f;
        }
    }

    /// <summary>
    /// Can this plot accept a plant or stalk drop into the uprooted hole?
    /// </summary>
    /// <returns>true if plot can accept, false if pause timer still running</returns>
    public bool CanAcceptPlantDrop()
    {
        return (uprootedTimer == 0f);
    }

    void CheckCursor()
    {
        // ignore if not active
        if (!cursorActive)
            return;
        // highlight cursor pulse
        cursorTimer += Time.deltaTime;
        float pulse = 1f - ((cursorTimer % CURSORPULSEDURATION) * 0.9f);
        SetCursorHighlight(pulse);
    }

    void UpdatePlotHazards()
    {
        // PLOT EFFECTS:
        if (FarmSystem.PlotHasEffect(data, PlotEffect.BlessI))
            return;
        
        // TODO: implement hazards update
    }

    /// <summary>
    /// Sets this plot to associate a player for transferring inventory
    /// </summary>
    /// <param name="player">player control manager reference</param>
    /// <returns>true if successful, false if a player already set as current</returns>
    public void SetCurrentPlayer( PlayerControlManager player )
    {
        // REVIEW: does this get confused in multiplayer?
        currentPlayer = player;
    }

    /// <summary>
    /// Sets this plot cursor pulse state
    /// </summary>
    /// <param name="active">if true, cursor will pulse</param>
    public void SetCursorPulse( bool active )
    {
        cursorActive = active;
        SetCursorHighlight(0f);
        cursorTimer = 0f;
        cursor.enabled = active;
        ActionClear();
        actionProgressDisplay = false;
        if ( active && currentPlayer == null )
            Debug.LogWarning("--- PlotManager [SetCursorPulse] : cursor active but no current player is defined. will ignore.");
    }

    void SetCursorHighlight( float value )
    {
        Color c = cursor.material.color;
        // REVIEW: with better art
        c.r = 1f;
        c.g = 1f;
        c.b = value;
        c.a = value;
        cursor.material.color = c;
    }

    /// <summary>
    /// Signals that no player plot action is currently active
    /// </summary>
    public void ActionClear()
    {
        actionDirty = false;
        actionClear = true;
        actionProgress = 0f;
        actionLimit = 5f;
        action = CurrentAction.None;
    }

    bool ActionComplete( float limit, string label )
    {
        bool retBool = false;

        actionProgressDisplay = true;
        actionLimit = limit;
        actionLabel = label;
        actionProgress += Time.deltaTime;
        if (actionProgress >= actionLimit)
        {
            actionCompleteTimer = ACTIONCOMPLETEDURATION;
            actionDirty = true;
            actionClear = false;
            action = CurrentAction.None;
            retBool = true;
        }

        return retBool;
    }

    /// <summary>
    /// Works the land, turning from wild to dirt to tilled
    /// </summary>
    public void WorkLand()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        ItemData iData = pcm.GetPlayerCurrentItemSelection();

        if (data.condition == PlotCondition.Tilled)
        {
            if (action == CurrentAction.Planting && (iData == null || 
                iData.type != ItemType.Seed))
            {
                actionLabel = "Need Seed Selected";
                return;
            }

            if (action != CurrentAction.Planting && action != CurrentAction.None)
                return;
            action = CurrentAction.Planting;

            if (!ActionComplete(PLANTWINDOW, "PLANTING..."))
                return;
        }
        else
        {
            // cannot work the land if planted
            if (plant != null)
                return;

            if (action != CurrentAction.Working && action != CurrentAction.None)
                return;
            action = CurrentAction.Working;

            if (!ActionComplete(WORKLANDWINDOW, "WORKING..."))
                return;
        }

        Renderer r = gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>();
        switch (data.condition)
        {
            case PlotCondition.Default:
                // should never be here
                break;
            case PlotCondition.Wild:
                // remove wild grasses
                GameObject grasses = gameObject.transform.Find("Plot Wild Grasses").gameObject;
                if (grasses == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " no wild grasses found. will ignore.");
                else
                    Destroy(grasses);
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                data.condition = PlotCondition.Dirt;
                // soil quality improved
                data.soil = Mathf.Clamp01(data.soil + (0.25f * RandomSystem.GaussianRandom01()));
                break;
            case PlotCondition.Dirt:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Tilled");
                data.condition = PlotCondition.Tilled;
                // soil quality improved
                data.soil = Mathf.Clamp01(data.soil + (0.25f * RandomSystem.GaussianRandom01()));
                break;
            case PlotCondition.Tilled:
                // plant seed with seed item selected in player inventory
                plant = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                plant.transform.parent = transform;
                plant.transform.position = transform.position;
                // insert distinct plant data from player inventory current selection
                data.plant = PlantSystem.InitializePlant(iData.plant);
                // configure plant from seed to size = 0f (growth) and quality = 0f
                data.plant.growth = 0f;
                data.plant.quality = 0f;
                // using data, remove from player inventory
                pcm.DeleteCurrentItemSelection();
                data.condition = PlotCondition.Growing;
                // PLOT EFFECTS:
                if (FarmSystem.PlotHasEffect(data, PlotEffect.FastGrowI))
                    data.plant.adjustedGrowthRate += 0.05f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.MalnutritionI))
                    data.plant.adjustedGrowthRate -= 0.1f;
                break;
            case PlotCondition.Uprooted:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                data.condition = PlotCondition.Dirt;
                break;
            default:
                break;
        }       
    }

    /// <summary>
    /// Waters this plot
    /// </summary>
    public void WaterPlot()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        if (action != CurrentAction.Watering && action != CurrentAction.None)
            return;
        action = CurrentAction.Watering;

        if (!ActionComplete(WATERWINDOW, "WATERING..."))
            return;

        data.water = 1f;
    }

    public void HarvestPlant()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        // cannot harvest unless a plant exists, plant at 100% growth and not yet harvested
        if (plant == null || data.plant.growth < 1f || data.plant.isHarvested)
            return;

        if (action != CurrentAction.Harvesting && action != CurrentAction.None)
            return;
        action = CurrentAction.Harvesting;

        if (!ActionComplete(HARVESTWINDOW,"HARVESTING..."))
            return;

        if ( data.condition == PlotCondition.Growing )
        {
            // harvest if plant is 100% grown and not yet harvested
            if (data.plant.growth < 1f)
                return;
            if (!data.plant.isHarvested)
            {
                data.plant.isHarvested = true;
                plant.transform.Find("Plant Image").GetComponent<Renderer>().material.mainTexture = (Texture2D)Resources.Load("ProtoPlant_Stalk");
                // drop as loose item fruit
                ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
                // attempt place all in player inventory first, drop as loose if no empty slot                
                Vector3 target;
                if (ism == null)
                    Debug.LogWarning("--- PlotManager [HarvestPlant] : " + gameObject.name + " no item spawn manager found in scene. will ignore.");
                else
                {
                    int harvestNumber = data.plant.harvestAmount; // if this is >1, flat random to this number
                    if (harvestNumber > 1)
                        harvestNumber = Random.Range(0, harvestNumber) + 1;
                    // PLOT EFFECTS:
                    if (FarmSystem.PlotHasEffect(data, PlotEffect.ProsperousI) && 
                        RandomSystem.FlatRandom01() < .1f)
                        harvestNumber *= 2;
                    // iterate to harvest multiple
                    for ( int i = 0; i < harvestNumber; i++ )
                    {
                        // PLOT EFFECTS:
                        if (FarmSystem.PlotHasEffect(data, PlotEffect.LesionI))
                            data.plant.quality -= 0.05f;
                        if (FarmSystem.PlotHasEffect(data, PlotEffect.GoldenThumbI))
                            data.plant.quality += 0.1f;
                        // check if empty inventory slot availbale on player, drop loose if not
                        if (currentPlayer != null && InventorySystem.InvHasSlot(currentPlayer.playerData.inventory))
                        {
                            // harvesting gives fruit
                            ItemData iData = InventorySystem.InitializeItem(ItemType.Fruit);
                            if (iData == null)
                                Debug.LogWarning("--- PlotManager [HarvestPlant] : unable to initialize fruit item. will ignore.");
                            else
                            {
                                iData.plant = data.plant.type;
                                iData.name += " (" + data.plant.plantName.ToString() + ")";
                            }
                            currentPlayer.playerData.inventory = InventorySystem.AddToInventory(currentPlayer.playerData.inventory, iData);
                        }
                        else
                        {
                            target = gameObject.transform.position;
                            target.x += RandomSystem.GaussianRandom01() - .5f;
                            target.z -= 0.01f; // in front of plant
                            // harvesting drops fruit
                            LooseItemData loose = InventorySystem.CreateItem(ItemType.Fruit);
                            // transfer properties of fruit to item (revise item data)
                            loose.inv.items[0] = InventorySystem.SetItemAsPlant(loose.inv.items[0], data.plant);
                            loose.inv.items[0].size = data.plant.growth;
                            loose.inv.items[0].quality = data.plant.quality;
                            ism.SpawnItem(loose, gameObject.transform.position, target);
                        }
                    }
                    // harvesting may drop seed
                    if (RandomSystem.FlatRandom01() < data.plant.seedPotential)
                    {
                        // calculate number of seed items
                        int numberOfSeeds = 1;
                        if (data.plant.seedPotential > 0.5f)
                        {
                            // += Mathf.RoundToInt(((seedPotential - .5f) / .2f) + .5f)
                            numberOfSeeds += Mathf.RoundToInt(((data.plant.seedPotential - .5f) / .2f) + .5f);
                            // gaussian random distribution from 1 to numberOfSeeds
                            numberOfSeeds = 1 + Mathf.RoundToInt((RandomSystem.GaussianRandom01() * (numberOfSeeds - 1)) + 0.5f);
                        }
                        // spawn seeds as individual copies of original plant, in seed form
                        for (int i = 0; i < numberOfSeeds; i++)
                        {
                            if (currentPlayer != null && InventorySystem.InvHasSlot(currentPlayer.playerData.inventory))
                            {
                                // harvesting gives seed if empty inventory slot on player
                                ItemData iData = InventorySystem.InitializeItem(ItemType.Seed);
                                if (iData == null)
                                    Debug.LogWarning("--- PlotManager [HarvestPlant] : unable to initialize seed item. will ignore.");
                                else
                                {
                                    iData.plant = data.plant.type;
                                    iData.name += " (" + data.plant.plantName.ToString() + ")";
                                }
                                currentPlayer.playerData.inventory = InventorySystem.AddToInventory(currentPlayer.playerData.inventory, iData);
                            }
                            else
                            {
                                // drop seed
                                target = gameObject.transform.position;
                                target.x += RandomSystem.GaussianRandom01() - .5f;
                                target.z -= 0.01f; // in front of plant
                                LooseItemData looseSeed = InventorySystem.CreateItem(ItemType.Seed);
                                looseSeed.inv.items[0] = InventorySystem.SetItemAsPlant(looseSeed.inv.items[0], data.plant);
                                // seeds begin with a size and quality of zero (when planted, start growing)
                                looseSeed.inv.items[0].size = 0f;
                                looseSeed.inv.items[0].quality = 0f;
                                target.x += RandomSystem.GaussianRandom01() - .5f;
                                ism.SpawnItem(looseSeed, gameObject.transform.position, target);
                            }
                        }
                    }
                }
                // harvest results display
                harvestDisplayTimer = HARVESTDISPLAYDURATION;
                harvestQualityValue = data.plant.quality;
            }
        }
    }

    public void UprootPlot()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        // cannot uproot unless plant exists in this plot
        if (plant == null)
            return;

        if (action != CurrentAction.Uprooting && action != CurrentAction.None)
            return;
        action = CurrentAction.Uprooting;

        if (!ActionComplete(UPROOTWINDOW, "UPROOTING..."))
            return;

        Renderer r = gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>();
        // change ground texture
        if (r == null)
            Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
        else
            r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Uprooted");
        // player would collect stalk or full plant as inventory at this point if growth >50%
        // stalk if harvested, plant if not (data retains both isHarvested and growth)
        if (data.plant.growth > 0.5f)
        {
            // check if empty inventory slot availbale on player, drop loose if not
            if (currentPlayer != null && InventorySystem.InvHasSlot(currentPlayer.playerData.inventory))
            {
                // harvesting gives fruit
                ItemData iData = InventorySystem.InitializeItem(ItemType.Plant);
                if (iData == null)
                    Debug.LogWarning("--- PlotManager [UprootPlot] : unable to initialize plant or stalk item. will ignore.");
                else
                {
                    if (data.plant.isHarvested)
                    {
                        iData.type = ItemType.Stalk;
                        iData.name = "Stalk";
                    }
                    iData = InventorySystem.SetItemAsPlant(iData, data.plant);
                }
                currentPlayer.playerData.inventory = InventorySystem.AddToInventory(currentPlayer.playerData.inventory, iData);
            }
            else
            {
                ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
                // drop as loose item
                Vector3 target = gameObject.transform.position;
                target.x += RandomSystem.GaussianRandom01() - .5f;
                target.z -= 0.01f; // in front of plant
                LooseItemData loose = InventorySystem.CreateItem(ItemType.Plant);
                // transfer properties of plant to item (revise item data)
                loose.inv.items[0] = InventorySystem.SetItemAsPlant(loose.inv.items[0], data.plant);
                if (data.plant.isHarvested)
                {
                    loose.inv.items[0].type = ItemType.Stalk;
                    loose.inv.items[0].name = "Stalk (" + data.plant.type.ToString() + ")";
                }
                ism.SpawnItem(loose, gameObject.transform.position, target);
            }
        }
        // remove plant
        Destroy(plant);
        data.plant = PlantSystem.InitializePlant(PlantType.Default);
        data.condition = PlotCondition.Uprooted;
        uprootedTimer = UPROOTEDPLOTPAUSE; // disallow plant drop in hole for a short time
    }

    void OnGUI()
    {
        if (!cursorActive && !actionProgressDisplay && harvestDisplayTimer <= 0f)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        string s = actionLabel;
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;

        // locate display over plot
        Vector3 disp = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        disp.y = (1f - disp.y);

        if (harvestDisplayTimer > 0f)
        {
            float progress = 1f - (harvestDisplayTimer / HARVESTDISPLAYDURATION);
            float fade = Mathf.Clamp01( (1f - progress) * 3f);

            r.x = (disp.x - 0.05f) * w;
            r.y = disp.y * h;
            r.y -= (0.355f + (progress * 0.025f)) * h;
            r.width = 0.1f * w;
            r.height = 0.075f * h;

            g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
            s = "Quality: " + (harvestQualityValue * 100f).ToString("00.0") + "%";
            c = Color.yellow;
            c.a = fade;
            GUI.color = c;
            GUI.depth = -3;

            GUI.Label(r, s, g);
        }

        if (!actionProgressDisplay)
        {
            r.x = (disp.x - 0.05f) * w;
            r.y = disp.y * h;
            r.y -= 0.3f * h;
            r.width = 0.1f * w;
            if (plant == null)
                r.height = 0.08f * h;
            else
                r.height = 0.15f * h;

            // display stats background
            c = Color.gray;
            c.a = .8f;
            GUI.color = c;
            GUI.depth = 2;

            GUI.DrawTexture(r, t);

            // plot stats display
            r.x += 0.01f * w;
            if (plant == null)
                r.y -= 0.025f * h;
            else
                r.y -= 0.05f * h;
            g.fontSize = Mathf.RoundToInt(10f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleLeft;

            GUI.color = Color.white;
            c.a = 1f;
            GUI.depth = 0;

            // stats drop shadow first, then text
            GUI.color = Color.black;
            r.x += 0.0005f * w;
            r.y += 0.001f * h;

            s = "Sun      : " + (data.sun * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
            s = "Water   : " + (data.water * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
            s = "Soil      : " + (data.soil * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            if (plant != null)
            {
                r.y += 0.025f * h;
                s = "Growth : " + (data.plant.growth * 100f).ToString("00.0")+"%";
                GUI.Label(r, s, g);

                r.y += 0.025f * h;
                s = "Quality : " + (data.plant.quality * 100f).ToString("00.0") + "%";
                GUI.Label(r, s, g);
            }

            // reset to top to draw text again
            if (plant == null)
                r.y -= 0.05f * h;
            else
                r.y -= 0.1f * h;
            GUI.color = Color.white;
            r.x -= 0.001f * w;
            r.y -= 0.002f * h;

            s = "Sun      : " + (data.sun * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
            s = "Water   : " + (data.water * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
            s = "Soil      : " + (data.soil * 100f).ToString("00.0") + "%";
            GUI.Label(r, s, g);

            if (plant != null)
            {
                r.y += 0.025f * h;
                s = "Growth : " + (data.plant.growth * 100f).ToString("00.0") + "%";
                GUI.Label(r, s, g);

                r.y += 0.025f * h;
                s = "Quality : " + (data.plant.quality * 100f).ToString("00.0") + "%";
                GUI.Label(r, s, g);
            }
        }

        if (!actionProgressDisplay)
            return;

        r.x = (disp.x - 0.05f) * w;
        r.y = disp.y * h;
        r.y -= 0.25f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;

        g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;

        // display action label with drop shadow
        GUI.color = Color.black;
        GUI.depth = 0;
        r.x += 0.0005f * w;
        r.y += 0.001f * h;

        GUI.Label(r,s,g);

        GUI.color = Color.white;
        r.x -= 0.001f * w;
        r.y -= 0.002f * h;

        GUI.Label(r,s,g);

        if (actionCompleteTimer != 0f && 
            actionCompleteTimer < ACTIONCOMPLETEDURATION * 0.5f)
            return;

        // display progress bar background
        r.y += 0.05f * h;
        r.height = 0.05f * h;
        c = Color.gray;
        c.a = .8f;
        GUI.color = c;
        GUI.depth = 2;

        GUI.DrawTexture(r, t);

        // display progress bar foreground
        c = Color.blue;
        c.r = 0.1f;
        c.g = 0.1f;
        if (actionCompleteTimer > 0f)
            c.b = 0f;
        c.r = (actionCompleteTimer / ACTIONCOMPLETEDURATION);
        c.g = (actionCompleteTimer / ACTIONCOMPLETEDURATION);
        c.a = 1f;
        GUI.color = c;

        r.height = 0.05f * h;
        GUI.depth = 1;
        // scale and position for 'inside' bg bar
        r.x += 0.00618f * w;
        r.y += 0.01f * h;
        r.width -= 0.01238f * w;
        r.height -= 0.02f * h;
        // scale for progress
        r.width = (actionProgress / actionLimit) * r.width;

        GUI.DrawTexture(r, t);
    }
}
