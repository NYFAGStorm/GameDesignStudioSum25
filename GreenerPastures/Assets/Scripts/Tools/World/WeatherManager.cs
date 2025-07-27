using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world weather

    public float windAmount;
    public float windDirection; // negative is right to left, positive left to right
    public float cloudAmount;
    public float rainAmount;

    // smoothing
    private PositionData previousWeather; // wind, dir, cloud, rain (x,y,z,w)
    private PositionData targetWeather; // wind, dir, cloud, rain (x,y,z,w)

    private float windFactor;
    private float windVector; // the delta of wind factor (not direction)
    private float cloudFactor;
    private float cloudVector;

    private long globalTimeProgress;
    private float timeMultiplier;
    private float weatherTimer;
    private TimeManager tim;
    private CameraManager cm;

    private AudioManager sfxAudio;
    private bool haltWeatherSFX;
    private bool indoorWeatherSFX;

    const float WEATHERCHECKINTERVAL = 15f; //.0618f;

    const float WINDFACTORSCALE = 1f;
    const float WINDFACTOROFFSET = 3.81f;
    const float WINDVECTOROFFSET = 0.1f;
    const float WINDCHANGEMULTIPLIER = 0.618f;
    const float WINDWEIGHT = 0.0381f;

    const float CLOUDFACTORSCALE = 0.618f;
    const float CLOUDFACTOROFFSET = 6.18f;
    const float CLOUDVECTOROFFSET = 0.2f;
    const float CLOUDCHANGEMULTIPLIER = 0.381f;
    const float CLOUDWEIGHT = 0.00618f;

    const float RAINCLOUDTHRESHOLD = 0.618f;
    const float RAINWATERINGRATE = 38.1f;


    private void OnDisable()
    {
        if (sfxAudio != null)
            sfxAudio.StopAllSounds();
    }

    void Start()
    {
        // validate
        tim = GameObject.FindAnyObjectByType<TimeManager>();
        if (tim == null)
        {
            Debug.LogError("--- WeatherManager [Start] : no time manager found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr Weather SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            weatherTimer = .0618f;
        }
    }

    public void ConfigCameraManager(CameraManager camMgr)
    {
        cm = camMgr;
    }

    void Update()
    {
        // fix rounding error values in weather conditions (clouds and rain not zero)
        if (targetWeather.x > 0f && previousWeather.x > 0f)
        {
            if (previousWeather.x < .01f)
            {
                if (targetWeather.x < 0.01f)
                {
                    targetWeather.x = 0f;
                    //Debug.Log("-- near zero target wind weather conditions. set target to zero. --");
                }
                if (GameSystem.PositionDistance(previousWeather, targetWeather) < 0.1f)
                {
                    previousWeather = targetWeather;
                    //Debug.Log("-- near zero wind difference in weather conditions. set prev to target. --");
                }
            }
        }
        if (previousWeather.x == 0f)
            previousWeather.y = 0f;
        if (targetWeather.x == 0f)
            targetWeather.y = 0f;
        if (targetWeather.z > 0f && previousWeather.z > 0f)
        {
            if (previousWeather.z < .01f)
            {
                if (targetWeather.z < 0.01f)
                {
                    targetWeather.z = 0f;
                    //Debug.Log("-- near zero target cloud weather conditions. set target to zero. --");
                }
                if (GameSystem.PositionDistance(previousWeather, targetWeather) < 0.1f)
                {
                    previousWeather = targetWeather;
                    //Debug.Log("-- near zero cloud difference in weather conditions. set prev to target. --");
                }
            }
        }

        // weather sfx
        if (sfxAudio != null && !haltWeatherSFX)
        {
            if (rainAmount > 0f)
            {
                // rain
                if (sfxAudio.IsSoundPlaying("Rain Loop"))
                {
                    float currentRain = sfxAudio.GetSoundVolume("Rain Loop");
                    currentRain = FadeTo(currentRain, rainAmount * .618f);
                    sfxAudio.SetSoundVolume("Rain Loop", currentRain);
                }
                else
                {
                    sfxAudio.StartSound("Rain Loop");
                    sfxAudio.SetSoundVolume("Rain Loop", rainAmount);
                }
            }
            else
            {
                // no rain
                if (sfxAudio.IsSoundPlaying("Rain Loop"))
                    sfxAudio.StopSound("Rain Loop");
            }
            if (windAmount > 0f)
            {
                float lightWind = Mathf.Sin(Mathf.Clamp01(windAmount / .5f) * Mathf.PI);
                if (lightWind < 0.01f)
                    lightWind = 0f;
                float medWind = Mathf.Sin(Mathf.Clamp01((windAmount - .25f) / .5f) * Mathf.PI);
                if (medWind < 0.01f)
                    medWind = 0f;
                float heavyWind = Mathf.Sin(Mathf.Clamp01(windAmount - .5f) * Mathf.PI);
                if (heavyWind < 0.01f)
                    heavyWind = 0f;
                if (windAmount > 0f)
                {
                    if (lightWind > 0f)
                    {
                        if (sfxAudio.IsSoundPlaying("Wind Loop Light"))
                        {
                            float current = sfxAudio.GetSoundVolume("Wind Loop Light");
                            if (current == 0f)
                                sfxAudio.StopSound("Wind Loop Light");
                            else
                            {
                                current = FadeTo(current, lightWind);
                                sfxAudio.SetSoundVolume("Wind Loop Light", current);
                            }
                        }
                        else
                        {
                            sfxAudio.StartSound("Wind Loop Light");
                            sfxAudio.SetSoundVolume("Wind Loop Light", lightWind);
                        }
                    }
                    else if (sfxAudio.IsSoundPlaying("Wind Loop Light"))
                        sfxAudio.StopSound("Wind Loop Light");
                    if (medWind > 0f)
                    {
                        if (sfxAudio.IsSoundPlaying("Wind Loop Medium"))
                        {
                            float current = sfxAudio.GetSoundVolume("Wind Loop Medium");
                            if (current == 0f)
                                sfxAudio.StopSound("Wind Loop Medium");
                            else
                            {
                                current = FadeTo(current, medWind * .618f);
                                sfxAudio.SetSoundVolume("Wind Loop Medium", current);
                            }
                        }
                        else
                        {
                            sfxAudio.StartSound("Wind Loop Medium");
                            sfxAudio.SetSoundVolume("Wind Loop Medium", medWind * .618f);
                        }
                    }
                    else if (sfxAudio.IsSoundPlaying("Wind Loop Medium"))
                        sfxAudio.StopSound("Wind Loop Medium");
                    if (heavyWind > 0f)
                    {
                        if (sfxAudio.IsSoundPlaying("Wind Loop Heavy"))
                        {
                            float current = sfxAudio.GetSoundVolume("Wind Loop Heavy");
                            if (current == 0f)
                                sfxAudio.StopSound("Wind Loop Heavy");
                            else
                            {
                                current = FadeTo(current, heavyWind * .381f);
                                sfxAudio.SetSoundVolume("Wind Loop Heavy", current);
                            }
                        }
                        else
                        {
                            sfxAudio.StartSound("Wind Loop Heavy");
                            sfxAudio.SetSoundVolume("Wind Loop Heavy", heavyWind * .381f);
                        }
                    }
                    else if (sfxAudio.IsSoundPlaying("Wind Loop Heavy"))
                        sfxAudio.StopSound("Wind Loop Heavy");
                }
                else
                {
                    // no wind
                    if (sfxAudio.IsSoundPlaying("Wind Loop Light"))
                        sfxAudio.StopSound("Wind Loop Light");
                    if (sfxAudio.IsSoundPlaying("Wind Loop Medium"))
                        sfxAudio.StopSound("Wind Loop Medium");
                    if (sfxAudio.IsSoundPlaying("Wind Loop Heavy"))
                        sfxAudio.StopSound("Wind Loop Heavy");
                }
            }
            KeepLowPassFilterOnBottom();
        }

        // run weather timer
        if (weatherTimer > 0f)
        {
            weatherTimer -= Time.deltaTime;
            float smoothProgress = Mathf.Clamp01(1f - (weatherTimer / (WEATHERCHECKINTERVAL / (timeMultiplier / 60f))));
            if (weatherTimer > 0f)
            {
                // smooth results with lerp between checks
                windAmount = Mathf.Lerp(previousWeather.x, targetWeather.x, smoothProgress);
                windDirection = Mathf.Lerp(previousWeather.y, targetWeather.y, smoothProgress);
                if (windDirection < 0f)
                    windDirection = -1f;
                else if (windDirection > 0f)
                    windDirection = 1f;
                cloudAmount = Mathf.Lerp(previousWeather.z, targetWeather.z, smoothProgress);
                rainAmount = Mathf.Lerp(previousWeather.w, targetWeather.w, smoothProgress);

                return;
            }
        }

        // set previous weather
        previousWeather.x = windAmount;
        previousWeather.y = windDirection;
        previousWeather.z = cloudAmount;
        previousWeather.w = rainAmount;

        // timer set
        weatherTimer = WEATHERCHECKINTERVAL / (timeMultiplier / 60f);

        // check the weather
        CalculateCurrentWeather(0f);

        // tell camera manager about rain
        if (cm != null)
            cm.SetRain(rainAmount, windAmount, windDirection < 0f);

        // water all plots per rain amount
        if (rainAmount > 0f)
        {
            PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
            for (int i = 0; i <  plots.Length; i++)
            {
                plots[i].data.water = Mathf.Clamp01(plots[i].data.water + (rainAmount * RAINWATERINGRATE * Time.deltaTime));
            }
        }

        // use wind and cloud to adjust temperature (on time manager)
        if ( windAmount > 0f || cloudAmount > 0f )
        {
            // NOTE: if this method is not called, this adjustment settles
            float adjust = (windAmount * windDirection) + (cloudAmount * -2f);
            tim.SetTemperatureAdjust(adjust);
        }
    }

    float FadeTo(float current, float target)
    {
        if (Mathf.Abs(target - current) < 0.01f)
            current = target;
        if (current < target)
            current += ((target - current) * 0.1f);
        if (current > target)
            current -= ((current - target) * 0.1f);
        return current;
    }

    public void HaltWeatherSFX(bool stopSFX)
    {
        haltWeatherSFX = stopSFX;
        if (stopSFX && sfxAudio != null)
        {
            sfxAudio.StopAllSounds();
            AudioLowPassFilter lowPass = sfxAudio.gameObject.GetComponent<AudioLowPassFilter>();
            if (lowPass != null)
                Destroy(lowPass);
        }
    }

    public void SFXForIndoors(bool indoorSFX)
    {
        if (sfxAudio != null)
        {
            indoorWeatherSFX = indoorSFX;
            AudioLowPassFilter lowPass = sfxAudio.gameObject.GetComponent<AudioLowPassFilter>();
            if (indoorSFX)
            {
                if (lowPass == null && sfxAudio.gameObject.GetComponent<AudioSource>() != null)
                    lowPass = sfxAudio.gameObject.AddComponent<AudioLowPassFilter>();
                if (lowPass != null)
                    lowPass.cutoffFrequency = 381;
            }
            else if (lowPass != null)
                Destroy(lowPass);
        }
    }

    void KeepLowPassFilterOnBottom()
    {
        if (sfxAudio == null)
            return;
        AudioLowPassFilter lowPass = sfxAudio.gameObject.GetComponent<AudioLowPassFilter>();
        if (lowPass == null)
        {
            if (!indoorWeatherSFX)
                return;
            if (sfxAudio.gameObject.GetComponent<AudioSource>() != null)
            {
                lowPass = sfxAudio.gameObject.AddComponent<AudioLowPassFilter>();
                if (lowPass != null)
                    lowPass.cutoffFrequency = 381;
            }
            return;
        }
        else if (!indoorWeatherSFX)
        {
            Destroy(lowPass);
            return;
        }
        int filterIndex = sfxAudio.gameObject.GetComponentIndex(lowPass);
        if (sfxAudio.gameObject.GetComponentCount() - 1 > filterIndex)
            Destroy(lowPass); // one will be made next tick
    }

    /// <summary>
    /// Calculates the procedural weather conditions based on time manager data
    /// </summary>
    /// <param name="offsetDays">global time progress offset (used in fast-forward)</param>
    /// <returns>position data with wind, wind dir, clouds and rain values</returns>
    PositionData CalculateCurrentWeather( float offsetDays )
    {
        PositionData weatherDelta = new PositionData(); // may be used in fast-forward

        // time check
        globalTimeProgress = tim.GetGlobalTimeProgress();
        globalTimeProgress += (long)offsetDays;
        timeMultiplier = tim.GetWorldTimeMultiplier();

        // calculate wind factor and vector
        windFactor = GetProceduralResult(WINDFACTORSCALE, WINDFACTOROFFSET);
        windVector = GetProceduralResult(WINDFACTORSCALE, WINDVECTOROFFSET) - windFactor;

        // calculate cloud factor and vector
        cloudFactor = GetProceduralResult(CLOUDFACTORSCALE, CLOUDFACTOROFFSET);
        cloudVector = GetProceduralResult(CLOUDFACTORSCALE, CLOUDVECTOROFFSET) - cloudFactor;

        // adjust wind
        float variableWindWeight = WINDWEIGHT + (0.5799f * (1f - Mathf.Abs(Mathf.Sin(tim.dayProgress * 2f * Mathf.PI))));
        targetWeather.x = Mathf.Clamp01(targetWeather.x - variableWindWeight + (windVector * WINDCHANGEMULTIPLIER));
        // calculate wind direction
        targetWeather.y = ((windFactor * 2f) - 1f) / Mathf.Abs((windFactor * 2f) - 1f);
        if (targetWeather.x == 0)
            targetWeather.y = 0f;
        // adjust cloud
        float variableCloudWeight = CLOUDWEIGHT + (0.03192f * (1f - Mathf.Abs(Mathf.Sin(tim.dayProgress * 2f * Mathf.PI))));
        targetWeather.z = Mathf.Clamp01(targetWeather.z - variableCloudWeight + (cloudVector * CLOUDCHANGEMULTIPLIER));
        // calculate rain (based on clouds)
        targetWeather.w = Mathf.Clamp01(targetWeather.z - RAINCLOUDTHRESHOLD) * (1f / (1f - RAINCLOUDTHRESHOLD));

        // record delta
        weatherDelta.x = targetWeather.x - previousWeather.x;
        weatherDelta.y = targetWeather.y - previousWeather.y;
        weatherDelta.z = targetWeather.z - previousWeather.z;
        weatherDelta.w = targetWeather.w - previousWeather.w;

        return weatherDelta;
    }

    float GetProceduralResult( float inputX, float inputY )
    {
        long timeprogress = globalTimeProgress % 1000000; // long going past perlin range
        return Mathf.PerlinNoise( timeprogress * timeMultiplier * inputX, inputY );
    }

    /// <summary>
    /// Sets weather conditions directly
    /// </summary>
    /// <param name="weatherConditions">position data (wind, wind dir, cloud and rain)</param>
    public void SetStartWeather( PositionData weatherConditions )
    {
        windAmount = weatherConditions.x;
        windDirection = weatherConditions.y;
        cloudAmount = weatherConditions.z;
        rainAmount = weatherConditions.w;
        previousWeather.x = weatherConditions.x;
        previousWeather.y = weatherConditions.y;
        previousWeather.z = weatherConditions.z;
        previousWeather.w = weatherConditions.w;

        // catch near-zero weather conditions
        if (previousWeather.x < 0.01f)
            previousWeather.x = 0f;
        if (previousWeather.z < 0.01f)
            previousWeather.z = 0f;
        if (previousWeather.w < 0.01f)
            previousWeather.w = 0f;
        if (!GameSystem.IsZero(previousWeather) && GameSystem.PositionDistance(previousWeather, GameSystem.Zero()) < 0.1f)
            previousWeather = GameSystem.Zero();

        targetWeather = previousWeather;
        weatherTimer = 0.0618f;
    }

    /// <summary>
    /// Fast-forwards weather conditions based on given days ahead, from current
    /// </summary>
    /// <param name="daysAhead">amount of days to fast forward</param>
    public void FastForwardWeather( float daysAhead )
    {
        // fast-forward time based on daysAhead * 60 * 24 for game minutes
        float weatherChecks = daysAhead * 24f * (60f / WEATHERCHECKINTERVAL);
        PositionData fastFwdWeather = new PositionData();
        fastFwdWeather.x = windAmount;
        fastFwdWeather.y = windDirection;
        fastFwdWeather.z = cloudAmount;
        fastFwdWeather.w = rainAmount;
        for (int i = 0; i < weatherChecks; i++)
        {
            PositionData delta = new PositionData();
            delta = CalculateCurrentWeather( (1f - (i / weatherChecks)) * -daysAhead );
            fastFwdWeather.x = Mathf.Clamp01(delta.x + fastFwdWeather.x);
            fastFwdWeather.y = Mathf.Clamp01(delta.y + fastFwdWeather.y);
            fastFwdWeather.z = Mathf.Clamp01(delta.z + fastFwdWeather.z);
            fastFwdWeather.w = Mathf.Clamp01(delta.w + fastFwdWeather.w);
        }
        // set current weather
        SetStartWeather(fastFwdWeather);
        // settle noisy near-zero values
        if (windAmount < 0.01f)
            windAmount = 0f;
        if (cloudAmount < 0.01f)
            cloudAmount = 0f;
        if (rainAmount < 0.01f)
            rainAmount = 0f;
        // reset global time progress
        globalTimeProgress = tim.GetGlobalTimeProgress();
        // set check timer
        weatherTimer = 0.618f;
    }
}
