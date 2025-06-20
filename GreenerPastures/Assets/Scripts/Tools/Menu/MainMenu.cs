using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Interface/MainMenu")]
public class MainMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the main menu

    public enum ButtonAction
    {
        Default,
        SceneSwitch,
        Quit,
        PopupLaunch,
    }

    public string titleText;
    [Tooltip("These values represent a proportion of screen space, as percentages of screen space. Like, less than 1")]
    public Rect title;

    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int titleFontSizeAt1024 = 60;

    [System.Serializable]
    public struct ButtonDef
    {
        public string buttonText;
        public Rect buttonPos;
        public ButtonAction buttonAction;
        public string sceneName;
        public bool buttonEnabled;
        public bool buttonVisible;
    }
    public ButtonDef[] buttons;

    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    [Tooltip("Font will scale appropriately based on active screen width, using this 1024 size")]
    public int buttonFontSizeAt1024 = 48;

    private float sceneSwitchTimer;
    private string sceneSwitchName;

    private SaveLoadManager saveMgr;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 2;


    void Start()
    {
        // validate
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr == null)
        {
            Debug.LogError("--- MainMenu [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- MainMenu [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            // TODO: use game data
        }
    }

    void Update()
    {
        // run switch timer
        if ( sceneSwitchTimer > 0f )
        {
            sceneSwitchTimer -= Time.deltaTime;
            if ( sceneSwitchTimer < .1f )
                SceneManager.LoadScene(sceneSwitchName);
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
        if (sceneSwitchTimer > 0f)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.font = titleFont;
        g.fontStyle = titleFontStyle;
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w / 1024f));
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

        if (buttons == null || buttons.Length == 0)
            return;

        // buttons array
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));

        for ( int i=0; i<buttons.Length; i++)
        {
            r = buttons[i].buttonPos;
            r.x *= w;
            r.y *= h;
            r.width *= w;
            r.height *= h;

            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == i)
                g.normal.textColor = Color.white;
            g.active.textColor = buttonFontColor;

            s = buttons[i].buttonText;

            GUI.enabled = (buttons[i].buttonEnabled);

            if (buttons[i].buttonVisible && GUI.Button(r,s,g) ||
            padButtonSelection == i && padMgr.gPadDown[0].aButton)
            {
                if (buttons[i].sceneName != "")
                {
                    if ((buttons[i].sceneName == "GreenerGame"))
                    {
                        // little cinematic menu fun
                        GameObject.FindAnyObjectByType<MenuLayerManager>().targetKey = 2;
                        sceneSwitchName = buttons[i].sceneName;
                        sceneSwitchTimer = 4f;
                    }
                    else
                        SceneManager.LoadScene(buttons[i].sceneName);
                }
                else
                    Application.Quit();
            }
        }

        GUI.enabled = true;
    }
}
