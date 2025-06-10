using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiGamepad : MonoBehaviour
{
    // Author: Glenn Storm
    // This manages the connection to multiple game pad inputs

    // REVIEW: consider additional properties, handled in this manager
    // - a set of values that track if this control has been 'first' pressed any amount
    // - for controls that are not already bool (pressLeftXaxisL, pressRightXaxisL)
    // - and for controls that need to be 'first frame press only' (like get key down)
    // - where activation of any amount causes press, no control = un-presses
    // - (this is used in multiple menu scripts, and could be handled here)

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

    // gPadDown records 'first frame down' control any amount (must un-press to clear)
    public GamepadStatus[] gPadDown = new GamepadStatus[2];
    // gPadPrev records previous frame status of control to determine gPadDown results
    private GamepadStatus[] gPadPrev = new GamepadStatus[2];

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

        // init gamepad data
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

        // init gPadPrev data (prev frame values of all gamepad controls)
        gPadPrev[0] = gamepads[0];
        gPadPrev[1] = gamepads[1];
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

        HandGamepadFirstPressStatus();
    }

    void HandGamepadFirstPressStatus()
    {
        // determine 'first press' state of gamepads controls as gPadDown
        for (int i = 0; i < gamepads.Length; i++)
        {
            if (i < Gamepad.all.Count)
            {
                // clear gPadDown, as 'first frame control' has happened last frame
                gPadDown[i] = new GamepadStatus();
                gPadDown[i].playerName = gPadPrev[i].playerName;
                gPadDown[i].isActive = gPadPrev[i].isActive;

                // determine gPadDown status by 'this frame change has happened'
                // all joystick/trigger values are 0f, -1f or 1f for this 'first press' signal
                if (gPadPrev[i].startButton != gamepads[i].startButton)
                    gPadDown[i].startButton = gamepads[i].startButton;
                if (gPadPrev[i].backButton != gamepads[i].backButton)
                    gPadDown[i].backButton = gamepads[i].backButton;
                //gamepads[i].modeButton = Gamepad.all[i].;
                //gamepads[i].centerButton = Gamepad.all[i].;
                if ((gPadPrev[i].XaxisL != 0f) != (gamepads[i].XaxisL != 0f))
                    gPadDown[i].XaxisL = (gamepads[i].XaxisL / Mathf.Abs(gamepads[i].XaxisL));
                if ((gPadPrev[i].YaxisL != 0f) != (gamepads[i].YaxisL != 0f))
                    gPadDown[i].YaxisL = (gamepads[i].YaxisL / Mathf.Abs(gamepads[i].YaxisL));
                if (gPadPrev[i].LJoyPress != gamepads[i].LJoyPress)
                    gPadDown[i].LJoyPress = gamepads[i].LJoyPress;
                if ((gPadPrev[i].XaxisR != 0f) != (gamepads[i].XaxisR != 0f))
                    gPadDown[i].XaxisR = (gamepads[i].XaxisR / Mathf.Abs(gamepads[i].XaxisR));
                if ((gPadPrev[i].YaxisR != 0f) != (gamepads[i].YaxisR != 0f))
                    gPadDown[i].YaxisR = (gamepads[i].YaxisR / Mathf.Abs(gamepads[i].YaxisR));
                if (gPadPrev[i].RJoyPress != gamepads[i].RJoyPress)
                    gPadDown[i].RJoyPress = gamepads[i].RJoyPress;
                //gamepads[i].DpadPress = Gamepad.all[i].;
                if (gPadPrev[i].DpadUp != gamepads[i].DpadUp)
                    gPadDown[i].DpadUp = gamepads[i].DpadUp;
                if (gPadPrev[i].DpadDown != gamepads[i].DpadDown)
                    gPadDown[i].DpadDown = gamepads[i].DpadDown;
                if (gPadPrev[i].DpadLeft != gamepads[i].DpadLeft)
                    gPadDown[i].DpadLeft = gamepads[i].DpadLeft;
                if (gPadPrev[i].DpadRight != gamepads[i].DpadRight)
                    gPadDown[i].DpadRight = gamepads[i].DpadRight;
                if (gPadPrev[i].aButton != gamepads[i].aButton)
                    gPadDown[i].aButton = gamepads[i].aButton;
                if (gPadPrev[i].bButton != gamepads[i].bButton)
                    gPadDown[i].bButton = gamepads[i].bButton;
                if (gPadPrev[i].xButton != gamepads[i].xButton)
                    gPadDown[i].xButton = gamepads[i].xButton;
                if (gPadPrev[i].yButton != gamepads[i].yButton)
                    gPadDown[i].yButton = gamepads[i].yButton;
                if (gPadPrev[i].LBump != gamepads[i].LBump)
                    gPadDown[i].LBump = gamepads[i].LBump;
                if ((gPadPrev[i].LTrigger != 0f) != (gamepads[i].LTrigger != 0f))
                    gPadDown[i].LTrigger = ( gamepads[i].LTrigger / Mathf.Abs(gamepads[i].LTrigger) );
                if (gPadPrev[i].RBump != gamepads[i].RBump)
                    gPadDown[i].RBump = gamepads[i].RBump;
                if ((gPadPrev[i].RTrigger != 0f) != (gamepads[i].RTrigger != 0f))
                    gPadDown[i].RTrigger = (gamepads[i].RTrigger / Mathf.Abs(gamepads[i].RTrigger));

                // hold previous frame state in gPadPrev for comparison next frame
                gPadPrev[i] = gamepads[i];
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
