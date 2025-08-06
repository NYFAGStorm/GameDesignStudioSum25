using UnityEngine;

public class EdenVisitManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles Eden visit routines

    public NPCController eden;

    public enum ScriptedBeatAction
    {
        Default,
        NPCSpawn,
        Dialog,
        EdenMark,
        NPCMarkToPlayerFarm,
        NPCWaitForPlayer,
        NPCTurnToPlayer,
        TeleportEden,
        VFXSpawn,
        RemoveNPC,
        EndVisit
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
    private int beatScriptEndIndex;

    private bool waitingForPlayer;
    private bool facingPlayer;

    private Texture2D[] buttonTex;

    private MultiGamepad padMgr;

    private AudioManager sfxAudio;

    const float PAUSETIME = 2f;
    const float PROXIMITY = 1.5f;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- EdenVisitManager [Start] : no player control manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- EdenVisitManager [Start] : no gamepad manager found in scene. will ignore.");
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
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
                Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has the same name as previous. this will cause errors at runtime.");
            visitBeats[i].name = "[" + i + "] " + visitBeats[i].name;
            if (visitBeats[i].transition == ScriptedBeatTransition.TimedDuration &&
                visitBeats[i].duration == 0f)
                Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has no duration, but is set to be timed. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.EdenMark)
            {
                if (visitBeats[i].npcMark == Vector3.zero)
                    Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has an npc mark configured, but mark is zero. this will cause errors at runtime.");
                else if (Vector3.Distance(prevMark, visitBeats[i].npcMark) <= .25f)
                {
                    Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has an npc mark configured, but mark too close to npc position (" + Vector3.Distance(prevMark, visitBeats[i].npcMark) + "). this will cause errors at runtime. will adjust this mark further away.");
                    Vector3 newMarkPos = visitBeats[i].npcMark;
                    newMarkPos += (visitBeats[i].npcMark - prevMark);
                    visitBeats[i].npcMark = newMarkPos;
                }
            }
            if (visitBeats[i].action == ScriptedBeatAction.Dialog && visitBeats[i].dialogLine == "")
                Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has dialog configured, but no dialog line is found. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.Dialog &&
                visitBeats[i].transition != ScriptedBeatTransition.PlayerResponse &&
                visitBeats[i].transition != ScriptedBeatTransition.TimedDuration)
                Debug.LogWarning("--- EdenVisitManager [ValidateVisitBeats] : visit beat " + i + " has dialog configured, but transition from this beat is '" + visitBeats[i].transition.ToString() + "'. this will cause errors at runtime.");
            if (visitBeats[i].action == ScriptedBeatAction.EndVisit)
                beatScriptEndIndex = i;

            prevName = visitBeats[i].name;
            if (visitBeats[i].npcMark != Vector3.zero)
                prevMark = visitBeats[i].npcMark;
        }
        if (beatScriptEndIndex == 0)
            Debug.LogWarning("--- EdenVisitManager [ValidatevisitBeats] : no final beat with 'end visit' configured. this will cause errors at runtime.");
    }

    Vector3 FindPlayerIsland()
    {
        Vector3 retVector = Vector3.zero;

        IslandManager iMgr = GameObject.FindFirstObjectByType<IslandManager>();
        if (iMgr == null)
        {
            Debug.LogError("--- EdenVisitManager [FindPlayerFarm] : no island manager found in scene. aborting.");
            return retVector;
        }
        retVector = GameSystem.GetVector(iMgr.islands[pcm.playerData.playerIsland].location);

        return retVector;
    }

    Vector3 FindPlayerFarm()
    {
        Vector3 retVector = Vector3.zero;

        if (pcm == null)
            Debug.LogWarning("--- EdenVisitManager [FindPlayerFarm] : no pcm. will ignore.");

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

        if (pcm == null)
            Debug.LogWarning("--- EdenVisitManager [IsPlayerClose] : no pcm. will ignore.");

        float dist = Vector3.Distance(pcm.gameObject.transform.position, eden.transform.position);
        retBool = (dist <= PROXIMITY);

        return retBool;
    }

    bool FacingDirectionToPlayer()
    {
        bool retBool = false;

        if (pcm == null)
            Debug.LogWarning("--- EdenVisitManager [FacingDirectionToPlayer] : no pcm. will ignore.");

        // face left?
        retBool = (pcm.gameObject.transform.position.x < eden.transform.position.x);

        return retBool;
    }

    void ConfigureVisitBeats()
    {
        visitBeats = new ScriptedBeat[33];
        int beat = 0;
        visitBeats[beat].name = "visit launch";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].npcMark = new Vector3(20f,0f,-20f);
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        beat++;
        visitBeats[beat].name = "move eden out market";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(20f, 0f, -22f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "move eden from market";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(18f, 0f, -21f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "eden to teleporter";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(16f, 0f, -16f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
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
        visitBeats[beat].name = "eden teleport";
        visitBeats[beat].action = ScriptedBeatAction.TeleportEden;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        visitBeats[beat].beatPosition.x = 4f;
        visitBeats[beat].beatPosition.z = -4f;
        beat++;
        visitBeats[beat].name = "eden off teleporter";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(4.5f, 0f, -3.25f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "eden toward farm";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(3f, 0f, -1f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "eden to player farm position";
        visitBeats[beat].action = ScriptedBeatAction.NPCMarkToPlayerFarm;
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "eden wait for player";
        visitBeats[beat].action = ScriptedBeatAction.NPCWaitForPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "eden turn to player";
        visitBeats[beat].action = ScriptedBeatAction.NPCTurnToPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "'you've been busy!'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Oh my! You've been busy! So much progress!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'have you seen?'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Have you seen the Arcana shrine in the market?";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'the genesis tree grants'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "The Genesis Tree grants special abilities to advanced Biomancers.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'visit the shrine'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Visit the shrine and use your Arcana to receive the Genesis Tree blessings.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'we're all so glad'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "We're all so glad you're a part of our community!";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'be well'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Be well and take care.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "eden turn to player";
        visitBeats[beat].action = ScriptedBeatAction.NPCTurnToPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        beat++;
        visitBeats[beat].name = "eden to teleporter";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(4f, 0f, -4f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "teleporter vfx (a)";
        visitBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        visitBeats[beat].beatPosition.x = 4f;
        visitBeats[beat].beatPosition.z = -4f;
        beat++;
        visitBeats[beat].name = "teleporter vfx (b)";
        visitBeats[beat].action = ScriptedBeatAction.VFXSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.Default;
        visitBeats[beat].beatPosition.x = 16f;
        visitBeats[beat].beatPosition.z = -16f;
        beat++;
        visitBeats[beat].name = "eden teleport";
        visitBeats[beat].action = ScriptedBeatAction.TeleportEden;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        visitBeats[beat].beatPosition.x = 16f;
        visitBeats[beat].beatPosition.z = -16f;
        beat++;
        visitBeats[beat].name = "eden off teleporter";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(17f, 0f, -16.5f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "move eden towards market";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(18f, 0f, -21f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "move eden to market";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(20f, 0f, -22f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "move eden into market";
        visitBeats[beat].action = ScriptedBeatAction.EdenMark;
        visitBeats[beat].npcMark = new Vector3(20f, 0f, -20f);
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "remove eden";
        visitBeats[beat].action = ScriptedBeatAction.RemoveNPC;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
        beat++;
        visitBeats[beat].name = "end visit";
        visitBeats[beat].action = ScriptedBeatAction.EndVisit;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 1f;
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

        // turn to face player
        if (eden != null && facingPlayer)
        {
            bool facingLeft = (eden.GetComponentInChildren<CharacterAnimManager>().GetImageFlipped());
            Vector3 newMove = eden.moveTarget;
            // should face left?
            if (FacingDirectionToPlayer())
            {
                if (!facingLeft)
                {
                    newMove += Vector3.left * 0.1f;
                    eden.moveTarget = newMove;
                }
            }
            else
            {
                if (facingLeft)
                {
                    newMove += Vector3.right * 0.1f;
                    eden.moveTarget = newMove;
                }
            }
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
        if (eden != null)
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
                case ScriptedBeatAction.NPCSpawn:
                    // eden arrives
                    eden = SpawnEden(GameSystem.GetVector(currentBeat.beatPosition));
                    eden.moveTarget = GameSystem.GetVector(currentBeat.beatPosition);
                    eden.ghostMode = true;
                    eden.mode = NPCController.NPCMode.Scripted;
                    break;
                case ScriptedBeatAction.Dialog:
                    dialogTimer = PAUSETIME;
                    playerResponse = false;
                    break;
                case ScriptedBeatAction.EdenMark:
                    eden.moveTarget = currentBeat.npcMark;
                    eden.destinationReached = false;
                    break;
                case ScriptedBeatAction.NPCMarkToPlayerFarm:
                    eden.moveTarget = FindPlayerFarm();
                    eden.destinationReached = false;
                    break;
                case ScriptedBeatAction.NPCWaitForPlayer:
                    waitingForPlayer = true;
                    break;
                case ScriptedBeatAction.NPCTurnToPlayer:
                    facingPlayer = !facingPlayer;
                    break;
                case ScriptedBeatAction.TeleportEden:
                    Vector3 pos = eden.gameObject.transform.position;
                    pos.x = currentBeat.beatPosition.x;
                    pos.z = currentBeat.beatPosition.z;
                    eden.gameObject.transform.position = pos;
                    eden.moveTarget = pos;
                    eden.destinationReached = false;
                    break;
                case ScriptedBeatAction.VFXSpawn:
                    Vector3 vfxPos = Vector3.zero;
                    // either teleport or magic
                    GameObject vfx = null;
                    if (currentBeat.beatPosition.w >= 0f)
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
                    RemoveEden();
                    break;
                case ScriptedBeatAction.EndVisit:
                    visitRunning = false;
                    break;
            }
            currentBeat.actionDone = true;
        }

        // end on no more beats
        if (currentBeatIndex >= visitBeats.Length)
        {
            visitRunning = false;
            if (eden != null)
                Destroy(eden.gameObject, 10f);
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
            else if (currentBeat.transition == ScriptedBeatTransition.EdenCallback)
            {
                // if npc callback transition, set mark
                if (eden.destinationReached && eden.gameObject.transform.position == currentBeat.npcMark)
                    s += " > DESTINATION ALREADY REACHED <";
                eden.moveTarget = currentBeat.npcMark;
                eden.destinationReached = false;
                npcCallback = false;
                s += " npc mark x:" + currentBeat.npcMark.x + " , z:" + currentBeat.npcMark.z;
            }
            //Debug.Log(s);
        }
    }

    public void LaunchVisit()
    {
        countdownToVisit = 1f;
    }

    void BeginVisit()
    {
        visitRunning = true;
        currentBeatIndex = 0;
        currentBeat = visitBeats[currentBeatIndex];
        beatTimer = currentBeat.duration;

        // eden arrives
        eden = SpawnEden(currentBeat.npcMark);
        eden.moveTarget = currentBeat.npcMark;
        eden.ghostMode = true;
        eden.mode = NPCController.NPCMode.Scripted;
    }

    void RemoveEden()
    {
        if (eden != null)
            Destroy(eden.gameObject);
    }

    NPCController SpawnEden(Vector3 pos)
    {
        GameObject eNPC = GameObject.Instantiate((GameObject)Resources.Load("NPC Eden"));
        eNPC.transform.position = pos;
        return eNPC.GetComponent<NPCController>();
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
