using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Game/QuitOnEscape")]
public class QuitOnEscape : MonoBehaviour
{
    // Author: Glenn Storm
    // This quits to the main menu if ESC is pressed

    public Font textFont;
    public FontStyle textStyle;
    public Color textColor = Color.white;

    private bool popup;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 1;

    const int FONTSIZEAT1024 = 36;


    void Start()
    {
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        // TODO: change this to error and abort if no gamepad manager found (allow no pad for testing)
        // (then clean up below checks for padMgr existing)
        if (padMgr == null)
            Debug.LogWarning("--- QuitOnEscape [Start] : no pad manager found in scene. will ignore.");
    }

    void Update()
    {
        if ( Input.GetKeyUp(KeyCode.Escape) || 
            (padMgr != null && padMgr.gamepads[0].startButton))
			popup = true;

        // determine ui selection from game pad input
        if (padMgr != null)
        {
            if (padMgr.gPadDown[0].XaxisL < 0f)
            {
                padButtonSelection--;
                if (padButtonSelection < 0)
                    padButtonSelection = padMaxButton;
            }
            else if (padMgr.gPadDown[0].XaxisL > 0f)
            {
                padButtonSelection++;
                if (padButtonSelection > padMaxButton)
                    padButtonSelection = 0;
            }
        }
    }

    void OnGUI()
    {
        if (!popup)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle();
        string s = "\nAre You Sure You Want To Quit?";

        r.x = 0.2f * w;
        r.y = 0.3f * h;
        r.width = 0.6f * w;
        r.height = 0.4f * h;
        g = GUI.skin.box;
        g.font = textFont;
        g.fontStyle = textStyle;
        g.fontSize = Mathf.RoundToInt( FONTSIZEAT1024 * (w/1024f) );
        g.alignment = TextAnchor.UpperCenter;
        g.normal.textColor = Color.white;
        g.active.textColor = Color.white;
        GUI.Box(r, s, g);

        r.x = 0.25f * w;
        r.y = 0.55f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.button);
        g.font = textFont;
        g.fontStyle = textStyle;
        g.fontSize = Mathf.RoundToInt(FONTSIZEAT1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = textColor;
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = textColor;
        s = "QUIT";

        if (GUI.Button(r, s, g) ||
            (padMgr != null && padButtonSelection == 0 && padMgr.gPadDown[0].aButton))
        {
            popup = false;
            SceneManager.LoadScene("Splash");
        }

        r.x = 0.55f * w;
        g.normal.textColor = textColor;
        if (padButtonSelection == 1)
            g.normal.textColor = Color.white;
        g.active.textColor = textColor;
        s = "CANCEL";

        if ( GUI.Button(r,s,g) || 
            (padMgr != null && padButtonSelection == 1 && padMgr.gPadDown[0].aButton ) )
        {
            popup = false;
        }
    }
}
