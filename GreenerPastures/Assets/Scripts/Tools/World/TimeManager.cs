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

    public float baseTemperature;
    public float currentTempC;
    public float currentTempF;
    public float dayProgress;
    public int dayOfMonth;
    public WorldMonth monthOfYear;
    public WorldSeason season;
    public float annualProgress;

    public float temperatureAdjust;

    // TODO: link to game for world data, run time based on game seed
    // TODO: revise UpdateGlobalTimeProgress() to include total game time
    // REVIEW: is that necessary if we use seed time? (doesn't this calculate from there?)
    // TODO: migrate temperature to weather manager
    private long gameSeedTime;
    private long globalTimeProgress;

    private float cheatTimeScale = 1f; // adjusts time rate from world time multiplier

    const float ABSOLUTEMINIMUMFLOAT = -999999999999999f; // used for timestamp difference

    const float SUNLIGHTINTENSITY = 1f;
    const float MOONLIGHTINTENSITY = 0.1f;
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
        // initialize
        if (enabled)
        {
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
        // TODO: seasonal sine wave longer-shorter days, sky light tilt
        // TODO: slow sun daytime during summer, speed up at night
        // TODO: speed up sun daytime during winter, slow down at night
        // ... update GetWorldData() below once this is in place
        float seasonalSin = Mathf.Sin((seasonProgress + SEASONALSINEOFFSET) * 2f * Mathf.PI); // season progress 0-1
        float skyLightTilt = 90f - (WINTERSOLSTICEMINANGLE + (SUMMERSOLSTICEANGLEDELTA / 2f) + ((SUMMERSOLSTICEANGLEDELTA / 2f) * seasonalSin));
        // seasonal tilt must be applied on parent object to skylight object (a gimble of z rotation)
        seasonalTiltGimble.transform.localEulerAngles = new Vector3(0f, 0f, skyLightTilt);
        // set sun rot based on day progress value
        skyLightObject.transform.localEulerAngles = new Vector3((dayProgress * 360f), ANGLETOSUN, 0f);

        // fade sun and moon lights at dawn and dusk (0.75f day progress = dusk, 0.25f = dawn)

        if (dayProgress > .3f && dayProgress < .7f)
        {
            sunLight.intensity = SUNLIGHTINTENSITY;
            moonLight.intensity = 0f;
        }
        else if (dayProgress < .2f && dayProgress > .8f)
        {
            sunLight.intensity = 0f;
            moonLight.intensity = MOONLIGHTINTENSITY;
        }
        if (dayProgress > 0.2f && dayProgress < 0.3f)
        {
            // dawn fade
            sunLight.intensity = (dayProgress - 0.2f) * 10f * SUNLIGHTINTENSITY;
            moonLight.intensity = MOONLIGHTINTENSITY - ( (dayProgress - 0.2f) * 10f * MOONLIGHTINTENSITY);

        }
        if (dayProgress > 0.7f && dayProgress < 0.8f)
        {
            // dusk fade
            sunLight.intensity = SUNLIGHTINTENSITY - ((dayProgress - 0.7f) * 10f * SUNLIGHTINTENSITY);
            moonLight.intensity = ((dayProgress - 0.7f) * 10f * MOONLIGHTINTENSITY);
        }
        RenderSettings.ambientIntensity = MOONLIGHTINTENSITY + (sunLight.intensity * (1f-MOONLIGHTINTENSITY));

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

    void UpdateGlobalTimeProgres( float seasonProgress )
    {
        // TODO: also add game data total game time
        globalTimeProgress = gameSeedTime + (long)(seasonProgress * (WORLDTIMEMULTIPLIER * cheatTimeScale));
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

        return retWData;
    }

    /// <summary>
    /// Sets the game seed time value for use in calculating global time progress. WARNING: use only with game data load
    /// </summary>
    /// <param name="seedTime">game seed time</param>
    public void SetGameSeedTime( long seedTime )
    {
        gameSeedTime = seedTime;
    }
}