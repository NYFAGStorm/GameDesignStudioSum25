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
        ActionX,
        ActionY
    }
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode actionAKey = KeyCode.E;

    public bool characterFrozen;

    private Vector3 characterMove;
    private PlotManager activePlot;

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

        // clear active plot if moving
        if (characterMove != Vector3.zero && activePlot != null)
        {
            activePlot.SetCursorPulse(false);
            activePlot = null;
        }
        // check near plot
        CheckNearPlot();

        // check action input
        if (ReadActionInput() == PlayerControlType.ActionA && activePlot)
            activePlot.WorkLand();
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

    // temp
    PlayerControlType ReadActionInput()
    {
        PlayerControlType retInput = PlayerControlType.Default;

        if (Input.GetKeyDown(actionAKey))
            retInput = PlayerControlType.ActionA;

        return retInput;
    }
}
