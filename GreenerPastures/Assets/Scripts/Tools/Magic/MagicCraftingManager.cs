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

    // NOTE: use this to refer to both grimoire recipe ingredient _and_ cauldron inventory item
    [System.Serializable]
    public class IngredientPiece
    {
        public int cauldronInventoryIndex; // when at cauldron
        public IngredientData ingredient;
        public Vector3 pos;
    }

    public struct IngredientTypeShape
    {
        public bool[] pieces;
    }

    public LibraryState libraryState;
    public CraftState craftState;

    public Texture2D grimoireBackground;
    public Texture2D cauldronBackgroundDark;
    public Texture2D cauldronBackgroundLight;

    public Texture2D[] cauldronBubbles; // 2 sets of 4
    private int cauldronBubbleFrame = -1;
    private Vector2 cauldronBubblePosition;
    private float cauldronBubbleTimer;

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
    private int topOfRecipeList;

    private InventoryData cauldronInventory; // ingredients from player inventory as grimoire recipe

    private int sizeOfCauldronGrid; // set this based on player level (max 5)
    private IngredientPiece heldIngredient; // ingredient item currently dragging
    private Vector3 heldPosition; // viewport position of current ingredient
    private bool[] heldIngredientShape; // 3x3 grid defining the shape of the held ingredient
    private IngredientPiece[] placedIngredients; // to draw placed ingredient pieces on grid
    private bool[] cauldronGridFilled; // 2 dimensional array (row, col) if spaces taken
    private bool craftingSolved; // has the player solved the crafting puzzle?

    // all ingredient types described as 3x3 shapes
    public ShapeLibraryManager.IngredientShapeType[] shapeLibrary;
    //private ShapeLibraryManager slm;

    private PlayerControlManager pcm;
    private PlayerControlManager leaving; // used in deactivation
    private ArtLibraryManager alm;
    private QuitOnEscape qoe; // disable to suspend use of start button during crafting
    private AudioManager sfxAudio;

    private MultiGamepad padMgr;
    // a button down to turn on padDragOn, with padDragOn true detect a button unpressed to turn off
    private int padIngredientSelection = -1;
    private bool padDragOn; // is the player currently dragging an ingredient with gamepad?
    private Vector3 padDragPos; // viewport space of held item
    private float padDragSpeed = 0.381f;

    private Texture2D[] buttonTex;

    const float LIBRARYSTATETIMERMAX = 1f;
    const float CRAFTSTATETIMERMAX = 1f;
    const float PLAYERCHECKTIME = 1f;
    const float PROXIMITYCHECKRADIUS = 0.381f;
    const float CAULDRONBUBBLETIME = 0.1f;


    void Start()
    {
        // validate
        // TODO: validate for grimoire and cauldron background images
        //slm = GameObject.FindFirstObjectByType<ShapeLibraryManager>();
        //if (slm == null)
        //{
        //    Debug.LogError("--- MagicCraftingManager [Start] : no shape library manager found in scene. aborting.");
        //    enabled = false;
        //}
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
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            checkTimer = PLAYERCHECKTIME;
            currentGrimoireEntry = -1;
            selectedGrimoireRecipe = -1;

            ClearPlacedPieces();
            heldIngredient = new IngredientPiece();
            heldIngredient.cauldronInventoryIndex = -1; // not holding anything (cleared, default)
            heldIngredient.ingredient = new IngredientData();
            // to be set based on item shape data
            heldIngredientShape = new bool[9]; // 3x3 grid makes a single shape

            InitializeIngredientShapeLibrary();

            // GUI Button Textures for build
            if (!Application.isEditor)
            {
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }
        }
    }

    void InitializeIngredientShapeLibrary()
    {
        ShapeLibraryManager slm = GameObject.FindFirstObjectByType<ShapeLibraryManager>();
        if (slm != null && slm.ingredientShapes != null && 
            slm.ingredientShapes.Length > 0)
        {
            shapeLibrary = slm.GetShapeLibrary();
            return;
        }
        else
            Debug.LogWarning("--- MagicCraftingManager [InitializeIngredientShapeLibrary] : no shape library manager found in scene or shape invalid data. will use temp shape library.");
    }

    bool[] GetShape( ItemType itype, PlantType pType )
    {
        bool[] retShape = new bool[9];

        for (int i = 0; i < shapeLibrary.Length; i++)
        {
            if ( ( pType == PlantType.Default && itype == shapeLibrary[i].item ) ||
                   ( itype == shapeLibrary[i].item && pType == shapeLibrary[i].plant ) )
            {
                retShape = shapeLibrary[i].pieces;
                break;
            }
        }

        return retShape;
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
                    // REVIEW: per player level (start 2x2 level 1, goes up one per 2 levels, up to 5)
                    sizeOfCauldronGrid = Mathf.RoundToInt(pcm.playerData.level / 2f) + 1;
                    sizeOfCauldronGrid = Mathf.Clamp(sizeOfCauldronGrid, 2, 5);
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
                        // show controls display hud item
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
                        Debug.LogWarning("--- MagicCraftingManager [HandleLibraryStates] : library state undefined. will ignore.");
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
                            if (currentBackground == null || 
                                currentBackground == cauldronBackgroundDark || currentBackground == cauldronBackgroundLight)
                                currentBackground = Texture2D.whiteTexture;
                        }
                        break;
                    case CraftState.Cauldron:
                        if (!fadingFromBlack)
                        {
                            if (cauldronBackgroundDark != null)
                                currentBackground = cauldronBackgroundDark;
                            TimeManager tm = GameObject.FindFirstObjectByType<TimeManager>();
                            if (tm != null)
                            {
                                if (tm.dayProgress > 0.25f && tm.dayProgress < 0.75f)
                                {
                                    if (cauldronBackgroundLight != null)
                                        currentBackground = cauldronBackgroundLight;
                                }
                            }
                            if (currentBackground != cauldronBackgroundDark &&
                                currentBackground != cauldronBackgroundLight)
                                currentBackground = Texture2D.whiteTexture;
                            cauldronBubbleTimer = 0f; // reset cauldron bubble
                            cauldronBubbleFrame = -1;
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
                        if (sfxAudio != null && sfxAudio.IsSoundPlaying("Magic Cauldron Bubble Loop"))
                            sfxAudio.StopSound("Magic Cauldron Bubble Loop");
                        break;
                    case CraftState.Cauldron:
                        fadingOverlay = false;
                        if (sfxAudio != null && !sfxAudio.IsSoundPlaying("Magic Cauldron Bubble Loop"))
                            sfxAudio.StartSound("Magic Cauldron Bubble Loop");
                        break;
                    case CraftState.Exiting:
                        libraryStateTimer = (LIBRARYSTATETIMERMAX/2f); // exit faster
                        ClearCauldronInventory();
                        craftState = CraftState.Default;
                        craftingDisplay = false;
                        fadingOverlay = false;
                        currentGrimoireEntry = -1;
                        selectedGrimoireRecipe = -1;
                        if (sfxAudio != null && sfxAudio.IsSoundPlaying("Magic Cauldron Bubble Loop"))
                            sfxAudio.StopSound("Magic Cauldron Bubble Loop");
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
                if ( pcm.playerData.magic.library.grimoire.Length > 0 && 
                    selectedGrimoireRecipe == -1)
                {
                    // allow player to change current recipe entry from grimoire listing
                    if (Input.GetKeyDown(pcm.upKey) || (padMgr != null && padMgr.gPadDown[0].YaxisL > 0f))
                        currentGrimoireEntry--;
                    if (Input.GetKeyDown(pcm.downKey) || (padMgr != null && padMgr.gPadDown[0].YaxisL < 0f))
                        currentGrimoireEntry++;
                    currentGrimoireEntry = Mathf.Clamp(currentGrimoireEntry, 0, pcm.playerData.magic.library.grimoire.Length - 1);
                    // set top of recipe list
                    if (currentGrimoireEntry < topOfRecipeList)
                        topOfRecipeList = currentGrimoireEntry;
                    if (currentGrimoireEntry > topOfRecipeList + 3)
                        topOfRecipeList = currentGrimoireEntry - 3;
                }
                if (currentGrimoireEntry != -1)
                {
                    // validate at least one of each ingredients in inventory
                    currentEntryValid = PlayerInventoryHasAllIngredients(pcm.playerData.magic.library.grimoire[currentGrimoireEntry]);
                    if (!currentEntryValid)
                        break;
                    // allow player to make selection of recipe to craft in cauldron state
                    if (Input.GetKeyDown(pcm.actionAKey) || (padMgr != null && padMgr.gPadDown[0].aButton))
                    {
                        selectedGrimoireRecipe = currentGrimoireEntry;
                        // fill cauldron inventory with first-found items matching grimoire recipe
                        FillCauldronInventory(pcm.playerData.magic.library.grimoire[selectedGrimoireRecipe]);
                        // ensure held ingredient is empty
                        heldIngredient.cauldronInventoryIndex = -1;
                    }
                }
                if (selectedGrimoireRecipe != -1)
                {
                    // allow player to cancel selection of recipe
                    if (Input.GetKeyDown(pcm.actionBKey) || (padMgr != null && padMgr.gPadDown[0].bButton))
                    {
                        selectedGrimoireRecipe = -1;
                        // clear cauldron inventory
                        ClearCauldronInventory();
                    }
                }
                break;
            case CraftState.Cauldron:
                // run cauldron bubble timer
                if (cauldronBubbleTimer > 0f)
                {
                    cauldronBubbleTimer -= Time.deltaTime;
                    if (cauldronBubbleTimer < 0f)
                    {
                        cauldronBubbleTimer = CAULDRONBUBBLETIME;
                        cauldronBubbleFrame++;
                        if(cauldronBubbleFrame == 4 || cauldronBubbleFrame == 8)
                        {
                            cauldronBubbleTimer = 0f; // reset
                            cauldronBubbleFrame = -1;
                        }
                    }
                }
                else if (craftStateTimer == 0f && RandomSystem.FlatRandom01() < 0.01f)
                {
                    Vector2 bubble = Vector2.zero;
                    bubble.x = 0.71f;
                    bubble.y = 0.36f;
                    bubble.x += (RandomSystem.GaussianRandom01() * 0.2f) - .1f;
                    bubble.y += (RandomSystem.GaussianRandom01() * 0.25f) - .125f;
                    cauldronBubblePosition = bubble;
                    if (RandomSystem.FlatRandom01() < 0.5f)
                        cauldronBubbleFrame = 0;
                    else
                        cauldronBubbleFrame = 4;
                    cauldronBubbleTimer = CAULDRONBUBBLETIME;
                }
                // gamepad for cauldron
                if (padMgr != null && padMgr.gamepads[0].isActive)
                {
                    ItemData[] cauldronInv = cauldronInventory.items;
                    // cauldron craft puzzle control via gamepad
                    // shoulder buttons to select ingredient inventory
                    if (padMgr.gPadDown[0].LBump)
                    {
                        padIngredientSelection--;
                        if (padIngredientSelection < 0)
                            padIngredientSelection = cauldronInv.Length - 1;
                    }
                    if (padMgr.gPadDown[0].RBump)
                    {
                        padIngredientSelection++;
                        if (padIngredientSelection > cauldronInv.Length - 1)
                            padIngredientSelection = 0;
                    }
                    padIngredientSelection = Mathf.Clamp(padIngredientSelection, 0, cauldronInv.Length - 1);

                    // use only for drag-and-drop of ingredients (a button hold and release)
                    if (!padDragOn &&
                        !isAmongPlacedPieces(padIngredientSelection) &&
                        padMgr.gPadDown[0].aButton)
                    {
                        padDragOn = true;

                        heldIngredient.cauldronInventoryIndex = padIngredientSelection;
                        heldIngredient.ingredient.name = cauldronInventory.items[padIngredientSelection].name;
                        heldIngredient.ingredient.item = cauldronInventory.items[padIngredientSelection].type;
                        heldIngredient.ingredient.plant = cauldronInventory.items[padIngredientSelection].plant;

                        heldIngredientShape = GetShape(heldIngredient.ingredient.item, heldIngredient.ingredient.plant);

                        // set padDragPos to center of this inventory space
                        padDragPos = Vector3.zero;
                        padDragPos.x = 0.15f + (padIngredientSelection * 0.075f);
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
                    padDragPos.y = Mathf.Clamp(padDragPos.y, 0.05f, 0.85f);
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
                cauldronBubbleTimer = 0f; // reset
                cauldronBubbleFrame = -1;
                break;
        }
    }

    bool PlayerInventoryHasAllIngredients( GrimoireData entry )
    {
        bool retBool = true;

        // NOTE: this needs to account for taking the ingredient and counting it only once
        int[] invItemsUsed = new int[entry.ingredients.Length];
        // initialize items used (none)
        for (int i = 0; i < invItemsUsed.Length; i++)
        {
            invItemsUsed[i] = -1;
        }
        // in all entry ingredients, find match item in player inventory
        for ( int i = 0; i < entry.ingredients.Length; i++ )
        {
            bool found = false;
            for (int n = 0; n < pcm.playerData.inventory.items.Length; n++)
            {
                bool alreadyUsed = false;
                // check inventory item not already used
                for (int t = 0; t < invItemsUsed.Length; t++)
                {
                    if (invItemsUsed[t] == n)
                        alreadyUsed = true;
                }
                if (alreadyUsed)
                    continue;
                // check item matches ingredient need
                // if default plant, any type; otherwise specific item needed
                if ((entry.ingredients[i].plant == PlantType.Default && 
                    pcm.playerData.inventory.items[n].type == entry.ingredients[i].item) ||
                    (entry.ingredients[i].item == pcm.playerData.inventory.items[n].type && 
                    entry.ingredients[i].plant == pcm.playerData.inventory.items[n].plant))
                {
                    found = true;
                    // store as inv item used
                    invItemsUsed[i] = n;
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

    void RemoveAllIngredientsFromPlayer()
    {
        if (cauldronInventory == null || cauldronInventory.items == null || cauldronInventory.items.Length == 0)
        {
            Debug.LogWarning("--- MagicCraftingManager [RemoveAllIngredientsFromPlayer] : cauldron inventory empty or invalid. will ignore.");
            return;
        }

        for (int i = 0; i < cauldronInventory.items.Length; i++)
        {
            pcm.playerData.inventory = InventorySystem.RemoveItemFromInventory(pcm.playerData.inventory, cauldronInventory.items[i]);
        }
    }

    void FillCauldronInventory( GrimoireData recipe )
    {
        cauldronInventory = InventorySystem.InitializeInventory(recipe.ingredients.Length);
        // fill inventory with first-found matching ingredients from player inventory
        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            bool found = false;
            for (int n = 0; n < pcm.playerData.inventory.items.Length; n++)
            {
                if ((recipe.ingredients[i].plant == PlantType.Default && 
                    recipe.ingredients[i].item == pcm.playerData.inventory.items[n].type) ||
                    (recipe.ingredients[i].item == pcm.playerData.inventory.items[n].type &&
                    recipe.ingredients[i].plant == pcm.playerData.inventory.items[n].plant))
                {
                    ItemData foundIngredient = pcm.playerData.inventory.items[n];
                    // confirm first-found match is not already in cauldron inventory
                    bool alreadyIn = false;
                    for (int t = 0; t < cauldronInventory.items.Length; t++)
                    {
                        if (cauldronInventory.items[t] == foundIngredient)
                        {
                            alreadyIn = true;
                            break;
                        }
                    }
                    if (!alreadyIn)
                    {
                        cauldronInventory = InventorySystem.AddToInventory(cauldronInventory, foundIngredient);
                        found = true;
                        break;
                    }
                }
            }
            if (!found)
                Debug.LogWarning("--- MagicCraftingManager [FillCauldronInventory] : missing recipe ingredient '" + recipe.ingredients[i].name + "'. will ignore.");
        }
    }

    bool DoesCauldronHaveEachPlacedPiece()
    {
        bool retBool = true;

        // player has solved puzzle, and still has inventory ingredients
        // we've already verified the cauldron inventory has valid ingredients
        // but we cannot guarantee their shapes are the same
        // (if not, puzzle needs to be reset for player to solve anew)
        // placed pieces must match shape, so seeds are always fine
        // otherwise match plant type
        
        for (int i = 0; i < placedIngredients.Length; i++)
        {
            if (placedIngredients[i].ingredient.item == ItemType.Plant ||
                placedIngredients[i].ingredient.item == ItemType.Stalk ||
                placedIngredients[i].ingredient.item == ItemType.Fruit)
            {
                // find plant match in cauldron
                bool found = false;
                for (int n = 0; n < cauldronInventory.items.Length; n++)
                {
                    if (cauldronInventory.items[n].type == placedIngredients[i].ingredient.item &&
                        cauldronInventory.items[n].plant == placedIngredients[i].ingredient.plant)
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
        }

        return retBool;
    }

    void ClearCauldronInventory()
    {
        cauldronInventory = null;
    }

    void ResetPuzzle()
    {
        heldIngredient.cauldronInventoryIndex = -1;
        heldIngredient.ingredient.name = "";
        heldIngredient.ingredient.item = ItemType.Default;
        heldIngredient.ingredient.plant = PlantType.Default;
        heldPosition = Vector3.zero;
        heldIngredientShape = new bool[9];
        ClearPlacedPieces();
        ClearCauldronGrid();
        craftingSolved = false;
    }

    // use cauldron inventory index to allow multiple of the same kind of ingredient
    void AddPlacedPiece( IngredientPiece piece, Vector3 pos, bool centerPiece )
    {
        IngredientPiece[] tmp = new IngredientPiece[placedIngredients.Length + 1];

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
            tmp[placedIngredients.Length] = new IngredientPiece();
            tmp[placedIngredients.Length].ingredient = new IngredientData();
            tmp[placedIngredients.Length].cauldronInventoryIndex = piece.cauldronInventoryIndex;
            tmp[placedIngredients.Length].ingredient.name = piece.ingredient.name;
            tmp[placedIngredients.Length].ingredient.item = piece.ingredient.item;
            tmp[placedIngredients.Length].ingredient.plant = piece.ingredient.plant;
            tmp[placedIngredients.Length].pos = pos;

            placedIngredients = tmp;
        }
    }

    Vector3 SnapToGrid( int row, int col )
    {
        Vector3 retVec = Vector3.zero;

        // based on sizeOfCauldronGrid and 0.075f * w per square grid space
        // starting from center of grid at 0.76f * w, 0.43f * h
        float ratioToX = (float)Screen.width / (float)Screen.height;
        retVec.x = 0.76f - (sizeOfCauldronGrid * 0.075f * 0.5f);
        retVec.y = 0.43f - (sizeOfCauldronGrid * 0.075f * ratioToX * 0.5f);
        retVec.x += col * (0.075f);
        retVec.y += row * (0.075f * ratioToX);

        return retVec;
    }

    bool isAmongPlacedPieces( int cauldronInvIdx )
    {
        bool retBool = false;

        for (int i = 0; i < placedIngredients.Length; i++)
        {
            if (placedIngredients[i].cauldronInventoryIndex == cauldronInvIdx)
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

        int sizeOfInv = cauldronInventory.items.Length;

        float ratioToX = ((float)Screen.width / (float)Screen.height);
        float leftX = 0.0825f;
        float topY = 0.675f;

        float floatCol = Mathf.RoundToInt(((viewport.x - leftX) / 0.075f) - 0.5f);
        // off inventory invalidation
        if (floatCol >= 0f && floatCol <= sizeOfInv - 1)
            retInvSlot = (int)Mathf.Clamp(floatCol, 0f, sizeOfInv - 1);
        // invalidate if out of row
        if (viewport.y < topY || viewport.y > (topY + (0.075f * ratioToX)))
            retInvSlot = -1;

        return retInvSlot;
    }

    void ClearPlacedPieces()
    {
        placedIngredients = new IngredientPiece[0];
    }

    void ConvertViewportSpaceToGrid( Vector3 viewport, out int row, out int col )
    {
        // set invalid by default
        int retRow = -1;
        int retCol = -1;

        // based on sizeOfCauldronGrid and 0.075f * w per square grid space
        // starting from center of grid at 0.7f * w, 0.45f * h
        float ratioToX = ((float)Screen.width/(float)Screen.height);
        float leftX = 0.76f - ((sizeOfCauldronGrid * 0.075f) / 2f);
        float topY = 0.43f - ((sizeOfCauldronGrid * 0.075f * ratioToX) / 2f);

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
        retBool = placedIngredients.Length == cauldronInventory.items.Length;

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

        // cauldron bubbles vfx
        if (craftState == CraftState.Cauldron && cauldronBubbleFrame > -1)
        {
            r.x = cauldronBubblePosition.x * w;
            r.y = cauldronBubblePosition.y * h;
            r.width = .1f * w;
            r.height = .1f * w; // square
            t = cauldronBubbles[cauldronBubbleFrame];
            c = Color.white;
            c.a = 0.618f;
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
            /*
            r.x = 0.375f * w;
            r.y = 0.025f * h;
            r.width = 0.25f * w;
            r.height = 0.075f * h;
            */
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.padding = new RectOffset(0, 0, 30, 0);
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
            if ( pcm.playerData.magic.library.grimoire == null ||
                pcm.playerData.magic.library.grimoire.Length == 0 )
            {
                g.fontStyle = FontStyle.BoldAndItalic;
                GUI.Label(r, s, g);
            }
            else
            {
                r.x = 0.15f * w;
                r.y = 0.15f * h;
                r.width = 0.7f * w;
                r.height = 0.065f * h;
                g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
                for ( int i = 0; i < pcm.playerData.magic.library.grimoire.Length; i++ )
                {
                    // display only four entries at a time
                    if (i < topOfRecipeList || i > topOfRecipeList + 3)
                        continue;
                    // grimoire entry display
                    c = Color.green;
                    c *= 0.381f;
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
                    GrimoireData grim = pcm.playerData.magic.library.grimoire[i];
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
                        s += grim.ingredients[n].name;
                        if (n < grim.ingredients.Length - 1)
                            s += ", ";
                    }
                    GUI.Label(r, s, g);
                    r.y += 0.05f * h;
                }

                r.y = 0.775f * h;
                GUI.color = Color.black;
                g.alignment = TextAnchor.MiddleCenter;
                s = "Spell Recipe " + (currentGrimoireEntry+1) + " of " + pcm.playerData.magic.library.grimoire.Length;
                GUI.Label(r, s, g);
            }
        }

        if (craftState == CraftState.Cauldron)
        {
            // caulron box overlay
            r.x = 0.375f * w;
            r.y = 0.025f * h;
            r.width = 0.25f * w;
            r.height = 0.075f * h;
            g = new GUIStyle(GUI.skin.box);
            g.fontSize = Mathf.RoundToInt(24 * (w / 1024f));
            g.padding = new RectOffset(0, 0, 15, 0);
            g.fontStyle = FontStyle.Bold;
            s = "THE CAULDRON";
            c = Color.white;
            GUI.color = c;

            GUI.Box(r, s, g);

            // spell book image (part of cauldron background)
            // NOTE: this sits top-left, to receive spell charge

            // grimoire recipe entry (cauldron inventory display)
            ItemData[] cauldronInv = new ItemData[0];
            if (cauldronInventory != null && cauldronInventory.items != null && cauldronInventory.items.Length > 0)
                cauldronInv = cauldronInventory.items; // else we should not be in cauldron anymore

            r.x = 0.0825f * w;
            r.y = 0.55f * h;
            r.width = 0.3f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            s = pcm.playerData.magic.library.grimoire[selectedGrimoireRecipe].name;
            c = Color.white;
            GUI.color = c;
            GUI.Label(r, s, g);

            r.y += 0.05f * h;
            g.fontSize = Mathf.RoundToInt(12 * (w / 1024f));
            g.alignment = TextAnchor.MiddleRight;
            s = "";
            for (int i = 0; i < cauldronInv.Length; i++)
            {
                s += cauldronInv[i].name;
                if (i < cauldronInv.Length - 1)
                    s += ", ";
                c = Color.white;
                GUI.color = c;
            }
            GUI.Label(r, s, g);

            Vector3 mouseClickPos = Vector3.zero;
            // acquire held item from mouse position and click
            if (currentEntryValid && heldIngredient.cauldronInventoryIndex == -1 && Input.GetMouseButtonDown(0))
            {
                mouseClickPos = Input.mousePosition;
                // convert mouse position pixels to viewport space
                mouseClickPos.x /= w;
                mouseClickPos.y /= h;
                mouseClickPos.y = 1f - mouseClickPos.y; // invert y

                // grab items from inventory slot spaces
                int ingredientIndex = ConvertViewportSpaceToInventory(mouseClickPos);
                if (ingredientIndex > -1)
                {
                    heldIngredient.cauldronInventoryIndex = ingredientIndex;
                    heldIngredient.ingredient.name = cauldronInventory.items[ingredientIndex].name;
                    heldIngredient.ingredient.item = cauldronInventory.items[ingredientIndex].type;
                    heldIngredient.ingredient.plant = cauldronInventory.items[ingredientIndex].plant;

                    heldIngredientShape = GetShape(heldIngredient.ingredient.item, heldIngredient.ingredient.plant);
                }
            }

            // ingredient inventory display
            r.y += 0.075f * h;
            r.width = 0.075f * w;
            r.height = r.width; // square
            c = Color.white;
            GUI.color = c;
            for (int i = 0; i < pcm.playerData.magic.library.grimoire[selectedGrimoireRecipe].ingredients.Length; i++)
            {
                // if not valid, we should never need cauldron inventory items available here
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
                        {
                            heldIngredient.cauldronInventoryIndex = i;
                            heldIngredient.ingredient.name = cauldronInventory.items[i].name;
                            heldIngredient.ingredient.item = cauldronInventory.items[i].type;
                            heldIngredient.ingredient.plant = cauldronInventory.items[i].plant;
                            //
                        }
                    }
                    t = alm.itemImages[alm.GetArtData(cauldronInventory.items[i].type, cauldronInventory.items[i].plant).artIndexBase];
                    if (heldIngredient.cauldronInventoryIndex == i)
                        c *= 0.381f; // gray out icon if held and dragging to cauldron
                    GUI.color = c;
                    if (placedIngredients == null || placedIngredients.Length == 0 || !isAmongPlacedPieces(i))
                        GUI.DrawTexture(r, t); // skip if ingredient is placed in grid
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
                    && padIngredientSelection == i)
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
                    bool[] thisIngredientShape = GetShape(placedIngredients[i].ingredient.item, placedIngredients[i].ingredient.plant);
                    int offsetX = -1;
                    int offsetY = -1;
                    for (int n=0; n<9; n++)
                    {
                        if (thisIngredientShape[n])
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
                            t = alm.itemImages[alm.GetArtData(placedIngredients[i].ingredient.item, placedIngredients[i].ingredient.plant).artIndexBase];
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

            r.x = 0.76f * w;
            r.y = 0.43f * h;
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
            if ( heldIngredient.cauldronInventoryIndex > -1 )
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
                    if (heldIngredientShape[i])
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
                        t = alm.itemImages[alm.GetArtData(heldIngredient.ingredient.item, heldIngredient.ingredient.plant).artIndexBase];
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
        if (currentEntryValid && heldIngredient.cauldronInventoryIndex > -1 && (
            ( (padMgr == null || !padMgr.gamepads[0].isActive) && Input.GetMouseButtonUp(0) ) || 
            ( padMgr != null && padMgr.gamepads[0].isActive && !padMgr.gamepads[0].aButton ) ) )
        {
            // determine if arbitrary item shape is valid on grid at this position
            bool valid = true;
            int offsetX = -1;
            int offsetY = -1;
            for ( int i = 0; i < 9; i++ )
            {
                if (heldIngredientShape[i])
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
                    if (heldIngredientShape[i])
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
                // sfx
                if (sfxAudio != null)
                {
                    float rnd = RandomSystem.FlatRandom01();
                    if (rnd < .333f)
                        sfxAudio.StartSound("Magic Cauldron Drop 1");
                    else if (rnd < .667)
                        sfxAudio.StartSound("Magic Cauldron Drop 2");
                    else
                        sfxAudio.StartSound("Magic Cauldron Drop 3");
                }
                // clear held item
                heldIngredient.cauldronInventoryIndex = -1;
                heldIngredient.ingredient.name = "";
                heldIngredient.ingredient.item = ItemType.Default;
                heldIngredient.ingredient.plant = PlantType.Default;
                heldPosition = Vector3.zero;
                heldIngredientShape = new bool[9];
                // handle gamepad control
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    padDragOn = false;

                // check puzzle solved
                craftingSolved = CheckPuzzleSolved();
            }
            else
            {
                // if not valid space on grid, reset to inventory
                heldIngredient.cauldronInventoryIndex = -1;
                heldIngredient.ingredient.name = "";
                heldIngredient.ingredient.item = ItemType.Default;
                heldIngredient.ingredient.plant = PlantType.Default;
                heldPosition = Vector3.zero;
                heldIngredientShape = new bool[9];
                // handle gamepad control
                if ( padMgr != null && padMgr.gamepads[0].isActive )
                    padDragOn = false;
            }
        }

        c = Color.white;
        GUI.color = c;

        // cauldron or crafting button
        r.x = 0.05f * w;
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
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
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
                GrimoireData gData = pcm.playerData.magic.library.grimoire[selectedGrimoireRecipe];
                string spellName = gData.name;
                pcm.playerData.magic.library = 
                    MagicSystem.AddChargeToSpellBook(gData.type, pcm.playerData.magic.library);
                // ARCANA SKILL : So Crafty (x2 charges)
                if (PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillSoCrafty))
                    pcm.playerData.magic.library = MagicSystem.AddChargeToSpellBook(gData.type, pcm.playerData.magic.library);
                // sfx
                if (sfxAudio != null)
                    sfxAudio.StartSound("Magic Cauldron Charge Crafted");
                pcm.AwardXP(PlayerData.XP_CRAFTMAGIC);

                // remove all cauldron inventory items from player inventory
                RemoveAllIngredientsFromPlayer();
                // clear cauldron inventory
                ClearCauldronInventory();
                // re-fill cauldron inventory if player has necessary ingredients
                if (PlayerInventoryHasAllIngredients(gData))
                {
                    FillCauldronInventory(gData);
                    // if placed pieces do not match cauldron inventory items (shape), reset puzzle
                    if (!DoesCauldronHaveEachPlacedPiece())
                        ResetPuzzle();
                }
                else
                {
                    // if player no longer has necessary ingredients available, un-solve puzzle
                    currentEntryValid = false;
                    // reset craft interface due to lack of ingredients
                    ResetPuzzle();
                }
            }
        }
        GUI.enabled = true;

        // reset crafting puzzle button
        r.x = 0.2875f * w;
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
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "RESET PUZZLE";
        if ( padMgr != null && padMgr.gamepads[0].isActive )
            s += "\n[X BUTTON]";

        if (placedIngredients != null && placedIngredients.Length == 0)
            GUI.enabled = false;
        if (craftState == CraftState.Cauldron && 
            (GUI.Button(r, s, g) || 
            (GUI.enabled && padMgr != null && padMgr.gamepads[0].isActive && 
                padMgr.gPadDown[0].xButton)))
            ResetPuzzle();
        GUI.enabled = true;

        // return to grimoire button
        // (pad y button)
        r.x = 0.5125f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.075f * h;
        g = new GUIStyle(GUI.skin.button);
        if (padMgr != null && padMgr.gamepads[0].isActive)
            g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "BACK TO GRIMOIRE";
        if (padMgr != null && padMgr.gamepads[0].isActive)
            s += "\n[Y BUTTON]";

        if (craftState == CraftState.Cauldron && 
            ( GUI.Button(r, s, g) ||
            (padMgr != null && padMgr.gamepads[0].isActive &&
                padMgr.gPadDown[0].yButton)))
        {
            ResetPuzzle();
            // to grimoire
            ClearCauldronInventory();
            selectedGrimoireRecipe = -1;
            currentGrimoireEntry = -1;
            craftState = CraftState.Grimoire;
            craftStateTimer = CRAFTSTATETIMERMAX;
            fadingOverlay = true;
        }

        // cancel / exit crafting button
        r.x = 0.75f * w;
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
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
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
