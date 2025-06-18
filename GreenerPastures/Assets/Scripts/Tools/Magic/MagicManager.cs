using UnityEngine;

public class MagicManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles a player's use of their spell book

    // casting routine includes:
    // suspend movement and actions from player
    // enable display player controls for selection / casting routine
    // allow player to cancel casting (D button down)
    //      (return movement and action control,
    //      disable display of player controls for casting routine,
    //      remove cast cursor if exists,
    //      present exiting cast mode)
    // present animation and effects of entering select mode
    // display spell book charge list, highlight current selection
    // allow player to change current selection among list
    // allow player to select the spell charge to cast, sends to casting mode
    // spawn cast cursor with AOE circle, based on spell data
    // allow player to navigate cursor (Up-Down-Left-Right)
    // present cursor as invalid if invalid location
    // allow player to perform casting action (A button down),
    //      if valid location
    // disallow player to cancel casting
    // present animation and effects of casting action
    // remove cast cursor
    // send cast data to cast manager via CastSpell()
    // disable display of player controls for selection / casting routine
    // present animation and effects of exiting cast mode
    //      (return movement and action control)

    public enum SpellCastMode
    {
        Default,
        Entering,
        Selecting,
        Casting,
        Exiting
    }

    public SpellCastMode mode;

    private float modeChangeTimer;
    private bool selectionDisplay;
    private bool castInstructionsDisplay;
    private bool playerCanCancel;
    private int selectedSpellCharge;
    private GameObject castingCursor;
    private float castCursorSpeed = 3.81f;
    private CastData cData;

    private PlayerControlManager pcm;
    private MultiGamepad padMgr;
    private CastManager castMgr;

    const float CASTMODECHANGETIME = 1f;


    void Start()
    {
        // validate
        pcm = gameObject.GetComponent<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- MagicManager [Start] : no player control manager found on this game object. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            // temp
            Debug.LogWarning("--- MagicManager [Start] : no gamepad manager found in scene. will ignore.");
        }
        castMgr = GameObject.FindFirstObjectByType<CastManager>();
        if (castMgr == null)
        {
            Debug.LogError("--- MagicManager [Start] : no cast manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            
        }
    }

    void Update()
    {
        // run cast mode change timer
        if ( modeChangeTimer > 0f )
        {
            modeChangeTimer -= Time.deltaTime;
            if ( modeChangeTimer < 0f )
            {
                modeChangeTimer = 0f;
                // handle change complete
                switch ( mode )
                {
                    case SpellCastMode.Default:
                        // we should never be here
                        break;
                    case SpellCastMode.Entering:
                        mode = SpellCastMode.Selecting;
                        selectionDisplay = true;
                        playerCanCancel = true;
                        break;
                    case SpellCastMode.Selecting:
                        mode = SpellCastMode.Casting;
                        castInstructionsDisplay = true;
                        SpawnCastCursor();
                        break;
                    case SpellCastMode.Casting:
                        mode = SpellCastMode.Exiting;
                        modeChangeTimer = CASTMODECHANGETIME;
                        ExitCastingPresentation();
                        break;
                    case SpellCastMode.Exiting:
                        mode = SpellCastMode.Default;
                        PlayerControlsAllowed(true);
                        break;
                    default:
                        break;
                }
            }
        }

        HandleCancelCast();

        if (mode == SpellCastMode.Selecting)
        {
            UpdateSelection();
            return;
        }

        if (mode != SpellCastMode.Casting)
            return;

        UpdateCastCursor();
            
        HandleCastAction();
    }

    /// <summary>
    /// Casts a spell charge into the world given spell type and world position
    /// </summary>
    /// <param name="spell">spell type</param>
    /// <param name="pos">position in the game world to center the cast</param>
    /// <returns>true if successful, false if no charge or no spell book entry or invalid spell book entry</returns>
    public bool CastSpell( SpellType spell, Vector3 pos )
    {
        bool retBool = false;

        if (MagicSystem.SpellBookHasCharge(spell, pcm.playerData.magic.library))
        {
            if (MagicSystem.CastSpellFromBook(spell, pcm.playerData.magic.library, out pcm.playerData.magic.library))
            {
                SpellBookData spellData = MagicSystem.GetSpellBookEntry(spell, pcm.playerData.magic.library);
                if (spellData != null)
                {
                    castMgr.AcquireNewCast(MagicSystem.InitializeCast(spellData, pos));
                    retBool = true;
                }
                else
                    Debug.LogWarning("--- MagicManager [CastSpell] : unable to get spell book entry data for spell type "+spell.ToString()+". will ignore.");
            }
        }       

        return retBool;
    }

    /// <summary>
    /// Begins player spell cast mode, if spell charges exist in player's spell book
    /// </summary>
    /// <returns>true if successfully entered casting routine, false if no charges</returns>
    public bool EnterSpellCastMode()
    {
        bool retBool = false;

        // validate has at least one spell charge in spell book
        for ( int i = 0; i < pcm.playerData.magic.library.spellBook.Length; i++ )
        {
            if (pcm.playerData.magic.library.spellBook[i].chargesAvailable > 0)
            {
                retBool = true;
                break;
            }
        }
        if (!retBool)
            return retBool;

        mode = SpellCastMode.Entering;
        modeChangeTimer = CASTMODECHANGETIME;
        PlayerControlsAllowed(false);
        EnterCastingPresentation();

        return retBool;
    }

    public void ExitSpellCastMode()
    {
        // will reach exiting from casting after timer
        modeChangeTimer = CASTMODECHANGETIME;
    }

    void PlayerControlsAllowed( bool allowed )
    {
        pcm.characterFrozen = !allowed;
        pcm.freezeCharacterActions = !allowed;
        pcm.hidePlayerHUD = !allowed;
    }

    void CancelCasting()
    {
        playerCanCancel = false;
        mode = SpellCastMode.Exiting;
        modeChangeTimer = CASTMODECHANGETIME;
        if (castingCursor != null)
            Destroy(castingCursor);
        castInstructionsDisplay = false;
        ExitCastingPresentation();
    }

    void EnterCastingPresentation()
    {
        // TODO: 
    }

    void CastingPresentation()
    {
        // TODO: 
    }

    void ExitCastingPresentation()
    {
        // TODO: 
    }

    void SpawnCastCursor()
    {
        castingCursor = GameObject.Instantiate<GameObject>((GameObject)Resources.Load("Cast Cursor"));
        if (castingCursor == null)
        {
            Debug.LogWarning("--- MagicManager [SpawnCastCursor] : no prefab for cast cursor found in resources folder. will ignore.");
            return;
        }
        castingCursor.transform.position = gameObject.transform.position;
        castingCursor.transform.position += (0.001f * Vector3.up);
        castingCursor.transform.parent = null;
        castingCursor.name = "Casting Cursor";
        castingCursor.GetComponent<Renderer>().material.color = Color.blue;
        // AOE circle scale (child object)
        Vector3 lScale = Vector3.one;
        lScale.x = Mathf.Max(1f, cData.rangeAOE * 2f);
        lScale.y = Mathf.Max(1f, cData.rangeAOE * 2f);
        castingCursor.transform.GetChild(0).transform.localScale = lScale;
    }

    bool IsValidCastLocation()
    {
        // REVIEW: 
        return true;
    }

    void UpdateSelection()
    {
        int min = 0;
        int max = pcm.playerData.magic.library.spellBook.Length-1;

        // up-down controls
        if (Input.GetKeyDown(pcm.upKey) || (padMgr != null &&
            padMgr.gamepads[0].isActive && padMgr.gPadDown[0].YaxisL > 0f))
        {
            selectedSpellCharge--;
            if (selectedSpellCharge < 0)
                selectedSpellCharge = max;
        }
        if (Input.GetKeyDown(pcm.downKey) || (padMgr != null &&
            padMgr.gamepads[0].isActive && padMgr.gPadDown[0].YaxisL < 0f))
        {
            selectedSpellCharge++;
            if (selectedSpellCharge > max)
                selectedSpellCharge = 0;
        }
        selectedSpellCharge = Mathf.Clamp(selectedSpellCharge, min, max);

        // selection control
        if ( pcm.playerData.magic.library.spellBook[selectedSpellCharge].chargesAvailable > 0 )
        {
            if (Input.GetKeyDown(pcm.actionAKey) || (padMgr != null &&
                padMgr.gamepads[0].isActive && padMgr.gPadDown[0].aButton))
            {
                // transition to casting mode
                selectionDisplay = false;
                modeChangeTimer = CASTMODECHANGETIME / 2f; // faster transition to cast
            }
        }
    }

    void UpdateCastCursor()
    {
        if (castingCursor == null)
            return;

        // handle cursor movement
        Vector3 pos = castingCursor.transform.position;
        Vector3 move = Vector3.zero;

        // Up-Down-Left-Right, keyboard, gamepad
        if (Input.GetKey(pcm.upKey) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gamepads[0].YaxisL > 0f))
            move += Vector3.forward;
        if (Input.GetKey(pcm.downKey) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gamepads[0].YaxisL < 0f))
            move += Vector3.back;
        if (Input.GetKey(pcm.leftKey) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gamepads[0].XaxisL < 0f))
            move += Vector3.left;
        if (Input.GetKey(pcm.rightKey) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gamepads[0].XaxisL > 0f))
            move += Vector3.right;

        pos += move * castCursorSpeed * Time.deltaTime;
        castingCursor.transform.position = pos;

        // update appearance based on location validity
        Color c = Color.blue;
        if (!IsValidCastLocation())
        {
            c = Color.red;
            c.a = (Time.time * 5f) % 1f;
        }
        castingCursor.GetComponent<Renderer>().material.color = c;
    }

    void HandleCancelCast()
    {
        if (!playerCanCancel)
            return;
        CancelCasting();
    }

    void HandleCastAction()
    {
        if (Input.GetKeyDown(pcm.actionAKey) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gPadDown[0].aButton))
        {
            if (IsValidCastLocation())
                PerformCast();
        }
    }

    void PerformCast()
    {
        playerCanCancel = false;
        CastingPresentation();
        modeChangeTimer = CASTMODECHANGETIME;
        if (castingCursor != null)
            Destroy(castingCursor);
        CastSpell(cData.type, castingCursor.transform.position);
        castInstructionsDisplay = false;
    }

    void OnGUI()
    {
        if (!selectionDisplay && !castInstructionsDisplay)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.fontSize = Mathf.RoundToInt(20f * (w/1024f));

        string s = "SPELL BOOK";
        if (mode == SpellCastMode.Casting)
            s = "CASTING SPELL '"+pcm.playerData.magic.library.spellBook[selectedSpellCharge].name+"'";

        Color c = Color.white;

        r.x = 0.25f * w;
        r.y = 0.1f * h;
        r.width = 0.5f * w;
        r.height = 0.3f * h;

        c.r *= 0.5f;
        c.g *= 0.5f;
        c.a *= 0.381f;

        GUI.color = c;
        GUI.Box(r, s, g);

        // cancel button control
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "CANCEL CAST";
        c = Color.white;
        GUI.color = c;
        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
            s = "CANCEL CAST\n[BACK BUTTON]";
        }
        GUI.enabled = (playerCanCancel);
        if (GUI.Button(r,s,g) || (padMgr != null && 
            padMgr.gamepads[0].isActive && padMgr.gPadDown[0].backButton))
        {
            CancelCasting();
        }
        GUI.enabled = true;

        // select or cast target control
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(18f * (w / 1024f));
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        g.active.textColor = Color.white;

        if (mode == SpellCastMode.Selecting)
        {
            // individual spell charges
            r.x = 0.3f * w;
            r.y = 0.125f * h;
            r.width = 0.4f * w;
            r.height = 0.05f * h;

            c = Color.white;
            GUI.color = c;
            s = "Up - Down = Select, A Button to Target";

            GUI.Label(r, s, g);
            r.y += 0.05f;

            for ( int i = 0; i < pcm.playerData.magic.library.spellBook.Length; i++ )
            {
                c = Color.white;
                if (pcm.playerData.magic.library.spellBook[i].chargesAvailable == 0)
                    c = Color.black;
                else if (i == selectedSpellCharge)
                    c = Color.yellow;
                GUI.color = c;

                s = "[" + pcm.playerData.magic.library.spellBook[i].chargesAvailable + "] " + pcm.playerData.magic.library.spellBook[i].name;
                
                GUI.Label(r, s, g);
                r.y += 0.05f;
            }
        }

        if (mode == SpellCastMode.Casting)
        {
            // casting instructions
            r.x = 0.3f * w;
            r.y = 0.125f * h;
            r.width = 0.4f * w;
            r.height = 0.25f * h;

            c = Color.white;
            GUI.color = c;

            s = "Place Casting Target\nW - S = Forward - Back\nA - D = Left - Right\nA Button = To Cast";

            GUI.Label(r, s, g);
        }
    }
}
