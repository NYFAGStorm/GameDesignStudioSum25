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

    public enum CraftState
    {
        Default,
        Grimoire,
        Cauldron,
        Exiting
    }

    public LibraryState libraryState;
    public CraftState craftState;

    public Texture2D grimoireBackground;
    public Texture2D cauldronBackground;

    private float libraryStateTimer;
    private float craftStateTimer;
    private float checkTimer;

    private bool craftingDisplay;

    private bool fadingOverlay;
    private bool fadingFromBlack;
    private Texture2D currentBackground;

    private PlayerControlManager pcm;
    private PlayerControlManager leaving; // used in deactivation
    private MagicManager mm; // REVIEW: need this at all here?

    const float LIBRARYSTATETIMERMAX = 1f;
    const float CRAFTSTATETIMERMAX = 1f;
    const float PLAYERCHECKTIME = 1f;
    const float PROXIMITYCHECKRADIUS = 0.381f;


    void Start()
    {
        // validate
        // TODO: validate for grimoire and cauldron background images
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

        RunCraftStateTimer();

        HandleCraftingStates();
    }

    bool DetectPlayer()
    {
        if (libraryState != LibraryState.Default && libraryState != LibraryState.Deactivating)
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
                        libraryState = LibraryState.Default;
                        libraryStateTimer = 0f;
                    }
                }
                else if (leaving != null && pcm == leaving)
                {
                    // remain in libraryState until leaving player not detected
                    pcm = null;
                    mm = null;
                    checkTimer = PLAYERCHECKTIME;
                }
                else
                {
                    // engage with player, activate library
                    pcm.characterFrozen = true;
                    pcm.hidePlayerHUD = true;
                    libraryState = LibraryState.Activating;
                    libraryStateTimer = LIBRARYSTATETIMERMAX;
                }
            }
        }

        return (pcm != null);
    }

    void HandleLibraryStates()
    {
        if (libraryStateTimer == 0f)
            return;

        // run libraryState timer
        if (libraryStateTimer > 0f)
        {
            libraryStateTimer -= Time.deltaTime;
            if (libraryStateTimer < 0f)
            {
                libraryStateTimer = 0f;
                switch (libraryState)
                {
                    case LibraryState.Default:
                        // should never be here
                        break;
                    case LibraryState.Activating:
                        // REVIEW: may do special stuff here, using libraryState timer
                        libraryState = LibraryState.Active;
                        craftingDisplay = true; // sets false in craft state handling
                        craftState = CraftState.Grimoire;
                        craftStateTimer = CRAFTSTATETIMERMAX;
                        fadingOverlay = true;
                        break;
                    case LibraryState.Active:
                        pcm.characterFrozen = false;
                        pcm.hidePlayerHUD = false;
                        libraryState = LibraryState.Deactivating;
                        libraryStateTimer = LIBRARYSTATETIMERMAX;
                        break;
                    case LibraryState.Deactivating:
                        if (pcm != null)
                        {
                            leaving = pcm;
                            pcm = null;
                            mm = null;
                        }
                        // remain in libraryState until leaving player not detected
                        checkTimer = PLAYERCHECKTIME;
                        libraryStateTimer = LIBRARYSTATETIMERMAX;
                        break;
                    default:
                        Debug.LogWarning("--- MagicCraftingManager [HandleLibraryStates] : library libraryState undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void RunCraftStateTimer()
    {
        if (craftStateTimer > 0f)
        {
            craftStateTimer -= Time.deltaTime;
            if (craftStateTimer < (CRAFTSTATETIMERMAX / 2f))
            {
                // configure craft background images between overlay fades
                switch (craftState)
                {
                    case CraftState.Default:
                        break;
                    case CraftState.Grimoire:
                        if (!fadingFromBlack)
                        {
                            if (currentBackground == null && grimoireBackground != null)
                                currentBackground = grimoireBackground;
                            if (currentBackground == null)
                                currentBackground = Texture2D.grayTexture; // TEMP
                        }
                        break;
                    case CraftState.Cauldron:
                        if (!fadingFromBlack)
                        {
                            if (currentBackground == null && cauldronBackground != null)
                                currentBackground = cauldronBackground;
                            if (currentBackground == null)
                                currentBackground = Texture2D.whiteTexture; // TEMP
                        }
                        break;
                    case CraftState.Exiting:
                        if (!fadingFromBlack)
                            currentBackground = null;
                        break;
                }
                fadingFromBlack = true;
            }
            if (craftStateTimer < 0f)
            {
                craftStateTimer = 0f;
                fadingFromBlack = false;
                // handle craft state changes
                switch (craftState)
                {
                    case CraftState.Default:
                        // we should never be here
                        break;
                    case CraftState.Grimoire:
                        craftState = CraftState.Cauldron;
                        fadingOverlay = false;
                        break;
                    case CraftState.Cauldron:
                        craftState = CraftState.Exiting;
                        fadingOverlay = false;
                        break;
                    case CraftState.Exiting:
                        libraryStateTimer = (LIBRARYSTATETIMERMAX/2f); // exit faster
                        craftState = CraftState.Default;
                        craftingDisplay = false;
                        fadingOverlay = false;
                        break;
                    default:
                        Debug.LogWarning("--- MagicCraftingManager [RunCraftStateTimer] : craft state undefined. will ignore.");
                        break;
                }

            }
        }

    }

    void HandleCraftingStates()
    {
        switch (craftState)
        {
            case CraftState.Default:
                // we should never be here
                break;
            case CraftState.Grimoire:
                break;
            case CraftState.Cauldron:
                break;
            case CraftState.Exiting:
                break;
        }
    }

    void OnGUI()
    {
        if (!craftingDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;

        r.x = 0f;
        r.y = 0f;
        r.width = w;
        r.height = h;

        // crafting background image appears halfway through overlay fading
        if (currentBackground != null)
        {
            t = currentBackground;
            c = Color.white;
            GUI.color = c;
            GUI.DrawTexture(r, t);
        }

        // handle fading to and from black for craft state transitions
        if (fadingOverlay)
        {
            c = Color.black;
            if (fadingFromBlack)
                c.a = ((craftStateTimer * 2f)/CRAFTSTATETIMERMAX);
            else
                c.a = 1f-(((craftStateTimer * 2f) / CRAFTSTATETIMERMAX)-1f);
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // if fading overlay, no other display
            return;
        }



        r.x = 0.1f * w;
        r.y = 0.1f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;

        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(20 * (w/1024f));

        string s = "words";

        c = Color.white;
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
            craftState = CraftState.Exiting;
            craftStateTimer = CRAFTSTATETIMERMAX;
            fadingOverlay = true;
        }
    }
}
