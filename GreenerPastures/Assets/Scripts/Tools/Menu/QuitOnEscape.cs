using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("NYFA Studio/Game/QuitOnEscape")]
public class QuitOnEscape : MonoBehaviour
{
    // Author: Glenn Storm
    // This quits to the main menu if ESC is pressed

    private bool popup;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 1;
    private bool padPressed;

    const int FONTSIZEAT1024 = 36;


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
        if ( Input.GetKeyUp(KeyCode.Escape) || 
            (padMgr != null && padMgr.gamepads[0].startButton))
			popup = true;

        // game pad input
        if (padPressed)
        {
            if (!padMgr.gamepads[0].LBump && !padMgr.gamepads[0].RBump)
                padPressed = false;
            return;
        }
        if (padMgr.gamepads[0].LBump)
        {
            padButtonSelection--;
            if (padButtonSelection < 0)
                padButtonSelection = padMaxButton;
            padPressed = true;
        }
        else if (padMgr.gamepads[0].RBump)
        {
            padButtonSelection++;
            if (padButtonSelection > padMaxButton)
                padButtonSelection = 0;
            padPressed = true;
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
        g.fontSize = Mathf.RoundToInt( FONTSIZEAT1024 * (w/1024f) );
        g.alignment = TextAnchor.UpperCenter;
        g.normal.textColor = Color.white;
        g.active.textColor = Color.white;
        GUI.Box(r, s, g);

        r.x = 0.25f * w;
        r.y = 0.55f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (padButtonSelection == 0)
            g.normal.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "QUIT";

        if (GUI.Button(r, s, g) ||
            padButtonSelection == 0 && padMgr.gamepads[0].aButton)
        {
            popup = false;
            SceneManager.LoadScene("Splash");
        }

        r.x = 0.55f * w;
        g.normal.textColor = Color.white;
        if (padButtonSelection == 1)
            g.normal.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "CANCEL";

        if ( GUI.Button(r,s,g) ||
            padButtonSelection == 1 && padMgr.gamepads[0].aButton)
        {
            popup = false;
        }
    }
}
