using UnityEngine;

public class SalesVisitManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the traveling salesman visit routine

    public NPCController salesman;

    public enum ScriptedBeatAction
    {
        Default,
        NPCSpawn,
        Dialog,
        SalesmanMark,
        NPCMarkToPlayerFarm,
        NPCWaitForPlayer,
        NPCTurnToPlayer,
        TeleportSalesman,
        VFXSpawn,
        RemoveNPC,
        EndVisit,
        IslandMenu,
        MenuDialog,
        MenuMark,
        MenuVFX
    }

    public enum ScriptedBeatTransition
    {
        Default,
        TimedDuration,
        PlayerResponse,
        SalesmanCallback,
        MenuClose
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
        public PositionData beatPosition;
    }
    public ScriptedBeat[] visitBeats;
    public ScriptedBeat currentBeat; // for use in debugging via inspector only

    private PlayerControlManager pcm;

    public float countdownToVisit = 3f;

    public bool visitRunning;
    public float dialogTimer;
    public bool dialogPop;

    private int currentBeatIndex;
    private float beatTimer;
    public bool beatTimeUp;
    public bool npcCallback;
    public bool playerResponse;
    public bool menuClose;
    private int beatScriptEndIndex;

    private bool waitingForPlayer;
    private bool facingPlayer;

    public bool menuBeatTimeUp;
    public bool menuNpcCallback;
    public bool menuPlayerResponse;

    private bool salesmanColorPlatform; // tether player to npc and display salesman color trail vfx

    private Texture2D[] buttonTex;

    private MultiGamepad padMgr;

    private AudioManager sfxAudio;

    const float PAUSETIME = 1.5f;
    const float PROXIMITY = 1.5f;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- SalesVisitManager [Start] : no player control manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- SalesVisitManager [Start] : no gamepad manager found in scene. will ignore.");
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        if (enabled)
        {
            ConfigureVisitBeats();
            ValidateVisitBeats();

            if (!Application.isEditor)
            {
                // GUI Button Textures for build
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }
        }
    }

    void ValidateVisitBeats()
    {
        string prevName = "";
        Vector3 prevMark = Vector3.zero;
        for (int i = 0; i < visitBeats.Length; i++)
        {
            if (prevName == visitBeats[i].name)
                Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has the same name as previous. this will cause errors at runtime.");
            visitBeats[i].name = "[" + i + "] " + visitBeats[i].name;
            if (visitBeats[i].transition == ScriptedBeatTransition.TimedDuration &&
                visitBeats[i].duration == 0f)
                Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has no duration, but is set to be timed. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.SalesmanMark)
            {
                if (visitBeats[i].npcMark == Vector3.zero)
                    Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has an npc mark configured, but mark is zero. this will cause errors at runtime.");
                else if (Vector3.Distance(prevMark, visitBeats[i].npcMark) <= .25f)
                {
                    Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has an npc mark configured, but mark too close to npc position (" + Vector3.Distance(prevMark, visitBeats[i].npcMark) + "). this will cause errors at runtime. will adjust this mark further away.");
                    Vector3 newMarkPos = visitBeats[i].npcMark;
                    newMarkPos += (visitBeats[i].npcMark - prevMark);
                    visitBeats[i].npcMark = newMarkPos;
                }
            }
            if (visitBeats[i].action == ScriptedBeatAction.Dialog && visitBeats[i].dialogLine == "")
                Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has dialog configured, but no dialog line is found. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.Dialog &&
                visitBeats[i].transition != ScriptedBeatTransition.PlayerResponse &&
                visitBeats[i].transition != ScriptedBeatTransition.TimedDuration)
                Debug.LogWarning("--- SalesVisitManager [ValidateVisitBeats] : visit beat " + i + " has dialog configured, but transition from this beat is '" + visitBeats[i].transition.ToString() + "'. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.EndVisit)
                beatScriptEndIndex = i;

            prevName = visitBeats[i].name;
            if (visitBeats[i].npcMark != Vector3.zero)
                prevMark = visitBeats[i].npcMark;
        }
        if (beatScriptEndIndex == 0)
            Debug.LogWarning("--- SalesVisitManager [ValidatevisitBeats] : no final beat with 'end visit' configured. this will cause errors at runtime.");

    }

    Vector3 FindPlayerIsland()
    {
        Vector3 retVector = Vector3.zero;

        IslandManager iMgr = GameObject.FindFirstObjectByType<IslandManager>();
        if (iMgr == null)
        {
            Debug.LogError("--- SalesVisitManager [FindPlayerFarm] : no island manager found in scene. aborting.");
            return retVector;
        }
        retVector = GameSystem.GetVector(iMgr.islands[pcm.playerData.playerIsland].location);

        return retVector;
    }

    Vector3 FindPlayerFarm()
    {
        Vector3 retVector = Vector3.zero;

        for (int i = 0; i < pcm.playerData.farm.plots.Length; i++)
        {
            retVector += GameSystem.GetVector(pcm.playerData.farm.plots[i].location);
        }
        retVector /= pcm.playerData.farm.plots.Length;

        return retVector;
    }

    bool IsPlayerClose()
    {
        bool retBool = false;

        float dist = Vector3.Distance(pcm.gameObject.transform.position, salesman.transform.position);
        retBool = (dist <= PROXIMITY);

        return retBool;
    }

    Vector3 FacingDirectionToPlayer()
    {
        Vector3 retVector = Vector3.zero;

        if (pcm == null)
            Debug.LogWarning("--- SalesVisitManager [FacingDirectionToPlayer] : no pcm. will ignore.");

        retVector = (pcm.gameObject.transform.position - salesman.transform.position);
        retVector.Normalize();

        bool facingLeft = (salesman.GetComponentInChildren<CharacterAnimManager>().GetImageFlipped());
        if (facingLeft && pcm.gameObject.transform.position.x > salesman.transform.position.x)
            retVector += Vector3.right * 0.381f;
        else if (!facingLeft && pcm.gameObject.transform.position.x < salesman.transform.position.x)
            retVector += Vector3.left * 0.381f;

        return retVector;
    }

    void ConfigureVisitBeats()
    {
        visitBeats = new ScriptedBeat[30];
        int beat = 0;
        visitBeats[beat].name = "visit launch";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].npcMark = new Vector3(20f, 0f, -20f);
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        beat++;
        visitBeats[beat].name = "move salesman out market";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(20f, 0f, -22f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "move salesman from market";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(17f, 0f, -19.5f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "salesman to teleporter";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(16f, 0f, -16f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "teleporter vfx (a)";
        visitBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        visitBeats[beat].beatPosition.x = 16f;
        visitBeats[beat].beatPosition.z = -16f;
        beat++;
        visitBeats[beat].name = "teleporter vfx (b)";
        visitBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        visitBeats[beat].beatPosition.x = 4f;
        visitBeats[beat].beatPosition.z = -4f;
        beat++;
        visitBeats[beat].name = "salesman teleport";
        visitBeats[beat].action = ScriptedBeatAction.TeleportSalesman;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        visitBeats[beat].beatPosition.x = 4f;
        visitBeats[beat].beatPosition.z = -4f;
        beat++;
        visitBeats[beat].name = "salesman off teleporter";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(4.5f, 0f, -3.25f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "salesman toward farm";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(2.5f, 0f, -2.75f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "salesman wait for player";
        visitBeats[beat].action = ScriptedBeatAction.NPCWaitForPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "salesman turn to player";
        visitBeats[beat].action = ScriptedBeatAction.NPCTurnToPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "'hey ho, Biomancer friend!'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Hey ho, Biomancer friend! I've got island upgrades for you!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'Let's do business'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Let's do business! I can review your current island, move things you buy...";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'and of course sell'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "... and of course, I can sell you the finest in island upgrades!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'take a look!'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Just take a look at what I can do for you!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        // island upgrades menu
        beat++;
        visitBeats[beat].name = "island upgrade menu";
        visitBeats[beat].action = ScriptedBeatAction.IslandMenu;
        visitBeats[beat].transition = ScriptedBeatTransition.MenuClose;
        // back from menu
        beat++;
        visitBeats[beat].name = "'I think your island looks great'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "I think your island looks great! I do enjoy our visits each month.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'I must be off now'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "I must be off now! Many places to go. Fare well, Biomancer friend!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "salesman turn to player";
        visitBeats[beat].action = ScriptedBeatAction.NPCTurnToPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "salesman toward teleporter";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(3.5f, 0f, -3f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "salesman onto teleporter";
        visitBeats[beat].action = ScriptedBeatAction.SalesmanMark;
        visitBeats[beat].npcMark = new Vector3(4f, 0f, -4f);
        visitBeats[beat].transition = ScriptedBeatTransition.SalesmanCallback;
        beat++;
        visitBeats[beat].name = "teleporter vfx (b)";
        visitBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        visitBeats[beat].beatPosition.x = 4f;
        visitBeats[beat].beatPosition.z = -4f;
        beat++;
        visitBeats[beat].name = "remove salesman";
        visitBeats[beat].action = ScriptedBeatAction.RemoveNPC;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "end visit";
        visitBeats[beat].action = ScriptedBeatAction.EndVisit;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 2f;
    }

    public void ToggleSalesmanPlatform()
    {
        salesmanColorPlatform = !salesmanColorPlatform;
        pcm.characterFrozen = !salesmanColorPlatform;
    }

    void TetherPlayerToSalesman()
    {
        pcm.playerData.island.x = salesman.gameObject.transform.position.x;
        pcm.playerData.island.z = salesman.gameObject.transform.position.z;
        pcm.playerData.island.w = 0.618f;
    }

    void DisplaySalesmanPlatformVFX()
    {
        if (RandomSystem.FlatRandom01() >= .381f)
            return;

        GameObject lightingFolderObject = GameObject.Find("Lighting");
        Color c = Color.white;
        c.r = 0.9f * RandomSystem.GaussianRandom01();
        c.g = 0.8f * RandomSystem.GaussianRandom01();
        c.b = 0.618f * RandomSystem.GaussianRandom01();
        GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
        vfx.transform.position = salesman.transform.position;
        Vector3 offset = Vector3.zero;
        offset.x = RandomSystem.GaussianRandom01() - 0.5f;
        offset.z = RandomSystem.GaussianRandom01() - 0.5f;
        offset *= 0.618f;
        vfx.transform.position += offset;
        Vector3 scl = Vector3.one;
        scl *= RandomSystem.GaussianRandom01() * 2f;
        vfx.transform.localScale = scl;
        vfx.name = "VFX Salesman Platform";
        vfx.transform.parent = lightingFolderObject.transform;
        vfx.GetComponent<Renderer>().material.color = c;
        Destroy(vfx, 3.81f);
    }

    void Update()
    {
        // countdown to visit
        if (countdownToVisit > 0f)
        {
            countdownToVisit -= Time.deltaTime;
            if (countdownToVisit < 0f)
            {
                countdownToVisit = 0f;
                BeginVisit();
            }
            else
                return;
        }

        // waiting for player
        if (waitingForPlayer)
        {
            if (IsPlayerClose())
                waitingForPlayer = false;
            else
                return;
        }

        // run dialog timer
        if (dialogTimer > 0f)
        {
            dialogTimer -= Time.deltaTime;
            if (dialogTimer < 0f)
            {
                dialogTimer = 0f;
                dialogPop = true;
            }
        }

        if (!visitRunning)
            return;

        // salesman platform
        if (salesmanColorPlatform)
        {
            DisplaySalesmanPlatformVFX();
            TetherPlayerToSalesman();
        }

        // turn to face player
        if (salesman != null && facingPlayer)
        {
            salesman.SetCharacterAnimMoveVector(FacingDirectionToPlayer());
        }

        // run beat timer
        if (beatTimer > 0f)
        {
            beatTimer -= Time.deltaTime;
            if (beatTimer < 0f)
            {
                beatTimer = 0f;
                beatTimeUp = true;
            }
        }

        // detect npc destination reached ('callback')
        if (salesman != null)
            npcCallback = salesman.destinationReached;

        // handle beat script transition
        switch (currentBeat.transition)
        {
            case ScriptedBeatTransition.Default:
                if (currentBeat.action == ScriptedBeatAction.MenuVFX)
                {
                    currentBeat.beatPosition.w = 0f;
                    currentBeat.transition = ScriptedBeatTransition.MenuClose;
                }
                else
                {
                    currentBeatIndex++; // immediate transition
                    currentBeat.transitionDone = true;
                }
                break;
            case ScriptedBeatTransition.TimedDuration:
                if (beatTimeUp)
                {
                    if (currentBeat.action == ScriptedBeatAction.MenuVFX)
                    {
                        currentBeat.beatPosition.w = 0f;
                        currentBeat.duration = 0f;
                        currentBeat.transition = ScriptedBeatTransition.MenuClose;
                        beatTimeUp = false;
                        menuBeatTimeUp = true;
                    }
                    else
                    {
                        currentBeatIndex++;
                        beatTimeUp = false;
                        currentBeat.transitionDone = true;
                    }
                }
                break;
            case ScriptedBeatTransition.PlayerResponse:
                if (playerResponse)
                {
                    if (currentBeat.action == ScriptedBeatAction.MenuDialog)
                    {
                        currentBeat.dialogLine = "";
                        currentBeat.transition = ScriptedBeatTransition.MenuClose;
                        playerResponse = false;
                        menuPlayerResponse = true;
                    }
                    else
                    {
                        currentBeatIndex++;
                        playerResponse = false;
                        currentBeat.transitionDone = true;
                    }
                }
                break;
            case ScriptedBeatTransition.SalesmanCallback:
                if (npcCallback)
                {
                    if (currentBeat.action == ScriptedBeatAction.MenuMark)
                    {
                        npcCallback = false;
                        currentBeat.transition = ScriptedBeatTransition.MenuClose;
                        salesman.destinationReached = false;
                        menuNpcCallback = true;
                    }
                    else
                    {
                        currentBeatIndex++;
                        npcCallback = false;
                        salesman.destinationReached = false;
                        currentBeat.transitionDone = true;
                    }
                }
                break;
            case ScriptedBeatTransition.MenuClose:
                if (menuClose)
                {
                    currentBeatIndex++;
                    menuClose = false;
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
                case ScriptedBeatAction.NPCSpawn:
                    // salesman arrives
                    salesman = SpawnSalesman(GameSystem.GetVector(currentBeat.beatPosition));
                    salesman.moveTarget = GameSystem.GetVector(currentBeat.beatPosition);
                    salesman.ghostMode = true;
                    salesman.mode = NPCController.NPCMode.Scripted;
                    break;
                case ScriptedBeatAction.Dialog:
                    dialogTimer = PAUSETIME;
                    playerResponse = false;
                    break;
                case ScriptedBeatAction.SalesmanMark:
                    salesman.moveTarget = currentBeat.npcMark;
                    salesman.destinationReached = false;
                    break;
                case ScriptedBeatAction.NPCMarkToPlayerFarm:
                    salesman.moveTarget = FindPlayerFarm();
                    salesman.destinationReached = false;
                    break;
                case ScriptedBeatAction.NPCWaitForPlayer:
                    waitingForPlayer = true;
                    break;
                case ScriptedBeatAction.NPCTurnToPlayer:
                    facingPlayer = !facingPlayer;
                    break;
                case ScriptedBeatAction.TeleportSalesman:
                    Vector3 pos = salesman.gameObject.transform.position;
                    pos.x = currentBeat.beatPosition.x;
                    pos.z = currentBeat.beatPosition.z;
                    salesman.gameObject.transform.position = pos;
                    salesman.moveTarget = pos;
                    salesman.destinationReached = false;
                    break;
                case ScriptedBeatAction.VFXSpawn:
                    Vector3 vfxPos = Vector3.zero;
                    // either teleport or magic
                    GameObject vfx = null;
                    if (currentBeat.beatPosition.w >= 0f &&
                        currentBeat.beatPosition.w < 1f)
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
                    else if (currentBeat.beatPosition.w < 0f &&
                        currentBeat.beatPosition.w >= -1f)
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Cast Magic"));
                    else
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("Big Hint"));
                    vfxPos.x = currentBeat.beatPosition.x;
                    vfxPos.z = currentBeat.beatPosition.z;
                    vfx.transform.position = vfxPos;
                    if (vfx.GetComponentInChildren<SpriteRenderer>() != null)
                        vfx.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                    GameObject sfxTemp = new GameObject();
                    sfxTemp.name = "Teleport SFX Obj";
                    sfxTemp.transform.position = vfxPos;
                    if (currentBeat.beatPosition.w >= 0f)
                    {
                        // teleporter
                        sfxAudio.StartSound("Teleport", sfxTemp, 0f, 6.18f);
                        Destroy(sfxTemp, 2.2f);
                        Destroy(vfx, 1f);
                    }
                    else if (currentBeat.beatPosition.w < 0f &&
                            currentBeat.beatPosition.w >= -1f)
                    {
                        // magic
                        sfxAudio.StartSound("Magic Cast 2", vfx, 0f, 6.18f);
                        Destroy(vfx, 3.81f);
                    }
                    else
                        Destroy(vfx, 6.18f);
                    break;
                case ScriptedBeatAction.RemoveNPC:
                    RemoveSalesman();
                    break;
                case ScriptedBeatAction.EndVisit:
                    Destroy(gameObject, 3f);
                    visitRunning = false;
                    break;
                case ScriptedBeatAction.IslandMenu:
                    menuClose = false;
                    // ISLAND MENU OPEN
                    GameObject islandMenu = GameObject.Instantiate((GameObject)Resources.Load("Island Upgrade Menu"));
                    IslandUpgradeMenu im = islandMenu.GetComponent<IslandUpgradeMenu>();
                    if (im != null)
                        im.ConfigSalesVisit(this);
                    else
                        Debug.LogWarning("--- SalesVisitManager [Update] : unable to access island upgrade manager script for config. will ignore.");
                    break;
                case ScriptedBeatAction.MenuDialog:
                    dialogTimer = PAUSETIME;
                    playerResponse = false;
                    break;
                case ScriptedBeatAction.MenuMark:
                    salesman.moveTarget = currentBeat.npcMark;
                    salesman.destinationReached = false;
                    break;
                case ScriptedBeatAction.MenuVFX:
                    vfxPos = Vector3.zero;
                    // either teleport or magic
                    vfx = null;
                    if (currentBeat.beatPosition.w >= 0f &&
                        currentBeat.beatPosition.w < 1f)
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Tport Flash"));
                    else if (currentBeat.beatPosition.w < 0f &&
                        currentBeat.beatPosition.w >= -1f)
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("VFX Cast Magic"));
                    else
                        vfx = GameObject.Instantiate((GameObject)Resources.Load("Big Hint"));
                    vfxPos.x = currentBeat.beatPosition.x;
                    vfxPos.z = currentBeat.beatPosition.z;
                    vfx.transform.position = vfxPos;
                    if (vfx.GetComponentInChildren<SpriteRenderer>() != null)
                        vfx.GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
                    sfxTemp = new GameObject();
                    sfxTemp.name = "Teleport SFX Obj";
                    sfxTemp.transform.position = vfxPos;
                    if (currentBeat.beatPosition.w >= 0f)
                    {
                        // teleporter
                        sfxAudio.StartSound("Teleport", sfxTemp, 0f, 6.18f);
                        Destroy(sfxTemp, 2.2f);
                        Destroy(vfx, 1f);
                    }
                    else if (currentBeat.beatPosition.w < 0f &&
                            currentBeat.beatPosition.w >= -1f)
                    {
                        // magic
                        sfxAudio.StartSound("Magic Cast 2", vfx, 0f, 6.18f);
                        Destroy(vfx, 3.81f);
                    }
                    else
                        Destroy(vfx, 6.18f);
                    break;
            }
            currentBeat.actionDone = true;
        }

        // end on no more beats
        if (currentBeatIndex >= visitBeats.Length)
        {
            visitRunning = false;
            if (salesman != null)
                Destroy(salesman.gameObject, 10f);
            return;
        }

        // end of beat
        if (visitBeats[currentBeatIndex].name != currentBeat.name)
        {
            // reset transition flags
            beatTimeUp = false;
            playerResponse = false;
            npcCallback = false;
            // set current beat (transition incremented index value)
            currentBeat = visitBeats[currentBeatIndex];
            // display new beat name
            string s = "beat '" + visitBeats[currentBeatIndex].name + "'";
            if (currentBeat.transition == ScriptedBeatTransition.TimedDuration)
            {
                // if timed duration transition, set timer
                beatTimer = currentBeat.duration;
                beatTimeUp = false;
            }
            else if (currentBeat.transition == ScriptedBeatTransition.SalesmanCallback)
            {
                // if npc callback transition, set mark
                if (salesman.destinationReached && salesman.gameObject.transform.position == currentBeat.npcMark)
                    s += " > DESTINATION ALREADY REACHED <";
                salesman.moveTarget = currentBeat.npcMark;
                salesman.destinationReached = false;
                npcCallback = false;
                s += " npc mark x:" + currentBeat.npcMark.x + " , z:" + currentBeat.npcMark.z;
            }
            //Debug.Log(s);
        }
    }

    public void MenuDialogBeat(string dialog)
    {
        currentBeat.actionDone = false;
        currentBeat.action = ScriptedBeatAction.MenuDialog;
        currentBeat.transition = ScriptedBeatTransition.PlayerResponse;
        currentBeat.dialogLine = dialog;
    }

    public void MenuMarkBeat( Vector3 npcTarget )
    {
        currentBeat.actionDone = false;
        currentBeat.action = ScriptedBeatAction.MenuMark;
        currentBeat.transition = ScriptedBeatTransition.SalesmanCallback;
        currentBeat.npcMark = npcTarget;
    }

    public void MenuVFXBeat( float vfx, float time )
    {
        currentBeat.actionDone = false;
        currentBeat.action = ScriptedBeatAction.MenuVFX;
        currentBeat.transition = ScriptedBeatTransition.Default;
        currentBeat.beatPosition.x = salesman.gameObject.transform.position.x;
        currentBeat.beatPosition.z = salesman.gameObject.transform.position.z;
        if (time > 0f)
        {
            currentBeat.transition = ScriptedBeatTransition.TimedDuration;
            currentBeat.duration = time;
        }
        currentBeat.beatPosition.w = vfx;
    }

    public void IslandMenuClosed()
    {
        currentBeat.transition = ScriptedBeatTransition.MenuClose;
        menuClose = true;
    }

    void BeginVisit()
    {
        visitRunning = true;
        currentBeatIndex = 0;
        currentBeat = visitBeats[currentBeatIndex];
        beatTimer = currentBeat.duration;

        // salesman arrives
        salesman = SpawnSalesman(currentBeat.npcMark);
        salesman.moveTarget = currentBeat.npcMark;
        salesman.ghostMode = true;
        salesman.mode = NPCController.NPCMode.Scripted;
    }

    void RemoveSalesman()
    {
        if (salesman != null)
            Destroy(salesman.gameObject);
    }

    NPCController SpawnSalesman(Vector3 pos)
    {
        GameObject sNPC = GameObject.Instantiate((GameObject)Resources.Load("NPC Salesman"));
        sNPC.transform.position = pos;
        return sNPC.GetComponent<NPCController>();
    }

    void OnGUI()
    {
        if (!visitRunning || !dialogPop)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;
        GUIStyle g = new GUIStyle(GUI.skin.box);
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        if (dialogPop)
        {
            GUI.depth = -99;

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
            s = "SALESMAN";
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
            // ok button
            r.x = 0.86f * w;
            r.y = 0.3125f * h;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.button);
            if (padMgr != null && padMgr.gamepads[0].isActive)
                g.fontSize = Mathf.RoundToInt(12f * (w / 1024f));
            else
                g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
            g.fontStyle = FontStyle.Normal;
            g.alignment = TextAnchor.MiddleCenter;
            if (!Application.isEditor)
                c *= 0.618f;
            g.normal.textColor = c;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.yellow;
            if (!Application.isEditor)
            {
                g.normal.background = buttonTex[0];
                g.hover.background = buttonTex[1];
                g.active.background = buttonTex[2];
            }
            s = "OK";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                s += "\n[B BUTTON]";
            GUI.color = Color.white;
            if (GUI.Button(r, s, g) ||
                (padMgr != null && padMgr.gamepads[0].isActive && padMgr.gPadDown[0].bButton))
            {
                // next dialog
                dialogPop = false;
                playerResponse = true;
                if (currentBeatIndex >= visitBeats.Length)
                {
                    visitRunning = false;
                    return;
                }
                // consume, but why?
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    padMgr.gPadDown[0].bButton = false;
            }
        }
    }
}
