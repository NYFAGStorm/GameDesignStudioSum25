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

    private CameraManager cam;
    private PlayerAnimManager pam;

    const float PROXIMITYRANGE = 0.381f;


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
        // initialize
        if (enabled)
        {
            cam.SetPlayer(this);
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

        // clear active loose item if moving
        if (characterMove != Vector3.zero && activeItem != null)
        {
            activeItem.SetItemPulse(false);
            activeItem = null;
        }
        // check near loose item
        CheckNearItem();
        // check action input (pickup)
        if ( activeItem != null )
        {
            if (characterActions.actionA)
            {
                // pick up loose item, transfer to inventory
                print("- player picks up '" + activeItem.looseItem.inv.items[0].name +"' -");
            }
            return;
        }
        // NOTE: if loose item active, skip plot activity altogether

        // clear active plot if moving
        if (characterMove != Vector3.zero && activePlot != null)
        {
            activePlot.SetCursorPulse(false);
            activePlot = null;
        }
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
}
