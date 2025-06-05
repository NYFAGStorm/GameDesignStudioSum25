using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world time cycles and related world properties

    public GameObject skyLightObject;
    public Light sunLight;
    public Light moonLight;

    public float baseTemperature;
    public float currentTempC;
    public float currentTempF;
    public float dayProgress;
    public int dayOfMonth;
    public WorldMonth monthOfYear;
    public WorldSeason season;

    // TODO: link to game for world data, run time based on game seed
    // TODO: revise UpdateGlobalTimeProgress() to include total game time
    // TODO: migrate temperature to weather manager
    private long gameSeedTime;
    private long globalTimeProgress;

    const float SUNLIGHTINTENSITY = 1f;
    const float MOONLIGHTINTENSITY = 0.381f;
    const float WORLDTIMEMULTIPLIER = 60f; // set to 600f, 6000f, 60000f, for testing
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
    // weather effects temp as well


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
            sunLight = skyLightObject.transform.Find("Sun Light").GetComponent<Light>();
            moonLight = skyLightObject.transform.Find("Moon Light").GetComponent<Light>();
            if ( sunLight == null || moonLight == null )
            {
                Debug.LogError("--- TimeManager [Start] : sun and moon lights misconfigured. aborting.");
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
        dayProgress += Time.deltaTime * WORLDTIMEMULTIPLIER * (1f/(60f*60f*24f));
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
        float sunRotX = Time.deltaTime * WORLDTIMEMULTIPLIER * (360f / (24f * 60f * 60f));
        skyLightObject.transform.Rotate(sunRotX, 0f, 0f);
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
        RenderSettings.ambientIntensity = 0.1f + (sunLight.intensity * 0.9f);

        // REVIEW: base temperature based on season cycle
        // FIXME: the bottom seems 'to bounce' and not like a sine wave
        float seasonProgress = ((1 / 30) + (((dayProgress + dayOfMonth) / 30) + (int)monthOfYear)) / 12;
        baseTemperature = BASETEMPERATURE + (((Mathf.Sin(Mathf.PI * seasonProgress) * 2f) - 1f) * TEMPERATUREVARIANCE);
        baseTemperature = Mathf.RoundToInt(baseTemperature * 10f) / 10f;

        // vary current temperature from base by day/night cycle
        currentTempC = baseTemperature + (Mathf.Sin(((dayProgress+.55f)*2f)*Mathf.PI)*(DAYNIGHTTEMPVARIANCE/2f));
        currentTempF = (currentTempC * 1.8f) + 32f;

        currentTempC = Mathf.RoundToInt(currentTempC * 10f) / 10f;
        currentTempF = Mathf.RoundToInt(currentTempF * 10f) / 10f;

        UpdateGlobalTimeProgres(seasonProgress);
    }

    void UpdateGlobalTimeProgres( float seasonProgress )
    {
        // TODO: also add game data total game time
        globalTimeProgress = gameSeedTime + (long)(seasonProgress * WORLDTIMEMULTIPLIER);
    }

    /// <summary>
    /// Gets the global time progress from game initialization until current time
    /// </summary>
    /// <returns>global time progress value</returns>
    public long GetGlobalTimeProgress()
    {
        return globalTimeProgress;
    }

    public float GetWorldTimeMultiplier()
    {
        return WORLDTIMEMULTIPLIER;
    }
}
