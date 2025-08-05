using UnityEngine;

public class EdenVisitManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles Eden visit routines

    public NPCController eden;

    public enum ScriptedBeatAction
    {
        Default,
        Dialog,
        EdenMark,
        TeleportEden,
        VFXSpawn,
        ItemSpawn,
        DeleteItem,
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


    void Start()
    {
        // validate
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

    void ConfigureVisitBeats()
    {

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

    void Update()
    {
        
    }
}
