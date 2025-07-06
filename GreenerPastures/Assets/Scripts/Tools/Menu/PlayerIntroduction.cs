using UnityEngine;

public class PlayerIntroduction : MonoBehaviour
{
    // Author: Glenn Storm
    // Handles a routine seen only when a new player arrives (a.k.a. onboarding)

    public enum ScriptedBeatAction
    {
        Default,
        Dialog,
        EdenMark,
        TeleportConfig,
        HelpMessage,
        TeleportEden,
        CameraChange,
        EnableSkip,
        PlayerSetting,
        MoveIsland,
        VFXSpawn,
        ItemSpawn,
        PlotChange,
        DeleteItem,
        EndIntro
    }

    public enum ScriptedBeatTransition
    {
        Default,
        TimedDuration,
        PlayerResponse,
        EdenCallback
    }

    [System.Serializable]
    public struct ScriptedBeat
    {
        public string name;
        public ScriptedBeatAction action;
        public bool actionDone;
        public string dialogLine;
        public Vector3 npcMark;
        public ScriptedBeatTransition transition;
        public bool transitionDone;
        public float duration;
        public CameraManager.CameraMode cam;
        public PositionData islandPos;
    }
    public ScriptedBeat[] introBeats;
    public ScriptedBeat currentBeat; // for use in debugging via inspector only

    public bool introRunning;
    public bool introPop;
    public bool dialogPop;

    private Texture2D[] characterLines;
    private Texture2D[] characterSkins;
    private Texture2D[] characterAccents;
    private Texture2D[] characterFills;

    private Color[] characterSkinTones;
    private Color[] characterColors;

    private PlayerOptions configOptions;
    private bool configValid;
    private bool[] configsTouched = new bool[4];

    private enum IntroHelpMessage
    {
        Default,
        MoveControls,
        ActionButton,
        InventorySelect,
        DropButton,
        ControlsDisplay,
        AlmanacDisplay
    }
    private int helpMessage;
    private float helpMessageTimer;

    private bool canSkipIntro;
    private bool cancelIntro;
    private bool pauseIntro;

    private float introTimer;

    private NPCController eden;
    private PlotManager managedPlot;
    private LooseItemManager droppedItem;

    private int modelSelection = 0;
    private int skinSelection = 0;
    private int accentSelection = 0;
    private int fillSelection = 0;

    private PlayerControlManager pcm;
    private CameraManager camMgr;

    private int currentBeatIndex;
    private float beatTimer;
    public bool beatTimeUp;
    public bool npcCallback;
    public bool playerResponse;
    private int beatScriptEndIndex;

    const float DEFAULTINTROTIME = 0.618f;
    const float PAUSETIME = 1f;
    const float LONGPAUSETIME = 2f;
    const float HELPMESSAGETIME = 7f;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            InitializeCharacterTextures();
            InitializeCharacterColors();

            ConfigureIntroBeats();

