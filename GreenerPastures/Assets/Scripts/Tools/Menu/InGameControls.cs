using UnityEngine;

public class InGameControls : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the mid-game menu player control listing

    public struct ControlItem
    {
        public string controlName;
        public string keyboardLabel;
        public string gamepadLabel;
    }

    public bool controlsDisplay;
    public ControlItem[] controlItems;

    private PlayerControlManager pcm;
    private MultiGamepad padMgr;
    private QuitOnEscape qoe;
    private InGameAlmanac iga;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- InGameControls [Start] : no gamepad manager found in scene. will ignore.");
        qoe = GameObject.FindFirstObjectByType<QuitOnEscape>();
        if (qoe == null)
        {
            Debug.LogError("--- InGameControls [Start] : no quit on escape tool found in scene. aborting.");
            enabled = false;
        }
        iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
        if (iga == null)
        {
            Debug.LogError("--- InGameControls [Start] : no in game almanac tool found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            ConfigureControlItems();
        }
    }

    void Update()
    {
        if (pcm == null)
            return;

        if (iga.showAlmanac)
        {
            controlsDisplay = false;
            return;
        }

        controlsDisplay = Input.GetKey(KeyCode.Tab);

        // control player hud
        if (controlsDisplay && !pcm.hidePlayerHUD)
            pcm.hidePlayerHUD = true;
        else if (pcm.hidePlayerHUD && Input.GetKeyUp(KeyCode.Tab))
            pcm.hidePlayerHUD = false;
    }

    public void SetPlayerControlManager( PlayerControlManager pControlManager )
    {
        pcm = pControlManager;
    }

    void ConfigureControlItems()
    {
        int totalItems = 13;
        controlItems = new ControlItem[totalItems];

        controlItems[0].controlName = "CHARACTER MOVEMENT";
        controlItems[0].keyboardLabel = "W-A-S-D";
        controlItems[0].gamepadLabel = "Left Joystick";
        controlItems[1].controlName = "TILL A PLOT / ITEM PICKUP";
        controlItems[1].keyboardLabel = "E Key";
        controlItems[1].gamepadLabel = "A Button";
        controlItems[2].controlName = "WATER A PLOT";
        controlItems[2].keyboardLabel = "F Key";
        controlItems[2].gamepadLabel = "B Button";
        controlItems[3].controlName = "HARVEST PLANT / ITEM DROP";
        controlItems[3].keyboardLabel = "C Key";
        controlItems[3].gamepadLabel = "X Button";
        controlItems[4].controlName = "DIG UP PLANT";
        controlItems[4].keyboardLabel = "V Key";
        controlItems[4].gamepadLabel = "Y Button";
        controlItems[5].controlName = "GRAFT FRUIT TO STALK";
        controlItems[5].keyboardLabel = "G Key";
        controlItems[5].gamepadLabel = "-TBD-";
        controlItems[6].controlName = "CAST MAGIC SPELL";
        controlItems[6].keyboardLabel = "Q Key";
        controlItems[6].gamepadLabel = "D-Pad Press";
        controlItems[7].controlName = "SELECT NEXT INVENTORY ITEM";
        controlItems[7].keyboardLabel = "[ Key";
        controlItems[7].gamepadLabel = "Left Bump";
        controlItems[8].controlName = "SELECT PREV INVENTORY ITEM";
        controlItems[8].keyboardLabel = "] Key";
        controlItems[8].gamepadLabel = "Right Bump";
        controlItems[9].controlName = "ZOOM VIEW IN";
        controlItems[9].keyboardLabel = "Mouse Wheel Up";
        controlItems[9].gamepadLabel = "D-Pad Up";
        controlItems[10].controlName = "ZOOM VIEW OUT";
        controlItems[10].keyboardLabel = "Mouse Wheel Down";
        controlItems[10].gamepadLabel = "D-Pad Down";
        controlItems[11].controlName = "BIOMANCER'S ALMANAC";
        controlItems[11].keyboardLabel = "\\ Key";
        controlItems[11].gamepadLabel = "BACK Button";
        controlItems[12].controlName = "QUIT GAME";
        controlItems[12].keyboardLabel = "ESC Key";
        controlItems[12].gamepadLabel = "START Button";
    }

    string GetControlName( int control )
    {
        return controlItems[control].controlName;
    }

    string GetKeyboardLabel( int control )
    {
        return controlItems[control].keyboardLabel;
    }

    string GetGamepadLabel( int control )
    {
        return controlItems[control].gamepadLabel;
    }

    void OnGUI()
    {
        if (pcm == null || iga.showAlmanac)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.125f * h;
        r.width = 0.8f * w;
        r.height = 0.75f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0,0,30,0);
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        string s = "PLAYER CONTROLS";

        Color c = Color.white;

        if (!controlsDisplay)
        {
            r.x = 0.025f * w;
            r.y = 0.05f * h;
            r.width = 0.15f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = "CONTROLS [TAB]";
            GUI.Label(r, s, g);
            return;
        }

        GUI.Box(r, s, g);

        r.x = 0.15f * w;
        r.y = 0.2f * h;
        r.width = 0.4f * w;
        r.height = 0.05f * h;

        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleLeft;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        for (int i = 0; i < controlItems.Length; i++)
        {
            s = GetControlName(i);
            GUI.Label(r, s, g);
            r.y += 0.05f * h;
        }

        r.x = 0.45f * w;
        r.y = 0.2f * h;
        g.alignment = TextAnchor.MiddleRight;

        for (int i = 0; i < controlItems.Length; i++)
        {
            if (padMgr != null && padMgr.gamepads[0].isActive)
                s = GetGamepadLabel(i);
            else
                s = GetKeyboardLabel(i);
            GUI.Label(r, s, g);
            r.y += 0.05f * h;
        }
    }
}
