using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiGamepad : MonoBehaviour
{
    // Author: Glenn Storm
    // This manages the connection to multiple game pad inputs

    [System.Serializable]
    public struct GamepadStatus
    {
        public string playerName;
        public bool isActive;
        public bool startButton;
        public bool backButton;
        public bool modeButton;
        public bool centerButton;
        public float XaxisL;
        public float YaxisL;
        public bool LJoyPress;
        public float XaxisR;
        public float YaxisR;
        public bool RJoyPress;
        public bool DpadPress;
        public bool DpadUp;
        public bool DpadDown;
        public bool DpadLeft;
        public bool DpadRight;
        public bool aButton;
        public bool bButton;
        public bool xButton;
        public bool yButton;
        public bool LBump;
        public float LTrigger;
        public bool RBump;
        public float RTrigger;
    }

    public int numPads;
    public bool p1PadActive;
    public bool p2PadActive;
    public GamepadStatus[] gamepads = new GamepadStatus[2];
    public Texture2D gamepadIcon;
    public bool hideCursorIfGamepad;

    private int p1JoyIndex;
    private int p2JoyIndex;
    private bool showDebugLogs;
    private float checkTimer = 1f;
    private string[] controllerNames;
    private int oldnumPads;
    private float iconTimer;
    private bool disconnectedPad;

    const float CHECKINTERVAL = 3f;
    const float ICONDISPLAYTIME = 2f;


    void OnDisable()
    {
        CursorHide(false);
    }

    void Start()
    {
        // EXAMPLE USE:
        // We will have a prefab of the "Gamepad Manager" in our Splash scene,
        //    but if needed for testing, you can always drag on in from Prefabs/UI
        // In your script, have a global private MultiGamepad variable, called "padMgr"
        // In your script's Start() function, assign padMgr using this line:
        //    padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        // ... but next line, just validate it exists, like this:
        //    if ( padMgr == null ) { Debug.LogError("- ["+gameObject.name+"] : no pad manager. aborting."); enabled = false; }
        // after this, it is guaranteed to exist and be accessible, like this:
        //    if ( padMgr.numPads > 0 && padMgr.p1PadActive ) { print("p1 Left axis X value is "+padMgr.gamepads[0].XaxisL);

        // validate
        if (gamepadIcon == null)
            Debug.LogWarning("--- MultiGamepad [Start] : no gamepad icon configured. will ignore.");

        // initialize
        if (gameObject.GetComponent<SingletonObject>() == null)
        {
            gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        if (showDebugLogs)
            Debug.Log("--- MultiGamepad [Start] : game pad input system available.");
    }

    void Update()
    {
        // run check timer
        if (checkTimer > 0f)
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0f)
                checkTimer = CHECKINTERVAL;
            UpdatePads();

            if (oldnumPads != numPads)
            {
                if (showDebugLogs)
                    Debug.Log("--- MultiGamepad [Update] : gamepads active = " + numPads);
                disconnectedPad = (oldnumPads > numPads);
                iconTimer = ICONDISPLAYTIME;
                oldnumPads = numPads;
            }

            // controller status
            GetPadStatus();
        }

        if (gamepadIcon != null && iconTimer != 0f)
        {
            // run gamepad icon timer
            if (iconTimer > 0f)
                iconTimer -= Time.deltaTime;
            if (iconTimer < 0f)
                iconTimer = 0f;
        }
    }

    void CursorHide(bool hideCursor)
    {
        if (hideCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void UpdatePads()
    {
        controllerNames = Input.GetJoystickNames();
        numPads = 0;
        p1JoyIndex = -1;
        p2JoyIndex = -1;
        for ( int i=0; i<controllerNames.Length; i++ )
        {
            if (controllerNames[i] != "" )
            {
                if (p1JoyIndex == -1)
                    p1JoyIndex = i;
                else if (p2JoyIndex == -1)
                    p2JoyIndex = i;
            }
        }

        // init gamepade data
        gamepads[0].isActive = false;
        gamepads[1].isActive = false;
        gamepads[0].playerName = "";
        gamepads[1].playerName = "";

        if (controllerNames != null && controllerNames.Length > 0)
        {
            for (int i = 0; i < controllerNames.Length; i++)
            {
                if (controllerNames[i] != null && controllerNames[i] != "")
                    numPads++;
            }
            if (p1JoyIndex > -1 && controllerNames.Length > 0)
            {
                p1PadActive = (controllerNames[p1JoyIndex] != "");
                gamepads[0].isActive = (controllerNames[p1JoyIndex] != "");
                gamepads[0].playerName = "Player 1";
            }
            if (p2JoyIndex > -1 && controllerNames.Length > 1)
            {
                p2PadActive = (controllerNames[p2JoyIndex] != "");
                gamepads[1].isActive = (controllerNames[p2JoyIndex] != "");
                gamepads[1].playerName = "Player 2";
            }
        }

        // show / hide cursor
        if (hideCursorIfGamepad)
            CursorHide(gamepads[0].isActive || gamepads[1].isActive);
     }

    void GetPadStatus()
    {
        // REVIEW: find all input signals on our xbox style gamepads
        for (int i = 0; i < gamepads.Length; i++)
        {
            if (i < Gamepad.all.Count)
            {
                gamepads[i].startButton = Gamepad.all[i].startButton.isPressed;
                gamepads[i].backButton = Gamepad.all[i].selectButton.isPressed;
                //gamepads[i].modeButton = Gamepad.all[i].;
                //gamepads[i].centerButton = Gamepad.all[i].;
                gamepads[i].XaxisL = Gamepad.all[i].leftStick.x.value;
                gamepads[i].YaxisL = Gamepad.all[i].leftStick.y.value;
                gamepads[i].LJoyPress = Gamepad.all[i].leftStickButton.isPressed;
                gamepads[i].XaxisR = Gamepad.all[i].rightStick.x.value;
                gamepads[i].YaxisR = Gamepad.all[i].rightStick.y.value;
                gamepads[i].RJoyPress = Gamepad.all[i].rightStickButton.isPressed;
                //gamepads[i].DpadPress = Gamepad.all[i].;
                gamepads[i].DpadUp = Gamepad.all[i].dpad.up.isPressed;
                gamepads[i].DpadDown = Gamepad.all[i].dpad.down.isPressed;
                gamepads[i].DpadLeft = Gamepad.all[i].dpad.left.isPressed;
                gamepads[i].DpadRight = Gamepad.all[i].dpad.right.isPressed;
                gamepads[i].aButton = Gamepad.all[i].aButton.isPressed;
                gamepads[i].bButton = Gamepad.all[i].bButton.isPressed;
                gamepads[i].xButton = Gamepad.all[i].xButton.isPressed;
                gamepads[i].yButton = Gamepad.all[i].yButton.isPressed;
                gamepads[i].LBump = Gamepad.all[i].leftShoulder.isPressed;
                gamepads[i].LTrigger = Gamepad.all[i].leftTrigger.value;
                gamepads[i].RBump = Gamepad.all[i].rightShoulder.isPressed;
                gamepads[i].RTrigger = Gamepad.all[i].rightTrigger.value;
            }
        }
    }

    void OnGUI()
    {
        if (gamepadIcon == null || iconTimer <= 0f)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        r.x = 0.025f * w;
        r.y = 0.005f * w;
        r.width = 0.05f * w;
        r.height = 0.05f * w;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        Color c = Color.white;
        c.a = Mathf.Clamp(iconTimer,0f,1f);
        g.normal.textColor = c;
        g.fontSize = Mathf.RoundToInt( 18 * (w/1024f) );
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        string s = "x "+numPads.ToString();
        if (disconnectedPad)
            s = "- " + numPads.ToString();

        GUI.DrawTexture(r, gamepadIcon, ScaleMode.ScaleToFit, true, 0f, c, 0f, 0f);

        r.y = 0.025f * w;
        GUI.Label(r, s, g);
    }
}