            ValidateIntroBeats();
        }
    }

    void ValidateIntroBeats()
    {
        string prevName = "";
        Vector3 prevMark = Vector3.zero;
        for (int i = 0; i < introBeats.Length; i++)
        {
            if (prevName == introBeats[i].name)
                Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has the same name as previous. this will cause errors at runtime.");
            introBeats[i].name = "[" + i + "] " + introBeats[i].name;
            if (introBeats[i].transition == ScriptedBeatTransition.TimedDuration &&
                introBeats[i].duration == 0f)
                Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has no duration, but is set to be timed. this will cause errors at runtime.");
            if (introBeats[i].action == ScriptedBeatAction.EdenMark)
            {
                if (introBeats[i].npcMark == Vector3.zero)
                    Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has an npc mark configured, but mark is zero. this will cause errors at runtime.");
                else if (Vector3.Distance(prevMark, introBeats[i].npcMark) <= .25f )
                {
                    Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has an npc mark configured, but mark too close to npc position (" + Vector3.Distance(prevMark, introBeats[i].npcMark) + "). this will cause errors at runtime. will adjust this mark further away.");
                    Vector3 newMarkPos = introBeats[i].npcMark;
                    newMarkPos += (introBeats[i].npcMark-prevMark);
                    introBeats[i].npcMark = newMarkPos;
                }
            }
            if (introBeats[i].action == ScriptedBeatAction.Dialog && introBeats[i].dialogLine == "")
                Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has dialog configured, but no dialog line is found. this will cause errors at runtime.");
            if (introBeats[i].action == ScriptedBeatAction.Dialog && 
                introBeats[i].transition != ScriptedBeatTransition.PlayerResponse &&
                introBeats[i].transition != ScriptedBeatTransition.TimedDuration)
                Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : intro beat " + i + " has dialog configured, but transition from this beat is '" + introBeats[i].transition.ToString() +"'. this will cause errors at runtime.");
            if (introBeats[i].action == ScriptedBeatAction.EndIntro)
                beatScriptEndIndex = i;

            prevName = introBeats[i].name;
            if (introBeats[i].npcMark != Vector3.zero)
                prevMark = introBeats[i].npcMark;
        }
        if ( beatScriptEndIndex == 0)
            Debug.LogWarning("--- PlayerIntroduction [ValidateIntroBeats] : no final beat with 'end intro' configured. this will cause errors at runtime.");
    }

    void InitializeCharacterTextures()
    {
        characterLines = new Texture2D[2];
        characterLines[0] = (Texture2D)Resources.Load("ProtoWizard_LineArt");
        characterLines[1] = (Texture2D)Resources.Load("ProtoWizardF_LineArt");
        characterSkins = new Texture2D[2];
        characterSkins[0] = (Texture2D)Resources.Load("ProtoWizard_FillSkin");
        characterSkins[1] = (Texture2D)Resources.Load("ProtoWizardF_FillSkin");
        characterAccents = new Texture2D[2];
        characterAccents[0] = (Texture2D)Resources.Load("ProtoWizard_FillAccent");
        characterAccents[1] = (Texture2D)Resources.Load("ProtoWizardF_FillAccent");
        characterFills = new Texture2D[2];
        characterFills[0] = (Texture2D)Resources.Load("ProtoWizard_FillMain");
        characterFills[1] = (Texture2D)Resources.Load("ProtoWizardF_FillMain");
    }

    void InitializeCharacterColors()
    {
        characterSkinTones = new Color[8];
        for (int i = 0; i < 8; i++)
        {
            characterSkinTones[i] = PlayerSystem.GetPlayerSkinColor((PlayerSkinColor)i);
        }
        characterColors = new Color[16];
        for (int i = 0; i < 16; i++)
        {
            characterColors[i] = PlayerSystem.GetPlayerColor((PlayerColor)i);
        }
    }

    void FindManagedPlot()
    {
        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i = 0; i < plots.Length; i++)
        {
            if (plots[i].gameObject.transform.localPosition.x == 2f &&
                plots[i].gameObject.transform.localPosition.z == -2f)
            {
                managedPlot = plots[i];
                break;
            }
        }
        if (managedPlot == null)
        {
            Debug.LogError("--- PlayerIntroduction [FindManagedPlot] : no managed plot found at local position 2,0,-2 in scene. aborting.");
            enabled = false;
        }
    }

    void TeleporterControl( bool turnOn, string teleporterTag )
    {
        TeleportManager[] tporters = GameObject.FindObjectsByType<TeleportManager>(FindObjectsSortMode.None);
        for (int i = 0; i < tporters.Length; i++)
        {
            if (teleporterTag == "")
                tporters[i].enabled = turnOn;
            else if (tporters[i].teleporterTag == teleporterTag)
                tporters[i].enabled = turnOn;
        }
    }

    // THIS IS THE SCRIPT
    //
    void ConfigureIntroBeats()
    {
        introBeats = new ScriptedBeat[125]; // we use ~100 beats, currently
        int beat = 0;
        introBeats[beat].name = "intro launch - world view";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].npcMark = new Vector3(19.5f, 0f, -24f);
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 3f; // cam manager has a hard-coded 4f move
        beat++;
        introBeats[beat].name = "intro zoom in - eden move";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(20f, 0f, -26f);
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        beat++;
        introBeats[beat].name = "'welcome biomancer!'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Welcome Biomancer! My name is Eden. Tell me about yourself.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "config appearance";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "player appears - camera medium";
        introBeats[beat].action = ScriptedBeatAction.CameraChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        introBeats[beat].cam = CameraManager.CameraMode.Medium;
        beat++;
        introBeats[beat].name = "disable teleporters";
        introBeats[beat].action = ScriptedBeatAction.TeleportConfig;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 0f;
        beat++;
        introBeats[beat].name = "(can skip now)";
        introBeats[beat].action = ScriptedBeatAction.EnableSkip;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        introBeats[beat].name = "'happy you chose'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "We are so happy you chose to help grow our magical community.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "help message WASD";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 0f;
        beat++;
        introBeats[beat].name = "'we tend to the floating islands'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Here we tend to the floating islands that our Genesis Tree provides.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "set island pos wider";
        introBeats[beat].action = ScriptedBeatAction.PlayerSetting;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.x = 20f;
        introBeats[beat].islandPos.y = 0f;
        introBeats[beat].islandPos.z = -20f;
        introBeats[beat].islandPos.w = 7f;
        beat++;
        introBeats[beat].name = "move eden wider";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(22f, 0f, -24f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'This central island'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "This central island connects every other through our teleporter nodes.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "move eden near market";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(20.25f, 0f, -23.5f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'Here is the market'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Here is the market, where we can buy supplies and sell your wares.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden purchase";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(18.75f, 0f, -23.25f);
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "'Let me purchase'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Let me purchase a couple seeds and take you to your floating island.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "pause for purchase";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        beat++;
        introBeats[beat].name = "eden move away from market";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(18f, 0f, -22.5f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "brief pause";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 0.5f;
        beat++;
        introBeats[beat].name = "eden to magic spot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(17f, 0f, -19f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "magic vfx";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = 17f;
        introBeats[beat].islandPos.z = -19f;
        introBeats[beat].islandPos.w = -1f;
        beat++;
        introBeats[beat].name = "cam long for island rise";
        introBeats[beat].action = ScriptedBeatAction.CameraChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = .1f;
        introBeats[beat].cam = CameraManager.CameraMode.Long;
        beat++;
        introBeats[beat].name = "island rise";
        introBeats[beat].action = ScriptedBeatAction.MoveIsland;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 4f;
        beat++;
        introBeats[beat].name = "'isn't that beautiful'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Isn't that a beautiful sight! Your new island with greener pastures.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "pause";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = .5f;
        beat++;
        introBeats[beat].name = "eden toward teleporter";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(16.5f, 0f, -17.5f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "enable teleporters";
        introBeats[beat].action = ScriptedBeatAction.TeleportConfig;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 1f;
        beat++;
        introBeats[beat].name = "cam medium";
        introBeats[beat].action = ScriptedBeatAction.CameraChange;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].cam = CameraManager.CameraMode.Medium;
        beat++;
        introBeats[beat].name = "eden to teleporter";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(16f, 0f, -16f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "teleporter vfx (a)";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.x = 16f;
        introBeats[beat].islandPos.z = -16f;
        beat++;
        introBeats[beat].name = "teleporter vfx (b)";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.x = 4f;
        introBeats[beat].islandPos.z = -4f;
        beat++;
        introBeats[beat].name = "eden teleport";
        introBeats[beat].action = ScriptedBeatAction.TeleportEden;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        introBeats[beat].islandPos.x = 4f;
        introBeats[beat].islandPos.z = -4f;
        beat++;
        introBeats[beat].name = "eden off teleporter";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(4.5f, 0f, -3.25f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "force player to teleporter";
        introBeats[beat].action = ScriptedBeatAction.PlayerSetting;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = 16f;
        introBeats[beat].islandPos.z = -16f;
        introBeats[beat].islandPos.w = 0.1f;
        beat++;
        introBeats[beat].name = "eden walk over to farm";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3.25f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "- pause for player -";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "disable tower teleporters";
        introBeats[beat].action = ScriptedBeatAction.TeleportConfig;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 2f;
        beat++;
        introBeats[beat].name = "eden step to farm edge";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "- pause at farm -";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "eden step up to speak";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2.75f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'here is your farm'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Here is your farm, where you are to grow crops, harvest and sell them.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden moves to plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.75f, 0f, -2.25f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'we work the land'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "We work the land and improve the soil. First let's get this plot ready.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden to plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2.25f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "tilling plot from wild";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.w = -3f; // <=-3 = wild to dirt
        beat++;
        introBeats[beat].name = "eden around plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.75f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "help message action button";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 1f;
        beat++;
        introBeats[beat].name = "tilling plot from dirt";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.w = -2f; // <=-2 dirt to tilled
        beat++;
        introBeats[beat].name = "'we care for the land'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "We care for the land and the land provides for us. Let's water this plot.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden moves back to plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2.25f, 0f, -2.25f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "water";
        introBeats[beat].action = ScriptedBeatAction.PlotChange; // water
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        introBeats[beat].islandPos.w = -1f; // <=-1 watered
        beat++;
        introBeats[beat].name = "eden moves around plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.75f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'it's time to plant our seed'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine = 
            "And with this plot all ready, it's time to plant our seed.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "drop seed";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // seed
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = .381f;
        introBeats[beat].islandPos.w = -1f; // <= -1 seed
        beat++;
        introBeats[beat].name = "- planted -";
        introBeats[beat].action = ScriptedBeatAction.PlotChange; // planted
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 0f; // <=0 planted
        beat++;
        introBeats[beat].name = "eden steps away from plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3.25f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden turns around";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'Different plants grow faster'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Different plants grow faster or slower, yield more or less, ...";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'...some grow better'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "...some grow better in certain seasons, some even grow only at night!";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'in your biomancer's almanac'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Later, you can learn more about everything in your Biomancer's Almanac.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "cheat grow 1";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        introBeats[beat].islandPos.w = 4f; // <4 magic grow
        beat++;
        introBeats[beat].name = "- pause -";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "cheat grow 2";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 3f;
        introBeats[beat].islandPos.w = 4f; // <4 magic grow
        beat++;
        introBeats[beat].name = "'with our plant all grown'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "With our plant all grown, we can harvest the fruit or flower.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden moves to harvest";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "harvest plant";
        introBeats[beat].action = ScriptedBeatAction.PlotChange; // harvest
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.w = 1f; // <=1 harvested
        beat++;
        introBeats[beat].name = "flower drops";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // flower
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = -1f;
        introBeats[beat].islandPos.w = 0f; // <= 0 fruit
        beat++;
        introBeats[beat].name = "eden moves around plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.5f, 0f, -1.5f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden turns around";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.75f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'take your flower'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Take your flower with the action button. Now, I'll dig up this stalk.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "help message action button";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 1f;
        beat++;
        introBeats[beat].name = "move eden to plant";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "digging up stalk pause";
        introBeats[beat].action = ScriptedBeatAction.Default; // dig up
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 3f;
        beat++;
        introBeats[beat].name = "uprooted";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 2f; // <=2 uprooted
        beat++;
        introBeats[beat].name = "drop stalk";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // stalk
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = -.381f;
        introBeats[beat].islandPos.w = 1f; // <= 1 stalk
        beat++;
        introBeats[beat].name = "'stalks and other plant material'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Stalks and other plant material go in the compost bin to make fertilizer.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "eden moves to plant";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.75f, 0f, -1.75f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "help message inventory";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 2f;
        beat++;
        introBeats[beat].name = "eden picks up stalk";
        introBeats[beat].action = ScriptedBeatAction.DeleteItem;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "eden moves to compost bin";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(-2.75f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden drops stalk in bin";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // stalk
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = -.381f;
        introBeats[beat].islandPos.w = 1f; // <= 1 stalk
        beat++;
        introBeats[beat].name = "step away";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(-2.25f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'By adding fertilizer to the soil'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "By adding fertilizer to the soil, it improves and so does you harvest.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "compost pause";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 3f;
        beat++;
        introBeats[beat].name = "help message drop";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 3f;
        beat++;
        introBeats[beat].name = "poo drop";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // fertilizer
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = -0.1f;
        introBeats[beat].islandPos.w = 2f; // <= 2 fertilizer
        beat++;
        introBeats[beat].name = "step to poo";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(-2.75f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden picks up fertilizer";
        introBeats[beat].action = ScriptedBeatAction.DeleteItem; // fertilizer
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "eden moves back to plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(1.5f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "drops fertilizer on plot";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // fertilizer
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = .618f;
        introBeats[beat].islandPos.w = 2f; // <= 2 fertilizer
        beat++;
        introBeats[beat].name = "eden goes to till plot";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(2f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "till plot to dirt";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.w = 3f; // <3 uproot to dirt
        beat++;
        introBeats[beat].name = "eden moves back";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3.25f, 0f, -2.25f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden turns around";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(3f, 0f, -2f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "'Here's the rest of the seed'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Here's the rest of the seed I got for you from the market.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "drop seed";
        introBeats[beat].action = ScriptedBeatAction.ItemSpawn; // seed
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = -.618f;
        introBeats[beat].islandPos.w = -1f; // <= -1 seed
        beat++;
        introBeats[beat].name = ".short pause.";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = .5f;
        beat++;
        introBeats[beat].name = "'Now you try'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Now you try. You can use your action key 'E' to till and plant this plot.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "help message action button";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 1f;
        beat++;
        introBeats[beat].name = "'All this hard work pays'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "All this hard work pays off at the market, and it all leads to magic.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'When you level up'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "When you level up, you'll find the magic crafting table in your tower.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'On the table, your grimiore'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "On the table, your grimiore will have new spells available to craft.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'Crafted spells are stored'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Crafted spells are stored in your spell book, so you can cast them.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "- dramatic pause -";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "'Like this!'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Like this!";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "- magic pause -";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = .5f;
        beat++;
        introBeats[beat].name = "magic vfx";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 2f;
        introBeats[beat].islandPos.x = 3f;
        introBeats[beat].islandPos.z = -2f;
        introBeats[beat].islandPos.w = -1f;
        beat++;
        introBeats[beat].name = "plant grows";
        introBeats[beat].action = ScriptedBeatAction.PlotChange;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].islandPos.w = 4f; // <4 magic grow
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "'Great! your're a natural'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Great! You're a natural. May the grace of the Genesis Tree be with you.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "'Again, welcome'";
        introBeats[beat].action = ScriptedBeatAction.Dialog;
        introBeats[beat].dialogLine =
            "Again, welcome and enjoy your time with us.";
        introBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        introBeats[beat].name = "pause end";
        introBeats[beat].action = ScriptedBeatAction.Default;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        beat++;
        introBeats[beat].name = "help message player controls";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 4f;
        beat++;
        introBeats[beat].name = "enable teleporters";
        introBeats[beat].action = ScriptedBeatAction.TeleportConfig;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 1f;
        beat++;
        introBeats[beat].name = "walk to teleporter";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(4f, 0f, -4f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "teleport vfx (a)";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.x = 4f;
        introBeats[beat].islandPos.z = -4f;
        beat++;
        introBeats[beat].name = "teleport vfx (b)";
        introBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.x = 16f;
        introBeats[beat].islandPos.z = -16f;
        beat++;
        introBeats[beat].name = "eden teleports";
        introBeats[beat].action = ScriptedBeatAction.TeleportEden;
        introBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        introBeats[beat].duration = 1f;
        introBeats[beat].islandPos.x = 16f;
        introBeats[beat].islandPos.z = -16f;
        beat++;
        introBeats[beat].name = "eden walks to market";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(18f, 0f, -19f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "eden walks into market";
        introBeats[beat].action = ScriptedBeatAction.EdenMark;
        introBeats[beat].npcMark = new Vector3(19.5f, 0f, -22f);
        introBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        introBeats[beat].name = "help message almanac";
        introBeats[beat].action = ScriptedBeatAction.HelpMessage;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
        introBeats[beat].islandPos.w = 5f;
        beat++;
        introBeats[beat].name = "--- intro end ---";
        introBeats[beat].action = ScriptedBeatAction.EndIntro;
        introBeats[beat].transition = ScriptedBeatTransition.Default;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            pauseIntro = !pauseIntro;
            print("INTRO PAUSE [" + pauseIntro + "] beat (" + currentBeat + ")");
        }
        if (pauseIntro)
            return;

        // handle intro dialog step
        if (cancelIntro)
        {
            cancelIntro = false;
            dialogPop = false;
            // set cut-to-end beat index
            currentBeatIndex = beatScriptEndIndex - 11;
            currentBeat = introBeats[currentBeatIndex - 1];
            currentBeat.transition = ScriptedBeatTransition.TimedDuration;
            beatTimeUp = false;
            beatTimer = DEFAULTINTROTIME;
            camMgr.allowPlayerControlCam = true;
        }
        // run intro timer
        if (introTimer > 0f)
        {
            introTimer -= Time.deltaTime;
            if (introTimer < 0f)
            {
                introTimer = 0f;
                dialogPop = true;
            }
        }
        // run help message timer
        if (helpMessageTimer > 0f)
        {
            helpMessageTimer -= Time.deltaTime;
            if (helpMessageTimer < 0f)
            {
                helpMessageTimer = 0f;
                helpMessage = 0;
            }
        }

        // intro beat script
        if (!introRunning)
            return;

        // run beat timer
        if (beatTimer > 0f)
        {
            beatTimer -= Time.deltaTime;
            if (beatTimer < 0f)
            {
                beatTimer = 0f;
                beatTimeUp = true;
            }
            // island move over time
            if (currentBeat.action == ScriptedBeatAction.MoveIsland)
            {
                AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                GameObject islandA = GameObject.Find("Island Alpha");
                Vector3 iPos = Vector3.zero;
                iPos.x = 4f;
                iPos.y = -40f;
                iPos.z = -8f;
                islandA.transform.position = Vector3.Lerp(iPos,Vector3.zero,1f-curve.Evaluate(beatTimer / currentBeat.duration));
            }
        }
        // detect npc destination reached ('callback')
        npcCallback = eden.destinationReached;
        // handle beat script transition
        switch (currentBeat.transition)
        {
            case ScriptedBeatTransition.Default:
                currentBeatIndex++; // immediate transition
                currentBeat.transitionDone = true;
                break;
            case ScriptedBeatTransition.TimedDuration:
                if (beatTimeUp)
                {
                    currentBeatIndex++;
                    beatTimeUp = false;
                    currentBeat.transitionDone = true;
                }
                break;
            case ScriptedBeatTransition.PlayerResponse:
                if (playerResponse)
                {
                    currentBeatIndex++;
                    playerResponse = false;
                    currentBeat.transitionDone = true;
                }
                break;
            case ScriptedBeatTransition.EdenCallback:
                if (npcCallback)
                {
                    currentBeatIndex++;
                    npcCallback = false;
                    eden.destinationReached = false;
                    currentBeat.transitionDone = true;
                }
                break;
            default:
                break;
        }
        if (!currentBeat.actionDone)
        {
            // handle beat script action
            switch (currentBeat.action)
            {
                case ScriptedBeatAction.Default:
                    // do nothing (pause)
                    break;
                case ScriptedBeatAction.Dialog:
                    introTimer = DEFAULTINTROTIME;
                    playerResponse = false;
                    break;
                case ScriptedBeatAction.EdenMark:
                    eden.moveTarget = currentBeat.npcMark;
                    eden.destinationReached = false;
                    break;
                case ScriptedBeatAction.TeleportConfig:
                    if (currentBeat.islandPos.w <= 0f)
                        TeleporterControl(false, "");
                    else if (currentBeat.islandPos.w > 0f &&
                        currentBeat.islandPos.w <= 1f)
                        TeleporterControl(true, "");
                    else if (currentBeat.islandPos.w > 1f &&
                        currentBeat.islandPos.w <= 2f)
                        TeleporterControl(false, "tower");
                    break;
                case ScriptedBeatAction.HelpMessage:
                    if (currentBeat.islandPos.w <= 0f)
                    {
                        // WASD
                        helpMessage = 1;
                        helpMessageTimer = HELPMESSAGETIME;
                    }
                    else if (currentBeat.islandPos.w > 0f &&
                        currentBeat.islandPos.w <= 1f)
                    {
                        // ACTION
                        helpMessage = 2;
                        helpMessageTimer = HELPMESSAGETIME;
                    }
                    else if (currentBeat.islandPos.w > 1f &&
                        currentBeat.islandPos.w <= 2f)
                    {
                        // INVENTORY
                        if (pcm != null)
                            pcm.hidePlayerHUD = false;
                        helpMessage = 3;
                        helpMessageTimer = HELPMESSAGETIME;
                    }
                    else if (currentBeat.islandPos.w > 2f &&
                        currentBeat.islandPos.w <= 3f)
                    {
                        // DROP
                        helpMessage = 4;
                        helpMessageTimer = HELPMESSAGETIME;
                    }
                    else if (currentBeat.islandPos.w > 3f &&
                        currentBeat.islandPos.w <= 4f)
                    {
                        // CONTROLS
                        helpMessage = 5;
                        helpMessageTimer = HELPMESSAGETIME;
                        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                        if (igc != null)
                        {
                            igc.enabled = true;
                        }
                    }
                    else if (currentBeat.islandPos.w > 4f &&
                        currentBeat.islandPos.w <= 5f)
                    {
                        // ALMANAC
                        helpMessage = 6;
                        helpMessageTimer = HELPMESSAGETIME;
                        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                        if (iga != null)
                        {
                            iga.enabled = true;
                        }
                    }
                    break;
                case ScriptedBeatAction.TeleportEden:
                    Vector3 pos = eden.gameObject.transform.position;
                    pos.x = currentBeat.islandPos.x;
                    pos.z = currentBeat.islandPos.z;
                    eden.gameObject.transform.position = pos;
                    eden.moveTarget = pos;
                    eden.destinationReached = false;
                    break;
                case ScriptedBeatAction.CameraChange:
                    camMgr.SetCameraViewIntro(currentBeat.cam);
                    break;
                case ScriptedBeatAction.EnableSkip:
                    canSkipIntro = true;
                    break;
                case ScriptedBeatAction.PlayerSetting:
                    // all the stuff
                    if (currentBeat.islandPos.x != 0f)
                    {
                        pcm.playerData.island.x = currentBeat.islandPos.x;
                        pcm.playerData.island.y = currentBeat.islandPos.y;
                        pcm.playerData.island.z = currentBeat.islandPos.z;
                        pcm.playerData.island.w = currentBeat.islandPos.w;
                    }
                    break;
                case ScriptedBeatAction.MoveIsland:
                    // configure move based on target position
                    // (move performed during timer above)
                    break;
                case ScriptedBeatAction.VFXSpawn:
                    Vector3 vfxPos = Vector3.zero;
                    // either teleport or magic
                    GameObject vfx = null;
                    if (currentBeat.islandPos.w >= 0f)
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
                    else
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX_LocalMagic"));
                    vfxPos.x = currentBeat.islandPos.x;
                    vfxPos.z = currentBeat.islandPos.z;
                    vfx.transform.position = vfxPos;
                    if (vfx.GetComponentInChildren<SpriteRenderer>()!= null)
                        vfx.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                    Destroy(vfx, 1f);
                    break;
                case ScriptedBeatAction.ItemSpawn:
                    // item type
                    // using data in islandPos
                    ItemType it = ItemType.Seed;
                    // .seed
                    if( currentBeat.islandPos.w <= -1f)
                        it = ItemType.Seed;
                    // .flower
                    if (currentBeat.islandPos.w > -1f && 
                        currentBeat.islandPos.w <= 0f)
                        it = ItemType.Fruit;
                    // .stalk
                    if (currentBeat.islandPos.w > 0f &&
                        currentBeat.islandPos.w <= 1f)
                        it = ItemType.Stalk;
                    // .fertilizer
                    if (currentBeat.islandPos.w > 1f &&
                        currentBeat.islandPos.w <= 2f)
                        it = ItemType.Fertilizer;
                    // start end positions (start on eden, end eden + island.x)
                    // hit up item spawn manager
                    ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                    if (ism != null)
                    {
                        Vector3 dropVector = Vector3.zero;
                        dropVector.x = currentBeat.islandPos.x;
                        ism.SpawnNewItem(it, eden.transform.position, eden.transform.position + dropVector);
                        droppedItem = GameObject.FindFirstObjectByType<LooseItemManager>();
                        if (it == ItemType.Fruit || it == ItemType.Seed)
                        {
                            droppedItem.looseItem.inv.items[0].plant = PlantType.Corn;
                            droppedItem.looseItem.inv.items[0].name = "Fruit (Corn)";
                            droppedItem.looseItem.inv.items[0].health = 1f;
                            if (it == ItemType.Fruit)
                            {
                                droppedItem.looseItem.inv.items[0].quality = 1f;
                                droppedItem.looseItem.inv.items[0].size = 1f;
                            }
                        }
                        if (droppedItem != null && it == ItemType.Fruit)
                            droppedItem = null; // gift to player
                    }
                    break;
                case ScriptedBeatAction.PlotChange:
                    // plot reference - type of change
                    // using data in islandPos
                    PlotCondition pc = PlotCondition.Dirt;
                    float water = 0f;
                    float soil = 0f;
                    float grow = 0f;
                    // wild to dirt
                    if (currentBeat.islandPos.w <= -3f)
                    {
                        pc = PlotCondition.Dirt;
                    }
                    // dirt to tilled
                    if (currentBeat.islandPos.w > -3f &&
                       currentBeat.islandPos.w <= -2f)
                    {
                        pc = PlotCondition.Tilled;
                        soil = 0.2f;
                    }
                    // watered
                    if (currentBeat.islandPos.w > -2f &&
                       currentBeat.islandPos.w <= -1f)
                    {
                        pc = PlotCondition.Tilled;
                        water = 1f;
                    }
                    // planted
                    if (currentBeat.islandPos.w > -1f &&
                       currentBeat.islandPos.w <= 0f)
                    {
                        pc = PlotCondition.Growing;
                        if (managedPlot.plant != null)
                            Destroy(managedPlot.plant);
                        managedPlot.plant = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                        managedPlot.plant.transform.parent = managedPlot.transform;
                        managedPlot.plant.transform.position = managedPlot.transform.position;
                        // just corn
                        managedPlot.data.plant = PlantSystem.InitializePlant(PlantType.Corn);
                        // fast grow
                        managedPlot.data.plant.health = 1f;
                        managedPlot.data.plant.growth = .1f;
                        managedPlot.data.plant.quality = .1f;
                        // set to growing
                        managedPlot.data.condition = PlotCondition.Growing;
                    }
                    // harvested
                    if (currentBeat.islandPos.w > 0f &&
                       currentBeat.islandPos.w <= 1f)
                    {
                        pc = PlotCondition.Growing;
                        managedPlot.data.plant.growth = 1f;
                        managedPlot.data.plant.isHarvested = true;
                    }
                    // uprooted
                    if (currentBeat.islandPos.w > 1f &&
                       currentBeat.islandPos.w <= 2f)
                    {
                        pc = PlotCondition.Uprooted;
                        if (managedPlot.plant != null)
                            Destroy(managedPlot.plant);
                    }
                    // uprooted to dirt
                    if (currentBeat.islandPos.w > 2f &&
                       currentBeat.islandPos.w <= 3f)
                    {
                        pc = PlotCondition.Dirt;
                        soil = 0.15f;
                    }
                    // magic grow
                    if (currentBeat.islandPos.w > 3f &&
                       currentBeat.islandPos.w <= 4f)
                    {
                        pc = PlotCondition.Growing;
                        if (managedPlot.data.plant.growth < 5f)
                            grow = 0.618f;
                        else
                            grow = 0.9f;
                    }
                    // use managed plot
                    managedPlot.data.condition = pc;
                    Renderer r = managedPlot.gameObject.transform.Find("Ground").gameObject.GetComponent<Renderer>();
                    switch (pc)
                    {
                        case PlotCondition.Dirt:
                            GameObject grass = null;
                            if (managedPlot.transform.childCount > 0 && managedPlot.gameObject.transform.Find("Plot Wild Grasses") != null)
                                grass = managedPlot.gameObject.transform.Find("Plot Wild Grasses").gameObject;
                            if (grass != null)
                                Destroy(grass);
                            if (r != null)
                                r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                            break;
                        case PlotCondition.Tilled:
                            if (r != null)
                                r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Tilled");
                            break;
                        case PlotCondition.Growing:
                            // grow baby grow
                            managedPlot.data.plant.vitality = 1f;
                            // if harvested show stalk
                            if (managedPlot.data.plant.isHarvested)
                            {
                                managedPlot.plant.transform.Find("Plant Image").GetComponent<Renderer>().material.mainTexture = (Texture2D)Resources.Load("ProtoPlant_Stalk");
                                managedPlot.data.plant.growth = 1f;
                                // FIXME: prevent grow image update
                                managedPlot.plant.GetComponent<PlantManager>().ForceGrowthImage(managedPlot.data.plant);
                                managedPlot.plant.GetComponent<PlantManager>().enabled = false;
                            }
                            break;
                        case PlotCondition.Uprooted:
                            if (r != null)
                                r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Uprooted");
                            break;
                    }
                    managedPlot.data.water = Mathf.Clamp01(managedPlot.data.water + water);
                    managedPlot.data.soil = Mathf.Clamp01(managedPlot.data.soil + soil);
                    managedPlot.data.plant.growth = Mathf.Clamp01(managedPlot.data.plant.growth + grow)-0.01f;
                    break;
                case ScriptedBeatAction.DeleteItem:
                    // item reference (not needed?)
                    if (droppedItem != null)
                        Destroy(droppedItem.gameObject, 0.1f);
                    else
                    {
                        // we have eden 'picking up' fertilizer than may not be dropped item
                        LooseItemManager[] allLoose = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
                        for (int i=0; i<allLoose.Length; i++)
                        {
                            if (allLoose[i].looseItem.inv.items[0].type == ItemType.Fertilizer)
                            {
                                droppedItem = allLoose[i];
                                break;
                            }
                        }
                        if (droppedItem != null)
                            Destroy(droppedItem.gameObject, 0.1f);
                    }
                    break;
                case ScriptedBeatAction.EndIntro:
                    introRunning = false;
                    TakeOverHUD(false);
                    Destroy(eden.gameObject, 10f);
                    break;
            }
            currentBeat.actionDone = true;
        }
        // end on no more beats
        if (currentBeatIndex >= introBeats.Length)
        {
            introRunning = false;
            TakeOverHUD(false);
            Destroy(eden.gameObject, 10f);
            return;
        }
        if (introBeats[currentBeatIndex].name != currentBeat.name)
        {
            // reset transition flags
            beatTimeUp = false;
            playerResponse = false;
            npcCallback = false;
            // set current beat (transition incremented index value)
            currentBeat = introBeats[currentBeatIndex];
            // display new beat name
            string s = "beat '" + introBeats[currentBeatIndex].name + "'";
            if (currentBeat.transition == ScriptedBeatTransition.TimedDuration)
            {
                // if timed duration transition, set timer
                beatTimer = currentBeat.duration;
                beatTimeUp = false;
            }
            else if (currentBeat.transition == ScriptedBeatTransition.EdenCallback)
            {
                // if npc callback transition, set mark
                if (eden.destinationReached && eden.gameObject.transform.position == currentBeat.npcMark)
                    s += " > DESTINATION ALREADY REACHED <";
                eden.moveTarget = currentBeat.npcMark;
                eden.destinationReached = false;
                npcCallback = false;
                s += " npc mark x:"+currentBeat.npcMark.x+" , z:"+currentBeat.npcMark.z;
            }
            Debug.Log(s);
        }
    }

    public void LaunchIntro()
    {
        // 
        // PLAYER INTRODUCTION
        //
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        camMgr = GameObject.FindAnyObjectByType<CameraManager>();
        // REVIEW: we are making a big assumption that only a first-run host can see the introduction
        // configure player character frozen, hidden, hud hidden
        if (pcm != null)
        {
            pcm.characterFrozen = true;
            pcm.freezeCharacterActions = true;
            pcm.hidePlayerHUD = true;
            pcm.playerData.island.x = 20.5f;
            pcm.playerData.island.z = -26.5f;
            pcm.playerData.island.w = .381f;
            GameObject playerObj = pcm.gameObject;
            playerObj.transform.GetChild(0).GetComponent<Renderer>().enabled = false;
            playerObj.transform.position = new Vector3(20.5f, 0, -26.5f);
            camMgr.SetWorldViewIntro();
            camMgr.allowPlayerControlCam = false;
        }

        // find managed plot
        FindManagedPlot();

        // alpha island out of frame
        GameObject islandA = GameObject.Find("Island Alpha");
        Vector3 iPos = Vector3.zero;
        iPos.x = 4f;
        iPos.y = -40f;
        iPos.z = -8f;
        islandA.transform.position = iPos;

        // configure beat script start
        TakeOverHUD(true);
        introRunning = true;
        currentBeatIndex = 0;
        currentBeat = introBeats[currentBeatIndex];
        beatTimer = currentBeat.duration;

        // eden arrives
        eden = SpawnEden(currentBeat.npcMark);
        eden.moveTarget = currentBeat.npcMark;
        eden.ghostMode = true;
        eden.mode = NPCController.NPCMode.Scripted;

        // set to morning
        TimeManager tim = GameObject.FindFirstObjectByType<TimeManager>();
        if (tim != null)
        {
            WorldData morning = tim.GetWorldData();
            morning.worldTimeOfDay = 0.381f;
            tim.SetWorldData(morning);
        }
    }

    void TakeOverHUD( bool claim )
    {
        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
        if (igc != null)
        {
            igc.enabled = !claim;
        }
        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
        if (iga != null)
        {
            iga.enabled = !claim;
        }
        if (pcm != null)
        {
            pcm.hidePlayerHUD = claim;
        }
        camMgr.allowPlayerControlCam = !claim;
        TeleporterControl(!claim, "");
    }

    NPCController SpawnEden( Vector3 pos )
    {
        GameObject eNPC = GameObject.Instantiate((GameObject)Resources.Load("NPC Eden"));
        eNPC.transform.position = pos;
        return eNPC.GetComponent<NPCController>();
    }

    void TryConfigPlayerInScene( PlayerOptions options )
    {
        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
            return;
        pcm.playerData.options = options;
        pcm.ConfigureAppearance(pcm.playerData.options);
    }

    void OnGUI()
    {
        if ((helpMessage == 0 && !introRunning) || (!canSkipIntro && !introPop && !dialogPop))
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.box);
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        if (helpMessage > 0 && helpMessageTimer > 0f)
        {
            // help message label
            r.x = 0.35f * w;
            r.y = 0.775f * h;
            r.width = 0.3f * w;
            r.height = 0.2f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            g.wordWrap = true;
            c = Color.white;
            s = "";
            switch ((IntroHelpMessage)helpMessage)
            {
                case IntroHelpMessage.MoveControls:
                    s = "Move your character\nwith W-A-S-D";
                    break;
                case IntroHelpMessage.ActionButton:
                    s = "Your action button\nis E";
                    break;
                case IntroHelpMessage.InventorySelect:
                    s = "Use [ and ] to select\nitems in your inventory";
                    break;
                case IntroHelpMessage.DropButton:
                    s = "Use C to drop your\ncurrently selected item";
                    break;
                case IntroHelpMessage.ControlsDisplay:
                    s = "All player controls are\ndisplayed by holding TAB";
                    break;
                case IntroHelpMessage.AlmanacDisplay:
                    s = "You may toggle the\nBiomancer's Almanac with \\";
                    break;
                default:
                    break;
            }
            GUI.color = Color.white;
            // drop shadow
            r.x += 0.0025f * w;
            r.y += 0.003f * h;
            c = Color.black;
            c.a = Mathf.Clamp01(helpMessageTimer / 1f);
            g.normal.textColor = c;
            g.hover.textColor = c;
            g.active.textColor = c;
            GUI.Label(r, s, g);
            r.x -= 0.0025f * w;
            r.y -= 0.003f * h;
            c = Color.white;
            c.a = Mathf.Clamp01(helpMessageTimer / 1f);
            g.normal.textColor = c;
            g.hover.textColor = c;
            g.active.textColor = c;
            GUI.Label(r, s, g);
        }

        if (canSkipIntro)
        {
            r.x = 0.875f * w;
            r.y = 0.05f * h;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.black;
            GUI.color = Color.white;
            s = "SKIP INTRO";
            if (GUI.Button(r, s, g))
            {
                cancelIntro = true;
                canSkipIntro = false;
            }
        }

        if (dialogPop)
        {
            // box
            r.x = 0.675f * w;
            r.y = 0.125f * h;
            r.width = 0.3f * w;
            r.height = 0.25f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.padding = new RectOffset(0, 0, 20, 0);
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            t = Texture2D.whiteTexture;
            c.r = 0.85f;
            c.g = 0.8f;
            c.b = 0.618f;
            c.a = 1f;
            g.normal.background = t;
            g.hover.background = t;
            g.active.background = t;
            GUI.color = c;
            s = "EDEN";
            GUI.Box(r, s, g);

            // dialog label
            r.x = 0.6875f * w;
            r.y = 0.14f * h;
            r.width = 0.2625f * w;
            r.height = 0.225f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            g.alignment = TextAnchor.MiddleLeft;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            g.wordWrap = true;
            s = currentBeat.dialogLine;
            GUI.color = Color.white;
            GUI.Label(r, s, g);
            // next button
            r.x = 0.86f * w;
            r.y = 0.3125f * h;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Normal;
            g.alignment = TextAnchor.MiddleCenter;
            g.normal.textColor = c;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            s = "NEXT";
            GUI.color = Color.white;
            if (GUI.Button(r, s, g))
            {
                // next dialog
                dialogPop = false;
                //introScriptStep++;
                playerResponse = true;
                if (currentBeatIndex >= introBeats.Length)
                {
                    introRunning = false;
                    TakeOverHUD(false);
                    return;
                }
                else if (currentBeatIndex == 2)
                {
                    introPop = true;
                    return;
                }
            }
        }

        if (!introPop)
            return;

        // INTRODUCTION POP UP
        r.x = 0.1f * w;
        r.y = 0.1f * h;
        r.width = 0.8f * w;
        r.height = 0.8f * h;

        g.fontSize = Mathf.RoundToInt(20f * ( w / 1024f ));
        g.fontStyle = FontStyle.Bold;
        g.padding = new RectOffset(0, 0, 30, 0);
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        c.r = 0.85f;
        c.g = 0.8f;
        c.b = 0.618f;
        c.a = 1f;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        GUI.color = c;
        s = "Player Character Customization";

        GUI.Box(r, s, g);

        // CHARACTER IMAGE BOX
        r.x = 0.15f * w;
        r.y = 0.2f * h;
        r.width = 0.325f * w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.box);
        s = "";
        GUI.color = Color.white;
        GUI.Box(r, s, g);
        // frame
        r.x += 0.0125f * w;
        r.y += 0.0125f * w;
        r.width -= 0.025f * w;
        r.height = r.width;
        t = Texture2D.whiteTexture;
        c = Color.white;
        c.r = 0.85f;
        c.g = 0.8f;
        c.b = 0.618f;
        c.a = 1f;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        GUI.color = c;
        GUI.Box(r, s, g);
        // bg
        r.x += 0.0125f * w;
        r.y += 0.0125f * w;
        r.width -= 0.025f * w;
        r.height = r.width;
        t = Texture2D.whiteTexture;
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = Color.white;
        GUI.color = c;
        GUI.Box(r, s, g);
        // main
        t = characterFills[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterColors[fillSelection];
        c.a = 1f;
        GUI.color = c;
        GUI.Box(r, s, g);
        // accent
        t = characterAccents[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterColors[accentSelection];
        GUI.color = c;
        GUI.Box(r, s, g);
        // skin
        t = characterSkins[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = characterSkinTones[skinSelection];
        GUI.color = c;
        GUI.Box(r, s, g);
        // line
        t = characterLines[modelSelection];
        g.normal.background = t;
        g.hover.background = t;
        g.active.background = t;
        c = Color.white;
        GUI.color = c;
        GUI.Box(r, s, g);

        // CHARACTER DETAILS
        // name label
        r.x = 0.5f * w;
        r.y = 0.3f * h;
        r.width = 0.375f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.BoldAndItalic;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.black;
        s = "New Player";
        if (pcm != null)
            s = pcm.playerData.playerName;
        GUI.color = Color.white;
        GUI.Label(r, s, g);
        // model label
        r.x = 0.55f * w;
        r.y += 0.075f * h;
        r.width = 0.3f * w;
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        s = "PLAYER MODEL";
        GUI.Label(r, s, g);
        // skin tone label
        r.y += 0.075f * h;
        s = "SKIN TONE";
        GUI.Label(r, s, g);
        // main color label
        r.y += 0.075f * h;
        s = "MAIN COLOR";
        GUI.Label(r, s, g);
        // accent color label
        r.y += 0.075f * h;
        s = "ACCENT COLOR";
        GUI.Label(r, s, g);

        // CONTROL BUTTONS
        r.x = 0.525f * w;
        r.y = 0.35f * h;
        r.width = 0.05f * w;
        r.height = r.width; // square
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(24f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.black;
        g.hover.textColor = Color.black;
        g.active.textColor = Color.yellow;
        GUI.color = Color.white;
        // model -/+
        s = "<";
        if (GUI.Button(r, s, g))
        {
            modelSelection--;
            if (modelSelection < 0)
                modelSelection = characterLines.Length-1;
            configsTouched[0] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            modelSelection++;
            if (modelSelection > characterLines.Length - 1)
                modelSelection = 0;
            configsTouched[0] = true;
        }
        // skin -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            skinSelection--;
            if (skinSelection < 0)
                skinSelection = characterSkinTones.Length - 1;
            configsTouched[1] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            skinSelection++;
            if (skinSelection > characterSkinTones.Length - 1)
                skinSelection = 0;
            configsTouched[1] = true;
        }
        // main -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            fillSelection--;
            if (fillSelection < 0)
                fillSelection = characterColors.Length - 1;
            configsTouched[2] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            fillSelection++;
            if (fillSelection > characterColors.Length - 1)
                fillSelection = 0;
            configsTouched[2] = true;
        }
        // accent -/+
        r.x = 0.525f * w;
        r.y += 0.075f * h;
        s = "<";
        if (GUI.Button(r, s, g))
        {
            accentSelection--;
            if (accentSelection < 0)
                accentSelection = characterColors.Length - 1;
            configsTouched[3] = true;
        }
        r.x += 0.3f * w;
        s = ">";
        if (GUI.Button(r, s, g))
        {
            accentSelection++;
            if (accentSelection > characterColors.Length - 1)
                accentSelection = 0;
            configsTouched[3] = true;
        }

        // detect config valid
        configValid = (configsTouched[0] && configsTouched[1] &&
            configsTouched[2] && configsTouched[3]);

        // accept button
        r.x = 0.4f * w;
        r.y = 0.8f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "ACCEPT";
        GUI.enabled = configValid;
        if (GUI.Button(r,s,g))
        {
            // store player options
            configOptions.model = (PlayerModelType)modelSelection + 1;
            configOptions.skinColor = (PlayerSkinColor)skinSelection;
            configOptions.mainColor = (PlayerColor)fillSelection;
            configOptions.accentColor = (PlayerColor)accentSelection;
            // end of player options configuration
            introPop = false;
            // confirm player options
            TryConfigPlayerInScene(configOptions);
            // reveal player character
            if (pcm != null)
            {
                pcm.characterFrozen = false;
                pcm.freezeCharacterActions = false;
                GameObject playerObj = pcm.gameObject;
                playerObj.transform.GetChild(0).GetComponent<Renderer>().enabled = true;
            }
            // reset intro timer
            introTimer = LONGPAUSETIME;
            // may skip remainder of introduction
            canSkipIntro = true;
            // indicaate player reponse has happened
            playerResponse = true;
        }
    }
}
