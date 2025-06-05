using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world weather

    public float windAmount;
    public float cloudAmount;

    private float windFactor;
    private float windVector;
    private float cloudFactor;
    private float cloudVector;

    private long globalTimeProgress;
    private float timeMultiplier;
    private float weatherTimer;
    private TimeManager tim;

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
    }

    float GetProceduralResult( float inputX, float inputY )
    {
        return Mathf.PerlinNoise( globalTimeProgress * timeMultiplier * inputX, inputY );
    }
}
