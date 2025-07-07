using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world weather

    public float windAmount;
    public float windDirection; // negative is left to right, positive right to left
    public float cloudAmount;

    public float rainAmount;

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

    const float RAINCLOUDTHRESHOLD = 0.8f;
    const float RAINWATERINGRATE = 0.1f;


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
            weatherTimer = 1f;
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
            if (weatherTimer < 0)
            {
                globalTimeProgress = tim.GetGlobalTimeProgress();
                timeMultiplier = tim.GetWorldTimeMultiplier();
                weatherTimer = WEATHERCHECKINTERVAL / (timeMultiplier/60f);
            }
            else
                return;
        }

        // TODO: smooth results with lerp between checks

        // calculate wind factor and vector
        windFactor = GetProceduralResult(WINDFACTORSCALE, WINDFACTOROFFSET);
        windVector = GetProceduralResult(WINDFACTORSCALE, WINDVECTOROFFSET) - windFactor;

        // calculate cloud factor and vector
        cloudFactor = GetProceduralResult(CLOUDFACTORSCALE, CLOUDFACTOROFFSET);
        cloudVector = GetProceduralResult(CLOUDFACTORSCALE, CLOUDVECTOROFFSET) - cloudFactor;

        // adjust wind
        windAmount = Mathf.Clamp01(windAmount - WINDWEIGHT + (windVector * WINDCHANGEMULTIPLIER));
        // adjust cloud
        cloudAmount = Mathf.Clamp01(cloudAmount - CLOUDWEIGHT + (cloudVector * CLOUDCHANGEMULTIPLIER));

        // calculate rain
        rainAmount = Mathf.Clamp01(cloudAmount - RAINCLOUDTHRESHOLD) * (1f / (1f - RAINCLOUDTHRESHOLD)); 

        // calculate wind direction
        windDirection = ((windFactor * 2f) - 1f) / Mathf.Abs( (windFactor * 2f) - 1f );
        if (windAmount == 0)
            windDirection = 0f;

        // tell camera manager about rain
        if (cm != null)
            cm.SetRain(rainAmount, windAmount, windDirection < 0f);

        // water all plots per rain amount
        if (rainAmount > 0f)
        {
            PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
            for (int i = 0; i <  plots.Length; i++)
            {
                plots[i].data.water += Mathf.Clamp01(rainAmount * RAINWATERINGRATE * Time.deltaTime);
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

    float GetProceduralResult( float inputX, float inputY )
    {
        long timeprogress = globalTimeProgress % 1000000; // long going past perlin range
        return Mathf.PerlinNoise( timeprogress * timeMultiplier * inputX, inputY );
    }
}
