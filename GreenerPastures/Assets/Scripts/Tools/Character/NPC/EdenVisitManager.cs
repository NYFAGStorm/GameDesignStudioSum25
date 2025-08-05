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
        ItemSpawn,
        DeleteItem,
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

    public bool visitRunning;
    public bool dialogPop;

    private int currentBeatIndex;
    private float beatTimer;
    public bool beatTimeUp;
    public bool npcCallback;
    public bool playerResponse;
    private int beatScriptEndIndex;

    private Texture2D[] buttonTex;

    private MultiGamepad padMgr;

    private AudioManager sfxAudio;

    const float PAUSETIME = 2f;
    const float PROXIMITY = 1.5f;


    void Start()
    {
        // validate
        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
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

        float dist = Vector3.Distance(pcm.gameObject.transform.position, eden.transform.position);
        retBool = (dist <= PROXIMITY);

        return retBool;
    }

    bool FacingDirectionToPlayer()
    {
        bool retBool = false;

        // face left?
        retBool = (pcm.gameObject.transform.position.x < eden.transform.position.x);

        return retBool;
    }

    void ConfigureVisitBeats()
    {
        visitBeats = new ScriptedBeat[33];
        int beat = 0;
        visitBeats[beat].name = "visit launch - npc spawn";
        visitBeats[beat].action = ScriptedBeatAction.NPCSpawn;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].beatPosition.x = 20f;
        visitBeats[beat].beatPosition.y = 0f;
        visitBeats[beat].beatPosition.z = -20f;
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
        visitBeats[beat].npcMark = new Vector3(18f, 0f, -19f);
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
        visitBeats[beat].name = "eden to player farm position";
        visitBeats[beat].action = ScriptedBeatAction.NPCMarkToPlayerFarm;
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "eden wait for player";
        visitBeats[beat].action = ScriptedBeatAction.NPCWaitForPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
        beat++;
        visitBeats[beat].name = "eden turn to player";
        visitBeats[beat].action = ScriptedBeatAction.NPCTurnToPlayer;
        visitBeats[beat].transition = ScriptedBeatTransition.EdenCallback;
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
            "Have you seen the Arcana shrine is in the market?";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "'the genesis tree grants'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "The Genesis Tree grants special abilities to advanced Biomancers.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "'visit the shrine'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "Visit the shrine and use your Arcana to receive the Genesis Tree blessings.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
        beat++;
        visitBeats[beat].name = "'we're all so glad'";
        visitBeats[beat].action = ScriptedBeatAction.Dialog;
        visitBeats[beat].dialogLine =
            "We're all so glad you're a part of our community! Be well and take care.";
        visitBeats[beat].transition = ScriptedBeatTransition.PlayerResponse;
        beat++;
        visitBeats[beat].name = "brief pause";
        visitBeats[beat].action = ScriptedBeatAction.Default;
        visitBeats[beat].transition = ScriptedBeatTransition.TimedDuration;
        visitBeats[beat].duration = 0.5f;
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
        visitBeats[beat].npcMark = new Vector3(18f, 0f, -19f);
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
        
    }
}
