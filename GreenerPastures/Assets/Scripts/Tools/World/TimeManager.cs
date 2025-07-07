using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world time cycles and related world properties

    // NOTE: our global time progress is the master clock, driven by season progress
    // in data, our game begins from a point in real time, used as a starting point
    // global time progress uses that starting point and game time progress
    // .
    // various elements in our game have timers and durations (cooldowns, etc)
    // to represent valid time markers with our global time progress, we save timestamps
    // data holds 'long' type timestamps of gloabl time progress + scheduled durations
    // when we load data, we will then un-pack these various element timers
    // we will compare the timestamp difference and assign timer and duration values

    public GameObject skyLightObject;
    public GameObject seasonalTiltGimble;
    public Light sunLight;
    public Light moonLight;

    // TODO: migrate temperature to weather manager
    public float baseTemperature;
    public float currentTempC;
    public float currentTempF;
    public float dayProgress;
    public int dayOfMonth;
    public WorldMonth monthOfYear;
    public WorldSeason season;
    public float annualProgress;

    public float temperatureAdjust;

    // TODO: revise UpdateGlobalTimeProgress() to include total game time
    // REVIEW: is that necessary if we use seed time? (doesn't this calculate from there?)
    private long gameSeedTime;
    private long globalTimeProgress;

    private float savedTimeOfDay; // day progress before fast-forward
    private float fastForwardTime; // time amount to fast-forward, signal to other features

    private WeatherManager wm;
    private BackgroundManager bm;

    private float cheatTimeScale = 1f; // adjusts time rate from world time multiplier

    const float ABSOLUTEMINIMUMFLOAT = -999999999999999f; // used for timestamp difference

    const float SUNLIGHTINTENSITY = 1f;
    const float MOONLIGHTINTENSITY = 0.1f;
    const float MAXCLOUDAMBIENTLIGHTMULTIPLIER = 0.1f;
    const float WORLDTIMEMULTIPLIER = 60f; // default time rate
    const float BASETEMPERATURE = 20f; // C
    const float TEMPERATUREVARIANCE = 10f;
    const float DAYNIGHTTEMPVARIANCE = 5f;
    // F = (C * 9/5) + 32
    // (in the middle of seasons, linear shift between mid-points)
    // SPRING/FALL 6am-6pm day, 6pm-6am night (12 hr days), base 20C
    // SUMMER 5am-7pm day, 7pm-5am night (14 hr days), base 30C
    // WINTER 7am-5pm day, 5pm-7am night (10 hr days), base 10C
    // day/night cycles vary temperature from base by 10C
    // (cool down until dawn, warm up until dusk)
    // weather effects temp as well (as temperatureAdjust)
    const float TEMPADJUSTSETTLERATE = 0.01f;

    const float SEASONALSINEOFFSET = 0.71166667f;
    const float WINTERSOLSTICEMINANGLE = 38.1f;
    const float SUMMERSOLSTICEANGLEDELTA = 47f;

    const float ANGLETOSUN = -12.36f; // not facing exactly west (cheat pretty sunset)


    void Start()
    {
        // validate
        skyLightObject = GameObject.Find("Sky Lights");
        if (skyLightObject == null)
        {
            Debug.LogError("--- TimeManager [Start] : no Sky Lights object found. aborting.");
            enabled = false;
        }
        else
        {
            seasonalTiltGimble = skyLightObject.transform.parent.gameObject;
            sunLight = skyLightObject.transform.Find("Sun Light").GetComponent<Light>();
            moonLight = skyLightObject.transform.Find("Moon Light").GetComponent<Light>();
            if ( seasonalTiltGimble == null || sunLight == null || moonLight == null )
            {
                Debug.LogError("--- TimeManager [Start] : gimble or sun or moon lights misconfigured. aborting.");
                enabled = false;
            }
        }
        wm = GameObject.FindFirstObjectByType<WeatherManager>();
        if (wm == null)
        {
            Debug.LogError("--- TimeManager [Start] : no weather manager found in scene. aborting.");
            enabled = false;
        }
        bm = GameObject.FindFirstObjectByType<BackgroundManager>();
        if (bm == null)
        {
            Debug.LogError("--- TimeManager [Start] : no background manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            // NOTE: data distribution will override
            dayProgress = 0.5f;
            dayOfMonth = 1;
            monthOfYear = WorldMonth.Mar;
            season = WorldSeason.Spring;
        }
    }

    void Update()
    {
        dayProgress += Time.deltaTime * (WORLDTIMEMULTIPLIER * cheatTimeScale) * (1f/(60f*60f*24f));
        if ( dayProgress > 1f )
        {
            dayProgress = 0f;
            dayOfMonth++;
            if ( dayOfMonth > 30 )
            {
                dayOfMonth = 1;
                monthOfYear++;
                if ((int)monthOfYear == 2 || (int)monthOfYear == 5 ||
                    (int)monthOfYear == 8 || (int)monthOfYear == 11)
                {
                    season++;
                    if ((int)season > 3)
                        season = 0;
                }
                if ( (int)monthOfYear > 11 )
                {
                    monthOfYear = 0;
                }
            }
        }
        // rotate sky lights object once per game day
        // set sun angle to seasonal sine wave
        float seasonProgress = ((1 / 30) + (((dayProgress + dayOfMonth) / 30) + (int)monthOfYear)) / 12;
        // seasonal sine wave sky light tilt
        // TODO: longer-shorter days, slow sun daytime during summer, speed up at night
        // TODO: speed up sun daytime during winter, slow down at night
        // ... update GetWorldData() below once this is in place
        float seasonalSin = Mathf.Sin((seasonProgress + SEASONALSINEOFFSET) * 2f * Mathf.PI); // season progress 0-1
        float skyLightTilt = 90f - (WINTERSOLSTICEMINANGLE + (SUMMERSOLSTICEANGLEDELTA / 2f) + ((SUMMERSOLSTICEANGLEDELTA / 2f) * seasonalSin));
        // seasonal tilt must be applied on parent object to skylight object (a gimble of z rotation)
        seasonalTiltGimble.transform.localEulerAngles = new Vector3(0f, 0f, skyLightTilt);
        // set sun rot based on day progress value
        skyLightObject.transform.localEulerAngles = new Vector3((dayProgress * 360f), ANGLETOSUN, 0f);

        // ambient light change per day/night cycle
        UpdateAmbientLighting();

        // REVIEW: base temperature based on season cycle
        // FIXME: the bottom seems 'to bounce' and not like a sine wave
        baseTemperature = BASETEMPERATURE + (((Mathf.Sin(Mathf.PI * seasonProgress) * 2f) - 1f) * TEMPERATUREVARIANCE);
        baseTemperature += temperatureAdjust;
        baseTemperature = Mathf.RoundToInt(baseTemperature * 10f) / 10f;

        // vary current temperature from base by day/night cycle
        currentTempC = baseTemperature + (Mathf.Sin(((dayProgress+.55f)*2f)*Mathf.PI)*(DAYNIGHTTEMPVARIANCE/2f));
        currentTempF = (currentTempC * 1.8f) + 32f;

        currentTempC = Mathf.RoundToInt(currentTempC * 10f) / 10f;
        currentTempF = Mathf.RoundToInt(currentTempF * 10f) / 10f;

        // settle temperature adjustment from weather manager
        if (temperatureAdjust > 0f)
            temperatureAdjust -= ( TEMPADJUSTSETTLERATE * Time.deltaTime ) / (WORLDTIMEMULTIPLIER * cheatTimeScale);
        if (temperatureAdjust < 0f)
            temperatureAdjust += ( TEMPADJUSTSETTLERATE * Time.deltaTime ) / (WORLDTIMEMULTIPLIER * cheatTimeScale);
        if (Mathf.Abs(temperatureAdjust) < 0.001f)
            temperatureAdjust = 0f;

        UpdateGlobalTimeProgres(seasonProgress);
        annualProgress = seasonProgress;
    }

    void UpdateAmbientLighting()
    {
        // fade sun and moon lights at dawn and dusk (0.75f day progress = dusk, 0.25f = dawn)
        if (dayProgress > .3f && dayProgress < .7f)
        {
            sunLight.intensity = SUNLIGHTINTENSITY;
            moonLight.intensity = 0f;
        }
        else if (dayProgress < .2f || dayProgress > .8f)
        {
            sunLight.intensity = 0f;
            moonLight.intensity = MOONLIGHTINTENSITY;
        }
        if (dayProgress > 0.2f && dayProgress < 0.3f)
        {
            // dawn fade
            sunLight.intensity = (dayProgress - 0.2f) * 10f * SUNLIGHTINTENSITY;
            moonLight.intensity = MOONLIGHTINTENSITY - ((dayProgress - 0.2f) * 10f * MOONLIGHTINTENSITY);
        }
        if (dayProgress > 0.7f && dayProgress < 0.8f)
        {
            // dusk fade
            sunLight.intensity = SUNLIGHTINTENSITY - ((dayProgress - 0.7f) * 10f * SUNLIGHTINTENSITY);
            moonLight.intensity = ((dayProgress - 0.7f) * 10f * MOONLIGHTINTENSITY);
        }
        // TODO: re-implement weather lighting results
        // get cloud cover from weather manager
        float clouds = wm.cloudAmount;
        // signal background manager of cloud cover (fog, etc.)
        bm.SetCloudCover(clouds, sunLight.intensity);
        // adjust ambient lighting for cloud cover
        float cloudLightMult = 1f - (clouds * (1f-MAXCLOUDAMBIENTLIGHTMULTIPLIER));
        // adjust sun (and moonlight) for cloud cover
        sunLight.intensity *= cloudLightMult;
        float moonAdjust = MOONLIGHTINTENSITY; //MOONLIGHTINTENSITY * cloudLightMult;
        RenderSettings.ambientIntensity = moonAdjust + (sunLight.intensity * (1f - moonAdjust));
    }

    void UpdateGlobalTimeProgres( float seasonProgress )
    {
        // TODO: also add game data total game time
        // REVIEW: is seasonProgess (annualProgress) irrelevant if we take real time - seed?
        globalTimeProgress = gameSeedTime + (long)(seasonProgress * (WORLDTIMEMULTIPLIER * cheatTimeScale));
    }

    /// <summary>
    /// Returns a percentage the world is currently in given season (0-1)
    /// </summary>
    /// <param name="season">world season</param>
    /// <returns>0-1 value representing amount of season in effect</returns>
    public float GetAmountOfSeason( WorldSeason season )
    {
        float retFloat = 0f;

        float dayProgressInYear = dayProgress;
        dayProgressInYear += dayOfMonth;
        dayProgressInYear += (float)monthOfYear * 30f; // Jan = 0
        // center of the season is in the center of the middle month
        // extents of the season is +/- 55 days
        // overlap of seasons is 10 days between months (50/50 at start of season)
        dayProgressInYear--;
        float dayOfSeasonCenter = 60f + ((((float)season + 1) * 90f) - 45f);
        if (dayOfSeasonCenter > 360 && dayProgressInYear >= 15f && dayProgressInYear < 310f)
            dayOfSeasonCenter -= 360f;
        float deltaSeason = Mathf.Abs(dayOfSeasonCenter - dayProgressInYear);
        if (deltaSeason > 360f && dayProgressInYear < 15f)
            deltaSeason -= 360f;
        retFloat = Mathf.Clamp(50f - deltaSeason, 0f, 10f) / 10f;

        //Debug.Log("--- TimeManager [GetAmountOfSeason] : given day of year is "+dayProgressInYear+" and season center is "+dayOfSeasonCenter+", this is "+retFloat+" of season "+season.ToString()+" with a season delta of "+deltaSeason);

        return retFloat;
    }

    /// <summary>
    /// Gets the global time progress from game initialization until current time
    /// </summary>
    /// <returns>global time progress value</returns>
    public long GetGlobalTimeProgress()
    {
        return globalTimeProgress;
    }

    /// <summary>
    /// Gets a gloabl time progress value with additional schedule delay
    /// </summary>
    /// <param name="schedule">amount of delay to schedule</param>
    /// <returns>global time progress at scheduled timestamp</returns>
    public long GetGlobalTimestamp( float schedule )
    {
        return globalTimeProgress + (long)schedule;
    }

    /// <summary>
    /// Get a float of remaining duration from a scheduled delay timestamp
    /// </summary>
    /// <param name="timestamp">schedule timestamp of global time progress</param>
    /// <returns>amount of time remaining (positive value if any time remaining)</returns>
    public float GetTimestampDifference( long timestamp )
    {
        if ( (timestamp - globalTimeProgress) < ABSOLUTEMINIMUMFLOAT )
            return ABSOLUTEMINIMUMFLOAT;
        else
            return timestamp - globalTimeProgress;
    }

    /// <summary>
    /// Gets the world time multiplier
    /// </summary>
    /// <returns>world time multiplier</returns>
    public float GetWorldTimeMultiplier()
    {
        return (WORLDTIMEMULTIPLIER * cheatTimeScale);
    }

    /// <summary>
    /// Sets the cheat time scale to adjust world time multiplier
    /// </summary>
    /// <param name="cheatScale">time scale (default 1f)</param>
    public void SetCheatTimeScale( float cheatScale )
    {
        cheatTimeScale = cheatScale;
    }

    /// <summary>
    /// Gets the current temperature in either C or F
    /// </summary>
    /// <returns>current temp</returns>
    public float GetCurrentTemperature( bool F )
    {
        if (F)
            return currentTempF;
        else
            return currentTempC;
    }

    /// <summary>
    /// Sets temperature adjust, a settling value added to base temperature
    /// </summary>
    /// <param name="adjust">amount of temp adjustment in C</param>
    public void SetTemperatureAdjust( float adjust )
    {
        temperatureAdjust = adjust;
    }

    /// <summary>
    /// Get world data for storage in game data
    /// </summary>
    /// <returns>world data</returns>
    public WorldData GetWorldData()
    {
        WorldData retWData = new WorldData();

        retWData.worldTimeOfDay = dayProgress; // 24 hours in each day cycle
        retWData.worldDayOfMonth = dayOfMonth; // 30 days in each month cycle
        retWData.worldMonth = monthOfYear;
        retWData.worldSeason = season;
        retWData.annualProgress = annualProgress; // percentage of year cycle (0-1)
        retWData.baseTemperature = baseTemperature;
        retWData.dawnTime = .25f; // temp
        retWData.duskTime = .75f; // temp
        retWData.windAmount = wm.windAmount;
        retWData.windDirection = wm.windDirection;
        retWData.cloudAmount = wm.cloudAmount;
        retWData.rainAmount = wm.rainAmount;

        return retWData;
    }

    /// <summary>
    /// Sets the world data from game data storage
    /// </summary>
    /// <param name="wData">world data</param>
    public void SetWorldData( WorldData wData )
    {
        dayProgress = wData.worldTimeOfDay; // 24 hours in each day cycle
        dayOfMonth = wData.worldDayOfMonth; // 30 days in each month cycle
        monthOfYear = wData.worldMonth;
        season = wData.worldSeason;
        annualProgress = wData.annualProgress; // percentage of year cycle (0-1)
        baseTemperature = wData.baseTemperature;
        // wData.dawnTime = .25f; // temp
        // wData.duskTime = .75f; // temp
        PositionData startWeather = new PositionData();
        startWeather.x = wData.windAmount;
        startWeather.y = wData.windDirection;
        startWeather.z = wData.cloudAmount;
        startWeather.w = wData.rainAmount;
        wm.SetStartWeather(startWeather);

        UpdateAmbientLighting();
    }

    /// <summary>
    /// Sets the game seed time value for use in calculating global time progress. WARNING: use only with game data load
    /// </summary>
    /// <param name="seedTime">game seed time</param>
    public void SetGameSeedTime( long seedTime )
    {
        gameSeedTime = seedTime;

        // perform global time progress calculation based on current time and seed
        // 60 * 24 * 30 * 12 = how many real time seconds per year in game
        // time forward (s) / 518,400 = season progress forward

        savedTimeOfDay = dayProgress; // used for fast fwd features
        // calculate fast forward time delta from current world data
        WorldData now = GetWorldData();

        long rightNow = System.DateTime.Now.ToFileTimeUtc();
        System.TimeSpan timeForward = System.DateTime.FromFileTimeUtc(rightNow).Subtract(System.DateTime.FromFileTimeUtc(gameSeedTime));
        double realSecondsForward = timeForward.TotalSeconds;

        // crank world data forward from Mar 1st at noon
        WorldData future = new WorldData();
        future.worldTimeOfDay = 0.5f;
        future.worldDayOfMonth = 1;
        future.worldMonth = WorldMonth.Mar;

        float daysAhead = (float)(realSecondsForward / (60 * 24));
        // NOTE: we will hold onto this value until all features ready to fast-forward
        
        future.worldTimeOfDay += daysAhead;
        future.worldTimeOfDay %= 1f;
        // REVIEW: due to int (we do not want this to round up?)
        future.worldDayOfMonth += Mathf.RoundToInt(daysAhead);
        future.worldDayOfMonth %= 30;
        // REVIEW: due to int
        future.worldMonth += Mathf.RoundToInt((daysAhead/30f));
        future.worldMonth = (WorldMonth)((int)future.worldMonth % 12);
        // set season
        future.worldSeason = WorldSeason.Winter;
        if (future.worldMonth > WorldMonth.Feb)
            future.worldSeason = WorldSeason.Spring;
        if (future.worldMonth > WorldMonth.May)
            future.worldSeason = WorldSeason.Summer;
        if (future.worldMonth > WorldMonth.Aug)
            future.worldSeason = WorldSeason.Fall;
        if (future.worldMonth > WorldMonth.Nov)
            future.worldSeason = WorldSeason.Winter;

        // preserve weather conditions (already distributed)
        future.windAmount = wm.windAmount;
        future.windDirection = wm.windDirection;
        future.cloudAmount = wm.cloudAmount;
        future.rainAmount = wm.rainAmount;

        SetWorldData(future);

        float futureDayAmount = (future.worldTimeOfDay +
            future.worldDayOfMonth + ((int)future.worldMonth * 30));
        float nowDayAmount = (now.worldTimeOfDay +
            now.worldDayOfMonth + ((int)now.worldMonth * 30));
        if ((int)future.worldMonth < (int)now.worldMonth)
            futureDayAmount += (12f * 30f); // assume only one year?
        fastForwardTime = futureDayAmount - nowDayAmount;
    }

    public void FastForwardFeatures()
    {
        // use fastForwardTime value
        if (fastForwardTime > 0f)
        {
            // cast manager
            FindFirstObjectByType<CastManager>().FastForwardCasts(fastForwardTime);
            // plots managers
            PlotManager[] pms = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
            for (int i = 0; i < pms.Length; i++)
            {
                pms[i].FastForwardPlot(fastForwardTime, savedTimeOfDay);
            }
            // weather manager
            wm.FastForwardWeather(fastForwardTime);
            // reset
            savedTimeOfDay = 0f;
            fastForwardTime = 0f;
        }
    }
}