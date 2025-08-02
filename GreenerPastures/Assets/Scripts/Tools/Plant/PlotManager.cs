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
        Uprooting,
        Grafting
    }

    public PlotData data;
    public GameObject plant;

    private TimeManager tim;

    private PositionData seasonValues; // all four seasons as a percentage

    private PlayerControlManager currentPlayer;
    private Renderer cursor;
    private bool cursorActive;
    private float cursorTimer;
    private float plotTimer;
    private float uprootedTimer;

    private CurrentAction action;
    private CurrentAction previous; // to prevent switching action and cheat progress taps
    private bool actionDirty; // complete, not yet cleared
    private bool actionClear; // no action pressed
    private bool actionProgressDisplay;
    private float actionProgress;
    private float actionLimit;
    // step progress forward on tap, or hold wait for progress step
    private float actionProgressStep; // amount to step forward (either tap or timed hold)
    private float actionStepTimer; // duration between steps if holding action
    private float actionDrainTimer; // time before progress is draining (reset if tap or hold)
    private string actionLabel;
    private float actionCompleteTimer;

    private float harvestDisplayTimer;
    private string plotPlantName;
    private float harvestQualityValue;

    private Renderer plotTexture;

    private AudioManager sfxAudio;

    private bool introRunning;

    private bool hasDaylightVFX;
    private bool autoHarvestOnce;
    private bool autoUprootOnce;
    private bool forceReFruit;
    private bool autoPlantOnce;

    const float CURSORPULSEDURATION = 0.618f;
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
    const float GRAFTWINDOW = 3f;
    //
    const float ACTIONHOLDSTEPINTERVAL = .25f;
    const float ACTIONPROGRESSDRAINRATE = 0.1f;
    const float ACTIONDRAINDELAYDURATION = 1f;
    // 
    const float ACTIONCOMPLETEDURATION = 0.618f;
    const float HARVESTDISPLAYDURATION = 2f;
    const float UPROOTEDPLOTPAUSE = 1.5f; // disallow dropped items after uprooted

    const float MASTERPLANTPROGRESSRATE = 0.000618f;


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
        GameObject ground = gameObject.transform.Find("Ground").gameObject;
        if ( ground == null )
        {
            Debug.LogError("--- PlotManager [Start] : no ground child found. aborting.");
            enabled = false;
        }
        else
            plotTexture = ground.GetComponent<Renderer>();
        if ( plotTexture == null )
        {
            Debug.LogError("--- PlotManager [Start] : no ground child renderer found. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // inititalize
        if (enabled)
        {
            cursor.enabled = false;
            plotTimer = 0.1f;
            // wild plots change grasses texture
            GameObject grasses = gameObject.transform.Find("Plot Wild Grasses").gameObject;
            if (grasses != null)
            {
                Renderer r = grasses.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    int rand = Mathf.RoundToInt(RandomSystem.FlatRandom01() * 3f);
                    rand = Mathf.Clamp(rand, 1, 3);
                    r.material.mainTexture = (Texture2D)Resources.Load("Plot_Wild"+rand);
                    if (RandomSystem.FlatRandom01() < .5f)
                        r.material.SetTextureScale("_MainTex", new Vector2(-1, 1));
                }
            }
        }
    }

    public void LaunchTheGreatHarvest()
    {
        autoHarvestOnce = true;
    }

    public void LaunchTheReaper()
    {
        autoUprootOnce = true;
    }

    public void ForceReFruit( bool reFruit )
    {
        forceReFruit = reFruit;
    }

    void Update()
    {
        if (autoHarvestOnce)
            HarvestPlant();
        if (autoUprootOnce)
            UprootPlot();
        if (autoPlantOnce)
            WorkLand();

        if (!cursorActive)
        {
            actionProgress = 0f;
            ActionClear();
        }

        CheckCursor();

        // run action timers
        if (actionProgress > 0f)
        {
            if (actionDrainTimer > 0f)
            {
                actionDrainTimer -= Time.deltaTime;
                if (actionDrainTimer < 0f)
                    actionDrainTimer = 0f;
            }
            else
            {
                actionProgress -= ACTIONPROGRESSDRAINRATE * Time.deltaTime;
                if (actionProgress < 0f)
                {
                    // clear action
                    actionClear = true;
                    actionProgress = 0f;
                    actionLimit = 5f;
                    action = CurrentAction.None;
                    // disable action display
                    actionCompleteTimer = 0f;
                    actionLabel = "";
                    actionProgressDisplay = false;
                }
            }
        }

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
                data.water = Mathf.Clamp01(data.water - (WATERDRAINRATE * 0.0027f * PLOTCHECKINTERVAL));
                if (plant != null)
                {
                    // set plant name
                    plotPlantName = data.plant.plantName;
                    // water drain faster if plant, based on plant growth
                    if (plant != null)
                        data.water = Mathf.Clamp01(data.water - (data.plant.growth * WATERDRAINWITHPLANTRATE * MASTERPLANTPROGRESSRATE * PLOTCHECKINTERVAL));
                    // soil degrade, if plant exists
                    if (plant != null)
                        data.soil = Mathf.Clamp01(data.soil - (data.plant.growth * data.plant.vitality * SOILDEGRADERATE * MASTERPLANTPROGRESSRATE * PLOTCHECKINTERVAL));
                }
                // PLOT EFFECTS:
                if (FarmSystem.PlotHasEffect(data, PlotEffect.DaylightI))
                    data.sun = 1f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.SummonWaterI) ||
                    FarmSystem.PlotHasEffect(data, PlotEffect.SummonWaterII))
                    data.water = 1f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.EclipseI))
                    data.sun = Mathf.Clamp( (data.sun * 0.5f), 0f, 0.5f );
                if (FarmSystem.PlotHasEffect(data, PlotEffect.DaylightII))
                    data.sun = 1f; // daylight II overrides eclipse

                // update season values
                seasonValues.x = tim.GetAmountOfSeason(WorldSeason.Spring);
                seasonValues.y = tim.GetAmountOfSeason(WorldSeason.Summer);
                seasonValues.z = tim.GetAmountOfSeason(WorldSeason.Fall);
                seasonValues.w = tim.GetAmountOfSeason(WorldSeason.Winter);

                // SPELL DAYLIGHT I  & II
                if (!hasDaylightVFX && 
                    (FarmSystem.PlotHasEffect(data, PlotEffect.DaylightI) ||
                    FarmSystem.PlotHasEffect(data, PlotEffect.DaylightII)))
                {
                    GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Daylight"));
                    vfx.name = "Daylight VFX";
                    vfx.transform.position = gameObject.transform.position;
                    vfx.transform.parent = gameObject.transform;
                    hasDaylightVFX = true;
                }
                else if (hasDaylightVFX &&
                    (!FarmSystem.PlotHasEffect(data, PlotEffect.DaylightI) &&
                    !FarmSystem.PlotHasEffect(data, PlotEffect.DaylightII)))
                {
                    Destroy(gameObject.transform.Find("Daylight VFX"));
                    hasDaylightVFX = false;
                }
            }
        }

        // update plot color
        UpdatePlotColor();

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
                // reset action progress for next plot action
                actionProgress = 0f;
            }
        }

        // run harvest display timer
        if (harvestDisplayTimer > 0f)
        {
            harvestDisplayTimer -= Time.deltaTime;
            if (harvestDisplayTimer < 0f)
                harvestDisplayTimer = 0f;
        }

        // run uprooted timer
        if (uprootedTimer > 0f)
        {
            uprootedTimer -= Time.deltaTime;
            if (uprootedTimer < 0f)
                uprootedTimer = 0f;
        }
    }

    public void IntroRunning( bool runningNow )
    {
        introRunning = runningNow;
    }

    /// <summary>
    /// Progresses all plot properties by given time passage amount
    /// </summary>
    /// <param name="daysAhead">game days that have passed</param>
    /// <param name="timeOfDayStart">0-1 time of day value at beginning</param>
    public void FastForwardPlot( float daysAhead, float timeOfDayStart )
    {
        // daysAhead * (60 * 24) = game minutes (real time seconds)
        float gameMinutes = daysAhead * (60f * 24f);
        // NOTE: delta time is used as time rate once every plot check interval (1s)
        // _approximate_ with cycles representing one minute
        // REVIEW: consider how cast effects (like summon water) are handled, interact?

        //print("plot fast forward called - days ahead " + daysAhead + " ("+gameMinutes+" min) , time of day start " + timeOfDayStart);

        // NOTE: must start at current time (offset)
        float dayCycle = timeOfDayStart;
        float timeRate = MASTERPLANTPROGRESSRATE;
        for (int i = 0; i < gameMinutes; i++)
        {
            // approximate minute cycles
            dayCycle += timeRate;

            // sun to be calculated (sine of dayprogress, clamp 01, offset +.25f, *2*PI)
            data.sun = Mathf.Clamp01(Mathf.Sin((dayCycle - .25f) * 2f * Mathf.PI));

            //print("min " + i + " day cycle " + dayCycle + " sun = "+data.sun);

            // plant growth to be calculated (with resources * rate * time)
            if (data.plant.type != PlantType.Default)
            {
                float sunValue = Mathf.Clamp01(data.sun * 4f);
                if (data.plant.isDarkPlant)
                    sunValue = 1f - sunValue;
                // TODO: PLANT EFFECTS: DayNightPlant
                float resources = (sunValue + data.water + data.soil) / 3f;
                float vitalityDelta = (0.667f - resources) * -0.1f;
                // TODO: properly update seasonal values minute-by-minute
                vitalityDelta *= GetPlantSeasonalVitality();
                data.plant.vitality = Mathf.Clamp01(data.plant.vitality + vitalityDelta);
                float healthDelta = (0.5f - data.plant.vitality) + (0.5f - resources);
                healthDelta *= -0.001f;
                data.plant.health = Mathf.Clamp01(0.01f + data.plant.health + healthDelta);
                float growthDelta = resources * 0.2f * 0.1f * data.plant.vitality * data.plant.growthRate * data.plant.adjustedGrowthRate;
                if (data.plant.growth < 1f)
                {
                    data.plant.growth = Mathf.Clamp01(data.plant.growth + growthDelta);
                    // calculate quality
                    data.plant.quality += growthDelta * data.plant.vitality;
                }
                // soil and water drains added (rate * growth * time) (if plant)
                if (plant != null)
                {
                    data.water = Mathf.Clamp01(data.water - (data.plant.growth * WATERDRAINWITHPLANTRATE * timeRate));
                    data.soil = Mathf.Clamp01(data.soil - (data.plant.growth * data.plant.vitality * SOILDEGRADERATE * timeRate));
                }
            }
            // water drains to be calculated (rate * time)
            data.water = Mathf.Clamp01(data.water - (WATERDRAINRATE * timeRate));
        }
        if (plant != null)
        {
            // plant image set
            plant.GetComponent<PlantManager>().ForceGrowthImage(data.plant);
        }
    }

    public float GetPlantSeasonalVitality()
    {
        float retFloat = 0f;

        // take plant data, and profile of seasonal vitalities
        // use current season values and evaluate to arrive at
        // this plant's current seasonal vitality
        // (will be a multiplier to plant's vitality delta value)
        float sp = seasonValues.x * data.plant.springVitality;
        float sm = seasonValues.y * data.plant.summerVitality;
        float fl = seasonValues.z * data.plant.fallVitality;
        float wn = seasonValues.w * data.plant.winterVitality;
        retFloat = sp + sm + fl + wn;

        return retFloat;
    }

    /// <summary>
    /// Can this plot accept a plant or stalk drop into the uprooted hole?
    /// </summary>
    /// <returns>true if plot can accept, false if pause timer still running</returns>
    public bool CanAcceptPlantDrop()
    {
        return (uprootedTimer == 0f);
    }

    /// <summary>
    /// Called from item spawn manager upon detection of valid drop award conditions
    /// </summary>
    /// <param name="dropAward">xp amount</param>
    public void DropAwardXP( int dropAward )
    {
        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        pcm.AwardXP(dropAward);
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

    void UpdatePlotColor()
    {
        float hue = 1f;
        float sat = 1f;
        float val = 1f;
        // brown hue
        hue = (38.1f/255f);
        // soil level correlated with higher color saturation
        sat = data.soil;
        // water level correlated with lower color value
        val = 1f - ((0.618f-0.381f) * data.water) - (0.381f * data.soil);
        plotTexture.material.color = Color.HSVToRGB(hue, sat, val);
    }

    void UpdatePlotHazards()
    {
        // PLOT EFFECTS:
        if (FarmSystem.PlotHasEffect(data, PlotEffect.BlessI) || 
            FarmSystem.PlotHasEffect(data, PlotEffect.BlessII))
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
        if (cursor == null)
            return;
        if (!cursorActive && active)
            actionProgress = 0f; // begin new plot active with zero progress
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
        if (cursor == null)
            return;
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
        //actionProgress = 0f; // allowing a tap to reserve progress (cleared if plot inactive)
        actionLimit = 5f;
        action = CurrentAction.None;

        actionStepTimer = 0f; // not holding and waiting for next progress step, allow next tap
    }

    bool ActionComplete( float limit, string label )
    {
        bool retBool = false;

        if (action != previous)
            actionProgress = 0f;

        if (actionCompleteTimer > 0f)
            return retBool;

        actionProgressDisplay = true;
        actionLimit = limit;
        actionLabel = label;

        // press to step forward progress and hold to wait to progress step
        previous = action;
        // run step timer
        if (actionStepTimer > 0f)
        {
            actionStepTimer -= Time.deltaTime;
            return retBool;
        }
        // progress step
        actionProgressStep = 1f / (actionLimit / ACTIONHOLDSTEPINTERVAL);
        actionProgress += actionProgressStep;
        // SPELL LIGHT WORK
        if (PlayerSystem.PlayerHasEffect(currentPlayer.playerData, PlayerEffect.SpellLightWork))
            actionProgress += actionProgressStep; // x2 work rate
        // ARCANA SKILL : Focus Flow
        if (PlayerSystem.PlayerHasEffect(currentPlayer.playerData, PlayerEffect.SkillFocusFlow))
        {
            if (currentPlayer.IsPlayerFocusFlowing(action))
                actionProgress = Mathf.Clamp01(actionProgress + actionProgressStep); // x2 work rate
        }
        actionStepTimer = ACTIONHOLDSTEPINTERVAL;
        actionDrainTimer = ACTIONDRAINDELAYDURATION;
        // detect completion
        if (actionProgress >= 1f)
        {
            actionProgress = 0f;
            actionCompleteTimer = ACTIONCOMPLETEDURATION;
            actionDirty = true;
            actionClear = false;
            action = CurrentAction.None;
            //
            actionStepTimer = 0f;
            actionDrainTimer = 0f;
            //
            // ARCANA SKILL : Focus Flow
            currentPlayer.FocusFlowActionComplete(action); // sets focus flow timer
            retBool = true;
        }

        return retBool;
    }

    /// <summary>
    /// Works the land, turning from wild to dirt to tilled
    /// </summary>
    public void WorkLand()
    {
        if (!autoPlantOnce && actionDirty)
            return;

        if (!autoPlantOnce && actionCompleteTimer > 0f && !actionClear)
            return;

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        ItemData iData = pcm.GetPlayerCurrentItemSelection();

        if (data.condition == PlotCondition.Tilled)
        {
            // ARCANA SKILL : Cover Crop
            if (autoPlantOnce)
                action = CurrentAction.Planting;

            if (action == CurrentAction.Planting && (iData == null || 
                iData.type != ItemType.Seed))
            {
                actionLabel = "Need Seed Selected";
                actionProgress = 0f;
                return;
            }

            if (action != CurrentAction.Planting && action != CurrentAction.None)
                return;
            action = CurrentAction.Planting;

            if (!autoPlantOnce && !ActionComplete(PLANTWINDOW, "PLANTING..."))
                return;

            // PLAYER STATS:
            currentPlayer.playerData.stats.totalPlanted++;

            pcm.AwardXP(PlayerData.XP_PLANTASEED);
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

            pcm.AwardXP(PlayerData.XP_WORKTHEPLOT);
        }

        if (autoPlantOnce)
            autoPlantOnce = false; // just once

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
                if (plotTexture == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    plotTexture.material.mainTexture = (Texture2D)Resources.Load("Plot_Dirt");
                data.condition = PlotCondition.Dirt;
                // soil quality improved
                data.soil = Mathf.Clamp01(data.soil + (0.25f * RandomSystem.GaussianRandom01()));
                break;
            case PlotCondition.Dirt:
                // change ground texture
                if (plotTexture == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    plotTexture.material.mainTexture = (Texture2D)Resources.Load("Plot_Tilled");
                data.condition = PlotCondition.Tilled;
                // soil quality improved
                data.soil = Mathf.Clamp01(data.soil + (0.25f * RandomSystem.GaussianRandom01()));
                break;
            case PlotCondition.Tilled:
                // skip if no seed
                if (iData == null || iData.type != ItemType.Seed)
                    return;
                // plant seed with seed item selected in player inventory
                plant = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                plant.transform.parent = transform;
                plant.transform.position = transform.position;
                // insert distinct plant data from player inventory current selection
                data.plant = PlantSystem.InitializePlant(iData.plant);
                // configure plant from seed to size = 0f (growth) and quality = 0f
                data.plant.growth = 0f;
                data.plant.quality = 0f;
                // ARCANA SKILL : Dark Biomage
                if (PlantSystem.IsDarkPlant(data.plant.type) && 
                    PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillDarkBiomage))
                    data.plant = PlantSystem.AddPlantEffect(data.plant, PlantEffect.DarkBiomagePlanted);
                // ARCANA SKILL : Plant Doctor
                if (PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillPlantDoctor))
                    data.plant = PlantSystem.AddPlantEffect(data.plant, PlantEffect.PlantDoctorPlanted);
                // using data, remove from player inventory
                pcm.DeleteCurrentItemSelection();
                data.condition = PlotCondition.Growing;
                // PLOT EFFECTS:
                if (FarmSystem.PlotHasEffect(data, PlotEffect.FastGrowI))
                    data.plant.adjustedGrowthRate += 0.33f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.FastGrowII))
                    data.plant.adjustedGrowthRate += 0.67f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.MalnutritionI))
                    data.plant.adjustedGrowthRate -= 0.33f;
                if (FarmSystem.PlotHasEffect(data, PlotEffect.MalnutritionII))
                    data.plant.adjustedGrowthRate -= 0.67f;
                // PLANT EFFECTS:
                if (data.plant.type == PlantType.WalkingStick)
                {
                    data.plant.plantEffects = new PlantEffect[1];
                    data.plant.plantEffects[0] = PlantEffect.WalkingStickScrollHarvest;
                }
                break;
            case PlotCondition.Uprooted:
                // change ground texture
                if (plotTexture == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    plotTexture.material.mainTexture = (Texture2D)Resources.Load("Plot_Dirt");
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

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        pcm.AwardXP(PlayerData.XP_WATERTHEPLOT);
        // sfx
        if (sfxAudio != null)
            sfxAudio.StartSound("Player Watering");

        data.water = 1f;
    }

    public void HarvestPlant()
    {
        if (!autoHarvestOnce && actionDirty)
            return;

        if (!autoHarvestOnce && actionCompleteTimer > 0f && !actionClear)
            return;

        // cannot harvest unless a plant exists, plant at 100% growth and not yet harvested
        if (plant == null || data.plant.growth < 1f || (data.plant.isHarvested && !data.plant.canReFruit))
            return;

        if (!autoHarvestOnce && action != CurrentAction.Harvesting && action != CurrentAction.None)
            return;

        action = CurrentAction.Harvesting;

        if (!autoHarvestOnce && !ActionComplete(HARVESTWINDOW,"HARVESTING..."))
            return;

        if ( data.condition == PlotCondition.Growing )
        {
            if (autoHarvestOnce)
                autoHarvestOnce = false; // just once

            // harvest if plant is 100% grown and not yet harvested
            if (data.plant.growth < 1f)
                return;
            if (!data.plant.isHarvested || data.plant.canReFruit)
            {
                // PLAYER STATS:
                if (currentPlayer != null)
                    currentPlayer.playerData.stats.totalHarvested++;

                PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
                pcm.AwardXP(PlayerData.XP_HARVESTPLANT);

                data.plant.isHarvested = true;
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
                    if (FarmSystem.PlotHasEffect(data, PlotEffect.ProsperousI))
                        harvestNumber *= 2;
                    if (FarmSystem.PlotHasEffect(data, PlotEffect.ProsperousII))
                        harvestNumber *= 3;
                    // iterate to harvest multiple
                    for ( int i = 0; i < harvestNumber; i++ )
                    {
                        // ARCANA SKILL : Midas Biomancer
                        bool dropGold = (currentPlayer != null && PlayerSystem.PlayerHasEffect(currentPlayer.playerData, PlayerEffect.SkillMidasBiomancer) &&
                            !PlantSystem.PlantHasEffect(data.plant, PlantEffect.WalkingStickScrollHarvest) &&
                            RandomSystem.FlatRandom01() < 0.05f);

                        // PLOT EFFECTS:
                        if (FarmSystem.PlotHasEffect(data, PlotEffect.LesionI))
                            data.plant.quality -= 0.5f;
                        if (FarmSystem.PlotHasEffect(data, PlotEffect.GoldenThumbI))
                            data.plant.quality += 0.5f;
                        // check if empty inventory slot availbale on player, drop loose if not
                        if (!dropGold && currentPlayer != null && InventorySystem.InvHasSlot(currentPlayer.playerData.inventory))
                        {
                            // TODO: unify the item data creation routine
                            // harvesting gives fruit
                            ItemData iData = InventorySystem.InitializeItem(ItemType.Fruit);
                            // (unless plant has a special effect)
                            if (data.plant.plantEffects.Length == 0 || data.plant.plantEffects[0] != PlantEffect.WalkingStickScrollHarvest)
                            {
                                if (iData == null)
                                    Debug.LogWarning("--- PlotManager [HarvestPlant] : unable to initialize fruit item. will ignore.");
                                else
                                {
                                    iData.plant = data.plant.type;
                                    iData.name += " (" + data.plant.plantName.ToString() + ")";
                                    // transfer properties of fruit to item (revise item data)
                                    iData.size = data.plant.growth;
                                    iData.quality = data.plant.quality;
                                }
                            }
                            else
                            {
                                iData = InventorySystem.InitializeItem(ItemType.Scroll);
                                if (iData == null)
                                    Debug.LogWarning("--- PlotManager [HarvestPlant] : unable to initialize scroll item. will ignore.");
                                else
                                {
                                    iData.plant = PlantType.Default;
                                    iData.name += " (Unknown)";
                                    iData.effects = new ItemEffect[1];
                                    iData.effects[0] = ItemEffect.ScrollRandomSpellCharge;
                                }
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
                            // (unless player or plant has a special effect)
                            if (!dropGold && (data.plant.plantEffects.Length == 0 || data.plant.plantEffects[0] != PlantEffect.WalkingStickScrollHarvest))
                            { 
                                // transfer properties of fruit to item (revise item data)
                                loose.inv.items[0] = InventorySystem.SetItemAsPlant(loose.inv.items[0], data.plant);
                                loose.inv.items[0].size = data.plant.growth;
                                loose.inv.items[0].quality = data.plant.quality;
                                ism.SpawnItem(loose, gameObject.transform.position, target, true);
                            }
                            else if (!dropGold)
                            {
                                loose = InventorySystem.CreateItem(ItemType.Scroll);
                                if (loose == null)
                                    Debug.LogWarning("--- PlotManager [HarvestPlant] : unable to initialize scroll item. will ignore.");
                                else
                                {
                                    // configure special scroll harvest
                                    loose.inv.items[0].name += " (Unknown)";
                                    loose.inv.items[0].effects = new ItemEffect[1];
                                    loose.inv.items[0].effects[0] = ItemEffect.ScrollRandomSpellCharge;
                                    ism.SpawnItem(loose, gameObject.transform.position, target, true);
                                }
                            }
                            else
                            {
                                // ARCANA SKILL : Midas Biomancer
                                // collect gold sack filled with x2 market value of total fruit harvested
                                int marketValue = 0;
                                MarketManager mm = GameObject.FindFirstObjectByType<MarketManager>();
                                if (mm != null)
                                    marketValue = mm.GetFinalMarketSellValue(loose.inv.items[0]);
                                marketValue *= 2;
                                loose.inv.items[0].type = ItemType.GoldSack;
                                loose.inv.items[0].size = 1f;
                                loose.inv.items[0].quality = 1f;
                                for (int n = 0; n < marketValue; n++)
                                {
                                    loose.inv = InventorySystem.AddToInventory(loose.inv, InventorySystem.InitializeItem(ItemType.GoldCoin));
                                }
                                ism.SpawnItem(loose, gameObject.transform.position, target, true);
                                dropGold = false; // reset
                            }
                        }
                    }
                    // harvesting may drop seed
                    if (FarmSystem.PlotHasEffect(data, PlotEffect.SeedingEcho) || RandomSystem.FlatRandom01() < data.plant.seedPotential)
                    {
                        // PLOT EFFECTS: (complete after just one harvest)
                        if (FarmSystem.PlotHasEffect(data, PlotEffect.SeedingEcho))
                            FarmSystem.RemovePlotEffect(data, PlotEffect.SeedingEcho);
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
                                ism.SpawnItem(looseSeed, gameObject.transform.position, target, true);
                            }
                        }
                    }
                    // sfx
                    if (sfxAudio != null)
                        sfxAudio.StartSound("Player Harvest");
                }
                // harvest results display
                harvestDisplayTimer = HARVESTDISPLAYDURATION;
                harvestQualityValue = data.plant.quality;
                // plants that can re-fruit reset growth to 20%
                if (forceReFruit || data.plant.canReFruit)
                    data.plant.growth = .2f;
                // PLOT EFFECT: BLESSED SPRING
                plant.GetComponent<PlantManager>().SetForceReFruit(forceReFruit);
                // set proper plant art
                plant.GetComponent<PlantManager>().ForceGrowthImage(data.plant);
                // ARCANA SKILL : Cover Crop
                if (PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillCoverCrop))
                {
                    // if player has seed selected, if plant being harvested is not re-fruiting
                    if (!data.plant.canReFruit && !forceReFruit)
                    {
                        ItemData seed = pcm.GetPlayerCurrentItemSelection();
                        if (seed != null && seed.type == ItemType.Seed)
                        {
                            // uproot automatically
                            autoUprootOnce = true;
                            // plant seed automatically
                            autoPlantOnce = true;
                            // immediately uproot and plant
                            UprootPlot();
                        }
                    }
                }
            }
        }
    }

    public void UprootPlot()
    {
        if (!autoUprootOnce && actionDirty)
            return;

        if (!autoUprootOnce && actionCompleteTimer > 0f && !actionClear)
            return;

        if (!autoUprootOnce && action != CurrentAction.Uprooting && action != CurrentAction.None)
            return;

        action = CurrentAction.Uprooting;

        if (!autoUprootOnce && !ActionComplete(UPROOTWINDOW, "DIGGING..."))
            return;

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        pcm.AwardXP(PlayerData.XP_DIGAHOLE);

        // remove any wild grasses
        if (gameObject.transform.Find("Plot Wild Grasses") != null)
            Destroy(gameObject.transform.Find("Plot Wild Grasses").gameObject);
        // change ground texture
        if (plotTexture == null)
            Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
        else
            plotTexture.material.mainTexture = (Texture2D)Resources.Load("Plot_Uprooted");
        // player would collect stalk or full plant as inventory at this point if growth >50%
        // stalk if harvested, plant if not (data retains both isHarvested and growth)
        if (data.plant.growth > 0.5f || (data.plant.isHarvested && data.plant.canReFruit) )
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
                    if (data.plant.isHarvested && !data.plant.canReFruit)
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
                if (data.plant.isHarvested && !data.plant.canReFruit)
                {
                    loose.inv.items[0].type = ItemType.Stalk;
                    loose.inv.items[0].name = "Stalk (" + data.plant.plantName + ")";
                }
                ism.SpawnItem(loose, gameObject.transform.position, target, true);
            }
            // sfx
            if (sfxAudio != null)
                sfxAudio.StartSound("Player Uproot");
        }
        else
        {
            // sfx
            sfxAudio.StartSound("Player Dig Hole");
        }

        // remove plant
        Destroy(plant);
        data.plant = PlantSystem.InitializePlant(PlantType.Default);
        data.condition = PlotCondition.Uprooted;
        plotPlantName = "";
        uprootedTimer = UPROOTEDPLOTPAUSE; // disallow plant drop in hole for a short time
        if (autoUprootOnce)
        {
            autoUprootOnce = false; // just once
            // ARCANA SKILL : Cover Crop
            if (autoPlantOnce)
            {
                // set condition of plot to tilled
                plotTexture.material.mainTexture = (Texture2D)Resources.Load("Plot_Tilled");
                data.condition = PlotCondition.Tilled;
                // immediately plant selected seed
                WorkLand();
            }
        }
    }

    public void GraftPlant()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        if (plant == null || (data.plant.growth < 1f && !data.plant.canReFruit) || !data.plant.isHarvested)
            return;

        if (action != CurrentAction.Grafting && action != CurrentAction.None)
            return;

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        ItemData iData = pcm.GetPlayerCurrentItemSelection();

        if (pcm.playerData.level < 2)
        {
            actionLabel = "Plant Grafting LOCKED";
            actionProgress = 0f;
            return;
        }
        else if (iData == null || iData.type != ItemType.Fruit)
        {
            actionLabel = "Need Fruit Selected";
            actionProgress = 0f;
            return;
        }
        else if (iData.plant == data.plant.type)
        {
            actionLabel = "Need Different Fruit";
            actionProgress = 0f;
            return;
        }
        action = CurrentAction.Grafting;

        if (!ActionComplete(GRAFTWINDOW, "GRAFTING..."))
            return;

        // graft result is either null (plant graft failed, drop fruit)
        // or graft result is whole plant with plant type changed (un-harvested)

        PlantData stalk = data.plant;
        PlantData fruit = PlantSystem.InitializePlant(iData.plant);
        fruit.type = iData.plant;
        fruit.growth = iData.size;
        fruit.health = iData.health;
        fruit.quality = iData.quality;
        PlantData newPlant = PlantSystem.GetGraftResult(stalk, fruit);
        if (newPlant == stalk)
        {
            // result failed, drop fruit
            ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
            // drop as loose item
            Vector3 target = gameObject.transform.position;
            target.x += RandomSystem.GaussianRandom01() - .5f;
            target.z -= 0.01f; // in front of plant
            LooseItemData loose = InventorySystem.CreateItem(ItemType.Fruit);
            // transfer properties of plant to item (revise item data)
            loose.inv.items[0] = InventorySystem.SetItemAsPlant(loose.inv.items[0], fruit);
            ism.SpawnItem(loose, gameObject.transform.position, target, true);
            // using data, remove from player inventory
            pcm.DeleteCurrentItemSelection();
        }
        else
        {
            // graft succeeded, delete fruit from player inventory, change stalk
            data.plant = newPlant;
            data.plant.growth = .4f; // show 'stalk' growth upon successful graft
            plotPlantName = newPlant.plantName;
            plant.GetComponent<PlantManager>().ForceGrowthImage(data.plant);
            // using data, remove from player inventory
            pcm.DeleteCurrentItemSelection();
            //
            pcm.AwardXP(PlayerData.XP_GRAFTPLANT);
        }
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

            r.x = (disp.x - 0.2f) * w;
            r.y = (disp.y - 0.05f) * h;
            r.y -= (0.255f + (progress * 0.025f)) * h;
            r.width = 0.4f * w;
            r.height = 0.15f * h;

            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            s = "Harvested "+plotPlantName+"\n";
            s += "Quality: " + (harvestQualityValue * 100f).ToString("00.0") + "%";
            c = Color.yellow;
            c.a = fade;
            GUI.color = c;
            GUI.depth = -3;

            GUI.Label(r, s, g);
        }

        if (currentPlayer == null || (currentPlayer.hidePlayerHUD && !introRunning))
            return;

        if (harvestDisplayTimer == 0f && !actionProgressDisplay)
        {
            r.x = (disp.x - 0.05f) * w;
            r.y = disp.y * h;
            if (plant == null)
                r.y -= 0.3f * h;
            else
                r.y -= 0.35f * h;
            r.width = 0.1f * w;
            if (plant == null)
                r.height = 0.105f * h;
            else
                r.height = 0.2f * h;

            // display stats background
            c = Color.gray;
            c.a = .8f;
            GUI.color = c;
            GUI.depth = 2;

            GUI.DrawTexture(r, t);

            // plot stats display
            r.x += 0.01f * w;
            if (plant == null)
                r.y -= 0.03625f * h;
            else
                r.y -= 0.075f * h;
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

            if (plant != null)
            {
                //g.alignment = TextAnchor.MiddleCenter;
                s = plotPlantName;
                GUI.Label(r, s, g);
                r.y += 0.025f * h;

                //g.alignment = TextAnchor.MiddleLeft;
            }

            if (plant != null && data.plant != null &&
                !data.plant.canReFruit && data.plant.isHarvested &&
                !FarmSystem.PlotHasEffect(data, PlotEffect.BlessedSpring))
                s = "is Harvested";
            else
                s = data.condition.ToString();
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
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
                r.y -= 0.075f * h;
            else
                r.y -= 0.15f * h;
            GUI.color = Color.white;
            r.x -= 0.001f * w;
            r.y -= 0.002f * h;

            if (plant != null)
            {
                //g.alignment = TextAnchor.MiddleCenter;
                s = plotPlantName;
                GUI.Label(r, s, g);
                r.y += 0.025f * h;

                //g.alignment = TextAnchor.MiddleLeft;
            }

            if (plant != null && data.plant != null &&
                !data.plant.canReFruit && data.plant.isHarvested &&
                !FarmSystem.PlotHasEffect(data, PlotEffect.BlessedSpring))
                s = "is Harvested";
            else
                s = data.condition.ToString();
            GUI.Label(r, s, g);

            r.y += 0.025f * h;
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

        if (harvestDisplayTimer > 0f || !actionProgressDisplay)
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
        if (actionCompleteTimer == 0f)
            r.width = actionProgress * r.width;

        GUI.DrawTexture(r, t);
    }
}
