using UnityEngine;

public class PlotManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plot of land that is able to hold a single plant

    public PlotData data;
    public GameObject plant;

    private Renderer cursor;
    private bool cursorActive;
    private float cursorTimer;

    const float CURSORPULSEDURATION = 0.5f;


    void Start()
    {
        // validate
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
        }
    }

    void Update()
    {
        CheckCursor();

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
            case PlotCondition.Growing:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Uprooted");
                // remove plant
                Destroy(plant); // temp
                data.plant = PlantSystem.InitializePlant();
                data.condition = PlotCondition.Uprooted;
                break;
            case PlotCondition.Uprooted:
                // change ground texture
                if (r == null)
                    Debug.LogWarning("--- PlotManager [WorkLand] : " + gameObject.name + " unable to accees ground renderer. will ignore.");
                else
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                data.condition = PlotCondition.Dirt;
                break;
        }       
    }
}
