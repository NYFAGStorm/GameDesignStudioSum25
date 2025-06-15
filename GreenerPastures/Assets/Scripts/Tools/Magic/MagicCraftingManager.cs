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
        Default,        // ready to enter crafting
        Grimoire,       // gazing at the list of recipes in the grimoire
        Cauldron,       // fixing to mix up a selected spell (craft a charge)
        Exiting         // leaving crafting
    }

    public struct ItemPiece
    {
        public ItemType type;
        public Vector3 pos;
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

    private int currentGrimoireEntry;
    private int selectedGrimoireRecipe;

    private int sizeOfCauldronGrid;
    private InventoryData cauldronInventory; // TODO: hold ingredients for crafting
    private ItemType heldIngredient; // ingredient item currently dragging
    private Vector3 heldPosition; // viewport position of current ingredient
    private ItemPiece[] placedIngredients; // to draw placed ingredient pieces on grid
    private bool[] cauldronGridFilled; // 2 dimensional array (row, col) if spaces taken
    private bool craftingSolved; // has the player solved the crafting puzzle?

    private PlayerControlManager pcm;
    private PlayerControlManager leaving; // used in deactivation
    private MultiGamepad padMgr;
    private ArtLibraryManager alm;

    const float LIBRARYSTATETIMERMAX = 1f;
    const float CRAFTSTATETIMERMAX = 1f;
    const float PLAYERCHECKTIME = 1f;
    const float PROXIMITYCHECKRADIUS = 0.381f;


    void Start()
    {
        // validate
        // TODO: validate for grimoire and cauldron background images
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if ( padMgr == null )
        {
            Debug.LogWarning("--- MagicCraftingManager [Start] : no gamepad manager found in scene. will ignore.");
        }
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- MagicCraftingManager [Start] : no art library manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            checkTimer = PLAYERCHECKTIME;
            currentGrimoireEntry = -1;
            selectedGrimoireRecipe = -1;
            if (selectedGrimoireRecipe == 0)
                print("a silly use of a variable not implemented yet");

            placedIngredients = new ItemPiece[0];
            // TODO: set this based on player level
            sizeOfCauldronGrid = 4; // testing
            cauldronGridFilled = new bool[sizeOfCauldronGrid * sizeOfCauldronGrid];
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
                                currentBackground = Texture2D.whiteTexture; // TEMP
                        }
                        break;
                    case CraftState.Cauldron:
                        if (!fadingFromBlack)
                        {
                            if (cauldronBackground != null)
                                currentBackground = cauldronBackground;
                            if (currentBackground != cauldronBackground)
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
                        fadingOverlay = false;
                        break;
                    case CraftState.Cauldron:
                        fadingOverlay = false;
                        break;
                    case CraftState.Exiting:
                        libraryStateTimer = (LIBRARYSTATETIMERMAX/2f); // exit faster
                        craftState = CraftState.Default;
                        craftingDisplay = false;
                        fadingOverlay = false;
                        currentGrimoireEntry = -1;
                        selectedGrimoireRecipe = -1;
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
                if ( pcm.playerData.magic.library.grimiore.Length > 0 )
                {
                    // allow player to change current recipe entry from grimoire listing
                    if (Input.GetKeyDown(pcm.upKey) || (padMgr != null && padMgr.gPadDown[0].YaxisL > 0f))
                        currentGrimoireEntry--;
                    if (Input.GetKeyDown(pcm.downKey) || (padMgr != null && padMgr.gPadDown[0].YaxisL < 0f))
                        currentGrimoireEntry++;
                    currentGrimoireEntry = Mathf.Clamp(currentGrimoireEntry, 0, pcm.playerData.magic.library.grimiore.Length - 1);
                }
                if (currentGrimoireEntry != -1)
                {
                    // TODO: validate at least one of all ingredients in inventory
                    // allow player to make selection of recipe to craft in cauldron state
                    if (Input.GetKeyDown(pcm.actionAKey) || (padMgr != null && padMgr.gPadDown[0].aButton))
                    {
                        selectedGrimoireRecipe = currentGrimoireEntry;
                        // transfer all matching ingredients to cauldron inventory
                        // if ( InventorySystem.StoreItem() )
                        // ...
                        // NOTE: if we exit, remaining cauldron inventory must transfer back to player
                    }
                }
                break;
            case CraftState.Cauldron:
                break;
            case CraftState.Exiting:
                break;
        }
    }

    void AddPlacedPiece( ItemType type, Vector3 pos )
    {
        ItemPiece[] tmp = new ItemPiece[placedIngredients.Length + 1];

        for (int i = 0; i < placedIngredients.Length; i++)
        {
            tmp[i] = placedIngredients[i];
        }
        tmp[placedIngredients.Length] = new ItemPiece();
        tmp[placedIngredients.Length].type = type;
        tmp[placedIngredients.Length].pos = pos;

        placedIngredients = tmp;
    }

    bool isAmongPlacedPieces( ItemType type )
    {
        bool retBool = false;

        for (int i = 0; i < placedIngredients.Length; i++)
        {
            if (placedIngredients[i].type == type)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    void ClearPlacedPieces()
    {
        placedIngredients = new ItemPiece[0];
    }

    void ConvertViewportSpaceToGrid( Vector3 viewport, out int row, out int col )
    {
        int retRow = 0;
        int retCol = 0;

        // based on sizeOfCauldronGrid and 0.075f * w per square grid space
        // starting from center of grid at 0.7f * w, 0.45f * h
        // TODO: ...

        row = retRow;
        col = retCol;
    }

    void SetGridSpaceFilled( Vector3 viewport )
    {
        int outRow = 0;
        int outCol = 0;
        ConvertViewportSpaceToGrid(viewport, out outRow, out outCol);
        SetGridSpaceFilled(outRow, outCol);
    }

    void SetGridSpaceFilled( int row, int col )
    {
        int resultIndex = row + (col * sizeOfCauldronGrid);
        cauldronGridFilled[resultIndex] = true;
    }

    bool IsGridSpaceOpen( Vector3 viewport )
    {
        bool retBool = false;

        int outRow = 0;
        int outCol = 0;
        ConvertViewportSpaceToGrid(viewport, out outRow, out outCol);
        IsGridSpaceOpen(outRow, outCol);
        
        return retBool;
    }

    bool IsGridSpaceOpen( int row, int col )
    {
        bool retBool = false;

        int gridIndex = row + (col * sizeOfCauldronGrid);
        retBool = cauldronGridFilled[gridIndex];

        return retBool;
    }

    void ClearCauldronGrid()
    {
        for (int i = 0; i < cauldronGridFilled.Length; i++)
        {
            cauldronGridFilled[i] = false;
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
        GUIStyle g = new GUIStyle(GUI.skin.label);
        string s = "words go here";
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

        if (craftStateTimer > 0f)
            return;

        if (craftState == CraftState.Grimoire)
        {
            // grimoire box overlay
            r.x = 0.1f * w;
            r.y = 0.05f * h;
            r.width = 0.8f * w;
            r.height = 0.8f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            s = "THE GRIMOIRE";
            c = Color.white;
            GUI.color = c;

            GUI.Box(r, s, g);

            // grimoire spell listing
            r.x = 0.15f * w;
            r.y = 0.1f * h;
            r.width = 0.7f * w;
            r.height = 0.7f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));
            s = "No spell recipes have been acquired.\nLevel up to gain new recipes.";
            c = Color.white;
            GUI.color = c;
            // default empty grimoire display
            if ( pcm.playerData.magic.library.grimiore == null ||
                pcm.playerData.magic.library.grimiore.Length == 0 )
            {
                g.fontStyle = FontStyle.BoldAndItalic;
                GUI.Label(r, s, g);
            }
            else
            {
                r.x = 0.15f * w;
                r.y = 0.1f * h;
                r.width = 0.7f * w;
                r.height = 0.075f * h;
                g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
                for ( int i = 0; i < pcm.playerData.magic.library.grimiore.Length; i++ )
                {
                    c = Color.white;
                    if (i == currentGrimoireEntry)
                        c = Color.yellow;
                    GUI.color = c;
                    GrimioreData grim = pcm.playerData.magic.library.grimiore[i];
                    // spell name
                    g.alignment = TextAnchor.MiddleLeft;
                    s = grim.name;
                    GUI.Label(r, s, g);
                    // spell description
                    r.y += 0.05f * h;
                    g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
                    s = grim.description;
                    GUI.Label(r, s, g);
                    // spell ingredients
                    r.y += 0.05f * h;
                    g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
                    g.alignment = TextAnchor.MiddleRight;
                    s = "";
                    for (int n = 0; n < grim.ingredients.Length; n++)
                    {
                        s += grim.ingredients[n].ToString();
                        if (n < grim.ingredients.Length - 1)
                            s += ", ";
                    }
                    GUI.Label(r, s, g);
                    r.y += 0.05f * h;
                }
            }
        }

        if (craftState == CraftState.Cauldron)
        {
            // caulron box overlay
            r.x = 0.1f * w;
            r.y = 0.05f * h;
            r.width = 0.8f * w;
            r.height = 0.8f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            s = "THE CAULDRON";
            c = Color.white;
            GUI.color = c;

            GUI.Box(r, s, g);

            // spell book image (part of cauldron background)
            // NOTE: this sits top-left, to receive spell charge

            // grimoire recipe entry
            GrimioreData grim = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe];
            r.x = 0.15f * w;
            r.y = 0.55f * h;
            r.width = 0.3f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            s = grim.name;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);

            r.y += 0.05f * h;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.alignment = TextAnchor.MiddleRight;
            s = "";
            for (int i = 0; i < grim.ingredients.Length; i++)
            {
                s += grim.ingredients[i].ToString();
                if (i < grim.ingredients.Length - 1)
                    s += ", ";
                c = Color.white;
                GUI.color = c;
            }
            GUI.Label(r, s, g);

            Vector3 mouseClickPos = Vector3.zero; // REVIEW: safe?
            // acquire held item from mouse position and click
            if (heldIngredient == ItemType.Default && Input.GetMouseButtonDown(0))
            {
                mouseClickPos = Input.mousePosition;
                // convert mouse position pixels to viewport space
                mouseClickPos.x /= w;
                mouseClickPos.y /= h;
                mouseClickPos.y = 1f - mouseClickPos.y; // invert y
                print("mouse click position is "+mouseClickPos);
            }

            // ingredient inventory display
            r.y += 0.075f * h;
            r.width = 0.075f * w;
            r.height = r.width; // square
            c = Color.white;
            GUI.color = c;
            for (int i = 0; i < grim.ingredients.Length; i++)
            {
                // item icon
                c = Color.white; // adjust to gray if missing or empty?
                // adjust smaller
                r.x += 0.005f * w;
                r.y += 0.005f * w;
                r.width -= (0.01f * w);
                r.height -= (0.01f * w);
                // determine if this icon space contains a 'first press' mouse click
                if (mouseClickPos != Vector3.zero)
                {
                    if (r.Contains(mouseClickPos))
                        heldIngredient = grim.ingredients[i];
                }
                t = alm.itemImages[alm.GetArtData(grim.ingredients[i]).artIndexBase];
                if (heldIngredient == grim.ingredients[i])
                    c *= 0.5f; // gray out icon if held and dragging to cauldron
                GUI.color = c;
                if (!isAmongPlacedPieces(grim.ingredients[i]))
                    GUI.DrawTexture(r, t); // skip if ingredient is place in grid
                c = Color.white;
                // re-adjust larger
                r.x -= 0.005f * w;
                r.y -= 0.005f * w;
                r.width += (0.01f * w);
                r.height += (0.01f * w);
                // inventory slot frame
                t = (Texture2D)Resources.Load("Plot_Cursor");
                GUI.color = c;
                GUI.DrawTexture(r, t);
                r.x += r.width + (0.01f * w);
            }

            // cauldron image (part of cauldron background)
            // NOTE: this sits middle right, to hold crafting puzzle

            // placed pieces
            // NOTE: currently (test) simply using the single icon as placed
            // TODO: revise for shapes made from icon, up to 3x3 block shapes
            if (placedIngredients != null && placedIngredients.Length > 0)
            {
                for (int i = 0; i < placedIngredients.Length; i++)
                {
                    // NOTE: no need to use grid spacing, items have saved positions
                    r.x = placedIngredients[i].pos.x;
                    r.y = placedIngredients[i].pos.y;
                    c = Color.white;
                    // adjust smaller
                    r.x += 0.005f * w;
                    r.y += 0.005f * w;
                    r.width -= (0.01f * w);
                    r.height -= (0.01f * w);
                    t = alm.itemImages[alm.GetArtData(placedIngredients[i].type).artIndexBase];
                    GUI.color = c;
                    GUI.DrawTexture(r, t);
                    c = Color.white;
                    // re-adjust larger
                    r.x -= 0.005f * w;
                    r.y -= 0.005f * w;
                    r.width += (0.01f * w);
                    r.height += (0.01f * w);
                }
            }

            // cauldron crafting grid
            // centered at 0.7f * w, 0.45f * h
            // each grid space is 0.075f * w squared
            // no spacing between grid squares
            // sizeOfCauldronGrid determines starting position
            // sizeOfCauldronGrid is both vertical and horizontal size (square)
            r.x = 0.7f * w;
            r.y = 0.45f * h;
            r.width = 0.075f * w;
            r.height = r.width; // square
            r.x -= ((sizeOfCauldronGrid * r.width) / 2f);
            r.y -= ((sizeOfCauldronGrid * r.width) / 2f);
            float savedXPos = r.x;
            for ( int i=0; i < sizeOfCauldronGrid; i++ )
            {
                for ( int n=0; n < sizeOfCauldronGrid; n++ )
                {
                    GUI.DrawTexture(r,t);
                    r.x += r.width;
                }
                r.x = savedXPos;
                r.y += r.width;
            }

            // NOTE: will need to refer to those grid space locations

            // drag and drop item
            if ( heldIngredient != ItemType.Default )
            {
                // get mouse position
                // TODO: handle gamepad control
                heldPosition = Input.mousePosition;
                // convert mouse position pixels to viewport space
                heldPosition.x /= w;
                heldPosition.y /= h;
                heldPosition.y = 1f - heldPosition.y; // invert y

                // clamp held position to cauldron box
                heldPosition.x = Mathf.Clamp(heldPosition.x, 0.1f, 0.9f);
                heldPosition.y = Mathf.Clamp(heldPosition.y, 0.05f, 0.85f);
                heldPosition.z = 0f; // need to use this data?

                r.x = heldPosition.x - (0.075f * w * 0.5f);
                r.y = heldPosition.y - (0.075f * w * 0.5f);
                r.width = 0.075f * w;
                r.height = r.width;
                c = Color.white;
                GUI.color = c;
                GUI.DrawTexture(r, t);
            }
        }

        // detect mouse release held item
        if (heldIngredient != ItemType.Default && Input.GetMouseButtonUp(0))
        {
            // determine if placement is valid
            if (IsGridSpaceOpen(heldPosition))
            {
                // add to placed ingredient pieces
                AddPlacedPiece(heldIngredient, heldPosition);
                SetGridSpaceFilled(heldPosition); // TODO: revise for arbitrary item shapes
                // clear held item
                heldIngredient = ItemType.Default;
                heldPosition = Vector3.zero; // REVIEW: safe?
                // TODO: check puzzle solved
            }
        }

        c = Color.white;
        GUI.color = c;

        // cauldron or crafting button
        r.x = 0.15f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (craftState == CraftState.Grimoire)
            s = "TO MAGIC CAULDRON";
        else
            s = "CRAFT SPELL CHARGE";

        // TODO: un-comment this and require recipe selection to craft
        if (craftState == CraftState.Grimoire && selectedGrimoireRecipe == -1)
            GUI.enabled = false;
        // TODO: un-comment this and require crafting is solved
        //if (craftState == CraftState.Cauldron && !craftingSolved)
        //    GUI.enabled = false;

        if (craftState != CraftState.Exiting && GUI.Button(r, s, g))
        {
            if (craftState == CraftState.Grimoire)
            {
                craftState = CraftState.Cauldron;
                craftStateTimer = CRAFTSTATETIMERMAX;
                fadingOverlay = true;
            }
            else
            {
                // add spell charge to spell book (stay in this state)
                string spellName = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe].name;
                print("add '"+spellName+"' charge to spell book for completed spell craft");
                // REVIEW: how to handle craftingSolved?
                // craftingSolved = false;
            }
        }
        GUI.enabled = true;

        // reset crafting puzzle button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "RESET PUZZLE";

        if (placedIngredients.Length == 0)
            GUI.enabled = false;
        if (GUI.Button(r, s, g))
        {
            ClearPlacedPieces();
            ClearCauldronGrid();
        }
        GUI.enabled = true;

        // cancel / exit crafting button
        r.x = 0.65f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w/1024f));
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
