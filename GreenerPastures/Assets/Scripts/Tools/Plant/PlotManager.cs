using UnityEngine;

public class PlotManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plot of land that is able to hold a single plant

    public enum CurrentAction
    {
        None,
        Working,
        Planting,
        Watering,
        Harvesting,
        Uprooting
    }

    public PlotData data;
    public GameObject plant;

    private TimeManager tim;

    private Renderer cursor;
    private bool cursorActive;
    private float cursorTimer;
    private float plotTimer;

    private CurrentAction action;
    private bool actionDirty; // complete, not yet cleared
    private bool actionClear; // no action pressed
    private bool actionProgressDisplay;
    private float actionProgress;
    private float actionLimit;
    private string actionLabel;
    private float actionCompleteTimer;

    const float CURSORPULSEDURATION = 0.5f;
    // temp use time manager multiplier
    const float WATERDRAINRATE = 0.25f;
    const float PLOTCHECKINTERVAL = 1f;
    // action hold time windows to complete
    const float WORKLANDWINDOW = 2f;
    const float WATERWINDOW = 1f;
    const float PLANTWINDOW = .5f;
    const float HARVESTWINDOW = 1.5f;
    const float UPROOTWINDOW = 2.5f;
    const float ACTIONCOMPLETEDURATION = 1f;


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

        // run action complete timer
        if ( actionCompleteTimer > 0f )
        {
            actionCompleteTimer -= Time.deltaTime;
            if ( actionCompleteTimer < 0f )
            {
                actionCompleteTimer = 0f;
                actionLabel = "";
                actionProgressDisplay = false;
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
        ActionClear();
        actionProgressDisplay = false;
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
    /// Signals that no player plot action is currently active
    /// </summary>
    public void ActionClear()
    {
        actionDirty = false;
        actionClear = true;
        actionProgress = 0f;
        actionLimit = 5f;
        action = CurrentAction.None;
    }

    bool ActionComplete( float limit, string label )
    {
        bool retBool = false;

        actionProgressDisplay = true;
        actionLimit = limit;
        actionLabel = label;
        actionProgress += Time.deltaTime;
        if (actionProgress >= actionLimit)
        {
            actionCompleteTimer = ACTIONCOMPLETEDURATION;
            actionDirty = true;
            actionClear = false;
            action = CurrentAction.None;
            retBool = true;
        }

        return retBool;
    }

    /// <summary>
    /// Works the land, turning from wild to dirt to tilled
    /// </summary>
    public void WorkLand()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        if (data.condition == PlotCondition.Tilled)
        {
            if (action != CurrentAction.Planting && action != CurrentAction.None)
                return;
            action = CurrentAction.Planting;

            if (!ActionComplete(PLANTWINDOW, "PLANTING..."))
                return;
        }
        else
        {
            // cannot work the land if planted
            if (plant != null)
                return;

            if (action != CurrentAction.Working && action != CurrentAction.None)
                return;
            action = CurrentAction.Working;

            if (!ActionComplete(WORKLANDWINDOW, "WORKING..."))
                return;
        }

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
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        if (action != CurrentAction.Watering && action != CurrentAction.None)
            return;
        action = CurrentAction.Watering;

        if (!ActionComplete(WATERWINDOW, "WATERING..."))
            return;

        data.water = 1f;
    }

    public void HarvestPlant()
    {
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        // cannot harvest unless plant at 100% growth
        if (data.plant.growth < 1f)
            return;

        if (action != CurrentAction.Harvesting && action != CurrentAction.None)
            return;
        action = CurrentAction.Harvesting;

        if (!ActionComplete(HARVESTWINDOW,"HARVESTING..."))
            return;

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
        if (actionDirty)
            return;

        if (actionCompleteTimer > 0f && !actionClear)
            return;

        // cannot uproot unless plant exists in this plot
        if (plant == null)
            return;

        if (action != CurrentAction.Uprooting && action != CurrentAction.None)
            return;
        action = CurrentAction.Uprooting;

        if (!ActionComplete(UPROOTWINDOW, "UPROOTING..."))
            return;

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

    void OnGUI()
    {
        if (!actionProgressDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        string s = actionLabel;

        // locate display over plot
        Vector3 disp = Camera.main.WorldToViewportPoint(gameObject.transform.position);
        disp.y = 1f - disp.y;

        r.x = (disp.x - 0.05f) * w;
        r.y = disp.y * h;
        r.y -= 0.25f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;

        // display action label with drop shadow
        GUI.color = Color.black;
        r.x += 0.000618f * w;
        r.y += 0.001f * h;
        GUI.Label(r,s,g);
        GUI.color = Color.white;
        r.x -= 0.0012382f * w;
        r.y -= 0.002f * h;
        GUI.Label(r,s,g);

        if (actionCompleteTimer != 0f && 
            actionCompleteTimer < ACTIONCOMPLETEDURATION * 0.5f)
            return;

        r.y += 0.01f * h;
        r.height = 0.08f * h;
        Texture2D t = Texture2D.grayTexture;
        Color c = Color.white;
        c.a = 0.618f;
        GUI.color = c;

        // display progress bar background
        GUI.depth = 2;
        GUI.DrawTexture(r, t);

        t = Texture2D.whiteTexture;
        c = Color.blue;
        c.r = 0.1f;
        c.g = 0.1f;
        if (actionCompleteTimer > 0f)
            c.b = 0f;
        c.r = (actionCompleteTimer / ACTIONCOMPLETEDURATION);
        c.g = (actionCompleteTimer / ACTIONCOMPLETEDURATION);
        c.a = 1f;
        GUI.color = c;

        // display progress bar foreground
        r.y += 0.03f * h;
        r.height = 0.05f * h;
        GUI.depth = 1;
        // scale and position for 'inside' bg bar
        r.x += 0.00618f * w;
        r.y += 0.01f * h;
        r.width -= 0.01238f * w;
        r.height -= 0.02f * h;
        // scale for progress
        r.width = (actionProgress / actionLimit) * r.width;
        GUI.DrawTexture(r, t);
    }
}
