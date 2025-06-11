using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the local player controls for their character

    public float characterSpeed = 2.7f;

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

    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode actionAKey = KeyCode.E;
    public KeyCode actionBKey = KeyCode.F;
    public KeyCode actionCKey = KeyCode.C;
    public KeyCode actionDKey = KeyCode.V;
    public KeyCode lBumpKey = KeyCode.LeftBracket;
    public KeyCode rBumpKey = KeyCode.RightBracket;

    public bool characterFrozen;

    private Vector3 characterMove;
    private LooseItemManager activeItem;
    private PlotManager activePlot;
    private PlayerActions characterActions;

    private InventoryData playerInventory;
    private int currentInventorySelection;

    private MultiGamepad padMgr;

    private CameraManager cam;
    private PlayerAnimManager pam;
    private ArtLibraryManager alm;

    const float PROXIMITYRANGE = 0.381f;
    const float INVENTORYSELECTIONTIME = 999f; // was 2f


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        // TODO: change this to error and abort if no gamepad manager found (allow no pad for testing)
        // (then clean up below checks for padMgr existing)
        if (padMgr == null )
            Debug.LogWarning("--- PlayerControlManager [Start] : " + gameObject.name + " no pad manager. will ignore.");
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
            playerInventory.items[2] = InventorySystem.InitializeItem(ItemType.Seed);
        }
    }

    void Update()
    {
        if (characterFrozen)
            return;

        // read move input
        ReadMoveInput();
        // move
        DoCharacterMove();
        // check action input
        ReadActionInput();

        // detect inventory selection input
        DetectInventorySelectionInput();
        // run inventory selection timer
        CheckInventorySelectionDrop();

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
            if (characterActions.actionADown)
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
            if (characterActions.actionA)
                activePlot.WorkLand();
            if (characterActions.actionB)
                activePlot.WaterPlot();
            if (characterActions.actionC)
                activePlot.HarvestPlant();
            if (characterActions.actionD)
                activePlot.UprootPlot();
            // if all controls un-pressed, signal plot action clear
            if (!characterActions.actionA && !characterActions.actionB &&
                !characterActions.actionC && !characterActions.actionD)
                activePlot.ActionClear();
        }
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
        if ( padMgr != null && padMgr.gamepads[0].isActive )
        {
            float padX = padMgr.gamepads[0].XaxisL;
            float padY = padMgr.gamepads[0].YaxisL;
            upPad = Mathf.Clamp01(padY);
            downPad = Mathf.Clamp01(-padY);
            leftPad = Mathf.Clamp01(-padX);
            rightPad = Mathf.Clamp01(padX);
        }

        // in each direction, test physics collision first, apply move if clear
        if (Input.GetKey(upKey) || upPad > 0f)
        {
            if (upPad == 0f)
                upPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.forward * upPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.forward * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(downKey) || downPad > 0f)
        {
            if (downPad == 0f)
                downPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.back * downPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.back * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(leftKey) || leftPad > 0f)
        {
            if (leftPad == 0f)
                leftPad = 1f;
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.left * leftPad * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.left * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rightKey) || rightPad > 0f)
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
                plots[i].SetCursorPulse(true);
                break;
            }
        }
    }

    void ReadActionInput()
    {
        characterActions = new PlayerActions();

        characterActions.actionA = Input.GetKey(actionAKey);
        characterActions.actionADown = Input.GetKeyDown(actionAKey);
        characterActions.actionB = Input.GetKey(actionBKey);
        characterActions.actionBDown = Input.GetKeyDown(actionBKey);
        characterActions.actionC = Input.GetKey(actionCKey);
        characterActions.actionCDown = Input.GetKeyDown(actionCKey);
        characterActions.actionD = Input.GetKey(actionDKey);
        characterActions.actionDDown = Input.GetKeyDown(actionDKey);
        characterActions.lBump = Input.GetKey(lBumpKey);
        characterActions.lBumpDown = Input.GetKeyDown(lBumpKey);
        characterActions.rBump = Input.GetKey(rBumpKey);
        characterActions.rBumpDown = Input.GetKeyDown(rBumpKey);

        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            // use standard 'hold' signals from gamepad for these buttons
            characterActions.actionA = padMgr.gamepads[0].aButton;
            characterActions.actionADown = padMgr.gPadDown[0].aButton;
            characterActions.actionB = padMgr.gamepads[0].bButton;
            characterActions.actionBDown = padMgr.gPadDown[0].bButton;
            characterActions.actionC = padMgr.gamepads[0].xButton;
            characterActions.actionCDown = padMgr.gPadDown[0].xButton;
            characterActions.actionD = padMgr.gamepads[0].yButton;
            characterActions.actionDDown = padMgr.gPadDown[0].yButton;
            characterActions.lBump = padMgr.gamepads[0].LBump;
            characterActions.lBumpDown = padMgr.gPadDown[0].LBump;
            characterActions.rBump = padMgr.gamepads[0].RBump;
            characterActions.rBumpDown = padMgr.gPadDown[0].RBump;
        }
    }

    void DetectInventorySelectionInput()
    {
        // uses 'first press' control signal
        if (characterActions.lBumpDown)
        {
            currentInventorySelection--;
            if (currentInventorySelection < 0)
                currentInventorySelection = playerInventory.maxSlots - 1;
        }
        if (characterActions.rBumpDown)
        {
            currentInventorySelection++;
            if (currentInventorySelection > playerInventory.maxSlots - 1)
                currentInventorySelection = 0;
        }
    }

    void CheckInventorySelectionDrop()
    {
        // handle drop item selected, uses 'first press' control signal
        if (characterActions.actionCDown && currentInventorySelection < playerInventory.items.Length)
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
            activePlot.SetCursorPulse(false);
            activePlot = null;
        }
    }

    void OnGUI()
    {
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

        if (currentInventorySelection >= playerInventory.items.Length)
            return;

        // label for current item selected
        r.x = 0.25f * w;
        r.y = 0.075f * h;
        r.width = 0.5f * w;
        r.height = 0.1f * h;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt( 22f * (w/1204f));
        g.fontStyle = FontStyle.Bold;
        string s = playerInventory.items[currentInventorySelection].name;

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
