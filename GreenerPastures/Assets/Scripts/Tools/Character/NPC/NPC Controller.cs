using UnityEngine;

public class NPCController : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles NPC character movements via scripted action from another script

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

    public float movementSpeed = 1.5f;
    public Vector3 moveTarget;
    public bool destinationReached; // readable as a call-back

    private Vector3 moveVector;
    private Vector3 previousMoveVector;
    private CharacterAnimManager pam;

    const float MOVETARGETTHRESHOLD = 0.1f;


    void Start()
    {
        // validate
        pam = GetComponentInChildren<CharacterAnimManager>();
        if (pam == null)
        {
            Debug.LogError("--- NPCController [Start] : " + gameObject.name + " no character animation manager found on child obhect. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            if (pam.singleLayerCharacter)
                pam.ConfigureAppearance(new PlayerOptions());
        }
    }

    void Update()
    {
        // handle move target
        HandleMoveTarget();
        // handle movement
        if (HandleMovement())
        {
            // handle character animation
            HandleCharacterAnimation();
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
        if (move.magnitude < MOVETARGETTHRESHOLD)
            move = move.normalized * (move.magnitude / movementSpeed);
        else
            move.Normalize();
        moveVector = move * movementSpeed * Time.deltaTime;
        // used for anim control
        previousMoveVector = moveVector;
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

    public void SetCharacterAnimMoveVector( Vector3 animMoveVector )
    {
        pam.characterMoveVector = animMoveVector;
    }

    void HandleCharacterAnimation()
    {
        pam.characterMoveVector = previousMoveVector;
    }

    bool IsAtTarget()
    {
        return (Vector3.Distance(transform.position, moveTarget) < MOVETARGETTHRESHOLD);
    }
}
