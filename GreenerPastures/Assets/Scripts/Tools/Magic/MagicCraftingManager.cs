using UnityEngine;

public class MagicCraftingManager : MonoBehaviour
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
    const float PROXIMITYCHECKRADIUS = 0.381f;


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
            return true; // we must already have player engaged, skip

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
                        mm = pcs[i].gameObject.GetComponent<MagicManager>();
                        if (mm == null)
                            Debug.LogWarning("--- MagicCraftingManager [DetectPlayer] : acquired player has no magic manager component. will ignore, will cause errors.");
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
                    }
                }
                else if (leaving != null && pcm == leaving)
                {
                    // remain in state until leaving player not detected
                    pcm = null;
                    mm = null;
                    checkTimer = PLAYERCHECKTIME;
                }
                else
                {
                    // engage with player, activate library
                    pcm.characterFrozen = true;
                    pcm.hidePlayerHUD = true;
                    state = LibraryState.Activating;
                    stateTimer = STATETIMERMAX;
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
                        break;
                    case LibraryState.Active:
                        pcm.characterFrozen = false;
                        pcm.hidePlayerHUD = false;
                        state = LibraryState.Deactivating;
                        stateTimer = STATETIMERMAX;
                        craftingDisplay = false;
                        break;
                    case LibraryState.Deactivating:
                        if (pcm != null)
                        {
                            leaving = pcm;
                            pcm = null;
                            mm = null;
                        }
                        // remain in state until leaving player not detected
                        checkTimer = PLAYERCHECKTIME;
                        stateTimer = STATETIMERMAX;
                        break;
                    default:
                        Debug.LogWarning("--- MagicCraftingManager [HandleLibraryStates] : library state undefined. will ignore.");
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
        g.fontSize = Mathf.RoundToInt(20 * (w/1024f));

        Color c = Color.white;

        Texture2D t = Texture2D.whiteTexture;

        string s = "words";

        GUI.color = c;

        GUI.Label(r, s, g);


        // cancel / exit crafting button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;

        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(18 * (w/1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;

        s = "EXIT CRAFTING";

        if (GUI.Button(r, s, g))
        {
            stateTimer = STATETIMERMAX;
        }
    }
}
