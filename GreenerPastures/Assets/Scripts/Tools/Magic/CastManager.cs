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

        // REVIEW: the assumption is that most spells alter plots
        //  if not, we need to form lists of affected elements to operate on within switch

        // Spells not affecting plots

        // Structure effects
        // splaturn

        // Island effects
        // starbloom burst, fog of war

        // Player effects
        // mirror mirror, gilded wordsI, color trailI-III, swiftness, light work, gilded wordsII, rabbit hole


        float dist;
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
                case SpellType.MirrorMirror:
                    // Change your appearance
                    break;
                case SpellType.BlessI:
                    // Plots of land immune to hazards for one day
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.DaylightI:
                    // Summon sunlight on a plot for one day
                    break;
                case SpellType.GildedWordsI:
                    // Make yourself charming (-25% off market prices)
                    break;
                case SpellType.SeedingEcho:
                    // Harvest from plants guaranteed to drop seed
                    break;
                case SpellType.ColorTrailI:
                    // Leave a trail of your primary color behind you
                    break;
                case SpellType.ColorTrailII:
                    // Leave a trail of your secondary color behind you
                    break;
                case SpellType.ColorTrailIII:
                    // Leave a trail of your accent color behind you
                    break;
                case SpellType.FastGrowII:
                    // Plants grow faster for two days (+67 %)
                    break;
                case SpellType.MalnutritionI:
                    // Plants grow slower for one day (-33%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.MalnutritionI);
                    break;
                case SpellType.ProsperousI:
                    // Plant harvest yields twice as much
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.ProsperousI);
                    break;
                case SpellType.TheGreatHarvest:
                    // Harvest several plots immediately
                    break;
                case SpellType.Splaturn:
                    // Change the color of your tower
                    break;
                case SpellType.SummonWaterII:
                    // Plots of land stay hydrated for two days
                    break;
                case SpellType.LesionI:
                    // Decrease harvest quality (-50%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.TheReaper:
                    // Dig up several plots, leaving holes and destroying seedlings
                    break;
                case SpellType.Swiftness:
                    // Move faster (150%)
                    break;
                case SpellType.LightWork:
                    // Farm work goes faster (200%)
                    break;
                case SpellType.StarbloomBurst:
                    // Cast continuous fireworks into the sky
                    break;
                case SpellType.SoiledItII:
                    // Instantly maximize soil quality (100%)
                    break;
                case SpellType.GildedWordsII:
                    // Make yourself charming (-50% off market prices)
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
                    break;
                case SpellType.FogOfWar:
                    // Summon cloud over an area
                    break;
                case SpellType.BlessedSpring:
                    // Force multiple plants to re-fruit
                    break;
                case SpellType.MalnutritionII:
                    // Plants grow slower for two days (-67%)
                    break;
                case SpellType.BlessII:
                    // Plots of land immune to hazards for three days
                    break;
                case SpellType.ProsperousII:
                    // Plant harvest yields three times as much
                    break;
                case SpellType.DaylightII:
                    // Summon sunlight on plots for three days
                    break;
                case SpellType.RabbitHole:
                    // Another player is trapped for one day
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

        // REVIEW: the assumption is that most spells alter plots
        //  if not, we need to form lists of affected elements to operate on within switch

        // Spells not affecting plots

        // Island effects
        // starbloom burst, fog of war

        // Player effects
        // gilded wordsI, color trailI-III, swiftness, light work, gilded wordsII, rabbit hole


        float dist;
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
                case SpellType.MirrorMirror:
                    // Change your appearance
                    break;
                case SpellType.BlessI:
                    // Plots of land immune to hazards for one day
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.DaylightI:
                    // Summon sunlight on a plot for one day
                    break;
                case SpellType.GildedWordsI:
                    // Make yourself charming (-25% off market prices)
                    break;
                case SpellType.SeedingEcho:
                    // Harvest from plants guaranteed to drop seed
                    break;
                case SpellType.ColorTrailI:
                    // Leave a trail of your primary color behind you
                    break;
                case SpellType.ColorTrailII:
                    // Leave a trail of your secondary color behind you
                    break;
                case SpellType.ColorTrailIII:
                    // Leave a trail of your accent color behind you
                    break;
                case SpellType.FastGrowII:
                    // Plants grow faster for two days (+67 %)
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
                    break;
                case SpellType.Splaturn:
                    // Change the color of your tower
                    break;
                case SpellType.SummonWaterII:
                    // Plots of land stay hydrated for two days
                    break;
                case SpellType.LesionI:
                    // Decrease harvest quality (-50%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.TheReaper:
                    // Dig up several plots, leaving holes and destroying seedlings
                    break;
                case SpellType.Swiftness:
                    // Move faster (150%)
                    break;
                case SpellType.LightWork:
                    // Farm work goes faster (200%)
                    break;
                case SpellType.StarbloomBurst:
                    // Cast continuous fireworks into the sky
                    break;
                case SpellType.SoiledItII:
                    // Instantly maximize soil quality (100%)
                    break;
                case SpellType.GildedWordsII:
                    // Make yourself charming (-50% off market prices)
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
                case SpellType.FogOfWar:
                    // Summon cloud over an area
                    break;
                case SpellType.BlessedSpring:
                    // Force multiple plants to re-fruit
                    break;
                case SpellType.MalnutritionII:
                    // Plants grow slower for two days (-67%)
                    break;
                case SpellType.BlessII:
                    // Plots of land immune to hazards for three days
                    break;
                case SpellType.ProsperousII:
                    // Plant harvest yields three times as much
                    break;
                case SpellType.DaylightII:
                    // Summon sunlight on plots for three days
                    break;
                case SpellType.RabbitHole:
                    // Another player is trapped for one day
                    break;
                default:
                    Debug.LogWarning("--- CastManager [HandleCastExpiration] : spell type effect not found for cast index " + index + ". will ignore.");
                    break;
            }
        }
    }
}
