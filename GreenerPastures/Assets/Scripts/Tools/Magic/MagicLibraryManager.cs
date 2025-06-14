using UnityEngine;

public class MagicLibraryManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a player's use of their grimoire, including the crafting interface

    public enum LibraryState
    {
        Default,        // ready to be approached
        Activating,     // beginning to change interface
        Active,         // in crafting mode
        Deactivating,   // end crafting interface, detect player left to reset
    }

    public LibraryState state;
    private float stateTimer;
    private float checkTimer;

    private bool craftingDisplay;

    private PlayerControlManager pcm;
    private PlayerControlManager leaving; // used in deactivation
    private MagicManager mm;

    const float STATETIMERMAX = 1f;
    const float PLAYERCHECKTIME = 1f;
    const float PROXIMITYCHECKRADIUS = 0.1f;


    void Start()
    {
        // validate
        // initialize
        if (enabled)
        {
            checkTimer = PLAYERCHECKTIME;
        }
    }

    void Update()
    {
        if (!DetectPlayer())
            return;

        HandleLibraryStates();
    }

    bool DetectPlayer()
    {
        if (state != LibraryState.Default && state != LibraryState.Deactivating)
            return true; // we must already have player engaged

        // if no player, run player check timer 
        if (pcm == null && checkTimer > 0f)
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer < 0f)
            {
                checkTimer = 0f;
                // detect player in proximity
                PlayerControlManager[] pcs = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                // REVIEW: for multiplayer, should really find closest player here
                for (int i = 0; i < pcs.Length; i++)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcs[i].gameObject.transform.position);
                    if (dist < PROXIMITYCHECKRADIUS)
                    {
                        pcm = pcs[i];
                        break;
                    }
                }
                // if no player, reset check timer
                if (pcm == null)
                {
                    checkTimer = PLAYERCHECKTIME;
                    if ( leaving != null )
                    {
                        // engaged player has now left, reset
                        leaving = null;
                        state = LibraryState.Default;
                        stateTimer = 0f;
                        print("player left, library reset");
                    }
                }
                else if (leaving != null && pcm == leaving)
                {
                    // remain in state until leaving player not detected
                    pcm = null;
                    checkTimer = PLAYERCHECKTIME;
                    print("... waiting for player to leave ...");
                }
                else
                {
                    // engage with player, activate library
                    pcm.characterFrozen = true;
                    pcm.hidePlayerHUD = true;
                    state = LibraryState.Activating;
                    stateTimer = STATETIMERMAX;
                    print("engaging with player");
                }
            }
        }

        return (pcm != null);
    }

    void HandleLibraryStates()
    {
        if (stateTimer == 0f)
            return;

        // run state timer
        if (stateTimer > 0f)
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer < 0f)
            {
                stateTimer = 0f;
                switch (state)
                {
                    case LibraryState.Default:
                        // should never be here
                        break;
                    case LibraryState.Activating:
                        // REVIEW: may do special stuff here, using state timer
                        state = LibraryState.Active;
                        craftingDisplay = true;
                        print("library crafting interface active -");
                        break;
                    case LibraryState.Active:
                        state = LibraryState.Deactivating;
                        stateTimer = STATETIMERMAX;
                        craftingDisplay = false;
                        print("- library crafting interface inactive");
                        break;
                    case LibraryState.Deactivating:
                        if (pcm != null)
                        {
                            leaving = pcm;
                            pcm = null;
                        }                       
                        // remain in state until leaving player not detected
                        stateTimer = STATETIMERMAX;
                        break;
                    default:
                        Debug.LogWarning("--- MagicLibraryManager [HandleLibraryStates] : library state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void OnGUI()
    {
        if (!craftingDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.1f * w;
        r.y = 0.1f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;

        GUIStyle g = new GUIStyle(GUI.skin.label);

        Color c = Color.white;

        Texture2D t = Texture2D.whiteTexture;

        string s = "words";

        GUI.color = c;

        GUI.Label(r, s, g);
    }
}
