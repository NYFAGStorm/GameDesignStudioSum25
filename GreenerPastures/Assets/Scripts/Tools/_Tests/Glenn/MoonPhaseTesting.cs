using UnityEngine;
using UnityEngine.InputSystem;

public class MoonPhaseTesting : MonoBehaviour
{
    // Author: Glenn Storm
    // A general testing script, mostly to fine tune forumlae that would take too long to test in game

    // -- moon phase
    public int dayOfMonth = 1;
    public float moonPhase;

    public Renderer moonRenderer;
    // 

    // -- dynamic audio blend
    public float lightVal;
    public float medVal;
    public float heavyVal;
    [Range(0f,1f)]
    public float drivingValue;
    //

    // -- float rounding
    [Range(0f, 1f)]
    public float valueToRound;
    public float valueMultiplier;
    public int roundedResult;
    //

    // -- sine waves
    [Range(0f, 1f)]
    public float percentage;
    public float sineWaveResult;
    //

    [Range(0f, 1f)]
    public float dayProgress;
    //public int dayOfMonth;
    public float cheatTimeScale = 1f;
    public WorldMonth monthOfYear;
    public WorldSeason season;
    public long gameSeedTime;
    public long globalTimeProgress;
    public float seasonProgress;
    public float daysAhead;
    public bool goForward;
    [SerializeField]
    public WorldData future;


    const float WORLDTIMEMULTIPLIER = 60f; // default time rate


    void Start()
    {
        
    }

    void Update()
    {
        // - sine waves
        sineWaveResult = 1f - Mathf.Abs( Mathf.Sin((percentage) * 2f * Mathf.PI) );

        // - float rounding
        roundedResult = Mathf.RoundToInt((valueToRound * valueMultiplier) + 0.5f);
        roundedResult = Mathf.Clamp(roundedResult, 1, (int)valueMultiplier);

        // - dynamic audio blending
        lightVal = Mathf.Sin(Mathf.Clamp01( drivingValue / .5f ) * Mathf.PI);
        if (lightVal < 0.01f)
            lightVal = 0f;
        medVal = Mathf.Sin(Mathf.Clamp01( (drivingValue - .25f) / .5f ) * Mathf.PI);
        if (medVal < 0.01f)
            medVal = 0f;
        heavyVal = Mathf.Sin(Mathf.Clamp01( drivingValue - .5f ) * Mathf.PI);
        if (heavyVal < 0.01f)
            heavyVal = 0f;

        // - moon phase -
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dayOfMonth++;
            if (dayOfMonth > 30)
                dayOfMonth = 1;
            moonPhase = Mathf.Sin(((dayOfMonth) / (float)30) * Mathf.PI);
            if (moonPhase < 0.001f)
                moonPhase = 0f;
            moonPhase = 1f - moonPhase; // makes 15th new moon, 30th full moon
            moonRenderer.material.SetFloat("_MoonPhase", ((float)dayOfMonth / 30f));
        }

        dayProgress += Time.deltaTime * (WORLDTIMEMULTIPLIER * cheatTimeScale) * (1f / (60f * 60f * 24f));
        if (dayProgress > 1f)
        {
            dayProgress = 0f;
            dayOfMonth++;
            if (dayOfMonth > 30)
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
                if ((int)monthOfYear > 11)
                {
                    monthOfYear = 0;
                }
            }
        }
        seasonProgress = ((1 / 30) + (((dayProgress + dayOfMonth) / 30) + (int)monthOfYear)) / 12;
        //
        if (goForward)
        {
            goForward = false;
            future.worldMonth += Mathf.RoundToInt((daysAhead / 30f));
            future.worldMonth = (WorldMonth)((int)future.worldMonth % 12);
        }

    }
}
