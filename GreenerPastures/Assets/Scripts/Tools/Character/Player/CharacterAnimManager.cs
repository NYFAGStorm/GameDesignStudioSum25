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

    public void ConfigureAppearance( PlayerOptions options )
    {
        Renderer r = transform.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            if (options.model == PlayerModelType.Male)
            {
                // line (_LineArt)
                r.material.SetTexture("_LineArt", (Texture2D)Resources.Load("ProtoWizard_LineArt"));
                // skin (_AccentFill,_AccentCol)
                r.material.SetTexture("_AccentFill", (Texture2D)Resources.Load("ProtoWizard_FillSkin"));
                r.material.SetColor("_AccentCol", PlayerSystem.GetPlayerSkinColor(options.skinColor));
                // accent (_AltFill, _AltCol)
                r.material.SetTexture("_AltFill", (Texture2D)Resources.Load("ProtoWizard_FillAccent"));
                r.material.SetColor("_AltCol", PlayerSystem.GetPlayerColor(options.accentColor));
                // fill (_MainTex, _Color)
                r.material.SetTexture("_MainTex", (Texture2D)Resources.Load("ProtoWizard_FillMain"));
                r.material.SetColor("_Color", PlayerSystem.GetPlayerColor(options.mainColor));
            }
            else if (options.model == PlayerModelType.Female)
            {
                // line (_LineArt)
                r.material.SetTexture("_LineArt", (Texture2D)Resources.Load("ProtoWizardF_LineArt"));
                // skin (_AccentFill,_AccentCol)
                r.material.SetTexture("_AccentFill", (Texture2D)Resources.Load("ProtoWizardF_FillSkin"));
                r.material.SetColor("_AccentCol", PlayerSystem.GetPlayerSkinColor(options.skinColor));
                // accent (_AltFill, _AltCol)
                r.material.SetTexture("_AltFill", (Texture2D)Resources.Load("ProtoWizardF_FillAccent"));
                r.material.SetColor("_AltCol", PlayerSystem.GetPlayerColor(options.accentColor));
                // fill (_MainTex, _Color)
                r.material.SetTexture("_MainTex", (Texture2D)Resources.Load("ProtoWizardF_FillMain"));
                r.material.SetColor("_Color", PlayerSystem.GetPlayerColor(options.mainColor));
            }
        }
    }
}
