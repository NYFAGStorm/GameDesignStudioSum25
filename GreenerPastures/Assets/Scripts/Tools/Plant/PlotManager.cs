using UnityEngine;

public class PlotManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a single plot of land that is able to hold a single plant



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
}
