using UnityEngine;

public class PlotManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plot of land that is able to hold a single plant

    public PlotData data;
    public GameObject plant;

    private TimeManager tim;

    private Renderer cursor;
    private bool cursorActive;
    private float cursorTimer;
    private float plotTimer;

    const float CURSORPULSEDURATION = 0.5f;
    // temp use time manager multiplier
    const float WATERDRAINRATE = 0.25f;
    const float PLOTCHECKINTERVAL = 1f;


    void Start()
    {
        // validate
        tim = GameObject.FindAnyObjectByType<TimeManager>();
        if ( tim == null )
        {
            Debug.LogError("--- PlotManager [Start] : no time manager found in scene. aborting.");
            enabled = false;
        }
        cursor = gameObject.transform.Find("Cursor").GetComponent<Renderer>();
        if ( cursor == null )
        {
            Debug.LogError("--- PlotManager [Start] : no cursor child found. aborting.");
            enabled = false;
        }
        // inititalize
        if (enabled)
        {
            data = FarmSystem.InitializePlot();
            cursor.enabled = false;
            plotTimer = 0.1f;
        }
    }

    void Update()
    {
        CheckCursor();

        // run plot timer
        if ( plotTimer > 0f )
        {
            plotTimer -= Time.deltaTime;
            if ( plotTimer < 0f )
            {
                plotTimer = PLOTCHECKINTERVAL;
                // set sun
                data.sun = Mathf.Clamp01(Mathf.Sin((tim.dayProgress - .25f) * 2f * Mathf.PI));
                // water drain
                data.water = Mathf.Clamp01(data.water - (WATERDRAINRATE * Time.deltaTime * PLOTCHECKINTERVAL));
            }
        }
    }

    void CheckCursor()
    {
        // ignore if not active
        if (!cursorActive)
            return;
        // highlight cursor pulse
        cursorTimer += Time.deltaTime;
        float pulse = 1f - ((cursorTimer % CURSORPULSEDURATION) * 0.9f);
        SetCursorHighlight(pulse);
    }

    public void SetCursorPulse( bool active )
    {
        cursorActive = active;
        SetCursorHighlight(0f);
        cursorTimer = 0f;
        cursor.enabled = active;
    }

    void SetCursorHighlight( float value )
    {
        Color c = cursor.material.color;
        // REVIEW: with better art
        c.r = 1f;
        c.g = 1f;
        c.b = value;
        c.a = value;
        cursor.material.color = c;
    }

    /// <summary>
    /// Works the land, turning from wild to dirt to tilled
    /// </summary>
    public void WorkLand()
    {
        Renderer r = gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>();
        switch (data.condition)
        {
            case PlotCondition.Default:
                // should never be here
                break;
            case PlotCondition.Wild:
                // remove wild grasses
                GameObject grasses = gameObject.transform.Find("Plot Wild Grasses").gameObject;
                if (grasses == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " no wild grasses found. will ignore.");
                else
                    Destroy(grasses);
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                data.condition = PlotCondition.Dirt;
                break;
            case PlotCondition.Dirt:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Tilled");
                data.condition = PlotCondition.Tilled;
                break;
            case PlotCondition.Tilled:
                // plant seed
                plant = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                plant.transform.parent = transform;
                plant.transform.position = transform.position;
                data.condition = PlotCondition.Growing;
                break;
            case PlotCondition.Uprooted:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                data.condition = PlotCondition.Dirt;
                break;
            default:
                break;
        }       
    }

    /// <summary>
    /// Waters this plot
    /// </summary>
    public void WaterPlot()
    {
        data.water = 1f;
    }

    public void HarvestPlant()
    {
        if ( data.condition == PlotCondition.Growing )
        {
            // harvest if plant is 100% grown and not yet harvested
            if (data.plant.growth < 1f)
                return;
            if (data.plant.segment == PlantSegment.Default)
            {
                data.plant.segment = PlantSegment.Stalk;
                plant.transform.Find("Plant Sprite").GetComponent<Renderer>().material.mainTexture = (Texture2D)Resources.Load("ProtoPlant_Stalk");
                // player would collect fruit as inventory at this point

                print("- player harvests plant fruit of "+(Mathf.RoundToInt(data.plant.quality*10000f)/100f)+"% quality. -");
            }
        }
    }

    public void UprootPlot()
    {
        Renderer r = gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>();
        // change ground texture
        if (r == null)
            Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
        else
            r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Uprooted");
        // player would collect stalk or full plant as inventory at this point
        // remove plant
        Destroy(plant);
        data.plant = PlantSystem.InitializePlant();
        data.condition = PlotCondition.Uprooted;
    }
}
