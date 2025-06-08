using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        public bool actionB;
        public bool actionC;
        public bool actionD;
    }

    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode actionAKey = KeyCode.E;
    public KeyCode actionBKey = KeyCode.F;
    public KeyCode actionCKey = KeyCode.C;
    public KeyCode actionDKey = KeyCode.V;

    public bool characterFrozen;

    private Vector3 characterMove;
    private LooseItemManager activeItem;
    private PlotManager activePlot;
    private PlayerActions characterActions;

    private InventoryData playerInventory;
    private int currentInventorySelection;
    private float inventorySelectionTimer;
    private bool itemTakenAction;

    private CameraManager cam;
    private PlayerAnimManager pam;
    private SpriteLibraryManager slm;

    const float PROXIMITYRANGE = 0.381f;
    const float INVENTORYSELECTIONTIME = 2f;


    void Start()
    {
        // validate
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
        slm = GameObject.FindFirstObjectByType<SpriteLibraryManager>();
        if (slm == null)
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no sprite library manager found. aborting.");
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
            playerInventory.items[0] = InventorySystem.InitializeItem(ItemType.ItemA);
            playerInventory.items[1] = InventorySystem.InitializeItem(ItemType.ItemB);
            playerInventory.items[2] = InventorySystem.InitializeItem(ItemType.ItemB);
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

        // reset item taken action
        ResetItemTakenAction();

        // detect inventory selection input
        DetectInventorySelectionInput();
        // run inventory selection timer
        CheckInventorySelectionDrop();

        // clear active loose item if moving
        ClearActiveItem();
        // check near loose item
        CheckNearItem();
        // check action input (pickup)
        if ( activeItem != null )
        {
            if (characterActions.actionA && !itemTakenAction)
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
                itemTakenAction = true; // player must un-press action button to take again
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
            // REVIEW: order?
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
        // in each direction, test physics collision first, apply move if clear
        if (Input.GetKey(upKey))
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.forward * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.forward * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(downKey))
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.back * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.back * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(leftKey))
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.left * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.left * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rightKey))
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.right * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.right * characterSpeed * Time.deltaTime;
        }
    }

    void DoCharacterMove()
    {
        Vector3 pos = gameObject.transform.position;
        pos += characterMove;
        // handle character sprite flip
        if (characterMove.x < 0f)
            pam.spriteFlipped = true;
        if (characterMove.x > 0f)
            pam.spriteFlipped = false;
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
        characterActions.actionB = Input.GetKey(actionBKey);
        characterActions.actionC = Input.GetKey(actionCKey);
        characterActions.actionD = Input.GetKey(actionDKey);
    }

    void ResetItemTakenAction()
    {
        if (itemTakenAction && !characterActions.actionA)
            itemTakenAction = false;
    }

    void DetectInventorySelectionInput()
    {
        // REVIEW: controls for inventory selection
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            inventorySelectionTimer = INVENTORYSELECTIONTIME;
            currentInventorySelection--;
            if (currentInventorySelection < 0)
                currentInventorySelection = playerInventory.maxSlots - 1;
        }
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            inventorySelectionTimer = INVENTORYSELECTIONTIME;
            currentInventorySelection++;
            if (currentInventorySelection > playerInventory.maxSlots - 1)
                currentInventorySelection = 0;
        }
    }

    void CheckInventorySelectionDrop()
    {
        if (inventorySelectionTimer > 0f)
        {
            inventorySelectionTimer -= Time.deltaTime;
            if (inventorySelectionTimer < 0f)
                inventorySelectionTimer = 0f;
            else
            {
                // handle drop item selected
                if (characterActions.actionC && currentInventorySelection < playerInventory.items.Length)
                {
                    inventorySelectionTimer = 0f; // prevents dropping multiple items
                    // spawn loose item dropped from inventory
                    ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
                    if (ism == null)
                        Debug.LogWarning("--- PlayerControlManager [Update] : no item spawn manager found in scene. will ignore.");
                    else
                    {
                        LooseItemData lid = InventorySystem.DropItem(playerInventory.items[currentInventorySelection], playerInventory, out playerInventory);
                        Vector3 pos = gameObject.transform.position;
                        if (pam.spriteFlipped)
                            pos += Vector3.left * PROXIMITYRANGE;
                        else
                            pos += Vector3.right * PROXIMITYRANGE;
                        pos.x += (RandomSystem.GaussianRandom01() * PROXIMITYRANGE) - (PROXIMITYRANGE / 2f);
                        ism.SpawnItem(lid, gameObject.transform.position, pos);
                    }
                }
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
                    t = (Texture2D)slm.itemSprites[slm.GetSpriteData(playerInventory.items[i].type).spriteIndexBase].texture;
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
            if (i == currentInventorySelection && inventorySelectionTimer > 0f)
                c = Color.yellow;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            GUI.color = Color.white;
        }
    }
}
