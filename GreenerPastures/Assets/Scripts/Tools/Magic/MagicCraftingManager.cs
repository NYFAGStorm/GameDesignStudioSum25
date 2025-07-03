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

    public struct ItemTypeShape
    {
        public bool[] pieces;
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
    private bool currentEntryValid; // player has all ingredients in inventory
    private int selectedGrimoireRecipe;

    private int sizeOfCauldronGrid; // set this based on player level (max 5)
    private ItemType heldIngredient; // ingredient item currently dragging
    private Vector3 heldPosition; // viewport position of current ingredient
    private bool[] heldItemShape; // 3x3 grid defining the shape of the held item
    private ItemPiece[] placedIngredients; // to draw placed ingredient pieces on grid
    private bool[] cauldronGridFilled; // 2 dimensional array (row, col) if spaces taken
    private bool craftingSolved; // has the player solved the crafting puzzle?

    private ShapeLibraryManager.ItemTypeShape[] shapeLibrary; // all item types described as 3x3 shapes
    // NOTE: the index for this array is the enum reference number of the item type

    private PlayerControlManager pcm;
    private PlayerControlManager leaving; // used in deactivation
    private ArtLibraryManager alm;
    private QuitOnEscape qoe; // disable to suspend use of start button during crafting

    private MultiGamepad padMgr;
    // a button down to turn on padDragOn, with padDragOn true detect a button unpressed to turn off
    private int padItemSelection = -1;
    private bool padDragOn; // is the player currently dragging an item with gamepad?
    private Vector3 padDragPos; // viewport space of held item
    private float padDragSpeed = 0.381f;

    const float LIBRARYSTATETIMERMAX = 1f;
    const float CRAFTSTATETIMERMAX = 1f;
    const float PLAYERCHECKTIME = 1f;
    const float PROXIMITYCHECKRADIUS = 0.381f;

    const int TOTALITEMSHAPETYPES = 6;


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
        qoe = GameObject.FindAnyObjectByType<QuitOnEscape>();
        if ( qoe == null )
        {
            Debug.LogError("--- MagicCraftingManager [Start] : no quit on escape found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            checkTimer = PLAYERCHECKTIME;
            currentGrimoireEntry = -1;
            selectedGrimoireRecipe = -1;

            placedIngredients = new ItemPiece[0];

            // to be set based on item shape data
            heldItemShape = new bool[9]; // 3x3 grid makes a single shape

            InitializeItemShapeLibrary();
        }
    }

    void InitializeItemShapeLibrary()
    {
        ShapeLibraryManager slm = GameObject.FindFirstObjectByType<ShapeLibraryManager>();
        if (slm != null && slm.itemShapes != null && 
            slm.itemShapes.Length > 0)
        {
            shapeLibrary = slm.GetShapeLibrary();
            return;
        }

        Debug.LogWarning("--- MagicCraftingManager [InitializeItemShapeLibrary] : no shape library manager found in scene or shape invalid data. will use temple shape library.");

        shapeLibrary = new ShapeLibraryManager.ItemTypeShape[TOTALITEMSHAPETYPES];

        for ( int i = 0; i < shapeLibrary.Length; i++ )
        {
            shapeLibrary[i].pieces = new bool[9];
            switch( (ItemType)i )
            {
                case ItemType.Default:
                    shapeLibrary[i].pieces[4] = true;
                    break;
                case ItemType.Fertilizer:
                    shapeLibrary[i].pieces[4] = true;
                    shapeLibrary[i].pieces[6] = true;
                    shapeLibrary[i].pieces[7] = true;
                    shapeLibrary[i].pieces[8] = true;
                    break;
                case ItemType.Seed:
                    shapeLibrary[i].pieces[4] = true;
                    break;
                case ItemType.Plant:
                    shapeLibrary[i].pieces[0] = true;
                    shapeLibrary[i].pieces[1] = true;
                    shapeLibrary[i].pieces[2] = true;
                    shapeLibrary[i].pieces[4] = true;
                    shapeLibrary[i].pieces[7] = true;
                    break;
                case ItemType.Stalk:
                    shapeLibrary[i].pieces[4] = true;
                    shapeLibrary[i].pieces[7] = true;
                    break;
                case ItemType.Fruit:
                    shapeLibrary[i].pieces[1] = true;
                    shapeLibrary[i].pieces[3] = true;
                    shapeLibrary[i].pieces[4] = true;
                    shapeLibrary[i].pieces[5] = true;
                    shapeLibrary[i].pieces[7] = true;
                    break;
                case ItemType.Rock:
                    shapeLibrary[i].pieces[0] = true;
                    shapeLibrary[i].pieces[1] = true;
                    shapeLibrary[i].pieces[3] = true;
                    shapeLibrary[i].pieces[4] = true;
                    break;
            }
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
                    pcm.freezeCharacterActions = true;
                    pcm.hidePlayerHUD = true;
                    // hide controls display hud item
                    InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                    if (igc != null)
                        igc.enabled = false;
                    libraryState = LibraryState.Activating;
                    libraryStateTimer = LIBRARYSTATETIMERMAX;
                    // configure cauldron grid size
                    // REVIEW: based on player level (start 2x2, goes up one per 10 levels?)
                    sizeOfCauldronGrid = 2 + Mathf.RoundToInt((pcm.playerData.level / 10f) - .5f);
                    sizeOfCauldronGrid = Mathf.Clamp(sizeOfCauldronGrid, 2, 5);
                    // TODO: remove this temp testing
                    sizeOfCauldronGrid = 3;
                    cauldronGridFilled = new bool[sizeOfCauldronGrid * sizeOfCauldronGrid];
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
                        qoe.enabled = false;
                        break;
                    case LibraryState.Active:
                        pcm.characterFrozen = false;
                        pcm.freezeCharacterActions = false;
                        pcm.hidePlayerHUD = false;
                        // hide controls display hud item
                        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                        if (igc != null)
                            igc.enabled = true;
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
                        qoe.enabled = true;
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
                if ( pcm.playerData.magic.library.grimiore.Length > 0 && 
                    selectedGrimoireRecipe == -1)
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
                    // validate at least one of each ingredients in inventory
                    currentEntryValid = PlayerInventoryHasAllIngredients(pcm.playerData.magic.library.grimiore[currentGrimoireEntry]);
                    if (!currentEntryValid)
                        break;
                    // allow player to make selection of recipe to craft in cauldron state
                    if (Input.GetKeyDown(pcm.actionAKey) || (padMgr != null && padMgr.gPadDown[0].aButton))
                        selectedGrimoireRecipe = currentGrimoireEntry;
                }
                if (selectedGrimoireRecipe != -1)
                {
                    // allow player to cancel selection of recipe
                    if (Input.GetKeyDown(pcm.actionBKey) || (padMgr != null && padMgr.gPadDown[0].bButton))
                        selectedGrimoireRecipe = -1;
                }
                break;
            case CraftState.Cauldron:
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    GrimioreData grim = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe];
                    // cauldron craft puzzle control via gamepad
                    // shoulder buttons to select ingredient inventory
                    if (padMgr.gPadDown[0].LBump)
                    {
                        padItemSelection--;
                        if (padItemSelection < 0)
                            padItemSelection = grim.ingredients.Length - 1;
                    }
                    if (padMgr.gPadDown[0].RBump)
                    {
                        padItemSelection++;
                        if (padItemSelection > grim.ingredients.Length - 1)
                            padItemSelection = 0;
                    }
                    padItemSelection = Mathf.Clamp(padItemSelection,0, grim.ingredients.Length - 1);
                    // use only for drag-and-drop of items (a button hold and release)
                    if (!padDragOn && 
                        !isAmongPlacedPieces(grim.ingredients[padItemSelection]) &&
                        padMgr.gPadDown[0].aButton)
                    {
                        padDragOn = true;
                        heldIngredient = grim.ingredients[padItemSelection];
                        heldItemShape = shapeLibrary[(int)heldIngredient].pieces;
                        // set padDragPos to center of this inventory space
                        padDragPos = Vector3.zero;
                        padDragPos.x = 0.15f + (padItemSelection * 0.075f);
                        padDragPos.y = 0.675f;
                        padDragPos.x += (0.075f * 0.5f);
                        padDragPos.y += 0.075f;
                    }
                    // handle dragging, clamp to bounds of cauldron box
                    if (!padDragOn)
                        break;
                    if (padMgr.gamepads[0].YaxisL > 0f)
                    {
                        padDragPos.y -= Time.deltaTime * padDragSpeed;
                    }
                    else if (padMgr.gamepads[0].YaxisL < 0f)
                    {
                        padDragPos.y += Time.deltaTime * padDragSpeed;
                    }
                    padDragPos.y = Mathf.Clamp(padDragPos.y,0.05f,0.85f);
                    if (padMgr.gamepads[0].XaxisL < 0f)
                    {
                        padDragPos.x -= Time.deltaTime * padDragSpeed;
                    }
                    else if (padMgr.gamepads[0].XaxisL > 0f)
                    {
                        padDragPos.x += Time.deltaTime * padDragSpeed;
                    }
                    padDragPos.x = Mathf.Clamp(padDragPos.x, 0.1f, 0.9f);
                    // handle drop in OnGUI
                }
                break;
            case CraftState.Exiting:
                break;
        }
    }

    bool PlayerInventoryHasAllIngredients( GrimioreData entry )
    {
        bool retBool = true;

        for ( int i = 0; i < entry.ingredients.Length; i++ )
        {
            ItemType iType = entry.ingredients[i];
            bool found = false;
            for ( int n = 0; n < pcm.playerData.inventory.items.Length; n++ )
            {
                if (pcm.playerData.inventory.items[n].type == iType)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                retBool = false;
                break;
            }
        }

        return retBool;
    }

    void RemoveAllIngredientsFromPlayer( GrimioreData entry )
    {
        for (int i = 0; i < entry.ingredients.Length; i++)
        {
            ItemType iType = entry.ingredients[i];
            pcm.playerData.inventory = InventorySystem.RemoveFromInventory(pcm.playerData.inventory, iType);
        }
    }

    void AddPlacedPiece( ItemType type, Vector3 pos, bool centerPiece )
    {
        ItemPiece[] tmp = new ItemPiece[placedIngredients.Length + 1];

        // convert pos to snapped at center of grid position
        int oRow = 0;
        int oCol = 0;
        ConvertViewportSpaceToGrid(pos, out oRow, out oCol);
        pos = SnapToGrid(oRow, oCol);

        if (centerPiece)
        {
            // add to placed piece array
            for (int i = 0; i < placedIngredients.Length; i++)
            {
                tmp[i] = placedIngredients[i];
            }
            tmp[placedIngredients.Length] = new ItemPiece();
            tmp[placedIngredients.Length].type = type;
            tmp[placedIngredients.Length].pos = pos;

            placedIngredients = tmp;
        }
    }

    Vector3 SnapToGrid( int row, int col )
    {
        Vector3 retVec = Vector3.zero;

        // based on sizeOfCauldronGrid and 0.075f * w per square grid space
        // starting from center of grid at 0.7f * w, 0.45f * h
        float ratioToX = (float)Screen.width / (float)Screen.height;
        retVec.x = 0.7f - (sizeOfCauldronGrid * 0.075f * 0.5f);
        retVec.y = 0.45f - (sizeOfCauldronGrid * 0.075f * ratioToX * 0.5f);
        retVec.x += col * (0.075f);
        retVec.y += row * (0.075f * ratioToX);

        return retVec;
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

    int ConvertViewportSpaceToInventory( Vector3 viewport )
    {
        // set invalid by default
        int retInvSlot = -1;

        int sizeOfInv = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe].ingredients.Length;

        float ratioToX = ((float)Screen.width / (float)Screen.height);
        float leftX = 0.15f;
        float topY = 0.675f;

        float floatCol = Mathf.RoundToInt(((viewport.x - leftX) / 0.075f) - 0.5f);
        // off inventory invalidation
        if (floatCol >= 0f && floatCol <= sizeOfCauldronGrid - 1)
            retInvSlot = (int)Mathf.Clamp(floatCol, 0f, sizeOfCauldronGrid - 1);
        // invalidate if out of row
        if (viewport.y < topY || viewport.y > (topY + (0.075f * ratioToX)))
            retInvSlot = -1;

        return retInvSlot;
    }

    void ClearPlacedPieces()
    {
        placedIngredients = new ItemPiece[0];
    }

    void ConvertViewportSpaceToGrid( Vector3 viewport, out int row, out int col )
    {
        // set invalid by default
        int retRow = -1;
        int retCol = -1;

        // based on sizeOfCauldronGrid and 0.075f * w per square grid space
        // starting from center of grid at 0.7f * w, 0.45f * h
        float ratioToX = ((float)Screen.width/(float)Screen.height);
        float leftX = 0.7f - ((sizeOfCauldronGrid * 0.075f) / 2f);
        float topY = 0.45f - ((sizeOfCauldronGrid * 0.075f * ratioToX) / 2f);

        float floatCol = Mathf.RoundToInt(( (viewport.x - leftX) / 0.075f ) - 0.5f);
        // off grid invalidation
        if (floatCol >= 0f && floatCol <= sizeOfCauldronGrid-1)
            retCol = (int)Mathf.Clamp(floatCol, 0f, sizeOfCauldronGrid - 1);

        float floatRow = Mathf.RoundToInt(( (viewport.y - topY) / (0.075f * ratioToX) ) - 0.5f);
        // off grid invalidation
        if (floatRow >= 0f && floatRow <= sizeOfCauldronGrid-1)
            retRow = (int)Mathf.Clamp(floatRow, 0f, sizeOfCauldronGrid - 1);

        if (retCol == -1 || retRow == -1)
        {
            // invalidate both axis if off grid
            retRow = -1;
            retCol = -1;
        }

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
        if (row == -1 || col == -1)
        {
            Debug.LogWarning("--- MagicCraftingManager [SetGridSpaceFilled] : invalid row and column. will ignore.");
            return;
        }
        int resultIndex = row + (col * sizeOfCauldronGrid);
        cauldronGridFilled[resultIndex] = true;
    }

    bool IsGridSpaceOpen( Vector3 viewport )
    {
        bool retBool = false;

        int outRow = 0;
        int outCol = 0;
        ConvertViewportSpaceToGrid(viewport, out outRow, out outCol);
        if ( outRow > -1 && outCol > -1 )
            retBool = IsGridSpaceOpen(outRow, outCol);
        
        return retBool;
    }

    bool IsGridSpaceOpen( int row, int col )
    {
        bool retBool = false;

        if (row == -1 || col == -1)
            return retBool; // invalid space off grid

        int gridIndex = row + (col * sizeOfCauldronGrid);
        retBool = !cauldronGridFilled[gridIndex];

        return retBool;
    }

    void ClearCauldronGrid()
    {
        for (int i = 0; i < cauldronGridFilled.Length; i++)
        {
            cauldronGridFilled[i] = false;
        }
    }

    bool CheckPuzzleSolved()
    {
        bool retBool = false;

        // REVIEW: simply, all ingredients used?
        retBool = (pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe].ingredients.Length == placedIngredients.Length);

        return retBool;
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
                    c = Color.gray;
                    if (i == currentGrimoireEntry)
                        c = Color.white;
                    if (i == currentGrimoireEntry && !currentEntryValid)
                        c = Color.black; // invalid due to lack of ingredients in inventory
                    if (i == selectedGrimoireRecipe)
                    {
                        // recipe selected, may toggle un-select with a button press
                        c = Color.blue;
                        c.r = 0.2f;
                        c.g = 0.2f;
                    }
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

            Vector3 mouseClickPos = Vector3.zero;
            // acquire held item from mouse position and click
            if (heldIngredient == ItemType.Default && Input.GetMouseButtonDown(0))
            {
                mouseClickPos = Input.mousePosition;
                // convert mouse position pixels to viewport space
                mouseClickPos.x /= w;
                mouseClickPos.y /= h;
                mouseClickPos.y = 1f - mouseClickPos.y; // invert y

                // grab items from inventory slot spaces
                int itemIndex = ConvertViewportSpaceToInventory(mouseClickPos);
                if (itemIndex > -1)
                {
                    ItemType iType = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe].ingredients[itemIndex];
                    heldIngredient = iType;
                    heldItemShape = shapeLibrary[(int)iType].pieces;
                }
            }

            // ingredient inventory display
            r.y += 0.075f * h;
            r.width = 0.075f * w;
            r.height = r.width; // square
            c = Color.white;
            GUI.color = c;
            for (int i = 0; i < grim.ingredients.Length; i++)
            {
                if (currentEntryValid)
                {
                    // item icon
                    c = Color.white;
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
                        c *= 0.381f; // gray out icon if held and dragging to cauldron
                    GUI.color = c;
                    if (!isAmongPlacedPieces(grim.ingredients[i]))
                        GUI.DrawTexture(r, t); // skip if ingredient is place in grid
                    c = Color.white;
                    // re-adjust larger
                    r.x -= 0.005f * w;
                    r.y -= 0.005f * w;
                    r.width += (0.01f * w);
                    r.height += (0.01f * w);
                }
                // inventory slot frame
                t = (Texture2D)Resources.Load("Plot_Cursor");
                if (padMgr != null && padMgr.gamepads[0].isActive 
                    && padItemSelection == i)
                    c = Color.yellow;
                GUI.color = c;
                GUI.DrawTexture(r, t);
                r.x += r.width;
            }
            GUI.color = Color.white;

            // cauldron image (part of cauldron background)
            // NOTE: this sits middle right, to hold crafting puzzle

            // placed pieces
            // handle arbitrary shapes made from icon, up to 3x3 block shapes
            if (placedIngredients != null && placedIngredients.Length > 0)
            {
                for (int i = 0; i < placedIngredients.Length; i++)
                {
                    bool[] thisItemShape = shapeLibrary[(int)placedIngredients[i].type].pieces;
                    int offsetX = -1;
                    int offsetY = -1;
                    for (int n=0; n<9; n++)
                    {
                        if (thisItemShape[n])
                        {
                            Vector3 shapePart = placedIngredients[i].pos;
                            shapePart.x += offsetX * 0.075f;
                            shapePart.y += offsetY * 0.075f * (w / h);

                            // NOTE: no need to use grid spacing, items have saved positions
                            r.x = shapePart.x * w;
                            r.y = shapePart.y * h;
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
                        offsetX++;
                        if (offsetX > 1)
                        {
                            offsetX = -1;
                            offsetY++;
                        }
                    }
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
            t = (Texture2D)Resources.Load("Plot_Cursor");
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

            // drag and drop item
            if ( heldIngredient != ItemType.Default )
            {
                if ( padMgr == null || !padMgr.gamepads[0].isActive )
                {
                    // get mouse position
                    heldPosition = Input.mousePosition;
                    // convert mouse position pixels to viewport space
                    heldPosition.x /= w;
                    heldPosition.y /= h;
                    heldPosition.y = 1f - heldPosition.y; // invert y

                    // clamp held position to cauldron box
                    heldPosition.x = Mathf.Clamp(heldPosition.x, 0.1f, 0.9f);
                    heldPosition.y = Mathf.Clamp(heldPosition.y, 0.05f, 0.85f);
                    heldPosition.z = 0f; // need to use this data?
                }
                else
                {
                    // handle gamepad drag control
                    heldPosition = padDragPos;
                }

                // handle multiple squares for shapes (3x3)
                int shapeX = -1;
                int shapeY = -1;
                for ( int i = 0; i < 9; i++ )
                {
                    float offsetX = shapeX * 0.075f;
                    float offsetY = shapeY * 0.075f * (w / h);
                    if (heldItemShape[i])
                    {
                        r.x = heldPosition.x - (0.075f * 0.5f);
                        r.y = heldPosition.y - (0.075f * 0.5f * (w / h));
                        r.x += offsetX;
                        r.y += offsetY;
                        r.x *= w;
                        r.y *= h;
                        r.width = 0.075f * w;
                        r.height = r.width;

                        // shape background
                        t = Texture2D.whiteTexture;
                        c = Color.blue;
                        c.a = 0.1f;
                        GUI.color = c;
                        GUI.DrawTexture(r, t);

                        // item icon
                        t = alm.itemImages[alm.GetArtData(heldIngredient).artIndexBase];
                        c = Color.white;
                        GUI.color = c;
                        GUI.DrawTexture(r, t);
                    }
                    shapeX++;
                    if (shapeX > 1)
                    {
                        shapeX = -1;
                        shapeY++;
                    }
                }
            }
        }

        // detect mouse release held item or gamepad a button release
        if (heldIngredient != ItemType.Default && (
            ( (padMgr == null || !padMgr.gamepads[0].isActive) && Input.GetMouseButtonUp(0) ) || 
            ( padMgr != null && padMgr.gamepads[0].isActive && !padMgr.gamepads[0].aButton ) ) )
        {
            // determine if arbitrary item shape is valid on grid at this position
            bool valid = true;
            int offsetX = -1;
            int offsetY = -1;
            for ( int i = 0; i < 9; i++ )
            {
                if (heldItemShape[i])
                {
                    Vector3 shapeCheckPos = heldPosition;
                    shapeCheckPos.x += offsetX * 0.075f;
                    shapeCheckPos.y += offsetY * 0.075f * (w / h);
                    if (!IsGridSpaceOpen(shapeCheckPos))
                    {
                        valid = false;
                        break;
                    }
                }
                offsetX++;
                if (offsetX > 1)
                {
                    offsetX = -1;
                    offsetY++;
                }
            }

            if (valid)
            {
                // add all parts of shape to placed ingredient pieces
                offsetX = -1;                
                offsetY = -1;
                for ( int i = 0; i < 9; i++ )
                {
                    if (heldItemShape[i])
                    {
                        Vector3 shapeCheckPos = heldPosition;
                        shapeCheckPos.x += offsetX * 0.075f;
                        shapeCheckPos.y += offsetY * 0.075f * (w/h);
                        AddPlacedPiece(heldIngredient, shapeCheckPos, (i == 4));
                        SetGridSpaceFilled(shapeCheckPos);
                    }
                    offsetX++;
                    if (offsetX > 1)
                    {
                        offsetX = -1;
                        offsetY++;
                    }
                }
                // clear held item
                heldIngredient = ItemType.Default;
                heldPosition = Vector3.zero;
                heldItemShape = new bool[9];
                // handle gamepad control
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    padDragOn = false;

                // check puzzle solved
                craftingSolved = CheckPuzzleSolved();
            }
            else
            {
                // if not valid space on grid, reset to inventory
                heldIngredient = ItemType.Default;
                heldPosition = Vector3.zero;
                heldItemShape = new bool[9];
                // handle gamepad control
                if ( padMgr != null && padMgr.gamepads[0].isActive )
                    padDragOn = false;
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
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (craftState == CraftState.Grimoire)
            s = "TO MAGIC CAULDRON";
        else
            s = "CRAFT SPELL CHARGE";
        if (padMgr != null && padMgr.gamepads[0].isActive)
            s += "\n[START BUTTON]";

        // require recipe selection to craft
        if (craftState == CraftState.Grimoire && selectedGrimoireRecipe == -1)
            GUI.enabled = false;
        // require crafting is solved
        if (craftState == CraftState.Cauldron && !craftingSolved)
            GUI.enabled = false;

        if (craftState != CraftState.Exiting && 
            (GUI.Button(r, s, g) || 
            (GUI.enabled && padMgr != null && padMgr.gamepads[0].isActive && 
                padMgr.gPadDown[0].startButton)))
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
                GrimioreData gData = pcm.playerData.magic.library.grimiore[selectedGrimoireRecipe];
                string spellName = gData.name;
                print("adding '"+spellName+"' charge to spell book");
                pcm.playerData.magic.library = 
                    MagicSystem.AddChargeToSpellBook(gData.type, pcm.playerData.magic.library);

                // remove all recipe ingredient items from player inventory
                RemoveAllIngredientsFromPlayer(gData);
                // if player no longer has necessary ingredients available, un-solve puzzle
                if (!PlayerInventoryHasAllIngredients(gData))
                {
                    currentEntryValid = false;
                    // reset craft interface due to lack of ingredients
                    heldIngredient = ItemType.Default;
                    heldPosition = Vector3.zero;
                    heldItemShape = new bool[9];
                    ClearPlacedPieces();
                    ClearCauldronGrid();
                    craftingSolved = false; // disallow another spell charge
                }
            }
        }
        GUI.enabled = true;

        // reset crafting puzzle button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "RESET PUZZLE";
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            s += "\n[X BUTTON]";

        if (placedIngredients != null && placedIngredients.Length == 0)
            GUI.enabled = false;
        if (craftState == CraftState.Cauldron && 
            (GUI.Button(r, s, g) || 
            (GUI.enabled && padMgr != null && padMgr.gamepads[0].isActive && 
                padMgr.gPadDown[0].xButton)))
        {
            heldIngredient = ItemType.Default;
            heldPosition = Vector3.zero;
            heldItemShape = new bool[9];
            ClearPlacedPieces();
            ClearCauldronGrid();
            craftingSolved = false;
        }
        GUI.enabled = true;

        // cancel / exit crafting button
        r.x = 0.65f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        s = "EXIT CRAFTING";
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            s += "\n[BACK BUTTON]";

        if (GUI.Button(r, s, g) || 
            (padMgr != null && padMgr.gamepads[0].isActive && 
                padMgr.gPadDown[0].backButton))
        {
            craftState = CraftState.Exiting;
            craftStateTimer = CRAFTSTATETIMERMAX;
            fadingOverlay = true;
        }
    }
}
