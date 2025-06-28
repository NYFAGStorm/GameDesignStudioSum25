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

    public string backButtonText = "BACK";
    public Rect backButton;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    public int backButtonFontSizeAt1024 = 48;

    private SaveLoadManager saveMgr;
    private ProfileData currentProfile;
    private ProfileOptionsData profileOptions;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private string[] micNames = new string[0];
    private int currentMicIndex = -1;
    private float micCheckTimer;

    // REVIEW: there are only player stats per game, no profile stats
    private bool statsPopup; // is displaying popup
    private bool popupEnabled; // is popping up or down
    private float popupTimer;
    private float popupProgress;
    private AnimationCurve popupCurve;

    const float POPUPTIME = 1f;
    const float MICCHECKTIME = 1.5f;


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
            //
            currentProfile = saveMgr.GetCurrentProfile();
            profileOptions = currentProfile.options;
            //
            MicConfig();
            micCheckTimer = MICCHECKTIME;
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

        // run mic check timer
        if (micCheckTimer > 0f)
        {
            micCheckTimer -= Time.deltaTime;
            if (micCheckTimer < 0f)
            {
                micCheckTimer = MICCHECKTIME;
                // mic status update
                MicConfig();
            }
        }
    }

    void MicConfig()
    {
        micNames = Microphone.devices;
        for (int i = 0; i < micNames.Length; i++)
        {
            string s = micNames[i];
            int pos = s.IndexOf('(');
            pos++;
            if (pos + 18 < s.Length - 1)
                s = s.Substring(pos, 18);
            else
                s = s.Substring(pos, s.Length - pos);
            s = s.Replace(")", "");
            micNames[i] = s;
        }
        if (micNames.Length > 0)
            currentMicIndex = 0;
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

        // POPUP DISPLAY (REVIEW: there are no profile stats)
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

            return; // hide other profile options elements
        }

        // options control labels
        r.x = 0.25f * w;
        r.y = 0.2f * h;
        r.width = 0.5f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.label);
        g.normal.textColor = titleFontColor;
        g.hover.textColor = titleFontColor;
        g.active.textColor = titleFontColor;
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
        g.padding = new RectOffset(0, 20, 0, 0);
        GUI.color = Color.white;
        //profile name
        s = "CURRENT PROFILE : '" + currentProfile.loginName + "'";
        GUI.Label(r, s, g);
        //micAvailable
        r.y += 0.1f * h;
        r.width = 0.225f * w;
        g.alignment = TextAnchor.MiddleRight;
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
        s = "Microphone Available :";
        GUI.Label(r, s, g);
        //configuredMicName
        r.y += 0.1f * h;
        s = "Configured Mic Name :";
        GUI.Label(r, s, g);
        //micEnabled
        r.y += 0.1f * h;
        s = "Microphone Enabled :";
        GUI.Label(r, s, g);
        //voiceChatMuted
        r.y += 0.1f * h;
        s = "Voice Chat Muted :";
        GUI.Label(r, s, g);

        // REVIEW: test button and meter?

        // mic available is a label as well
        r.x = 0.5125f * w;
        r.y = 0.3f * h;
        r.width = 0.2f * w;
        g.alignment = TextAnchor.MiddleCenter;
        s = "YES";
        if (!profileOptions.micAvailable)
            s = "NO";
        GUI.Label(r, s, g);

        // options control buttons
        //r.x = 0.525f * w;
        //r.y = 0.2f * h;
        //r.width = 0.225f * w;
        //r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.button);
        g.normal.textColor = buttonFontColor;
        g.hover.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
        GUI.color = Color.white;
        //configuredMicName
        r.y += 0.1f * h;
        s = profileOptions.configuredMicName;
        GUI.enabled = (profileOptions.micAvailable && micNames.Length > 1);
        if (GUI.Button(r, s, g))
        {
            currentMicIndex++;
            // increment and wrap around
            if (currentMicIndex >= micNames.Length)
                currentMicIndex = 0;
            if (currentMicIndex < micNames.Length)
                profileOptions.configuredMicName = micNames[currentMicIndex];
            else
                currentMicIndex = -1;
        }
        GUI.enabled = true;
        //micEnabled
        r.y += 0.1f * h;
        s = "Enabled";
        if (!profileOptions.micEnabled)
            s = "Disabled";
        if (GUI.Button(r, s, g))
        {
            profileOptions.micEnabled = !profileOptions.micEnabled;
        }
        //voiceChatMuted
        r.y += 0.1f * h;
        s = "Open";
        if (!profileOptions.voiceChatMuted)
            s = "Muted";
        if (GUI.Button(r, s, g))
        {
            profileOptions.voiceChatMuted = !profileOptions.voiceChatMuted;
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
