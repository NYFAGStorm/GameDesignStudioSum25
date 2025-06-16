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

    public string startButtonText = "START";
    public string destinationScene;
    public Rect startButton;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    public int buttonFontSizeAt1024 = 48;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private bool splashLaunched;


    void Start()
    {
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- SplashScreen [Start] : no pad manager found in scene. aborting.");
            enabled = false;
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

    void OnGUI()
    {
        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle();
        g.font = titleFont;
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w/1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = titleFontColor;
        g.active.textColor = titleFontColor;
        string s = titleText;

        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

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
        s = startButtonText;

        if (GUI.Button(r,s,g) || 
            padMgr != null && padMgr.gPadDown[0].aButton)
        {
            // little cinematic menu fun
            GameObject.FindAnyObjectByType<MenuLayerManager>().targetKey = 1;
            splashLaunched = true;
        }
    }
}
