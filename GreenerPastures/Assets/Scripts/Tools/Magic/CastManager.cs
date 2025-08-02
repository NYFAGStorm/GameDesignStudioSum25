using UnityEngine;

public class CastManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles individual spell charges that have been cast into the world

    public CastData[] casts = new CastData[0];

    private int birthNewCast; // apply effect for one cast at birth
    private int singleCastToRemove; // remove effect and cast upon expiration


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            birthNewCast = -1;
            singleCastToRemove = -1;
        }
    }

    void Update()
    {
        if (casts == null || casts.Length == 0)
            return;

        singleCastToRemove = -1;

        // run cast lifetimes
        for (int i = 0; i < casts.Length; i++)
        {
            casts[i].lifetime -= Time.deltaTime;
            if (casts[i].lifetime <= 0f)
            {
                casts[i].lifetime = 0f;
                if (singleCastToRemove == -1)
                    singleCastToRemove = i;
            }
        }

        // birth new cast
        if (birthNewCast > -1)
        {
            HandleCastBirth(birthNewCast);
            birthNewCast = -1;
        }

        // remove expired cast
        if (singleCastToRemove > -1)
        {
            HandleCastExpiration(singleCastToRemove);
            RemoveCast(singleCastToRemove);
            singleCastToRemove = -1;
        }

        // update all cast effects in the game world per frame
        for (int i = 0; i < casts.Length; i++)
        {
            UpdateCastEffect(i);
        }
    }

    /// <summary>
    /// Returns the cast data array
    /// </summary>
    /// <returns>cast data array</returns>
    public CastData[] GetCastData()
    {
        return casts;
    }

    /// <summary>
    /// Sets the cast data array
    /// </summary>
    /// <param name="castData">cast data array</param>
    public void SetCastData( CastData[] castData )
    {
        casts = castData;
        // perform all cast births
        for (int i = 0; i < casts.Length; i++)
        {
            HandleCastBirth(i);
        }
    }

    void RemoveCast( int index )
    {
        CastData[] tmp = new CastData[casts.Length-1];
        int count = 0;
        for ( int i = 0; i < casts.Length; i++ )
        {
            if (i != index)
            {
                tmp[count] = casts[i];
                count++;
            }
        }
        casts = tmp;
    }

    /// <summary>
    /// Reduces all cast lifetimes by given time passage amount
    /// </summary>
    /// <param name="daysAhead">game days that have passed</param>
    public void FastForwardCasts( float daysAhead )
    {
        // daysAhead * (60 * 24) = game minutes (real time seconds)
        for (int i = 0; i < casts.Length; i++)
        {
            casts[i].lifetime -= (daysAhead * (60f * 24f));
            // expired casts are handled
        }
        // REVIEW: consider how effects of casts that have expired during this time should be handled
    }

    /// <summary>
    /// Acquires a new spell cast into the world, to be managed until expired
    /// </summary>
    /// <param name="cast">spell cast data</param>
    public void AcquireNewCast( CastData newCast )
    {
        CastData[] tmp = new CastData[casts.Length + 1];
        for (int i = 0; i < casts.Length; i++)
        {
            tmp[i] = casts[i];
        }
        tmp[casts.Length] = newCast;
        birthNewCast = casts.Length; // will catch to intialize cast effects
        casts = tmp;
    }

    // add cast effects to plots (and plants, items, players?)
    void HandleCastBirth( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;
        float dist;

        // Arcana skill 'spells'
        // ARCANA SKILL : Waste Management
        if (spellType == SpellType.SkillWasteManagement)
        {
            float closest = 999f;
            LooseItemManager loosey = null;
            LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
            for (int i = 0; i < looseItems.Length; i++)
            {
                dist = Vector3.Distance(looseItems[i].gameObject.transform.position, positionEffect);
                if (dist < areaOfEffect)
                {
                    if (dist < closest)
                    {
                        closest = dist;
                        loosey = looseItems[i];
                    }
                }
            }
            if (loosey != null)
            {
                // drop potentially rare seed
                ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                if (ism != null)
                {
                    float facing = loosey.gameObject.GetComponent<Renderer>().material.GetTextureScale("_MainTex").x;
                    Vector3 pos = loosey.gameObject.transform.position + (Vector3.down * .25f);
                    Vector3 targ = pos + (facing * Vector3.right);
                    int maybeRarePlantType = GameSystem.RoundedResult(RandomSystem.WeightedRandom01(), 41);
                    PlantType pt = (PlantType)(maybeRarePlantType);
                    PlantData p = PlantSystem.InitializePlant(pt);
                    LooseItemData seed = InventorySystem.CreateItem(ItemType.Seed);
                    seed.inv.items[0] = InventorySystem.SetItemAsPlant(seed.inv.items[0], p);
                    seed.inv.items[0].name = "Seed (" + p.plantName + ")";
                    seed.inv.items[0].plant = pt;
                    ism.SpawnItem(seed, pos, targ, true);
                    Destroy(loosey.gameObject);
                }
            }
        }
        // skill clean up
        if (spellType == SpellType.SkillCleanUp)
        {
            // launch black hole for plant items
        }
        // skill lighten up
        if (spellType == SpellType.SkillLightenUp)
        {
            // find closest local player character, set point light
            // REVIEW: use for remote player as well?
            PlayerControlManager[] players = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                dist = Vector3.Distance(players[i].gameObject.transform.position, positionEffect);
                if (dist > areaOfEffect)
                    continue;

                GameObject light = new GameObject();
                light.name = "VFX player light";
                light.transform.position = players[i].transform.position + Vector3.up;
                light.transform.parent = players[i].transform;
                Light l = light.AddComponent<Light>();
                l.range = 3.81f;
                l.intensity = .618f;
                l.bounceIntensity = 0f;
                l.shadows = LightShadows.Hard;
                Destroy(light, 180f); // 3 in-game hour duration
            }
        }
        // skill cash in
        if (spellType == SpellType.SkillCashIn)
        {
            // launch black hole for market value items
        }
        // skill take me home
        if (spellType == SpellType.SkillTakeMeHome)
        {
            // find player
            PlayerControlManager[] players = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                dist = Vector3.Distance(players[i].gameObject.transform.position, positionEffect);
                if (dist > areaOfEffect)
                    continue;

                // find center of all plots on player farm
                Vector3 centerOfFarm = Vector3.zero;
                for (int n = 0; n < players[i].playerData.farm.plots.Length; n++)
                {
                    centerOfFarm += GameSystem.GetVector(players[i].playerData.farm.plots[n].location);
                }
                centerOfFarm /= players[i].playerData.farm.plots.Length; // average position
                GameObject sfxObj = GameObject.Find("AudioMgr SFX");
                AudioManager sfxAudio = null;
                GameObject tPortSFXObjA = new GameObject();
                GameObject tPortSFXObjB = new GameObject();
                tPortSFXObjA.transform.position = players[i].gameObject.transform.position;
                tPortSFXObjB.transform.position = centerOfFarm;
                Destroy(tPortSFXObjA, 2f);
                Destroy(tPortSFXObjB, 2f);
                if (sfxObj != null)
                    sfxAudio = sfxObj.GetComponent<AudioManager>();
                if (sfxAudio != null)
                {
                    // teleport sfx
                    sfxAudio.StartSound("Teleport", tPortSFXObjA, 1f, 6.18f);
                    sfxAudio.StartSound("Teleport", tPortSFXObjB, 1f, 6.18f);
                }
                // teleport vfx
                GameObject vfxA = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
                GameObject vfxB = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
                vfxA.name = "VFX Teleport Flash";
                vfxB.name = "VFX Teleport Flash";
                vfxA.transform.position = tPortSFXObjA.transform.position;
                vfxB.transform.position = tPortSFXObjB.transform.position;
                Destroy(vfxA, 1.1f);
                Destroy(vfxB, 1.1f);
                // teleport
                // REVIEW: graceful timing
                players[i].transform.position = centerOfFarm;
            }
        }

        // Spells not affecting plots

        // Structure effects
        // splaturn
        // Island effects
        // starbloom burst, fog of war
        if (spellType == SpellType.Splaturn ||
            spellType == SpellType.StarbloomBurst ||
            spellType == SpellType.FogOfWar)
        {
            IslandManager islandMgr = GameObject.FindFirstObjectByType<IslandManager>();
            if (islandMgr != null)
            {
                for (int i = 0; i < islandMgr.islands.Length; i++)
                {
                    if (spellType == SpellType.Splaturn)
                    {
                        for (int n = 0; n < islandMgr.islands[i].structures.Length; n++)
                        {
                            if (islandMgr.islands[i].structures[n].type != StructureType.WizardTower)
                                continue;
                            Vector3 towerPosition = GameSystem.GetVector(islandMgr.islands[i].location);
                            towerPosition += GameSystem.GetVector(islandMgr.islands[i].structures[n].location);
                            dist = Vector3.Distance(towerPosition, positionEffect); // pad AoE a little (structure size)
                            if (dist > areaOfEffect + 3.81f)
                                continue;
                            // do it
                            casts[index].islandIndex = i;
                            islandMgr.InvokeSplaturnSpell(i, n);
                        }
                    }
                    else
                    {
                        Vector3 islandPosition = GameSystem.GetVector(islandMgr.islands[i].location);
                        dist = Vector3.Distance(islandPosition, positionEffect); // also use island range (.w)
                        if (dist > (areaOfEffect + (islandMgr.islands[i].location.w * 7f)))
                            continue;
                        // do it
                        casts[index].islandIndex = i;
                        if (spellType == SpellType.StarbloomBurst)
                            islandMgr.islands[i] = IslandSystem.AddIslandEffect(islandMgr.islands[i], IslandEffect.SpellStarbloomBurst);
                        else if (spellType == SpellType.FogOfWar)
                            islandMgr.islands[i] = IslandSystem.AddIslandEffect(islandMgr.islands[i], IslandEffect.SpellFogOfWar);
                    }
                }
            }
            return;
        }

        // Player effects
        // mirror mirror, gilded wordsI, color trailI-III, swiftness, light work, gilded wordsII
        if (spellType == SpellType.MirrorMirror ||
            spellType == SpellType.GildedWordsI ||
            spellType == SpellType.ColorTrailI ||
            spellType == SpellType.ColorTrailII ||
            spellType == SpellType.ColorTrailIII||
            spellType == SpellType.Swiftness ||
            spellType == SpellType.LightWork ||
            spellType == SpellType.GildedWordsII)
        {
            PlayerControlManager[] players = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
            for (int i = 0; i < players.Length; i++)
            {
                dist = Vector3.Distance(players[i].gameObject.transform.position, positionEffect);
                if (dist > areaOfEffect)
                    continue;
                // do it
                GameData gData = GameObject.FindFirstObjectByType<GreenerGameManager>().game;
                casts[index].profileID = GameSystem.GetProfileIDOfPlayer(gData, players[i].playerData.playerName);
                switch (spellType)
                {
                    case SpellType.MirrorMirror:
                        //players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellMirrorMirror);
                        PlayerIntroduction playerIntro = GameObject.FindFirstObjectByType<PlayerIntroduction>();
                        if (playerIntro != null)
                            playerIntro.SetMirrorMirrorActive(players[i]);
                        break;
                    case SpellType.GildedWordsI:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellGildedWordsI);
                        break;
                    case SpellType.ColorTrailI:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellColorTrailI);
                        break;
                    case SpellType.ColorTrailII:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellColorTrailII);
                        break;
                    case SpellType.ColorTrailIII:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellColorTrailIII);
                        break;
                    case SpellType.Swiftness:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellSwiftness);
                        break;
                    case SpellType.LightWork:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellLightWork);
                        break;
                    case SpellType.GildedWordsII:
                        players[i].playerData = PlayerSystem.AddPlayerEffect(players[i].playerData, PlayerEffect.SpellGildedWordsII);
                        break;
                }
            }
        }
        // rabbit hole
        if (spellType == SpellType.RabbitHole)
        {
            RemotePlayerManager[] rPlayers = GameObject.FindObjectsByType<RemotePlayerManager>(FindObjectsSortMode.None);
            for (int i = 0; i < rPlayers.Length; i++)
            {
                dist = Vector3.Distance(rPlayers[i].gameObject.transform.position, positionEffect);
                if (dist > areaOfEffect)
                    continue;
                // do it
                GameData gData = GameObject.FindFirstObjectByType<GreenerGameManager>().game;
                casts[index].profileID = GameSystem.GetProfileIDOfPlayer(gData, rPlayers[i].playerName);
                for (int n = 0; n < gData.players.Length; n++)
                {
                    if (gData.players[n].profileID == casts[index].profileID)
                    {
                        gData.players[n] = PlayerSystem.AddPlayerEffect(gData.players[n], PlayerEffect.SpellRabbitHole);
                        break;
                    }
                }
            }
        }

        // Plot effects
        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            dist = Vector3.Distance(plots[i].gameObject.transform.position, positionEffect);
            if (dist > areaOfEffect)
                continue;

            switch (spellType)
            {
                case SpellType.Default:
                    // should never be here
                    break;
                case SpellType.FastGrowI:
                    // Plants grow faster for one day (+33%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.FastGrowI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate += 0.33f;
                    break;
                case SpellType.SummonWaterI:
                    // Plot of land stays hydrated for one day
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.SummonWaterI);
                    break;
                case SpellType.SoiledItI:
                    // Instantly increase soil quality (+50%)
                    // no plot effect
                    plots[i].data.soil += 0.5f;
                    plots[i].data.soil = Mathf.Clamp01(plots[i].data.soil);
                    break;
                case SpellType.BlessI:
                    // Plots of land immune to hazards for one day
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.DaylightI:
                    // Summon sunlight on a plot for one day
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.DaylightI);
                    break;
                case SpellType.SeedingEcho:
                    // Harvest from plants guaranteed to drop seed
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.SeedingEcho);
                    break;
                case SpellType.FastGrowII:
                    // Plants grow faster for two days (+67 %)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.FastGrowII);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate += 0.67f;
                    break;
                case SpellType.MalnutritionI:
                    // Plants grow slower for one day (-33%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.MalnutritionI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate -= 0.33f;
                    break;
                case SpellType.ProsperousI:
                    // Plant harvest yields twice as much
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.ProsperousI);
                    break;
                case SpellType.TheGreatHarvest:
                    // Harvest several plots immediately
                    plots[i].LaunchTheGreatHarvest();
                    break;
                case SpellType.SummonWaterII:
                    // Plots of land stay hydrated for two days
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.SummonWaterII);
                    break;
                case SpellType.LesionI:
                    // Decrease harvest quality (-50%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.TheReaper:
                    // Dig up several plots, leaving holes and destroying seedlings
                    plots[i].LaunchTheReaper();
                    break;
                case SpellType.SoiledItII:
                    // Instantly maximize soil quality (100%)
                    plots[i].data.soil = 1f;
                    break;
                case SpellType.EclipseI:
                    // Block sunlight from plots for one day
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.EclipseI);
                    break;
                case SpellType.GoldenThumbI:
                    // Increase harvest quality (+50%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.GoldenThumbI);
                    break;
                case SpellType.DullEarth:
                    // Decrease soil quality of several plots (-50%)
                    plots[i].data.soil = Mathf.Clamp01(plots[i].data.soil - .5f);
                    break;
                case SpellType.BlessedSpring:
                    // Force multiple plants to re-fruit
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.BlessedSpring);
                    plots[i].ForceReFruit(true);
                    break;
                case SpellType.MalnutritionII:
                    // Plants grow slower for two days (-67%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.MalnutritionII);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate -= 0.67f;
                    break;
                case SpellType.BlessII:
                    // Plots of land immune to hazards for three days
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.BlessII);
                    break;
                case SpellType.ProsperousII:
                    // Plant harvest yields three times as much
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.ProsperousII);
                    break;
                case SpellType.DaylightII:
                    // Summon sunlight on plots for three days
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.DaylightII);
                    break;
                default:
                    Debug.LogWarning("--- CastManager [HandleCastBirth] : spell type effect not found for cast index " + index + ". will ignore.");
                    break;
            }
        }
    }

    // handle cast effects per frame
    void UpdateCastEffect( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;
        float dist;

        // Arcana skill 'spells'

        // ARCANA SKILL : Clean Up
        if (spellType == SpellType.SkillCleanUp)
        {
            // (black hole for plant type items)
            LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
            for (int i = 0; i < looseItems.Length; i++)
            {
                if (looseItems[i].looseItem.inv.items[0].plant == PlantType.Default)
                    continue;
                dist = Vector3.Distance(looseItems[i].gameObject.transform.position, positionEffect);
                Vector3 moveVector = Vector3.zero;
                if (dist < areaOfEffect)
                {
                    moveVector = positionEffect - looseItems[i].gameObject.transform.position;
                    moveVector *= 0.381f;
                    looseItems[i].gameObject.transform.position += moveVector;
                }
            }
        }

        // ARCANA SKILL : Cash In
        if (spellType == SpellType.SkillCashIn)
        {
            // (black hole for items with market value)
            LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
            for (int i = 0; i < looseItems.Length; i++)
            {
                if (looseItems[i].looseItem.inv.items[0].plant == PlantType.Default && 
                    (looseItems[i].looseItem.inv.items[0].type != ItemType.Scroll ||
                    looseItems[i].looseItem.inv.items[0].type != ItemType.Potion))
                    continue;
                if (looseItems[i].looseItem.inv.items[0].plant != PlantType.Default &&
                    looseItems[i].looseItem.inv.items[0].type == ItemType.Stalk)
                    continue;
                dist = Vector3.Distance(looseItems[i].gameObject.transform.position, positionEffect);
                Vector3 moveVector = Vector3.zero;
                if (dist < areaOfEffect)
                {
                    moveVector = positionEffect - looseItems[i].gameObject.transform.position;
                    moveVector *= 0.381f;
                    looseItems[i].gameObject.transform.position += moveVector;
                }
            }
        }
    }

    // remove cast effects from plots (and plants, items, players?)
    void HandleCastExpiration( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;
        float dist;

        // Arcana skill 'spells'

        // ARCANA SKILL : Clean Up
        if (spellType == SpellType.SkillCleanUp)
        {
            // find closest compost bin
            CompostManager[] compostBins = GameObject.FindObjectsByType<CompostManager>(FindObjectsSortMode.None);
            float closestDist = 999f;
            CompostManager closeBin = null;
            for (int i = 0; i < compostBins.Length; i++)
            {
                float d = Vector3.Distance(compostBins[i].transform.position, positionEffect);
                if (d < closestDist)
                {
                    closestDist = d;
                    closeBin = compostBins[i];
                }
            }
            // teleport effects
            GameObject sfxObj = GameObject.Find("AudioMgr SFX");
            AudioManager sfxAudio = null;
            GameObject tPortSFXObj = new GameObject();
            tPortSFXObj.transform.position = positionEffect;
            Destroy(tPortSFXObj, 2f);
            if (sfxObj != null)
                sfxAudio = sfxObj.GetComponent<AudioManager>();
            if (sfxAudio != null)
            {
                // teleport sfx
                sfxAudio.StartSound("Teleport", tPortSFXObj, 1f, 6.18f);
            }
            // teleport vfx
            GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
            vfx.name = "VFX Teleport Flash";
            vfx.transform.position = positionEffect;
            Destroy(vfx, 1.1f);
            // teleport all loose items to compost bin
            LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
            for (int i = 0; i < looseItems.Length; i++)
            {
                if (looseItems[i].looseItem.inv.items[0].plant == PlantType.Default)
                    continue;
                dist = Vector3.Distance(looseItems[i].gameObject.transform.position, positionEffect);
                if (dist < 1f) // now at center?
                    looseItems[i].gameObject.transform.position = closeBin.transform.position;
            }
        }

        // ARCANA SKILL : Cash In
        if (spellType == SpellType.SkillCashIn)
        {
            // teleport effects
            GameObject sfxObj = GameObject.Find("AudioMgr SFX");
            AudioManager sfxAudio = null;
            GameObject tPortSFXObj = new GameObject();
            tPortSFXObj.transform.position = positionEffect;
            Destroy(tPortSFXObj, 2f);
            if (sfxObj != null)
                sfxAudio = sfxObj.GetComponent<AudioManager>();
            if (sfxAudio != null)
            {
                // teleport sfx
                sfxAudio.StartSound("Teleport", tPortSFXObj, 1f, 6.18f);
            }
            // teleport vfx
            GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
            vfx.name = "VFX Teleport Flash";
            vfx.transform.position = positionEffect;
            Destroy(vfx, 1.1f);
            // exchange items for gold pouch of sum market value
            int marketValue = 0;
            MarketManager mm = GameObject.FindFirstObjectByType<MarketManager>();
            LooseItemManager[] looseItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
            for (int i = 0; i < looseItems.Length; i++)
            {
                if (looseItems[i].looseItem.inv.items[0].plant == PlantType.Default &&
                    (looseItems[i].looseItem.inv.items[0].type != ItemType.Scroll ||
                    looseItems[i].looseItem.inv.items[0].type != ItemType.Potion))
                    continue;
                if (looseItems[i].looseItem.inv.items[0].plant != PlantType.Default &&
                    looseItems[i].looseItem.inv.items[0].type == ItemType.Stalk)
                    continue;
                dist = Vector3.Distance(looseItems[i].gameObject.transform.position, positionEffect);
                if (dist < 1f) // now at center?
                {
                    if (mm != null)
                        marketValue += mm.GetFinalMarketSellValue(looseItems[i].looseItem.inv.items[0]);
                }
            }
            ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
            if (ism != null)
            {
                // spawn gold pouch, put gold in it, drop at cast position
                LooseItemData goldPouch = InventorySystem.CreateItem(ItemType.GoldSack);
                for (int i = 0; i < marketValue; i++)
                {
                    goldPouch.inv = InventorySystem.AddToInventory(goldPouch.inv, InventorySystem.InitializeItem(ItemType.GoldCoin));
                }
                ism.SpawnItem(goldPouch, positionEffect, positionEffect, true);
            }
        }

        // Spells not affecting plots

        // Island effects
        // starbloom burst, fog of war
        if (spellType == SpellType.StarbloomBurst ||
            spellType == SpellType.FogOfWar)
        {
            // use island index
            IslandManager islandMgr = GameObject.FindFirstObjectByType<IslandManager>();
            if (islandMgr != null)
            {
                if (spellType == SpellType.StarbloomBurst)
                    islandMgr.islands[casts[index].islandIndex] = IslandSystem.RemoveIslandEffect(islandMgr.islands[casts[index].islandIndex], IslandEffect.SpellStarbloomBurst);
                else if (spellType == SpellType.FogOfWar)
                    islandMgr.islands[casts[index].islandIndex] = IslandSystem.RemoveIslandEffect(islandMgr.islands[casts[index].islandIndex], IslandEffect.SpellFogOfWar);
                return;
            }
        }

        // Player effects
        // gilded wordsI, color trailI-III, swiftness, light work, gilded wordsII, rabbit hole
        if (spellType == SpellType.GildedWordsI ||
            spellType == SpellType.ColorTrailI ||
            spellType == SpellType.ColorTrailII ||
            spellType == SpellType.ColorTrailIII ||
            spellType == SpellType.Swiftness ||
            spellType == SpellType.LightWork ||
            spellType == SpellType.GildedWordsII ||
            spellType == SpellType.RabbitHole)
        {
            // use profile ID
            GameData gData = GameObject.FindFirstObjectByType<GreenerGameManager>().game;
            // access player data
            for (int i = 0; i < gData.players.Length; i++)
            {
                if (gData.players[i].profileID == casts[index].profileID)
                {
                    switch (spellType)
                    {
                        case SpellType.GildedWordsI:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellGildedWordsI);
                            break;
                        case SpellType.ColorTrailI:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellColorTrailI);
                            break;
                        case SpellType.ColorTrailII:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellColorTrailII);
                            break;
                        case SpellType.ColorTrailIII:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellColorTrailIII);
                            break;
                        case SpellType.Swiftness:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellSwiftness);
                            break;
                        case SpellType.LightWork:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellLightWork);
                            break;
                        case SpellType.GildedWordsII:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellGildedWordsII);
                            break;
                        case SpellType.RabbitHole:
                            gData.players[i] = PlayerSystem.RemovePlayerEffect(gData.players[i], PlayerEffect.SpellRabbitHole);
                            break;
                    }
                }
            }
            
            return;
        }

        // Plot effects
        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            dist = Vector3.Distance(plots[i].gameObject.transform.position, positionEffect);
            if (dist > areaOfEffect)
                continue;

            switch (spellType)
            {
                case SpellType.Default:
                    // should never be here
                    break;
                case SpellType.FastGrowI:
                    // Plants grow faster for one day (+33%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.FastGrowI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate -= 0.33f;
                    break;
                case SpellType.SummonWaterI:
                    // Plot of land stays hydrated for one day
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.SummonWaterI);
                    break;
                case SpellType.SoiledItI:
                    // Instantly increase soil quality (+50%)
                    // no plot effect
                    break;
                case SpellType.BlessI:
                    // Plots of land immune to hazards for one day
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.DaylightI:
                    // Summon sunlight on a plot for one day
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.DaylightI);
                    break;
                case SpellType.SeedingEcho:
                    // Harvest from plants guaranteed to drop seed
                    // (removed at plot upon harvest)
                    break;
                case SpellType.FastGrowII:
                    // Plants grow faster for two days (+67%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.FastGrowI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate -= 0.67f;
                    break;
                case SpellType.MalnutritionI:
                    // Plants grow slower for one day (-33%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.MalnutritionI);
                    break;
                case SpellType.ProsperousI:
                    // Plant harvest yields twice as much
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.ProsperousI);
                    break;
                case SpellType.TheGreatHarvest:
                    // Harvest several plots immediately
                    // (already done, just once)
                    break;
                case SpellType.SummonWaterII:
                    // Plots of land stay hydrated for two days
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.SummonWaterII);
                    break;
                case SpellType.LesionI:
                    // Decrease harvest quality (-50%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.TheReaper:
                    // Dig up several plots, leaving holes and destroying seedlings
                    // (already done, just once)
                    break;
                case SpellType.SoiledItII:
                    // Instantly maximize soil quality (100%)
                    // no plot effect
                    break;
                case SpellType.EclipseI:
                    // Block sunlight from plots for one day
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.EclipseI);
                    break;
                case SpellType.GoldenThumbI:
                    // Increase harvest quality (+50%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.GoldenThumbI);
                    break;
                case SpellType.DullEarth:
                    // Decrease soil quality of several plots (-50%)
                    break;
                case SpellType.BlessedSpring:
                    // Force multiple plants to re-fruit
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.BlessedSpring);
                    plots[i].ForceReFruit(false);
                    break;
                case SpellType.MalnutritionII:
                    // Plants grow slower for two days (-67%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.MalnutritionII);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate += 0.67f;
                    break;
                case SpellType.BlessII:
                    // Plots of land immune to hazards for three days
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.ProsperousII:
                    // Plant harvest yields three times as much
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.ProsperousII);
                    break;
                case SpellType.DaylightII:
                    // Summon sunlight on plots for three days
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.DaylightII);
                    break;
                default:
                    Debug.LogWarning("--- CastManager [HandleCastExpiration] : spell type effect not found for cast index " + index + ". will ignore.");
                    break;
            }
        }
    }
}
