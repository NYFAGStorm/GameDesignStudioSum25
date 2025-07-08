using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the color of the far background layer elements

    public GameObject[] childObjects;
    public Color[] elementColors;

    private Color lowColor;

    private bool discoSky; // silly cheat code
    private float cloudCover;
    private float daylight; // value from time manager

    private Color savedSkyTint;
    private Color savedGroundColor;

    const float LOWRED = 0.01f;
    const float LOWGREEN = 0.015f;
    const float LOWBLUE = 0.025f;
    const float LOWALPHA = 1f;


    void OnDisable()
    {
        // restore skybox colors
        RenderSettings.skybox.SetColor("_GroundColor", savedGroundColor);
        RenderSettings.skybox.SetColor("_SkyTint", savedSkyTint);
    }

    void Start()
    {
        // validate
        if (gameObject.transform.childCount == 0)
        {
            Debug.LogError("--- BackgroundManager [Start] : no child objects found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            childObjects = new GameObject[gameObject.transform.childCount];
            elementColors = new Color[gameObject.transform.childCount];
            for ( int i = 0; i < childObjects.Length; i++ )
            {
                childObjects[i] = gameObject.transform.GetChild(i).gameObject;
                elementColors[i] = childObjects[i].GetComponent<Renderer>().material.color;
            }
            lowColor = new Color(LOWRED,LOWGREEN,LOWBLUE,LOWALPHA);
            // skybox color save
            savedGroundColor = RenderSettings.skybox.GetColor("_GroundColor");
            savedSkyTint = RenderSettings.skybox.GetColor("_SkyTint");
            ConfigureSkyboxColors();
        }
    }

    void ConfigureSkyboxColors()
    {
        // green ground
        Color c = Color.HSVToRGB(150f / 255f, 38f / 255f, 62f / 255f);
        RenderSettings.skybox.SetColor("_GroundColor", c);
        c = savedSkyTint;
        RenderSettings.skybox.SetColor("_SkyTint", c);
    }

    void Update()
    {
        // update cloud cover lighting
        UpdateCloudCoverLighting();
        
        // update bg layer colors based on ambient light intensity
        for (int i = 0; i < childObjects.Length; i++)
        {
            childObjects[i].GetComponent<Renderer>().material.color =
                Color.Lerp((lowColor * elementColors[i]), elementColors[i], RenderSettings.ambientIntensity);
        }

        // fun disco sky
        if (discoSky)
            UpdateDiscoSky();
    }

    /// <summary>
    /// Sets the cloud cover
    /// </summary>
    /// <param name="cloudAmount">cloud cover 0-1</param>
    public void SetCloudCover(float cloudAmount, float dLight)
    {
        cloudCover = cloudAmount;
        daylight = dLight;

        // settle noisy near-zero values
        if (cloudCover < 0.01f)
            cloudCover = 0f;
        if (daylight < 0.001f)
            daylight = 0f;
    }

    void UpdateCloudCoverLighting()
    {
        // handle cloud cover effects
        RenderSettings.fog = (cloudCover > 0f);
        if (cloudCover > 0f)
        {
            // update fog for cloud cover
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = Mathf.Clamp(700f + (cloudCover * -1400f), -700f, 700f);
            CameraManager cm = GameObject.FindFirstObjectByType<CameraManager>();
            if (cm.mode == CameraManager.CameraMode.PanFollow)
                RenderSettings.fogStartDistance = 30f;
            RenderSettings.fogEndDistance = 700f;
            Color cloudGray = new Color(.27f, .33f, .381f); // full day color
            // fog color darker at night
            Color darkerGray = Color.Lerp(cloudGray, cloudGray * 0.1f, 1f - daylight);
            darkerGray.a = 1f;
            RenderSettings.fogColor = Color.Lerp(savedSkyTint, darkerGray, cloudCover);

            // update sky tint for cloud cover
            Color c = Color.Lerp(savedSkyTint, cloudGray, cloudCover);
            RenderSettings.skybox.SetColor("_SkyTint", c);
            // update skybox atmosphere and exposure for ugly gray
            // default 1
            RenderSettings.skybox.SetFloat("_AtmosphereThickness", 1f + (cloudCover * .33f));
            // default 1.3
            RenderSettings.skybox.SetFloat("_Exposure", 1.3f - (cloudCover * 1.27f));
        }
        else
            RenderSettings.skybox.SetFloat("_Exposure", 1.3f);
    }

    /// <summary>
    /// Toggles sky to display lots of colors or not
    /// </summary>
    public void DiscoSky()
    {
        discoSky = !discoSky;
        if (!discoSky)
        {
            // restore skybox colors
            RenderSettings.skybox.SetColor("_GroundColor", savedGroundColor);
            RenderSettings.skybox.SetColor("_SkyTint", savedSkyTint);
        }
    }

    void UpdateDiscoSky()
    {
        if (!discoSky)
            return;
        // affect skybox material colors
        Color c = Color.white;
        if (savedGroundColor == Color.black)
        {
            savedGroundColor = RenderSettings.skybox.GetColor("_GroundColor");
            c = Color.HSVToRGB(150f / 255f, 38f / 255f, 62f / 255f);
            RenderSettings.skybox.SetColor("_GroundColor", c);
        }
        if (savedSkyTint == Color.black)
            savedSkyTint = RenderSettings.skybox.GetColor("_SkyTint");
        float rFun = Mathf.Clamp01(Mathf.Sin((Time.time + 3.81f) * .381f * 2f * Mathf.PI));
        float gFun = Mathf.Clamp01(Mathf.Sin((Time.time + 6.18f) * .25f * 2f * Mathf.PI));
        float bFun = Mathf.Clamp01(Mathf.Sin(Time.time * .618f * 2f * Mathf.PI));
        c = savedSkyTint;
        c.r = rFun;
        c.g = gFun;
        c.b = bFun;
        RenderSettings.skybox.SetColor("_SkyTint", c);
    }
}
