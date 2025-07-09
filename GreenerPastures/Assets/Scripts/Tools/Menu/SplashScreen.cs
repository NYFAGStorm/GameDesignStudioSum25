using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/SplashScreen")]
public class SplashScreen : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the splash screen

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    public string taglineText;
    public Rect tagline;

    public Font taglineFont;
    public FontStyle taglineFontStyle;
    public Color taglineFontColor = Color.white;
    public int taglineFontSizeAt1024 = 24;

    public Rect version;

    public Font versionFont;
    public FontStyle versionFontStyle;
    public Color versionFontColor = Color.gray;
    public int versionFontSizeAt1024 = 12;

    public string startButtonText = "START";
    public string destinationScene;
    public Rect startButton;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    public int buttonFontSizeAt1024 = 48;

    public string legaleseText = "New York Film Academy | Burbank | 3D Game Design Studio | Summer 2025";
    public Rect legalese;
    public Font legaleseFont;
    public FontStyle legaleseFontStyle = FontStyle.Normal;
    public Color legaleseFontColor = Color.white;
    public int legaleseFontSizeAt1024 = 14;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private bool splashLaunched;

    private SaveLoadManager saveMgr;

    private Texture2D[] buttonTex;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- SplashScreen [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr == null)
        {
            Debug.LogError("--- SplashScreen [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

            // GUI Button Textures for build
            if (!Application.isEditor)
            {
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }
        }
    }

    void Update()
    {
        if (splashLaunched)
            SceneManager.LoadScene(destinationScene);

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

    string GetVersionText()
    {
        string retText = "";

        retText = "version ";
        retText += saveMgr.GetVersionNumber();

        return retText;
    }

    void OnGUI()
    {
        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.font = titleFont;
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w/1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = titleFontColor;
        g.hover.textColor = titleFontColor;
        g.active.textColor = titleFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        r = tagline;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g.font = taglineFont;
        g.fontStyle = taglineFontStyle;
        g.fontSize = Mathf.RoundToInt(taglineFontSizeAt1024 * (w / 1024f));
        g.normal.textColor = taglineFontColor;
        g.hover.textColor = taglineFontColor;
        g.active.textColor = taglineFontColor;
        s = taglineText;

        GUI.color = Color.white;
        GUI.Label(r, s, g);

        r = version;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g.font = versionFont;
        g.fontStyle = versionFontStyle;
        g.fontSize = Mathf.RoundToInt(versionFontSizeAt1024 * (w / 1024f));
        g.normal.textColor = versionFontColor;
        g.hover.textColor = versionFontColor;
        g.active.textColor = versionFontColor;
        s = GetVersionText();

        GUI.color = Color.white;
        GUI.Label(r, s, g);

        r = startButton;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));
        g.normal.textColor = buttonFontColor;
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = startButtonText;

        if (GUI.Button(r,s,g) || 
            padMgr != null && padMgr.gPadDown[0].aButton)
        {
            // little cinematic menu fun
            GameObject.FindAnyObjectByType<MenuLayerManager>().targetKey = 1;
            splashLaunched = true;
        }

        r = legalese;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.label);
        g.font = legaleseFont;
        g.fontStyle = legaleseFontStyle;
        g.fontSize = Mathf.RoundToInt(legaleseFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = legaleseFontColor;
        g.hover.textColor = legaleseFontColor;
        g.active.textColor = legaleseFontColor;
        s = legaleseText;

        GUI.color = Color.white;
        GUI.Label(r, s, g);
    }
}
