using UnityEngine;

public class CharacterAnimManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the animation of a character, as directed by character control

    public enum CharacterType
    {
        Default,
        Male,
        Female
    }
    public enum CharacterPose
    {
        Default,
        Front,
        Side
    }

    public CharacterType type;
    public CharacterPose pose;
    public Vector3 characterMoveVector; // animation operates from this updated move vector
    
    private bool imageFlipped;
    private Renderer rend;
    private Texture2D[] characterLayers;
    private CharacterPose previousPose;


    void Start()
    {
        // validate
        rend = gameObject.GetComponent<Renderer>();
        if ( rend == null )
        {
            Debug.LogError("--- CharacterAnimManager [Start] : " + gameObject.name + " no renderer found. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            type = CharacterType.Male;
            pose = CharacterPose.Front;
            previousPose = pose;
            characterLayers = new Texture2D[0];
        }
    }

    void Update()
    {
        ProcessMoveVector();

        UpdatePoseLayers();

        HandleImageFlip();
    }

    void ProcessMoveVector()
    {
        pose = CharacterPose.Side;
        if (characterMoveVector.z < 0f && characterMoveVector.x == 0f) // REVIEW:
            pose = CharacterPose.Front;
        imageFlipped = (characterMoveVector.x < 0f && pose == CharacterPose.Side);
    }

    public bool GetImageFlipped()
    {
        return imageFlipped;
    }

    void UpdatePoseLayers()
    {
        if (previousPose == pose)
            return;

        // TODO:

        previousPose = pose;
    }

    void HandleImageFlip()
    {
        // handle image flip
        Vector2 flipVec = new Vector2(1f, 1f);
        if (imageFlipped)
            flipVec.x = -1f;
        rend.material.SetTextureScale("_MainTex", flipVec);
    }
}
