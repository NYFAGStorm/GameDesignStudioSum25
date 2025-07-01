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
    private string selectionFeedback;
    private float feedbackTimer;

    public bool networkActive; // is there an active game network?

    private string popGameName;
    private string popPlayerName;
    private int popMaxPlayers;
    private bool popAllowCheats;
    private bool popAllowHazards;
    private bool popAllowCurses;

    private MultiGamepad padMgr;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;

    private bool gamePopup; // is displaying popup
    private bool popupEnabled; // is popping up or down
    private float popupTimer;
    private float popupProgress;
    private AnimationCurve popupCurve;

    const float POPUPTIME = 1f;
    const float FEEDBACKTIME = 4f;


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
                GUI.Label(r, s, g);
            }

            // text fields (read-only if not new)
            // . name
            r.x += 0.25f * w;
            r.y -= 0.375f * h;
            r.width = 0.2f * w;
            if (newGame)
                g = new GUIStyle(GUI.skin.textField);
            g.fontSize = Mathf.RoundToInt(labelFontSizeAt1024 * (w / 1024f));
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = labelFontColor;
            g.hover.textColor = labelFontColor;
            g.active.textColor = labelFontColor;
            if (newGame)
                popGameName = GUI.TextField(r, popGameName, 20, g);
            else
                GUI.Label(r, popGameName, g);

            // buttons (option buttons disabled if not new)
            GUI.enabled = newGame;
            // . + / - max players
            // . toggle cheats
            // . toggle hazards
            // . toggle curses
            // . (textfield) joining player name
            // . Create new game ('Unload' if not new)
            // . Cancel ('Close' if not new)
            r.x += 0.025f * w;
            r.y += 0.075f * h;
            r.width = 0.075f * w;
            g = new GUIStyle(GUI.skin.button);
            g.fontStyle = buttonFontStyle;
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            if (padButtonSelection == 0)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "-";
            if (GUI.Button(r, s, g)) // TODO: gamepad support
            {
                popMaxPlayers--;
                popMaxPlayers = Mathf.Clamp(popMaxPlayers, 1, 8);
            }
            r.x += 0.075f * w;
            g.normal.textColor = buttonFontColor;
            if (padButtonSelection == 1)
                g.normal.textColor = Color.white;
            s = "+";
            if (GUI.Button(r, s, g)) // TODO: gamepad support
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
            if (GUI.Button(r, s, g)) // TODO: gamepad support
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
            if (GUI.Button(r, s, g)) // TODO: gamepad support
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
            if (GUI.Button(r, s, g)) // TODO: gamepad support
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
                GUI.Label(r, ("Playing as '" + gamePlayerName + "' in this game"), g);
            }
            GUI.enabled = true;

            // popup buttons
            r.x = 0.25f * w;
            r.y += 0.1f * h;
            r.width = 0.2f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.button);
            g.normal.textColor = buttonFontColor;
            g.active.textColor = buttonFontColor;
            // UNLOAD / CREATE
            if (padButtonSelection == 5)
                g.normal.textColor = Color.white;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "UNLOAD";
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
        if ( gameLoaded )
        {
            GameData gData = saveMgr.GetCurrentGameData();
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
        if (padButtonSelection == 0)
            g.normal.textColor = Color.white;
        g.active.textColor = buttonFontColor;
        s = "Game Options";

        GUI.enabled = gameLoaded;
        if (GUI.Button(r, s, g) || 
            (padButtonSelection == 0 && padMgr.gPadDown[0].aButton))
        {
            newGame = false;
            GameData gData = saveMgr.GetCurrentGameData();
            if (gData != null)
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
        s = "Create Game";

        if (GUI.Button(r, s, g) || (padButtonSelection == 1 && padMgr.gPadDown[0].aButton))
        {
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

            GUI.Label(r, s, g);
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
                s = "[SAVED " + (i+1) + "] : " + list[i]; // REVIEW: proper string format for this button

                if (gameLoaded)
                    GUI.enabled = saveMgr.GetCurrentGameData().gameKey != list[i];

                if (GUI.Button(r, s, g) || (padButtonSelection == (2+i) && padMgr.gPadDown[0].aButton)) // TODO: gamepad support
                {
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
        int remotelistNum = 2; // how many hosts are pinging now?
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
            // TODO: use multiplayer information (host pings parsed to arrive at list)
            // temp - test data ---
            string currentProfileID = saveMgr.GetCurrentProfile().profileID;
            MultiplayerHostPing[] hostPings = new MultiplayerHostPing[2];
            hostPings[0].gameKey = "[000]-a really fun game";
            hostPings[0].availablePlayerSlots = 1;
            hostPings[0].profiles = new string[2];
            hostPings[0].profiles[0] = "hostprofile";
            hostPings[0].profiles[1] = currentProfileID;
            hostPings[0].playerNames = new string[2];
            hostPings[0].playerNames[0] = "host player name";
            hostPings[0].playerNames[1] = "your name";
            hostPings[1].gameKey = "[111]-another game";
            hostPings[1].availablePlayerSlots = 0;
            hostPings[1].profiles = new string[1];
            hostPings[1].profiles[0] = "hostprofile";
            hostPings[1].playerNames = new string[1];
            hostPings[1].playerNames[0] = "host player name";
            // ---
            string[] list = new string[hostPings.Length];
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
                if (padButtonSelection == (2 + i))
                    g.normal.textColor = Color.white;
                g.active.textColor = buttonFontColor;
                s = "[NET " + (i + 1) + "] : " + list[i]; // REVIEW: proper string format for this button

                GUI.enabled = MultiplayerSystem.CanProfileJoinGame(currentProfileID, hostPings[i]);

                if (GUI.Button(r, s, g) || (padButtonSelection == (2 + i) && padMgr.gPadDown[0].aButton)) // TODO: gamepad support
                {
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
                        // stay here and ask for joining player name
                        // TODO: tiny popup for new player name "Join" and "Cancel" buttons
                    }
                    MultiplayerRemoteJoin rJoin = MultiplayerSystem.FormRemoteJoin(saveMgr.GetCurrentProfile(), playerName);
                    // TODO: send remote join signal, hear back from host
                    // on host, that's MultiplayerSystem.HandleRemoteJoinRequest(saveMgr.GetCurrentGameData(), rJoin)
                    if ( false ) // we get that bool result back and use it here
                    {
                        // where a 'true' result allows join immediately

                        // if new player, host has ability to add player after create player
                        // PlayerSystem.InitializePlayer(playerName, profID), and add with
                        // GameSystem.AddPlayer(gameData, playerData) ... 
                        // ... or if profile already exists in game just pick up latest data
                    }
                    else
                    {
                        // join signal returned as rejected
                        selectionFeedback = "join request denied";
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
        s = backButtonText;

        if (GUI.Button(r, s, g) ||
            padButtonSelection == gamelistNum+2 && padMgr.gPadDown[0].aButton)
        {
            SceneManager.LoadScene("Menu");
        }
    }
}
