using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the color of the far background layer elements

    public GameObject[] childObjects;
    public Color[] elementColors;

    private Color lowColor;

    private bool discoSky;
    private Color savedSkyTint = Color.black;
    private Color savedGroundColor = Color.black;

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
        }
    }

    void Update()
    {
        // update current colors
        for (int i = 0; i < childObjects.Length; i++)
        {
            childObjects[i].GetComponent<Renderer>().material.color = Color.Lerp((lowColor * elementColors[i]), elementColors[i], RenderSettings.ambientIntensity);
        }

        // disco sky
        if (discoSky)
        {
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
            float rFun = Mathf.Clamp01(Mathf.Sin((Time.time + 3.81f) * .3f * 2f * Mathf.PI));
            float gFun = Mathf.Clamp01(Mathf.Sin((Time.time + 6.18f) * .2f * 2f * Mathf.PI));
            float bFun = Mathf.Clamp01(Mathf.Sin(Time.time * .5f * 2f * Mathf.PI));
            c = savedSkyTint;
            c.r = rFun;
            c.g = gFun;
            c.b = bFun;
            RenderSettings.skybox.SetColor("_SkyTint", c);
        }
        else if (RenderSettings.skybox.GetColor("_GroundColor") != savedGroundColor)
        {
            // restore skybox colors
            RenderSettings.skybox.SetColor("_GroundColor", savedGroundColor);
            RenderSettings.skybox.SetColor("_SkyTint", savedSkyTint);
        }
    }

    /// <summary>
    /// Toggles sky to display lots of colors or not
    /// </summary>
    public void DiscoSky()
    {
        discoSky = !discoSky;
    }
}
