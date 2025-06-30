using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;

public class NetworkPlayerController : NetworkBehaviour
{
    // Author: Glenn Storm
    // This handles the local player controls for their character

    public PlayerData playerData;

    public enum PlayerControlType
    {
        Default,
        Up,
        Down,
        Left,
        Right,
        ActionA,
        ActionB,
        ActionC,
        ActionD
    }

    public struct PlayerActions
    {
        public bool actionA;
        public bool actionADown; // 'first press' frame signal only (must un-press)
        public bool actionB;
        public bool actionBDown;
        public bool actionC;
        public bool actionCDown;
        public bool actionD;
        public bool actionDDown;
        public bool lBump;
        public bool lBumpDown;
        public bool rBump;
        public bool rBumpDown;
    }

    private PlayerInput inputs;

    public float characterSpeed = 2.7f;

    public bool characterFrozen;
    public bool hidePlayerHUD;

    private Vector3 characterMove;
    private LooseItemManager activeItem;
    private PlotManager activePlot;

    private InventoryData playerInventory;
    private int currentInventorySelection;

    private MultiGamepad padMgr;

    private CameraManager cam;
    private PlayerAnimManager pam;
    private ArtLibraryManager alm;

    const float PROXIMITYRANGE = 0.381f;


    void Start()
    {
        // validate
        // padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        // TODO: change this to error and abort if no gamepad manager found (allow no pad for testing)
        // (then clean up below checks for padMgr existing)
        // if (padMgr == null )
        //     Debug.LogWarning("--- PlayerControlManager [Start] : " + gameObject.name + " no pad manager. will ignore.");
        cam = GameObject.FindFirstObjectByType<CameraManager>();
        if ( cam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no camera manager found in scene. aborting.");
            enabled = false;
        }
        pam = gameObject.transform.GetComponentInChildren<PlayerAnimManager>();
        if ( pam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no player anim manager found in children. aborting.");
            enabled = false;
        }
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no art library manager found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            cam.SetPlayer(this);
            playerInventory = InventorySystem.InitializeInventory(5);
            currentInventorySelection = 2;

            // temp - fill player inventory for testing
            playerInventory.items = new ItemData[3];
            playerInventory.items[0] = InventorySystem.InitializeItem(ItemType.Fertilizer);
            playerInventory.items[1] = InventorySystem.InitializeItem(ItemType.Seed);
            playerInventory.items[1].name += " (Carrot)";
            playerInventory.items[1].plantIndex = (int)PlantType.Carrot;
            playerInventory.items[2] = InventorySystem.InitializeItem(ItemType.Seed);
            playerInventory.items[2].name += " (Tomato)";
            playerInventory.items[2].plantIndex = (int)PlantType.Tomato;

            // temp - player data (mainly for gold)
            playerData = PlayerSystem.InitializePlayer("Player", "Player", "pass");
            playerData.inventory = playerInventory;

            // temp - player magic
            playerData.magic = MagicSystem.IntializeMagic();
            playerData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SpellA, playerData.magic.library);
            playerData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SpellB, playerData.magic.library);
            playerData.magic.library.grimiore[0].name = "Fireball";
            playerData.magic.library.grimiore[0].type = SpellType.SpellA; // REVIEW: why this not already in?
            playerData.magic.library.grimiore[0].description = "Conjures a ball of fire to throw around the world.";
            playerData.magic.library.grimiore[0].ingredients = new ItemType[2];
            playerData.magic.library.grimiore[0].ingredients[0] = ItemType.Fertilizer;
            playerData.magic.library.grimiore[0].ingredients[1] = ItemType.Stalk;
            playerData.magic.library.grimiore[1].name = "Healing";
            playerData.magic.library.grimiore[1].description = "Heals friends and foes alike. You're just that nice.";
            playerData.magic.library.grimiore[1].ingredients = new ItemType[2];
            playerData.magic.library.grimiore[1].ingredients[0] = ItemType.Seed;
            playerData.magic.library.grimiore[1].ingredients[1] = ItemType.Fruit;
        }
    }

    void Update()
    {
        // check action input
        ReadActionInput();

        // detect inventory selection input
        DetectInventorySelectionInput();
        // check inventory selection drop
        CheckInventorySelectionDrop();

        if (characterFrozen)
            return;

        // read move input
        ReadMoveInput();
        // move
        DoCharacterMove();

        // clear active loose item if moving
        ClearActiveItem();
        // disallow item pick up if player inventory is full
        if (playerInventory.items.Length < playerInventory.maxSlots)
        {
            // check near loose item
            CheckNearItem();
        }
        // check action input (pickup)
        if ( activeItem != null )
        {
            // uses 'first press' control signal
            if (inputs.actionADown)
            {
                // validate
                if (activeItem.looseItem.inv.items.Length == 0)
                {
                    Debug.LogWarning("--- PlayerControlManager [Update] : trying to take empty loose item. will ignore.");
                    return;
                }
                // pick up loose item, transfer to inventory
                playerInventory = InventorySystem.TakeItem(activeItem.looseItem, out activeItem.looseItem, playerInventory);
                activeItem = null;
            }
            return;
        }
        // NOTE: if loose item active, skip plot activity altogether

        // clear active plot if moving
        ClearActivePlot();
        // check near plot
        CheckNearPlot();

        // temp (a = work land, b = water plot , c = harvest plant, d = uproot plot)
        // temp (hold-type control detection)
        if (activePlot != null)
        {
            if (inputs.actionA)
                activePlot.WorkLand();
            if (inputs.actionB)
                activePlot.WaterPlot();
            if (inputs.actionC)
                activePlot.HarvestPlant();
            if (inputs.actionD)
                activePlot.UprootPlot();
            // if all controls un-pressed, signal plot action clear
            if (!inputs.actionA && !inputs.actionB &&
                !inputs.actionC && !inputs.actionD)
                activePlot.ActionClear();
        }
    }
    
    /// <summary>
    /// Gets item data for current selection of player inventory
    /// </summary>
    /// <returns>item data or null if inventory selection slot is empty</returns>
    public ItemData GetPlayerCurrentItemSelection()
    {
        if ( currentInventorySelection >= playerInventory.items.Length )
            return null;
        return playerInventory.items[currentInventorySelection];
    }

    /// <summary>
    /// Removes the current item selection from player inventory ( *poof* )
    /// </summary>
    public void DeleteCurrentItemSelection()
    {
        if (currentInventorySelection >= playerInventory.items.Length)
            return;
        playerInventory = InventorySystem.RemoveItemFromInventory(playerInventory, playerInventory.items[currentInventorySelection]);
    }

    /// <summary>
    /// Get current player character actions
    /// </summary>
    /// <returns>player actions struct data for this frame</returns>
    public PlayerActions GetPlayerActions()
    {
        return new PlayerActions();
    }

    void ReadMoveInput()
    {
        // reset character move
        characterMove = Vector3.zero;

        // reset gamepad input
        float upPad = 0f;
        float downPad = 0f;
        float leftPad = 0f;
        float rightPad = 0f;

        // check gamepad move input (override if gamepad active)
        // if ( padMgr != null && padMgr.gamepads[0].isActive )
        // {
        //     float padX = padMgr.gamepads[0].XaxisL;
        //     float padY = padMgr.gamepads[0].YaxisL;
        //     upPad = Mathf.Clamp01(padY);
        //     downPad = Mathf.Clamp01(-padY);
        //     leftPad = Mathf.Clamp01(-padX);
        //     rightPad = Mathf.Clamp01(padX);
        // }

        // in each direction, test physics collision first, apply move if clear
        if (inputs.up || upPad > 0f)
        {
            if (upPad == 0f)
                upPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.forward * upPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.forward * characterSpeed * Time.deltaTime;
        }
        if (inputs.down || downPad > 0f)
        {
            if (downPad == 0f)
                downPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.back * downPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.back * characterSpeed * Time.deltaTime;
        }
        if (inputs.left || leftPad > 0f)
        {
            if (leftPad == 0f)
                leftPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.left * leftPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.left * characterSpeed * Time.deltaTime;
        }
        if (inputs.right || rightPad > 0f)
        {
            if (rightPad == 0f)
                rightPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.right * rightPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.right * characterSpeed * Time.deltaTime;
        }
    }

    void DoCharacterMove()
    {
        Vector3 pos = gameObject.transform.position;
        pos += characterMove;
        // handle character art flip
        if (characterMove.x < 0f)
            pam.imageFlipped = true;
        if (characterMove.x > 0f)
            pam.imageFlipped = false;
        gameObject.transform.position = pos;
    }

    void CheckNearItem()
    {
        if (activeItem != null)
            return;

        LooseItemManager[] items = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        for (int i=0; i<items.Length; i++)
        {
            if (Vector3.Distance(items[i].gameObject.transform.position, gameObject.transform.position) < PROXIMITYRANGE)
            {
                activeItem = items[i];
                items[i].SetItemPulse(true);
                break;
            }
        }
    }

    void CheckNearPlot()
    {
        if (activePlot != null)
            return;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i=0; i<plots.Length; i++)
        {
            if (Vector3.Distance(plots[i].gameObject.transform.position,gameObject.transform.position) < PROXIMITYRANGE)
            {
                activePlot = plots[i];
                activePlot.SetCurrentPlayerNetwork(this);
                plots[i].SetCursorPulse(true);
                break;
            }
        }
    }

    void ReadActionInput()
    {
        if (GetInput(out PlayerInput playerInput)) {
            inputs = playerInput;
        }
    }

    void DetectInventorySelectionInput()
    {
        // uses 'first press' control signal
        if (inputs.lBumpDown)
        {
            currentInventorySelection--;
            if (currentInventorySelection < 0)
                currentInventorySelection = playerInventory.maxSlots - 1;
        }
        if (inputs.rBumpDown)
        {
            currentInventorySelection++;
            if (currentInventorySelection > playerInventory.maxSlots - 1)
                currentInventorySelection = 0;
        }
    }

    void CheckInventorySelectionDrop()
    {
        // handle drop item selected, uses 'first press' control signal
        if (inputs.actionCDown && currentInventorySelection < playerInventory.items.Length)
        {
            // spawn loose item dropped from inventory
            ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
            if (ism == null)
                Debug.LogWarning("--- PlayerControlManager [Update] : no item spawn manager found in scene. will ignore.");
            else
            {
                LooseItemData lid = InventorySystem.DropItem(playerInventory.items[currentInventorySelection], playerInventory, out playerInventory);
                Vector3 pos = gameObject.transform.position;
                if (pam.imageFlipped)
                    pos += Vector3.left * PROXIMITYRANGE;
                else
                    pos += Vector3.right * PROXIMITYRANGE;
                pos.x += (RandomSystem.GaussianRandom01() * PROXIMITYRANGE) - (PROXIMITYRANGE / 2f);
                ism.SpawnItem(lid, gameObject.transform.position, pos);
            }
        }
    }

    void ClearActiveItem()
    {
        if (characterMove != Vector3.zero && activeItem != null)
        {
            activeItem.SetItemPulse(false);
            activeItem = null;
        }
    }

    void ClearActivePlot()
    {
        if (characterMove != Vector3.zero && activePlot != null)
        {
            activePlot.SetCurrentPlayer(null);
            activePlot.SetCursorPulse(false);
            activePlot = null;
        }
    }

    void OnGUI()
    {
        if (hidePlayerHUD)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;

        r.x = 0.475f * w;
        r.y = 0.01f * h;
        r.width = 0.05f * w;
        r.height = r.width;

        r.x -= (0.05f * w) * ((playerInventory.maxSlots/2f) + 0.5f);
        for (int i=0; i<5; i++)
        {
            r.x += 0.05f * w;
            if (playerInventory.items != null && playerInventory.items.Length > i)
            {
                if (playerInventory.items[i].type != ItemType.Default)
                {
                    // adjust smaller
                    r.x += 0.005f * w;
                    r.y += (0.005f * w);
                    r.width -= (0.01f * w);
                    r.height -= (0.01f * w);
                    // draw inventory item
                    t = alm.itemImages[alm.GetArtData(playerInventory.items[i].type).artIndexBase];
                    GUI.DrawTexture(r, t);
                    // re-adjust larger again
                    r.x -= 0.005f * w;
                    r.y -= (0.005f * w);
                    r.width += (0.01f * w);
                    r.height += (0.01f * w);
                }
            }
            // draw inventory slot frame
            t = (Texture2D)Resources.Load("Plot_Cursor");
            c = Color.white;
            if (i == currentInventorySelection)
                c = Color.yellow;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            GUI.color = Color.white;
        }

        // gold display
        r.x = 0.6375f * w;
        r.y = 0.02f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleLeft;
        g.fontSize = Mathf.RoundToInt(20f * (w / 1204f));
        g.fontStyle = FontStyle.Bold;
        string s = "GOLD: ";
        s += playerData.gold.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        if (currentInventorySelection >= playerInventory.items.Length)
            return;

        // bg and label for current item selected
        r.x = 0.375f * w;
        r.y = 0.1f * h;
        r.width = 0.25f * w;
        r.height = 0.05f * h;

        t = Texture2D.whiteTexture;
        c = Color.white;
        c.r = .1f;
        c.g = .1f;
        c.b = .1f;
        c.a = 0.25f;
        GUI.color = c;
        GUI.DrawTexture(r, t);
        GUI.color = Color.white;

        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt( 22f * (w/1204f));
        g.fontStyle = FontStyle.Bold;
        s = playerInventory.items[currentInventorySelection].name;

        r.x += 0.0005f * w;
        r.y += 0.0008f * w;
        GUI.color = Color.black;
        GUI.Label(r, s, g);

        r.x -= 0.001f * w;
        r.y -= 0.0016f * w;
        GUI.color = Color.white;
        GUI.Label(r, s, g);
    }
}
