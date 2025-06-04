using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the world time cycles and related world properties

    public GameObject skyLightObject;
    public Light sunLight;
    public Light moonLight;

    public float baseTemperature;
    public float dayProgress;
    public int dayOfMonth;
    public WorldMonth monthOfYear;
    public WorldSeason season;

    // TODO: link to game for world data, run time based on game seed
    //private long gameSeedTime;
    private float sunRotX;


    void Start()
    {
        // validate
        skyLightObject = GameObject.Find("Sky Lights");
        if (skyLightObject == null)
        {
            Debug.LogError("--- TimeManager [Start] : no Sky Lights object found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            
        }
    }

    void Update()
    {
        // temp
        // rotate sky lights object once per game day
        Vector3 sunRot = skyLightObject.transform.eulerAngles;
        sunRot.x += Time.deltaTime * 72f * (360f / (24f * 60f * 60f));
        skyLightObject.transform.eulerAngles = sunRot;
    }
}
