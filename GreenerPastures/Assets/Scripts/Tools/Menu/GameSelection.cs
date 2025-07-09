using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSelection : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the game selection screen

    public string titleText;
    public Rect title;
    public Font titleFont;
    public FontStyle titleFontStyle;
    public Color titleFontColor = Color.white;
    public int titleFontSizeAt1024 = 60;

    // game selection elements
    public Font labelFont;
    public FontStyle labelFontStyle;
    public Color labelFontColor = Color.white;
    public int labelFontSizeAt1024 = 36;
    public int buttonFontSizeAt1024 = 30;

    public string backButtonText = "BACK";
    public Rect backButton;
    public Font buttonFont;
    public FontStyle buttonFontStyle;
    public Color buttonFontColor = Color.white;
    public int backButtonFontSizeAt1024 = 48;

    private SaveLoadManager saveMgr;
    private bool gameLoaded;
    private string gamePlayerName; // player name in loaded game
    private bool newGame; // is user creating a new game
    private GameOptionsData currentGameOptions;

    private string selectionFeedback;
    private float feedbackTimer;

    private float pingPollTimer; // interval when collection of hostPings takes place
    private MultiplayerHostPing[] hostPings; // host pings heard within interval
    private bool tinyPopup; // ask new player name, offer "join" and "cancel"
    private MultiplayerHostPing currentNet; // the host ping selected to join

    private string popGameName;
    private string popPlayerName;
    private int popMaxPlayers;
    private bool popAllowCheats;
    private bool popAllowHazards;
    private bool popAllowCurses;
    private bool gameOptionsChanged;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private bool gamePopup; // is displaying popup
    private bool popupEnabled; // is popping up or down
    private float popupTimer;
    private float popupProgress;
    private AnimationCurve popupCurve;

    private Texture2D[] buttonTex;

    const float POPUPTIME = 1f;
    const float FEEDBACKTIME = 4f;
    const float POLLINTERVAL = 1f;


    void Start()
    {
        // validate
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr == null)
        {
            Debug.LogError("--- GameSelection [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- GameSelection [Start] : no pad manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            ConfigureCurrentGameEnabled();
            // if selected net game, restore player name and host ping
            if (saveMgr.IsRemoteClient())
            {
                popPlayerName = saveMgr.GetJoiningPlayerName();
                currentNet = saveMgr.GetJoinInfo();
            }
            // start host ping poll timer
            pingPollTimer = POLLINTERVAL;
            // initialize host ping array
            hostPings = new MultiplayerHostPing[0];

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
        // run popup timer
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0f)
            {
                popupTimer = 0f;
                popupEnabled = !popupEnabled;
                if (!popupEnabled)
                {
                    gamePopup = false;
                }
            }
            else
            {
                if (popupEnabled)
                    popupProgress = 1f - popupCurve.Evaluate(popupTimer / POPUPTIME);
                else
                    popupProgress = popupCurve.Evaluate(popupTimer / POPUPTIME);
            }
            popupProgress = Mathf.Clamp01(popupProgress);
        }

        // run feedback timer
        if ( feedbackTimer > 0f )
        {
            feedbackTimer -= Time.deltaTime;
            if ( feedbackTimer < 0f )
            {
                feedbackTimer = 0f;
                selectionFeedback = "";
            }
        }

        // run host ping poll timer
        if (pingPollTimer > 0f)
        {
            pingPollTimer -= Time.deltaTime;
            if ( pingPollTimer < 0f )
            {
                // NOTE: if there were any reason not to listen, can stop here
                pingPollTimer = POLLINTERVAL;
                // clear list of host pings
                hostPings = new MultiplayerHostPing[0];
                // TEMP - remove me (this just to demonstrate game selection)
                CreateFakeHostPings();
            }
            CollectHostPings();
        }

        if (gamePopup)
            padMaxButton = 6;
        else
            padMaxButton = saveMgr.GetCurrentProfile().gameKeys.Length + 2;

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

    // NOTE: we can do this lots of ways, and
    // if something like a public function to call when hostPing arrives makes sense,
    // we can just take that input here, for example (assuming for now we listen per tick)
    void CollectHostPings()
    {
        MultiplayerHostPing liveHostPingSignal = new MultiplayerHostPing();

        // NOTE: here, the function just treats 'liveHostPingSignal' as a currently heard ping
        if (liveHostPingSignal.options == null)
            return; // empty ping, invalid game options

        // add ping to array
        MultiplayerHostPing[] tmp = new MultiplayerHostPing[hostPings.Length + 1];
        for (int i = 0; i < hostPings.Length; i++)
        {
            tmp[i] = hostPings[i];
        }
        tmp[hostPings.Length] = liveHostPingSignal;
        hostPings = tmp;
    }

    void CreateFakeHostPings()
    {
        // TEMP : 'polling' fake host ping data for demonstraton of game select
        hostPings = new MultiplayerHostPing[2];
        hostPings[0].gameKey = "[000]-a really fun game";
        hostPings[0].availablePlayerSlots = 1;
        hostPings[0].profiles = new string[2];
        hostPings[0].profiles[0] = "hostprofile";
        hostPings[0].profiles[1] = saveMgr.GetCurrentProfile().profileID;
        hostPings[0].playerNames = new string[2];
        hostPings[0].playerNames[0] = "host player name";
        hostPings[0].playerNames[1] = "your name";
        hostPings[0].options = new GameOptionsData();
        hostPings[0].options.maxPlayers = 2;
        hostPings[0].options.allowCheats = true;
        hostPings[0].options.allowHazards = true;
        hostPings[0].options.allowCurses = true;
        hostPings[1].gameKey = "[111]-another game";
        hostPings[1].availablePlayerSlots = 0;
        hostPings[1].profiles = new string[1];
        hostPings[1].profiles[0] = "hostprofile";
        hostPings[1].playerNames = new string[1];
        hostPings[1].playerNames[0] = "host player name";
        hostPings[1].options = new GameOptionsData();
        hostPings[1].options.maxPlayers = 1;
        hostPings[1].options.allowCheats = false;
        hostPings[1].options.allowHazards = false;
        hostPings[1].options.allowCurses = false;
    }

    void ConfigureCurrentGameEnabled()
    {
        gameLoaded = saveMgr.IsGameCurrentlyLoaded();
        if (gameLoaded)
        {
            PlayerData pData = GameSystem.GetProfilePlayer(saveMgr.GetCurrentGameData(), saveMgr.GetCurrentProfile());
            gamePlayerName = pData.playerName;
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
        g.fontSize = Mathf.RoundToInt(titleFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        g.active.textColor = buttonFontColor;
        string s = titleText;
        Color c = Color.white;

        // title
        r = title;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;

        GUI.Label(r, s, g);

        // NEW GAME / GAME OPTIONS
        if (gamePopup)
        {
            r = new Rect();
            r.x = 0.2f * w;
            r.y = (0.2f * h) + (popupProgress * 0.85f * h);
            r.width = 0.6f * w;
            r.height = 0.7f * h;
            g = new GUIStyle(GUI.skin.box);
            g.normal.textColor = buttonFontColor;
            g.hover.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            g.fontStyle = FontStyle.Bold;
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.padding = new RectOffset(0, 0, 20, 0);
            s = "Game Options";
            GUI.color = Color.white;
            GUI.Box(r, s, g);

            // popup elements
            r.x = 0.25f * w;
            r.y += 0.1f * h;
            r.width = 0.25f * w;
            r.height = 0.05f * h;
            // game options / new game popup
            // labels
            // . game name
            // . max players
            // . allow cheats
            // . allow hazards
            // . allow curses
            // . joining player name
            g = new GUIStyle(GUI.skin.label);
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
            g.fontStyle = labelFontStyle;
            g.alignment = TextAnchor.MiddleRight;
            g.padding = new RectOffset(0, 20, 0, 0);
            s = "Game Name : ";
            GUI.Label(r, s, g);

            r.y += 0.075f * h;
            s = "Max Players [" + popMaxPlayers + "]: ";
            GUI.Label(r, s, g);

            r.y += 0.075f * h;
            s = "Cheats : ";
            GUI.Label(r, s, g);

            r.y += 0.075f * h;
            s = "Hazards : ";
            GUI.Label(r, s, g);

            r.y += 0.075f * h;
            s = "Curses : ";
            GUI.Label(r, s, g);

            if (newGame)
            {
                r.y += 0.075f * h;
                s = "Join as Player Name : ";
                GUI.Label(r, s, g);
            }
            else
            {
                r.y += 0.075f * h;
                r.width = 0.5f;
                g.alignment = TextAnchor.MiddleCenter;
                g.fontStyle = FontStyle.Italic;
                s = "Playing as '" + gamePlayerName + "' in this game";
                if (saveMgr.IsRemoteClient())
                    s = "Playing as '" + popPlayerName + "' in this game";
                GUI.Label(r, s, g);
            }

            // text fields (read-only if not new, or if remote)
            // . name
            r.x += 0.25f * w;
            r.y -= 0.375f * h;
            r.width = 0.2f * w;
            if (newGame && !saveMgr.IsRemoteClient())
                g = new GUIStyle(GUI.skin.textField);
            g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            if (newGame && !saveMgr.IsRemoteClient())
                popGameName = GUI.TextField(r, popGameName, 20, g);
            else
                GUI.Label(r, popGameName, g);

            // . + / - max players
            // . toggle cheats
            // . toggle hazards
            // . toggle curses
            // . (textfield) joining player name
            // . Create new game ('Unload' if not new, 'Accept' if editing)
            // . Cancel
            r.x += 0.025f * w;
            r.y += 0.075f * h;
            r.width = 0.075f * w;
            g = new GUIStyle(GUI.skin.button);
            g.fontStyle = buttonFontStyle;
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            if (padButtonSelection == 0)
                g.normal.textColor = Color.white;
            if (!Application.isEditor)
            {
                g.normal.background = buttonTex[0];
                g.hover.background = buttonTex[1];
                g.active.background = buttonTex[2];
            }
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "-";
            // allow less max players if new game or owner of game max is more than player length
            GUI.enabled = popMaxPlayers > 1 && (newGame || (!saveMgr.IsRemoteClient() && gameLoaded && saveMgr.GetCurrentGameData().players.Length < popMaxPlayers));
            if (GUI.Button(r, s, g) || (padMgr != null && padMgr.gamepads[0].isActive &&
                padButtonSelection == 0 && padMgr.gPadDown[0].aButton))
            {
                popMaxPlayers--;
                popMaxPlayers = Mathf.Clamp(popMaxPlayers, 1, 8);
            }
            r.x += 0.075f * w;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 1)
                g.normal.textColor = Color.white;
            s = "+";
            // alow more max players if new game or owner of game and less than 8
            GUI.enabled = popMaxPlayers < 8 && (newGame || !saveMgr.IsRemoteClient());
            if (GUI.Button(r, s, g) || (padMgr != null && padMgr.gamepads[0].isActive &&
                padButtonSelection == 1 && padMgr.gPadDown[0].aButton))
            {
                popMaxPlayers++;
                popMaxPlayers = Mathf.Clamp(popMaxPlayers, 1, 8);
            }
            r.x -= 0.1f * w;
            r.y += 0.075f * h;
            r.width = 0.2f * w;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 2)
                g.normal.textColor = Color.white;
            s = "ALLOWED";
            if (!popAllowCheats)
                s = "OFF";
            GUI.enabled = (!saveMgr.IsRemoteClient()); // only owner of data can change options
            if (GUI.Button(r, s, g) || (padMgr != null && padMgr.gamepads[0].isActive &&
                padButtonSelection == 2 && padMgr.gPadDown[0].aButton))
            {
                popAllowCheats = !popAllowCheats;
            }
            r.y += 0.075f * h;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 3)
                g.normal.textColor = Color.white;
            s = "ALLOWED";
            if (!popAllowHazards)
                s = "OFF";
            if (GUI.Button(r, s, g) || (padMgr != null && padMgr.gamepads[0].isActive &&
                padButtonSelection == 3 && padMgr.gPadDown[0].aButton))
            {
                popAllowHazards = !popAllowHazards;
            }
            r.y += 0.075f * h;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 4)
                g.normal.textColor = Color.white;
            s = "ALLOWED";
            if (!popAllowCurses)
                s = "OFF";
            if (GUI.Button(r, s, g) || (padMgr != null && padMgr.gamepads[0].isActive &&
                padButtonSelection == 4 && padMgr.gPadDown[0].aButton))
            {
                popAllowCurses = !popAllowCurses;
            }
            r.y += 0.075f * h; // either with newGame or not
            if (newGame)
            {
                g = new GUIStyle(GUI.skin.textField);
                g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
                g.alignment = TextAnchor.MiddleCenter;
                g.normal.textColor = labelFontColor;
                g.hover.textColor = labelFontColor;
                g.active.textColor = labelFontColor;
                GUI.enabled = true;
                popPlayerName = GUI.TextField(r, popPlayerName, g);
            }
            else
            {
                r.x -= 0.25f * w;
                r.width = 0.5f * w;
                g = new GUIStyle(GUI.skin.label);
                g.fontStyle = FontStyle.Italic;
                g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
                g.alignment = TextAnchor.MiddleCenter;
                g.normal.textColor = labelFontColor;
                g.hover.textColor = labelFontColor;
                g.active.textColor = labelFontColor;
                GUI.enabled = true;
                s = "Playing as '" + gamePlayerName + "' in this game";
                if (saveMgr.IsRemoteClient())
                    s = "Playing as '" + popPlayerName + "' in this game";
                GUI.Label(r, s, g);
            }
            GUI.enabled = true;

            gameOptionsChanged = (currentGameOptions != null &&
                (currentGameOptions.maxPlayers != popMaxPlayers ||
                currentGameOptions.allowCheats != popAllowCheats ||
                currentGameOptions.allowHazards != popAllowHazards ||
                currentGameOptions.allowHazards != popAllowHazards));

            // popup buttons
            r.x = 0.25f * w;
            r.y += 0.1f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            if (!Application.isEditor)
            {
                g.normal.background = buttonTex[0];
                g.hover.background = buttonTex[1];
                g.active.background = buttonTex[2];
            }
            // UNLOAD / ACCEPT / CREATE
            if (padButtonSelection == 5)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "UNLOAD";
            if (saveMgr.IsRemoteClient())
                s = "DESELECT";
            else if (gameOptionsChanged)
                s = "ACCEPT";
            if (newGame)
                s = "CREATE";
            // validate new game info (game name and player name)
            if (newGame)
                GUI.enabled = (popGameName != "" && popPlayerName != "");
            if (GUI.Button(r, s, g) || (padMgr != null &&
                padMgr.gamepads[0].isActive &&
                ((gamePopup && padButtonSelection == 5) || (!gamePopup && padButtonSelection == 0)) &&
                padMgr.gPadDown[0].aButton))
            {
                // if new game, create new
                if (newGame)
                {
                    GameData newGameData = GameSystem.InitializeGame(popGameName);
                    newGameData.options.maxPlayers = popMaxPlayers;
                    newGameData.options.allowCheats = popAllowCheats;
                    newGameData.options.allowHazards = popAllowHazards;
                    newGameData.options.allowCurses = popAllowCurses;
                    // add game key to profile data
                    ProfileData profData = saveMgr.GetCurrentProfile();
                    PlayerData pd = PlayerSystem.InitializePlayer(popPlayerName, profData.profileID);
                    profData = ProfileSystem.AddGameKey(profData, newGameData.gameKey);
                    // add this player to new game
                    newGameData = GameSystem.AddPlayer(newGameData, pd);
                    // set current before saving game data
                    saveMgr.SetCurrentGameData(newGameData);
                    saveMgr.SaveGameData(newGameData.gameKey);
                    gamePlayerName = popPlayerName;
                    selectionFeedback = "new game created";
                    feedbackTimer = FEEDBACKTIME;
                }
                else if (saveMgr.IsRemoteClient())
                {
                    currentNet = new MultiplayerHostPing(); // empty
                    popPlayerName = "";
                    saveMgr.SetIsRemoteClient(false);
                    saveMgr.SetHostPing(currentNet, popPlayerName);
                    selectionFeedback = "net game deselected";
                    feedbackTimer = FEEDBACKTIME;
                }
                else if (gameOptionsChanged)
                {
                    // game option changes accepted
                    GameData gData = saveMgr.GetCurrentGameData();
                    gData.options.maxPlayers = popMaxPlayers;
                    gData.options.allowCheats = popAllowCheats;
                    gData.options.allowHazards = popAllowHazards;
                    gData.options.allowCurses = popAllowCurses;
                    saveMgr.SetCurrentGameData(gData); // will overwrite
                }
                else
                {
                    // save current and clear current
                    saveMgr.SaveGameData(saveMgr.GetCurrentGameData().gameKey);
                    saveMgr.ClearCurrentGameData();
                    ConfigureCurrentGameEnabled();
                    selectionFeedback = "game unloaded";
                    feedbackTimer = FEEDBACKTIME;
                }
                popupTimer = POPUPTIME;
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    padButtonSelection = -1;
                    padMaxButton = 1;
                }
            }
            GUI.enabled = true;
            // cancel
            r.x += 0.3f * w;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 6)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "CANCEL";
            if (GUI.Button(r, s, g) || (padMgr != null &&
                padMgr.gamepads[0].isActive && 
                ((gamePopup && padButtonSelection == 6) || (!gamePopup && padButtonSelection == 1)) &&
                padMgr.gPadDown[0].aButton))
            {
                popupTimer = POPUPTIME;
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    padButtonSelection = -1;
                    padMaxButton = 1;
                }
            }

            return; // if popup, skip all other GUI
        }

        // game selection elements
        // current game display (name, key, timestamp)
        // label
        // . game name, key, timestamp (or 'no game' if unloaded)
        r.x = 0.15f * w;
        r.y = 0.15f * h;
        r.width = 0.7f * w;
        r.height = 0.15f * h;
        g = new GUIStyle(GUI.skin.label);
        g.normal.textColor = labelFontColor;
        g.hover.textColor = labelFontColor;
        g.active.textColor = labelFontColor;
        g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        s = "CURRENT GAME:\n";
        if (saveMgr.IsRemoteClient())
        {
            s += "Playing as '" + popPlayerName + "' in ";
            s += popGameName + "\n";
            s += "(a network game you may join)";
        }
        else if ( gameLoaded )
        {
            GameData gData = saveMgr.GetCurrentGameData();
            currentGameOptions = gData.options;
            gamePlayerName = GameSystem.GetProfilePlayer( gData, saveMgr.GetCurrentProfile() ).playerName;
            s += "Playing as '" + gamePlayerName + "' in ";
            s += gData.gameName + " " + gData.gameKey.Substring(0, gData.gameKey.IndexOf("]")+1 ) + "\n";
            s += System.DateTime.FromFileTimeUtc(gData.stats.gameInitTime).ToString() + " UTC";
        }
        else
        {
            s += "no game currently loaded";
            g.fontStyle = FontStyle.Italic;
        }
        GUI.color = c;
        GUI.Label(r, s, g);

        // new game control (button)
        // buttons (open popup)
        // . current game options (disabled if unloaded)
        // . create new game
        r.x = 0.2f * w;
        r.y = 0.3f * h;
        r.width = 0.25f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        s = "Game Options";

        GUI.enabled = ((gameLoaded || saveMgr.IsRemoteClient()) && !tinyPopup);
        if (GUI.Button(r, s, g) || 
            (padButtonSelection == 0 && padMgr.gPadDown[0].aButton))
        {
            newGame = false;
            GameData gData = new GameData();
            if (saveMgr.IsGameCurrentlyLoaded())
                gData = saveMgr.GetCurrentGameData();
            else
                ConfigureCurrentGameEnabled();
            if (saveMgr.IsRemoteClient())
            {
                int idx = currentNet.gameKey.IndexOf("]-");
                popGameName = currentNet.gameKey.Substring(idx + 2);
                popMaxPlayers = currentNet.options.maxPlayers;
                popAllowCheats = currentNet.options.allowCheats;
                popAllowHazards = currentNet.options.allowHazards;
                popAllowCurses = currentNet.options.allowCurses;
                // popPlayerName already received
            }
            else if (gData != null)
            {
                popGameName = gData.gameName;
                popMaxPlayers = gData.options.maxPlayers;
                popAllowCheats = gData.options.allowCheats;
                popAllowHazards = gData.options.allowHazards;
                popAllowCurses = gData.options.allowCurses;
            }
            else
                Debug.LogWarning("--- GameSelection [OnGUI] : unable to load current game data. will ignore.");
            gamePopup = true;
            popupTimer = POPUPTIME;
        }
        GUI.enabled = true;

        r.x += 0.35f * w;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(buttonFontSizeAt1024 * (w / 1024f));
        g.normal.textColor = buttonFontColor;
        if (padButtonSelection == 1)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "Create Game";

        if (tinyPopup)
            GUI.enabled = false;

        if (GUI.Button(r, s, g) || (padButtonSelection == 1 && padMgr.gPadDown[0].aButton))
        {
            // unload current game data
            if (saveMgr.IsGameCurrentlyLoaded())
            {
                // save current and clear current
                saveMgr.SaveGameData(saveMgr.GetCurrentGameData().gameKey);
                saveMgr.ClearCurrentGameData();
            }
            // REVIEW: if create game is selected, net game selection is abandoned
            currentNet = new MultiplayerHostPing(); // empty
            popPlayerName = "";
            saveMgr.SetIsRemoteClient(false);
            saveMgr.SetHostPing(currentNet, popPlayerName);
            // --
            newGame = true;
            GameData gData = GameSystem.InitializeGame("temp"); // get default options
            popGameName = "UNNAMED GAME";
            popMaxPlayers = gData.options.maxPlayers;
            popAllowCheats = gData.options.allowCheats;
            popAllowHazards = gData.options.allowHazards;
            popAllowCurses = gData.options.allowCurses;
            gamePopup = true;
            popupTimer = POPUPTIME;
        }
        GUI.enabled = true;

        // GAME SELECTION OPTIONS
        // save game column
        // associated games button list (based on profile game keys list)
        int gamelistNum = saveMgr.GetCurrentProfile().gameKeys.Length;
        if (gamelistNum == 0)
        {
            // label (if no games associated with this profile)
            // . 'no games available'
            r.x = 0.05f * w;
            r.y = 0.425f * h;
            r.width = 0.45f * w;
            r.height = 0.045f * h;
            g = new GUIStyle(GUI.skin.label);
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            g.alignment = TextAnchor.MiddleCenter;
            s = "no saved games available";

            if (tinyPopup)
                GUI.enabled = false;

            GUI.Label(r, s, g);

            GUI.enabled = true;
        }
        else
        {
            string[] list = saveMgr.GetCurrentProfile().gameKeys;
            // buttons (load game data from file)
            // . game name, key, timestamp
            // . TODO: nav buttons up / down for longer lists
            r.x = 0.05f * w;
            r.y = 0.425f * h;
            r.width = 0.45f * w;
            r.height = 0.07f * h;
            for (int i = 0; i < gamelistNum; i++)
            {
                g = new GUIStyle(GUI.skin.button);
                g.font = buttonFont;
                g.fontStyle = FontStyle.Normal;
                g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f)); // smaller
                g.alignment = TextAnchor.MiddleCenter;
                g.normal.textColor = buttonFontColor;
                if (padButtonSelection == (2 + i))
                    g.normal.textColor = Color.white;
                g.active.textColor = buttonFontColor;
                if (!Application.isEditor)
                {
                    g.normal.background = buttonTex[0];
                    g.hover.background = buttonTex[1];
                    g.active.background = buttonTex[2];
                }
                s = "[SAVED " + (i+1) + "] : " + list[i];

                if (gameLoaded && saveMgr.IsGameCurrentlyLoaded())
                    GUI.enabled = saveMgr.GetCurrentGameData().gameKey != list[i];
                else
                    ConfigureCurrentGameEnabled();

                if (tinyPopup)
                        GUI.enabled = false;

                if (GUI.Button(r, s, g) || (padButtonSelection == (2+i) && padMgr.gPadDown[0].aButton))
                {
                    // REVIEW: if saved game is selected, net game selection is abandoned
                    currentNet = new MultiplayerHostPing(); // empty
                    popPlayerName = "";
                    saveMgr.SetIsRemoteClient(false);
                    saveMgr.SetHostPing(currentNet, popPlayerName);
                    // --
                    // unload current game data
                    if (saveMgr.IsGameCurrentlyLoaded())
                    {
                        // save current and clear current
                        saveMgr.SaveGameData(saveMgr.GetCurrentGameData().gameKey);
                        saveMgr.ClearCurrentGameData();
                    }
                    // load game data
                    if (saveMgr.LoadGameData(list[i]))
                    {
                        ConfigureCurrentGameEnabled();
                        if (gameLoaded)
                            selectionFeedback = "game loaded";
                        else
                            selectionFeedback = "* ERROR loading game data *";
                        feedbackTimer = FEEDBACKTIME;
                    }
                    else
                    {
                        // game file not found, remove this game key from profile
                        ProfileData profData = saveMgr.GetCurrentProfile();
                        profData = ProfileSystem.RemoveGameKey(profData, list[i]);
                        selectionFeedback = "game file missing, removing selection";
                        feedbackTimer = FEEDBACKTIME;
                    }
                }

                GUI.enabled = true;
                r.y += 0.075f * h;
            }
        }

        // remote game column
        // associated games on network
        int remotelistNum = hostPings.Length;
        if (remotelistNum == 0)
        {
            // label (if no games associated with this profile or all games full)
            // . 'no network games available'
            r.x = 0.5125f * w;
            r.y = 0.425f * h;
            r.width = 0.45f * w;
            r.height = 0.045f * h;
            g = new GUIStyle(GUI.skin.label);
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            g.alignment = TextAnchor.MiddleCenter;
            s = "no network games available";

            GUI.Label(r, s, g);
        }
        else
        {
            // we ask network traffic for current host pings
            string[] list = new string[hostPings.Length];
            // use the current profile id
            string currentProfileID = saveMgr.GetCurrentProfile().profileID;
            // parse host pings' data
            for (int i = 0; i < hostPings.Length; i++)
            {
                list[i] = MultiplayerSystem.GetProfilePlayerName(currentProfileID, hostPings[i]);
                list[i] += " in ";
                if (!MultiplayerSystem.CanProfileJoinGame(currentProfileID, hostPings[i]))
                    list[i] = "(full game) ";
                list[i] += hostPings[i].gameKey;
            }
            // buttons (load game data from file)
            // . game name, key, timestamp
            // . TODO: nav buttons up / down for longer lists
            r.x = 0.5125f * w;
            r.y = 0.425f * h;
            r.width = 0.45f * w;
            r.height = 0.07f * h;
            for (int i = 0; i < remotelistNum; i++)
            {
                g = new GUIStyle(GUI.skin.button);
                g.font = buttonFont;
                g.fontStyle = FontStyle.Normal;
                g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f)); // smaller
                g.alignment = TextAnchor.MiddleCenter;
                g.normal.textColor = buttonFontColor;
                if (padButtonSelection == (2 + gamelistNum + i))
                    g.normal.textColor = Color.white;
                g.active.textColor = buttonFontColor;
                if (!Application.isEditor)
                {
                    g.normal.background = buttonTex[0];
                    g.hover.background = buttonTex[1];
                    g.active.background = buttonTex[2];
                }
                s = "[NET " + (i + 1) + "] : " + list[i];

                GUI.enabled = MultiplayerSystem.CanProfileJoinGame(currentProfileID, hostPings[i]);

                if (hostPings[i].gameKey == currentNet.gameKey)
                    GUI.enabled = false;

                if (tinyPopup)
                    GUI.enabled = false;

                if (GUI.Button(r, s, g) || 
                    (padButtonSelection == (2 + gamelistNum + i) && padMgr.gPadDown[0].aButton))
                {
                    // REVIEW: if net game is selected, previous net game selection is abandoned
                    currentNet = new MultiplayerHostPing(); // empty
                    popPlayerName = "";
                    saveMgr.SetIsRemoteClient(false);
                    saveMgr.SetHostPing(currentNet, popPlayerName);
                    // --
                    // unload current game data
                    if (saveMgr.IsGameCurrentlyLoaded())
                    {
                        // save current and clear current
                        saveMgr.SaveGameData(saveMgr.GetCurrentGameData().gameKey);
                        saveMgr.ClearCurrentGameData();
                    }
                    string playerName = MultiplayerSystem.GetProfilePlayerName(currentProfileID, hostPings[i]);
                    if (playerName == "New Player")
                    {
                        // ask for joining player name in tiny popup
                        // from there, "Join" does the routine below
                        currentNet = hostPings[i];
                        tinyPopup = true;
                    }
                    else
                        popPlayerName = playerName;
                    MultiplayerRemoteRequest rRequest = MultiplayerSystem.FormRemoteRequest(saveMgr.GetCurrentProfile(), popPlayerName);
                    // TODO: send remote request signal, hear back from host
                    // on host, that's a public function GreenerGameManager.ProcessRemoteInvitationRequest(rRequest)
                    // NOTE: ! the following actually needs to be that return bool value
                    if (true) //if (host says GreenerGameManager.ProcessRemoteInvitationRequest(rRequest) == true)
                    {
                        // 'true' holds hostPing data to allow main menu "play" to join
                        currentNet = hostPings[i];
                        int idx = currentNet.gameKey.IndexOf("]-");
                        popGameName = currentNet.gameKey.Substring(idx + 2);
                        popMaxPlayers = currentNet.options.maxPlayers;
                        popAllowCheats = currentNet.options.allowCheats;
                        popAllowHazards = currentNet.options.allowHazards;
                        popAllowCurses = currentNet.options.allowCurses;
                        saveMgr.SetIsRemoteClient(true);
                        saveMgr.SetHostPing(currentNet, popPlayerName);

                        // when "PLAY" is pressed on main menu, join

                        // if new player, host has ability to add player after create player
                        // PlayerSystem.InitializePlayer(playerName, profID), and add with
                        // GameSystem.AddPlayer(gameData, playerData) ... 
                        // ... or if profile already exists in game just pick up latest data

                        selectionFeedback = "net game selected";
                        feedbackTimer = FEEDBACKTIME;
                    }
                    else
                    {
                        // request signal returned as rejected
                        currentNet = new MultiplayerHostPing(); // empty
                        popPlayerName = "";
                        saveMgr.SetIsRemoteClient(false);
                        saveMgr.SetHostPing(currentNet, popPlayerName);
                        selectionFeedback = "invitation request denied";
                        feedbackTimer = FEEDBACKTIME;
                    }
                }

                GUI.enabled = true;
                r.y += 0.075f * h;
            }
        }

        // selection feedback
        r.x = 0.2f * w;
        r.y = (backButton.y - 0.1f) * h;
        r.width = 0.6f * w;
        g = new GUIStyle(GUI.skin.label);
        g.normal.textColor = labelFontColor;
        g.hover.textColor = labelFontColor;
        g.active.textColor = labelFontColor;
        g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
        g.fontStyle = FontStyle.Italic;
        g.alignment = TextAnchor.MiddleCenter;
        s = selectionFeedback;
        GUI.Label(r, s, g);

        // tiny popup
        if (tinyPopup)
        {
            r.x = 0.3f * w;
            r.y = 0.35f * h;
            r.width = 0.4f * w;
            r.height = 0.3f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(20f * (w/1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            g.padding = new RectOffset(0, 0, 20, 0);
            s = "Enter New Player Name";
            GUI.Box(r,s,g);

            r.x = 0.35f * w;
            r.y = 0.45f * h;
            r.width = 0.3f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.textField);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            popPlayerName = GUI.TextField(r, popPlayerName, g);

            r.x = 0.325f * w;
            r.y = 0.55f * h;
            r.width = 0.15f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            if (!Application.isEditor)
            {
                g.normal.background = buttonTex[0];
                g.hover.background = buttonTex[1];
                g.active.background = buttonTex[2];
            }
            s = "Accept";
            GUI.enabled = (popPlayerName != "");
            if (GUI.Button(r, s, g))
            {
                // hold hostPing data to allow main menu "play" to join
                int idx = currentNet.gameKey.IndexOf("]-");
                popGameName = currentNet.gameKey.Substring(idx + 2);
                popMaxPlayers = currentNet.options.maxPlayers;
                popAllowCheats = currentNet.options.allowCheats;
                popAllowHazards = currentNet.options.allowHazards;
                popAllowCurses = currentNet.options.allowCurses;
                saveMgr.SetIsRemoteClient(true);
                saveMgr.SetHostPing(currentNet, popPlayerName);
                tinyPopup = false;
                selectionFeedback = "net game selected";
                feedbackTimer = FEEDBACKTIME;
            }

            r.x = 0.525f * w;
            s = "Cancel";
            GUI.enabled = true;
            if (GUI.Button(r, s, g))
            {
                currentNet = new MultiplayerHostPing(); // empty
                popPlayerName = "";
                saveMgr.SetIsRemoteClient(false);
                saveMgr.SetHostPing(currentNet, popPlayerName);
                tinyPopup = false;
                selectionFeedback = "net game selection cancelled";
                feedbackTimer = FEEDBACKTIME;
            }
        }

        // back button
        r = backButton;
        r.x *= w;
        r.y *= h;
        r.width *= w;
        r.height *= h;
        g = new GUIStyle(GUI.skin.button);
        g.font = buttonFont;
        g.fontStyle = buttonFontStyle;
        g.fontSize = Mathf.RoundToInt(backButtonFontSizeAt1024 * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = buttonFontColor;
        if (padButtonSelection == gamelistNum + 2)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = backButtonText;

        if (tinyPopup)
            GUI.enabled = false;
        if (GUI.Button(r, s, g) ||
            padButtonSelection == gamelistNum+2 && padMgr.gPadDown[0].aButton)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
