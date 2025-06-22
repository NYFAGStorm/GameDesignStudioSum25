using UnityEngine;

public class CastManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles individual spell charges that have been cast into the world

    public CastData[] casts = new CastData[0];

    private int birthNewCast; // apply effect for one cast at birth
    private int singleCastToRemove; // remove effect and cast upon expiration

    private SaveLoadManager saveMgr;
    private bool performCastListBirths;


    void Awake()
    {
        saveMgr = GameObject.FindAnyObjectByType<SaveLoadManager>();
        if (saveMgr != null && saveMgr.GetCurrentGameData().casts != null)
            casts = saveMgr.GetCurrentGameData().casts;
        // TIME PASSAGE MECHANICS
        // apply effect of these casts
        // 1. Cast Manager gets cast list (flag set if casts)
        performCastListBirths = (casts.Length > 0);
        // 2. Start() check flag and birth casts
        // TODO: check in with time manager re: time passage
        // TODO: fast forward time wrt cast lifetime
        // TODO: remove casts for those that expired
        // REVIEW: in case casts were removed in time passage,
        //  ... find a way to allow a portion of effects to apply?
        // NOTE: for each system, time passage should be defined
        //  ... how does a unit of game time affect system?
    }

    void OnDestroy()
    {
        if (saveMgr != null)
            saveMgr.GetCurrentGameData().casts = casts;
    }

    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            birthNewCast = -1;
            singleCastToRemove = -1;

            if (performCastListBirths)
            {
                for (int i = 0; i < casts.Length; i++)
                {
                    HandleCastBirth(i);
                }
            }
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

    // TODO: implement effects on plots and plants for the current master list of spells

    // add cast effects to plots (and plants, items, players?)
    void HandleCastBirth( int index )
    {
        SpellType spellType = casts[index].type;
        Vector3 positionEffect = new Vector3(casts[index].posX, casts[index].posY, casts[index].posZ);
        float areaOfEffect = casts[index].rangeAOE;

        // REVIEW: the assumption is that most spells alter plots
        // REVIEW: if that assumption is false,
        //  we need to form lists of affected elements to operate on within switch
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
                    // Plants grow faster for one day. (5%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.FastGrowI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate += 0.05f;
                    break;
                case SpellType.SummonWaterI:
                    // Waters a 2x2 area that stays hydrated for one day.
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.SummonWaterI);
                    break;
                case SpellType.BlessI:
                    // Make plants immune to all hazards for one day.
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.MalnutritionI:
                    // Plants grow speed decreases for 1 day. (10%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.MalnutritionI);
                    break;
                case SpellType.ProsperousI:
                    // Have a chance of harvesting x2 from each plant. (10%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.ProsperousI);
                    break;
                case SpellType.LesionI:
                    // Curse plots and decrease harvest quality. (-5%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.EclipseI:
                    // Obscure sunlight from plots for 1 day.
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.EclipseI);
                    break;
                case SpellType.GoldenThumbI:
                    // Bless plots and increase harvest quality. (10%)
                    plots[i].data = FarmSystem.AddPlotEffect(plots[i].data, PlotEffect.GoldenThumbI);
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
        // REVIEW: if that assumption is false,
        //  we need to form lists of affected elements to operate on within switch
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
                    // Plants grow faster for one day. (5%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.FastGrowI);
                    if (plots[i].plant != null)
                        plots[i].data.plant.adjustedGrowthRate -= 0.05f;
                    break;
                case SpellType.SummonWaterI:
                    // Waters a 2x2 area that stays hydrated for one day.
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.SummonWaterI);
                    break;
                case SpellType.BlessI:
                    // Make plants immune to all hazards for one day.
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.BlessI);
                    break;
                case SpellType.MalnutritionI:
                    // Plants grow speed decreases for 1 day. (10%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.MalnutritionI);
                    break;
                case SpellType.ProsperousI:
                    // Have a chance of harvesting x2 from each plant. (10%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.ProsperousI);
                    break;
                case SpellType.LesionI:
                    // Curse plots and decrease harvest quality. (-5%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.LesionI);
                    break;
                case SpellType.EclipseI:
                    // Obscure sunlight from plots for 1 day.
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.EclipseI);
                    break;
                case SpellType.GoldenThumbI:
                    // Bless plots and increase harvest quality. (10%)
                    plots[i].data = FarmSystem.RemovePlotEffect(plots[i].data, PlotEffect.GoldenThumbI);
                    break;
                default:
                    Debug.LogWarning("--- CastManager [HandleCastExpiration] : spell type effect not found for cast index " + index + ". will ignore.");
                    break;
            }
        }
    }
}
