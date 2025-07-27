using UnityEngine;

public class ChickenRaceManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the stall with the chicken race betting mini-game

    public enum GuestState
    {
        Default,        // ready to be approached by guest
        Activating,     // beginning to chance interface
        Active,         // in mini-game, handled by race state
        Deactivating    // end race interface, detect player left to reset
    }
    public GuestState guestState;
    public enum RaceState
    {
        Default,        // ready to enter mini-game
        Entering,       // instructions and option to exit here only
        Betting,        // placing bet of gold and selecting chicken
        PreRace,        // set chickens ready
        Race,           // chickens run race
        PostRace,       // race finish, winner circle celebration
        Rewarding,      // bets settled, gold awarded, return to entering state
        Exiting         // leaving chicken race stall
    }
    public RaceState raceState;

    public Texture2D raceDishOL;
    public Texture2D[] goldOLs;
    public int goldOLIndex;
    public Texture2D raceOL;
    public Texture2D raceBG;

    private float playerCheckTimer;

    private PlayerControlManager currentGuest;
    private PlayerControlManager leavingGuest;

    private MultiGamepad padMgr;

    private QuitOnEscape qoe; // disable to suspend use of start button while in market

    private float guestStateTimer;
    private float raceStateTimer;
    private bool raceDisplay;

    private bool fadingOverlay;
    private bool fadingFromBlack;
    private Texture2D currentBackground;

    private Texture2D[] buttonTex;


    // -- chicken race variables --

    const float PLAYERCHECKTIME = 1f;
    const float RACEPROXIMITYRANGE = .5f;
    const float GUESTSTATETIMERMAX = 1f;
    const float RACESTATETIMERMAX = 1f;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- ChickenRaceManager [Start] : no multi gamepad found. will ignore.");
        qoe = GameObject.FindFirstObjectByType<QuitOnEscape>();
        if (qoe == null)
        {
            Debug.LogError("--- ChickenRaceManager [Start] : no quit on escape found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            playerCheckTimer = PLAYERCHECKTIME;

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
        if (!DetectPlayerGuest())
            return;

        UpdateChickenFrames();

        HandleGuestStates();

        UpdateRaceStateTimer();

        HandleRaceStates();
    }

    bool DetectPlayerGuest()
    {
        bool retBool = false;

        return retBool;
    }

    void UpdateChickenFrames()
    {

    }

    void HandleGuestStates()
    {

    }

    void UpdateRaceStateTimer()
    {

    }

    void HandleRaceStates()
    {

    }

    void OnGUI()
    {
        if (!raceDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0f;
        r.y = 0f;
        r.width = w;
        r.height = h;

        GUIStyle g = new GUIStyle(GUI.skin.label);
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        // crafting background image appears halfway through overlay fading
        if (currentBackground != null)
        {
            c = Color.white;
            GUI.color = c;
            // 
            if (raceState > RaceState.Default && raceState < RaceState.Exiting) // REVIEW:
            {
                // chickens will be able to be safely placed on top of all this
                t = raceBG;
                GUI.DrawTexture(r, t); // race bg
                t = raceOL;
                GUI.DrawTexture(r, t); // race ol
                if (goldOLIndex > -1)
                {
                    t = goldOLs[goldOLIndex];
                    GUI.DrawTexture(r, t); // gold ol
                }
                t = raceDishOL;
                GUI.DrawTexture(r, t); // dish ol
            }
        }

        // handle fading to and from black for race state transitions
        if (fadingOverlay)
        {
            t = Texture2D.whiteTexture;
            c = Color.black;
            if (fadingFromBlack)
                c.a = ((raceStateTimer * 2f) / RACEPROXIMITYRANGE);
            else
                c.a = 1f - (((raceStateTimer * 2f) / RACESTATETIMERMAX) - 1f);
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // if fading overlay, no other display
            return;
        }

        if (raceStateTimer > 0f)
            return;

        c = Color.white;
        GUI.color = c;

        // ...
        // very specific race gui stuff
        // ...

        // exit mini-game button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        if (padMgr != null && padMgr.gamepads[0].isActive)
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "EXIT MINI-GAME";
        if (padMgr != null && padMgr.gamepads[0].isActive)
            s += "\n[BACK BUTTON]";

        GUI.enabled = (raceState < RaceState.Betting);
        if (GUI.Button(r, s, g))
        {
            raceState = RaceState.Exiting;
            raceStateTimer = RACESTATETIMERMAX;
            fadingOverlay = true;
        }
    }
}
