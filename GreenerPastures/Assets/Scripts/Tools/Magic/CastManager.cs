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
                        if (dist > (areaOfEffect + islandMgr.islands[i].location.w))
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
                            playerIntro.SetMirrorMirrorActive();
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

    // REVIEW: handle cast effects per frame?
    void UpdateCastEffect( int index )
    {

    }

    // remove cast effects from plots (and plants, items, players?)
    void HandleCastExpiration( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;
        float dist;

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
