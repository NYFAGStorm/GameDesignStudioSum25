using UnityEngine;

public class NPCController : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles NPC character movements either alone or via scripted action from another script

    public enum NPCMode
    {
        Default,
        Scripted,
        Teleporting,
        Traveling,
        Wandering
    }
    public NPCMode mode;
    public bool ghostMode; // can move through solid objects

    public float movementSpeed = 1f;
    public Vector3 moveTarget;
    public bool destinationReached; // readable as a call-back

    private Vector3 moveVector;
    private bool imageFlipped;
    private Renderer rend;

    const float MOVETARGETTHRESHOLD = 0.2f;


    void Start()
    {
        // validate
        rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("--- NPCController [Start] : " + gameObject.name + " no renderer found on child obhect. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        // handle move target
        HandleMoveTarget();
        // handle movement
        if (HandleMovement())
        {
            // handle image flip
            HandleImageFlip();
            // detect target destination reached
            destinationReached = IsAtTarget();
        }
    }

    void HandleMoveTarget()
    {
        if (IsAtTarget())
        {
            moveVector = Vector3.zero;
            return;
        }

        // calculate move vector
        Vector3 move = (moveTarget - gameObject.transform.position);
        if (move.magnitude < movementSpeed)
            move = move.normalized * (move.magnitude / movementSpeed);
        else
            move.Normalize();
        moveVector = move * movementSpeed * Time.deltaTime;
    }

    bool HandleMovement()
    {
        if (moveVector == Vector3.zero)
            return false;

        if (mode == NPCMode.Teleporting)
        {
            // teleport character
            Vector3 pos = transform.position;
            pos += moveTarget;
            transform.position = pos;
        }
        else if (ghostMode)
        {
            // move character
            Vector3 pos = transform.position;
            pos += moveVector;
            transform.position = pos;
        }
        // non-ghost mode movement would stop at collision detection

        return true;
    }

    void HandleImageFlip()
    {
        // determine image flip
        imageFlipped = (moveVector.x < 0f);
        // handle image flip
        Vector2 flipVec = new Vector2(1f, 1f);
        if (imageFlipped)
            flipVec.x = -1f;
        rend.material.SetTextureScale("_MainTex", flipVec);
    }

    bool IsAtTarget()
    {
        return (Vector3.Distance(transform.position, moveTarget) < MOVETARGETTHRESHOLD);
    }
}
