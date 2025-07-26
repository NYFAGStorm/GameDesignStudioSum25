using UnityEngine;

public class CharacterAnimManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the animation of a character, as directed by character control

    public enum CharacterPose
    {
        Default,
        Front,
        Side
    }

    public PlayerModelType type;
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
            type = PlayerModelType.Male;
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

        // line (_LineArt)
        rend.material.SetTexture("_LineArt", characterLayers[0]);
        // hair (_HairFill)
        rend.material.SetTexture("_HairFill", characterLayers[1]);
        // skin (_SkinFill)
        rend.material.SetTexture("_SkinFill", characterLayers[2]);
        // light (_AccentFill)
        rend.material.SetTexture("_AccentFill", characterLayers[3]);
        // medium (_AltFill)
        rend.material.SetTexture("_AltFill", characterLayers[4]);
        // dark (_MainTex)
        rend.material.SetTexture("_MainTex", characterLayers[5]);

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
        if (rend == null)
            return;

        type = options.model;

        string artNameBase = "Wizard ";
        string charType = type.ToString() + " ";
        string poseName = pose.ToString() + " ";
        string suffix = "128 ";
        string currentArtSet = artNameBase + charType + poseName + suffix;

        // store layer images
        characterLayers = new Texture2D[6];
        characterLayers[0] = (Texture2D)Resources.Load(currentArtSet + "Line");
        characterLayers[1] = (Texture2D)Resources.Load(currentArtSet + "Hair");
        characterLayers[2] = (Texture2D)Resources.Load(currentArtSet + "Skin");
        characterLayers[3] = (Texture2D)Resources.Load(currentArtSet + "Light");
        characterLayers[4] = (Texture2D)Resources.Load(currentArtSet + "Medium");
        characterLayers[5] = (Texture2D)Resources.Load(currentArtSet + "Dark");

        // line (_LineArt)
        rend.material.SetTexture("_LineArt", characterLayers[0]);
        // hair (_HairFill, _HairCol)
        rend.material.SetTexture("_HairFill", characterLayers[1]);
        rend.material.SetColor("_HairCol", PlayerSystem.GetPlayerHairColor(options.hairColor));
        // skin (_SkinFill, _SkinCol)
        rend.material.SetTexture("_SkinFill", characterLayers[2]);
        rend.material.SetColor("_SkinCol", PlayerSystem.GetPlayerSkinColor(options.skinColor));
        // light (_AccentFill, _AccentCol)
        rend.material.SetTexture("_AccentFill", characterLayers[3]);
        rend.material.SetColor("_AccentCol", PlayerSystem.GetPlayerColor(options.accentColor));
        // medium (_AltFill, _AltCol)
        rend.material.SetTexture("_AltFill", characterLayers[4]);
        rend.material.SetColor("_AltCol", PlayerSystem.GetPlayerColor(options.secondaryColor));
        // dark (_MainTex, _Color)
        rend.material.SetTexture("_MainTex", characterLayers[5]);
        rend.material.SetColor("_Color", PlayerSystem.GetPlayerColor(options.mainColor));
    }
}
