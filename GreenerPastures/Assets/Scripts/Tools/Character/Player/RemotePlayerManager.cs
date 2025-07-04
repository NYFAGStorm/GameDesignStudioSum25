using UnityEngine;

public class RemotePlayerManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles un-packing and manipulation of a remote player's character in this net game

    // TODO: declare main reference to multiplayer data coming in

    public string profileID; // remote player profile ID
    public string playerName; // remote player name

    public PositionData playerPosition; // x,y,z + 'w' which = -1 if flipped art

    private PositionData previousPosition; // perhaps we use this to help smooth pops in movement
    private PlayerAnimManager pam;

    private bool remotePlayerIntialized;

    const float LERPDISTANCETHRESHOLD = 1f;


    void Start()
    {
        // validate
        pam = gameObject.GetComponentInChildren<PlayerAnimManager>();
        if (pam == null)
        {
            Debug.LogError("--- RemotePlayerManager [Start] : " + gameObject.name + " no player anim manager found on this object. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            previousPosition = playerPosition;
        }
    }

    void Update()
    {
        if (!remotePlayerIntialized)
            return;

        // update player position
        UpdatePlayerPosition();
    }

    public void InitializeRemotePlayer( string profID, string pName )
    {
        profileID = profID;
        playerName = pName;
        // get player options from game data, configure appearance
        SaveLoadManager saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr != null)
        {
            GameData gData = saveMgr.GetCurrentGameData();
            if (gData != null)
            {
                PlayerData pData = GameSystem.GetProfilePlayer(gData, profileID);
                if (pData != null)
                    ConfigureAppearance(pData.options);
            }
        }
        remotePlayerIntialized = true;
    }

    public void SetRemotePlayerPosition( Vector3 pos, bool artFlipped )
    {
        previousPosition = playerPosition;
        playerPosition.x = pos.x;
        playerPosition.y = pos.y;
        playerPosition.z = pos.z;
        playerPosition.w = artFlipped ? -1 : 1;
    }

    public void SetRemotePlayerPosition( PositionData pos )
    {
        previousPosition = playerPosition;
        playerPosition = pos;
    }

    void UpdatePlayerPosition()
    {
        // REVIEW: perform some lerp if distance is great?
        if (GameSystem.PositionDistance(previousPosition, playerPosition) > LERPDISTANCETHRESHOLD)
        {
            // instead of set, do fancy stuff like lerp position before next tick?
            //GameSystem.Lerp(previousPosition, playerPosition, 0.1f); // enter some progress value of (tick/Time.deltaTime)
            // REVIEW: could do 'art flipped' from here, based change from previous
        }
        Vector3 pos = Vector3.zero;
        pos.x = playerPosition.x;
        pos.y = playerPosition.y;
        pos.z = playerPosition.z;
        pam.imageFlipped = playerPosition.w < 0f;
        gameObject.transform.position = pos;
    }

    // the same routine as used in player control manager
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
