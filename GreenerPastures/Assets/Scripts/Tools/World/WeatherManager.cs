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

    const float WEATHERCHECKINTERVAL = 15f;

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


    void Start()
    {
        // validate
        tim = GameObject.FindAnyObjectByType<TimeManager>();
        if (tim == null)
        {
            Debug.LogError("--- WeatherManager [Start] : no time manager found in scene. aborting.");
            enabled = false;
        }
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
        // run weather timer
        if (weatherTimer > 0f)
        {
            weatherTimer -= Time.deltaTime;
            float smoothProgress = 1f - (weatherTimer / (WEATHERCHECKINTERVAL / (timeMultiplier / 60f)));
            if (weatherTimer > 0f)
            {
                // smooth results with lerp between checks
                windAmount = Mathf.Lerp(previousWeather.x, targetWeather.x, smoothProgress);
                windDirection = Mathf.Lerp(previousWeather.y, targetWeather.y, smoothProgress);
                cloudAmount = Mathf.Lerp(previousWeather.z, targetWeather.z, smoothProgress);
                rainAmount = Mathf.Lerp(previousWeather.w, targetWeather.w, smoothProgress);

                return;
            }
        }

        // set weather to target
        windAmount = targetWeather.x;
        windDirection = targetWeather.y;
        cloudAmount = targetWeather.z;
        rainAmount = targetWeather.w;

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

    /// <summary>
    /// Calculates the procedural weather conditions based on time manager data
    /// </summary>
    /// <param name="offsetDays">global time progress offset (used in fast-forward)</param>
    /// <returns>position data with wind, wind dir, clouds and rain values</returns>
    PositionData CalculateCurrentWeather( float offsetDays )
    {
        PositionData weatherDelta = new PositionData(); // may be used in fast-forward

        // smoothing
        previousWeather.x = targetWeather.x; // wind
        previousWeather.y = targetWeather.y; // wind dir
        previousWeather.z = targetWeather.z; // cloud
        previousWeather.w = targetWeather.w; // rain

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
        targetWeather.x = Mathf.Clamp01(targetWeather.x - WINDWEIGHT + (windVector * WINDCHANGEMULTIPLIER));
        // calculate wind direction
        targetWeather.y = ((windFactor * 2f) - 1f) / Mathf.Abs((windFactor * 2f) - 1f);
        if (targetWeather.x == 0)
            targetWeather.y = 0f;
        // adjust cloud
        targetWeather.z = Mathf.Clamp01(targetWeather.z - CLOUDWEIGHT + (cloudVector * CLOUDCHANGEMULTIPLIER));
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
        // reset global time progress
        globalTimeProgress = tim.GetGlobalTimeProgress();
        // set check timer
        weatherTimer = 0.0618f;
    }
}
