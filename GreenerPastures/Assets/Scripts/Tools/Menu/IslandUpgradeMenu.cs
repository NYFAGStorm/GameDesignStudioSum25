using UnityEngine;

public class IslandUpgradeMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the island upgrade menu

    private PlayerControlManager pcm;
    private GreenerGameManager ggm;
    private IslandManager im;

    private MultiGamepad padMgr;
    private bool usingPad;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;
    private int padClickButton = -1;
    private int padMove = -1;

    private AudioManager sfxAudio;

    private SalesVisitManager salesVisit;
    private bool menuOpen;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no player control manager found in scene. aborting.");
            enabled = false;
        }
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no game manager manager found in scene. aborting.");
            enabled = false;
        }
        im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no island manager manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no multigamepad manager manager found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {

        }
        else if (salesVisit != null)
            salesVisit.IslandMenuClosed(); // abort menu operation, free the salesman
    }

    public void ConfigSalesVisit( SalesVisitManager sales )
    {
        salesVisit = sales;
        menuOpen = true;
    }

    void SignalMenuClose()
    {
        menuOpen = false;
        salesVisit.IslandMenuClosed();
        Destroy(gameObject, 1f);
    }

    void Update()
    {

        // gamepad control
        usingPad = false;
        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            usingPad = true;   
            padClickButton = -1;
            padMove = -1;
            // selection movement
            if (padMgr.gPadDown[0].YaxisL > 0f)
                padMove = 0; // up
            if (padMgr.gPadDown[0].YaxisL < 0f)
                padMove = 1; // down
            // consume input (for some reason)
            padMgr.gPadDown[0].YaxisL = 0f;
            if (padMgr.gPadDown[0].XaxisL < 0f)
                padMove = 2; // left
            if (padMgr.gPadDown[0].XaxisL > 0f)
                padMove = 3; // right
            // consume input (for some reason)
            padMgr.gPadDown[0].XaxisL = 0f;
            // general button navigation
            if (padMove == 0)
                padButtonSelection--;
            if (padMove == 1)
                padButtonSelection++;
            padButtonSelection = Mathf.Clamp(padButtonSelection,0,padMaxButton);
            // button press
            if (padMgr.gPadDown[0].aButton)
            {
                padClickButton = padButtonSelection;
                // consume input (for some reason)
                padMgr.gPadDown[0].aButton = false;
            }
        }

    }

    void OnGUI()
    {
        if (!menuOpen)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.1f * h;
        r.width = 0.8f * w;
        r.height = 0.875f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.padding = new RectOffset(0, 0, 30, 0);
        g.fontSize = Mathf.RoundToInt(20 * (w/1024f));
        g.fontStyle = FontStyle.Bold;
        //g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        // menu box
        s = "ISLAND UPGRADES";
        GUI.Box(r, s, g);

        // 

        // close menu button
        GUI.enabled = true;

        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        //g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (usingPad && padButtonSelection == padMaxButton)
            g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = "End Island Upgrades";
        if (GUI.Button(r, s, g) || 
            (usingPad && padClickButton == padMaxButton))
        {
            SignalMenuClose();
        }
    }
}
