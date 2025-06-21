using UnityEngine;
using UnityEngine.SceneManagement;

public class ProfileOptions : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the profile options screen

    public string titleText;
    public Rect title;
    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    public int titleFontSizeAt1024 = 60;

    // profile options elements
    // options control display
    // statistics popup (back button)

    public string backButtonText = "BACK";
    public Rect backButton;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    public int backButtonFontSizeAt1024 = 48;

    private SaveLoadManager saveMgr;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private bool statsPopup; // is displaying popup
    private bool popupEnabled; // is popping up or down
    private float popupTimer;
    private float popupProgress;
    private AnimationCurve popupCurve;

    const float POPUPTIME = 1f;


    void Start()
    {
        // validate
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr == null)
        {
            Debug.LogError("--- ProfileOptions [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- ProfileOptions [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    void Update()
    {
        // run popup timer
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0f)
            {
                popupTimer = 0f;
                popupEnabled = !popupEnabled;
                if (!popupEnabled)
                {
                    statsPopup = false;
                }
            }
            else
            {
                if (popupEnabled)
                    popupProgress = 1f - popupCurve.Evaluate(popupTimer / POPUPTIME);
                else
                    popupProgress = popupCurve.Evaluate(popupTimer / POPUPTIME);
            }
            popupProgress = Mathf.Clamp01(popupProgress);
        }

        // determine ui selection from game pad input
        if (padMgr.gPadDown[0].YaxisL > 0f)
        {
            padButtonSelection--;
            if (padButtonSelection < 0)
                padButtonSelection = padMaxButton;
        }
        else if (padMgr.gPadDown[0].YaxisL < 0f)
        {
            padButtonSelection++;
            if (padButtonSelection > padMaxButton)
                padButtonSelection = 0;
        }
    }

    void OnGUI()
    {
        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle();
        g.font = titleFont;
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        g.active.textColor = buttonFontColor;
        string s = titleText;
        Color c = Color.white;

        // title
        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        // PROFILE STATS DISPLAY
        if (statsPopup)
        {
            r = new Rect();
            r.x = 0.2f * w;
            r.y = (0.3f * h) + (popupProgress * 0.85f * h);
            r.width = 0.6f * w;
            r.height = 0.5f * h;
            g = new GUIStyle(GUI.skin.box);
            g.normal.textColor = buttonFontColor;
            g.hover.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            g.fontStyle = FontStyle.Bold;
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.padding = new RectOffset(0, 0, 20, 0);
            s = "Profile Statistics";
            GUI.color = Color.white;
            GUI.Box(r, s, g);

            // popup elements
            // statistics popup (back button)

            // popup buttons
            r.x = 0.25f * w;
            r.y += 0.0625f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            // ok
            if (padButtonSelection == 0)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "OK";
            if (GUI.Button(r, s, g) || (padMgr != null &&
                padMgr.gamepads[0].isActive && padButtonSelection == 0 &&
                padMgr.gPadDown[0].aButton))
            {
                popupTimer = POPUPTIME;
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    padButtonSelection = -1;
                    padMaxButton = 1;
                }
            }
            // cancel
            r.x += 0.3f * w;
            if (padButtonSelection == 1)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "CANCEL";
            if (GUI.Button(r, s, g) || (padMgr != null &&
                padMgr.gamepads[0].isActive && padButtonSelection == 1 &&
                padMgr.gPadDown[0].aButton))
            {
                popupTimer = POPUPTIME;
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    padButtonSelection = -1;
                    padMaxButton = 1;
                }
            }
        }
        else
        {
            // options control display
        }

        // back button
        r = backButton;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(backButtonFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        s = backButtonText;

        if (GUI.Button(r, s, g) ||
            padButtonSelection == 0 && padMgr.gPadDown[0].aButton)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
