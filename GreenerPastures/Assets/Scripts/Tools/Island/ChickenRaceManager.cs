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

    public enum ChickenAnimSet
    {
        Idle,
        Run,
        Win
    }

    [System.Serializable]
    public struct ChickenFrames
    {
        public ChickenAnimSet anim;
        public Texture2D[] lineFrames;
        public Texture2D[] fillFrames;
        public Texture2D[] colorFrames;
        public float frameTime;
        public float rateVariance;
    }
    public ChickenFrames[] chickenAnimation;

    [System.Serializable]
    public struct ChickenRunner
    {
        public string chickenID;   
        public int animIndex;
        public int animFrame;
        public float animTimer;
        public Vector2 position;
        public bool faceLeft;
    }
    private ChickenRunner[] chickens;

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

            // test chickens init
            chickens = new ChickenRunner[3];
            chickens[0] = InitializeChicken("larry", new Vector2(0.175f, 0.525f));
            chickens[1] = InitializeChicken("curly", new Vector2(0.15f, 0.6f));
            chickens[2] = InitializeChicken("moe", new Vector2(0.125f, 0.67f));
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
        if (guestState != GuestState.Default && guestState != GuestState.Deactivating)
            return true; // we must already have player engaged, skip

        // if no player, run player check timer
        if (currentGuest == null && playerCheckTimer > 0f)
        {
            playerCheckTimer -= Time.deltaTime;
            if (playerCheckTimer < 0f)
            {
                playerCheckTimer = 0f;
                // detect player in proximity
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                // 
                for (int i = 0; i < pcms.Length; i++)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if (dist < RACEPROXIMITYRANGE)
                    {
                        currentGuest = pcms[i];
                        break;
                    }
                }
                // if no player, reset check timer
                if (currentGuest == null)
                {
                    playerCheckTimer = PLAYERCHECKTIME;
                    if (leavingGuest != null)
                    {
                        // customer has left, reset
                        leavingGuest = null;
                        guestState = GuestState.Default;
                        guestStateTimer = 0f;
                    }
                }
                else if (leavingGuest != null && currentGuest == leavingGuest)
                {
                    // REVIEW: remain active until player has left?
                    currentGuest = null;
                    playerCheckTimer = PLAYERCHECKTIME;
                }
                else
                {
                    // customer engagement, activate
                    currentGuest.characterFrozen = true;
                    currentGuest.hidePlayerNameTag = true;
                    currentGuest.hidePlayerHUD = true;
                    // disable almanac
                    InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                    if (iga != null)
                        iga.enabled = false;
                    // disable controls display
                    InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                    if (igc != null)
                        igc.enabled = false;
                    guestState = GuestState.Activating;
                    guestStateTimer = GUESTSTATETIMERMAX;
                }
            }
        }

        return (currentGuest != null);
    }


    void HandleGuestStates()
    {
        if (guestStateTimer == 0f)
            return;

        if (guestStateTimer > 0f)
        {
            guestStateTimer -= Time.deltaTime;
            if (guestStateTimer < 0f)
            {
                guestStateTimer = 0f;
                // handle state
                switch (guestState)
                {
                    case GuestState.Default:
                        // we should never be here
                        break;
                    case GuestState.Activating:
                        guestState = GuestState.Active;
                        raceDisplay = true;
                        raceState = RaceState.Entering;
                        raceStateTimer = RACESTATETIMERMAX;
                        fadingOverlay = true;
                        qoe.enabled = false;
                        break;
                    case GuestState.Active:
                        currentGuest.characterFrozen = false;
                        currentGuest.freezeCharacterActions = false;
                        currentGuest.hidePlayerHUD = false;
                        // re-able almanac
                        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                        if (iga != null)
                            iga.enabled = true;
                        // show controls display hud item
                        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                        if (igc != null)
                            igc.enabled = true;
                        guestState = GuestState.Deactivating;
                        guestStateTimer = GUESTSTATETIMERMAX;
                        break;
                    case GuestState.Deactivating:
                        if (currentGuest != null)
                        {
                            leavingGuest = currentGuest;
                            currentGuest = null;
                        }
                        // remain in this state until leaving guest not detected
                        playerCheckTimer = PLAYERCHECKTIME;
                        guestStateTimer = GUESTSTATETIMERMAX;
                        qoe.enabled = true;
                        break;
                    default:
                        Debug.LogWarning("--- ChickenRaceManager [HandleGuestStates] : guest state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void UpdateRaceStateTimer()
    {
        if (raceStateTimer > 0f)
        {
            raceStateTimer -= Time.deltaTime;
            if (raceStateTimer < (RACESTATETIMERMAX / 2f))
            {
                // configure race background images between overlay fades
                switch (raceState)
                {
                    case RaceState.Default:
                        break;
                    case RaceState.Entering:
                        if (!fadingFromBlack)
                        {
                            if (currentBackground == null && raceBG != null)
                                currentBackground = raceBG;
                            if (currentBackground == null)
                                currentBackground = Texture2D.whiteTexture; // TEMP
                            //currentNPCFrame = 0;
                            //npcFrameTimer = RACESTATETIMERMAX;
                        }
                        break;
                    case RaceState.Exiting:
                        if (!fadingFromBlack)
                            currentBackground = null;
                        break;
                }
                fadingFromBlack = true;
            }
            if (raceStateTimer < 0f)
            {
                raceStateTimer = 0f;
                fadingFromBlack = false;
                // handle race state changes
                switch (raceState)
                {
                    case RaceState.Default:
                        // we should never be here
                        break;
                    case RaceState.Entering:
                        fadingOverlay = false;
                        break;
                    case RaceState.Exiting:
                        guestStateTimer = (GUESTSTATETIMERMAX / 2f); // exit faster
                        raceState = RaceState.Default;
                        raceDisplay = false;
                        fadingOverlay = false;
                        break;
                    default:
                        Debug.LogWarning("--- ChickenRaceManager [UpdateRaceStateTimer] : race state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void HandleRaceStates()
    {
        switch (raceState)
        {
            case RaceState.Default:
                // we should never be here
                break;
            case RaceState.Entering:
                // instructions and option to exit here only
                CheckGuestEngagement();
                break;
            case RaceState.Betting:
                // placing bet of gold and selecting chicken (allow bet 0 to just watch race)
                CheckGuestBet();
                break;
            case RaceState.PreRace:
                // set chickens ready
                break;
            case RaceState.Race:
                // chickens run race
                UpdateChickenRun();
                break;
            case RaceState.PostRace:
                // race finish, winner circle celebration
                break;
            case RaceState.Rewarding:
                // bets settled, gold awarded, return to entering state
                UpdateRewardGuest();
                break;
            case RaceState.Exiting:
                // leaving chicken race stall
                break;
        }
    }

    void CheckGuestEngagement()
    {
        PlayerControlManager.PlayerActions pa = currentGuest.GetPlayerActions();
    }

    void CheckGuestBet()
    {
        PlayerControlManager.PlayerActions pa = currentGuest.GetPlayerActions();
    }

    void UpdateChickenRun()
    {

    }

    void UpdateRewardGuest()
    {

    }

    ChickenRunner InitializeChicken(string name, Vector2 pos)
    {
        return InitializeChicken(name, ChickenAnimSet.Idle, 0, RandomSystem.FlatRandom01(), pos, false);
    }

    ChickenRunner InitializeChicken(string name, ChickenAnimSet set, int frame, float timer, Vector2 pos, bool faceLeft)
    {
        ChickenRunner retChicken = new ChickenRunner();

        retChicken.chickenID = name;
        retChicken.animIndex = (int)set;
        retChicken.animFrame = frame;
        retChicken.animTimer = timer;
        retChicken.position = pos;
        retChicken.faceLeft = faceLeft;

        return retChicken;
    }

    void UpdateChickenFrames()
    {
        for (int i = 0; i < chickens.Length; i++)
        {
            chickens[i].animTimer -= Time.deltaTime;
            if (chickens[i].animTimer < 0f)
            {
                // set timer
                chickens[i].animTimer = chickenAnimation[chickens[i].animIndex].frameTime;
                if (chickenAnimation[chickens[i].animIndex].rateVariance > 0f)
                {
                    float amt = RandomSystem.GaussianRandom01() * chickenAnimation[chickens[i].animIndex].rateVariance;
                    amt -= chickenAnimation[chickens[i].animIndex].rateVariance * 0.5f;
                    chickens[i].animTimer = chickenAnimation[chickens[i].animIndex].frameTime + amt;
                }
                // set frame (loop by default)
                chickens[i].animFrame++;
                if (chickens[i].animFrame >= chickenAnimation[chickens[i].animIndex].lineFrames.Length)
                    chickens[i].animFrame = 0;
                // win and idle may flip
                if (((ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Idle || 
                    (ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Win) && 
                    RandomSystem.FlatRandom01() < 0.381f)
                    chickens[i].faceLeft = !chickens[i].faceLeft;
                // idle variation
                if ((ChickenAnimSet)chickens[i].animIndex == ChickenAnimSet.Idle)
                    chickens[i].animFrame = Random.Range(0, 3);
            }
        }
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
            if (raceState > RaceState.Default && raceState <= RaceState.Exiting) // REVIEW:
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

        // draw chickens
        for (int i = 0; i < chickens.Length; i++)
        {
            r.width = 0.2f * w;
            r.height = 0.2f * w; // square
            r.x = chickens[i].position.x * w - (r.width * 0.5f);
            r.y = chickens[i].position.y * h - (r.width);

            int frame = chickens[i].animFrame;
            if (chickens[i].faceLeft)
            {
                r.x += r.width;
                r.width = -r.width;
            }

            t = chickenAnimation[chickens[i].animIndex].fillFrames[frame];
            GUI.color = c;
            GUI.DrawTexture(r, t); // fill

            /*
            t = chickenAnimation[chickens[i].animIndex].colorFrames[frame];
            switch (i)
            {
                case 0:
                    c = Color.blue;
                    break;
                case 1:
                    c = Color.green;
                    break;
                case 2:
                    c = Color.red;
                    break;
            }
            c.a = 0.1f;
            GUI.color = c;
            GUI.DrawTexture(r, t); // color
            */

            t = chickenAnimation[chickens[i].animIndex].lineFrames[frame];
            c = Color.white;
            GUI.color = c;
            GUI.DrawTexture(r, t); // line
            //
        }

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
