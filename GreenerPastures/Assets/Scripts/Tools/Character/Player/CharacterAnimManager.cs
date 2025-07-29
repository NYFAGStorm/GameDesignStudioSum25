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
            previousPose = pose;
        }
    }

    void Update()
    {
        ProcessMoveVector();

        UpdatePoseLayers();

        HandleImageFlip();
    }

    public bool GetImageFlipped()
    {
        return imageFlipped;
    }

    void ProcessMoveVector()
    {
        // force characters to predominently face forward while moving to camera
        if (characterMoveVector.z < 0f && 
            (Mathf.Abs(characterMoveVector.x) < Mathf.Abs(characterMoveVector.z)))
            characterMoveVector.x = 0f;

        // detect pose changes
        if (characterMoveVector.z < 0f)
            pose = CharacterPose.Front;
        if (Mathf.Abs(characterMoveVector.x) > 0f)
            pose = CharacterPose.Side;
        // detect change in facing direction
        if (imageFlipped && characterMoveVector.x > 0f)
            imageFlipped = false;
        if (!imageFlipped && characterMoveVector.x < 0f)
            imageFlipped = true;
    }

    void UpdatePoseLayers()
    {
        if (rend == null || characterLayers == null || characterLayers.Length == 0)
            return;

        if (previousPose == pose)
            return;

        int idx = 0;
        if (pose == CharacterPose.Side)
            idx += 6;

        // line (_LineArt)
        rend.material.SetTexture("_LineArt", characterLayers[idx++]);
        // hair (_HairFill)
        rend.material.SetTexture("_HairFill", characterLayers[idx++]);
        // skin (_SkinFill)
        rend.material.SetTexture("_SkinFill", characterLayers[idx++]);
        // light (_AccentFill)
        rend.material.SetTexture("_AccentFill", characterLayers[idx++]);
        // medium (_AltFill)
        rend.material.SetTexture("_AltFill", characterLayers[idx++]);
        // dark (_MainTex)
        rend.material.SetTexture("_MainTex", characterLayers[idx++]);

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
        {
            rend = gameObject.GetComponent<Renderer>();
            if (rend == null)
            {
                Debug.LogError("--- CharacterAnimManager [ConfigureAppearance] : " + gameObject.name + " no renderer found. aborting.");
                enabled = false;
            }
        }

        type = options.model;

        string artNameBase = "Wizard ";
        string charType = type.ToString() + " ";
        string poseName = pose.ToString() + " ";
        string suffix = "";
        string currentArtSet = artNameBase + charType + poseName + suffix;

        // store layer images
        // (front)
        characterLayers = new Texture2D[12];
        currentArtSet = artNameBase + charType + "Front " + suffix;
        characterLayers[0] = (Texture2D)Resources.Load(currentArtSet + "Line");
        characterLayers[1] = (Texture2D)Resources.Load(currentArtSet + "Hair");
        characterLayers[2] = (Texture2D)Resources.Load(currentArtSet + "Skin");
        characterLayers[3] = (Texture2D)Resources.Load(currentArtSet + "Light");
        characterLayers[4] = (Texture2D)Resources.Load(currentArtSet + "Medium");
        characterLayers[5] = (Texture2D)Resources.Load(currentArtSet + "Dark");
        // (side)
        currentArtSet = artNameBase + charType + "Side " + suffix;
        characterLayers[6] = (Texture2D)Resources.Load(currentArtSet + "Line");
        characterLayers[7] = (Texture2D)Resources.Load(currentArtSet + "Hair");
        characterLayers[8] = (Texture2D)Resources.Load(currentArtSet + "Skin");
        characterLayers[9] = (Texture2D)Resources.Load(currentArtSet + "Light");
        characterLayers[10] = (Texture2D)Resources.Load(currentArtSet + "Medium");
        characterLayers[11] = (Texture2D)Resources.Load(currentArtSet + "Dark");

        // apply front pose to current character art
        currentArtSet = artNameBase + charType + poseName + suffix;
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
